using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Celeste.Mod.Helpers;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.FemtoHelper.CustomFancyText;

public static class ObfuscatedFancyText
{
    private const string LogId = $"{nameof(FemtoHelper)}/{nameof(ObfuscatedFancyText)}";

    public const string ObfuscatedCommand = "femto_obfuscated";
    public const string ObfuscatedDynDataKey = "FemtoHelper_CurrentObfuscated";
    public const string ObfuscatedDynDataNoOrigChance = "FemtoHelper_CurrentObfuscated_NoOrigChance";
    public const string ObfuscatedDynDataFullObfuscation = "FemtoHelper_CurrentObfuscated_FullObfuscation";

    public const string CaseShiftCommand = "femto_caseshift";
    public const string CaseShiftDynDataKey = "FemtoHelper_CurrentCaseShift";

    /// <summary>
    ///   Characters of a <see cref="PixelFontSize"/> grouped by their width.
    /// </summary>
    /// <remarks>
    ///   The type of the <see cref="List{T}"/> is <see cref="int"/> instead of <see cref="char"/> to
    ///   avoid having to cast.
    /// </remarks>
    private static readonly Dictionary<PixelFontSize, Dictionary<int, List<int>>> CharWidthMap = [];

    private static ILHook Hook_FancyText_AddWord;

    internal static void Load()
    {
        On.Celeste.FancyText.Char.Draw += AddBeforeDrawCallHook;
        IL.Celeste.FancyText.Parse += ParseObfuscatedCommandHook;
        Hook_FancyText_AddWord = new ILHook(
            typeof(FancyText).GetMethod("orig_AddWord", BindingFlags.NonPublic | BindingFlags.Instance)!,
            InjectSpecialCharHook);
    }

    internal static void Unload()
    {
        On.Celeste.FancyText.Char.Draw -= AddBeforeDrawCallHook;
        IL.Celeste.FancyText.Parse -= ParseObfuscatedCommandHook;
        Hook_FancyText_AddWord?.Dispose();
    }

    /// <summary>
    ///     Runs before and after Draw callbacks for <see cref="ISpecialChar"/>s
    /// </summary>
    /// <remarks>
    ///     Enables swapping the characters in obfuscated text and case shift text before they get rendered.
    /// </remarks>
    private static void AddBeforeDrawCallHook(
        On.Celeste.FancyText.Char.orig_Draw orig,
        FancyText.Char self,
        PixelFont font,
        float baseSize,
        Vector2 position,
        Vector2 scale,
        float alpha)
    {
        ISpecialChar specialChar = self as ISpecialChar;
        //ObfuscatedChar corruptedChar = self as ObfuscatedChar;
        specialChar?.BeforeDraw(font, baseSize, scale);
        orig(self, font, baseSize, position, scale, alpha);
        specialChar?.AfterDraw(font, baseSize, scale);
    }

    /// <summary>
    ///   Inject the custom text command checks
    /// </summary>
    private static void ParseObfuscatedCommandHook(ILContext il)
    {
        ILCursor cursor = new(il);

        ILLabel @continue = null;

        // pain
        // need to match the br too, because else prismatic's hook will throw us off
        // once we find a match, go to the next ldloc.7 to go to the correct place
        if (!cursor.TryGotoNextBestFit(
            MoveType.Before,
            static instr => instr.MatchBr(out _),
            static instr => instr.MatchLdloc(7),
            static instr => instr.MatchLdstr("savedata"),
            static instr => instr.MatchCallvirt<string>("Equals"),
            instr => instr.MatchBrfalse(out @continue)))
        {
            Logger.Error(LogId, "Could not insert obfuscated text command check!");
            return;
        }

        cursor.GotoNext(MoveType.AfterLabel, static instr => instr.MatchLdloc(7));
        cursor.EmitLdarg0();
        cursor.EmitLdloc(7);
        cursor.EmitLdloc(8);
        cursor.EmitDelegate(TryMatchObfuscatedCommand);
        cursor.EmitBrtrue(@continue);
    }

    /// <summary>
    ///   Attempt to match custom text commands
    /// </summary>
    /// <returns>
    ///   Whether the start or end marker for the obfuscated command has been matched.
    /// </returns>
    private static bool TryMatchObfuscatedCommand(FancyText self, string text, List<string> list)
    {
        DynamicData selfData = DynamicData.For(self);

        switch (text)
        {
            case ObfuscatedCommand:
                selfData.Set(ObfuscatedDynDataKey, true);

                float noOrigChance = 0.5f;
                bool full = false;

                if (list.Count > 0 && float.TryParse(list[0], out var result))
                {
                    noOrigChance = result / 100f;
                }

                if (list.Count > 1 && list[1].Equals("full", StringComparison.CurrentCultureIgnoreCase))
                {
                    full = true;
                }

                selfData.Set(ObfuscatedDynDataNoOrigChance, noOrigChance);
                selfData.Set(ObfuscatedDynDataFullObfuscation, full);
                return true;
            case "/" + ObfuscatedCommand:
                selfData.Set(ObfuscatedDynDataKey, false);
                return true;
            case CaseShiftCommand:
                selfData.Set(CaseShiftDynDataKey, true);
                return true;
            case "/" + CaseShiftCommand:
                selfData.Set(CaseShiftDynDataKey, false);
                return true;
        }
        return false;
    }

    /// <summary>
    ///   Inject <see cref="SwapForSpecialChar"/>.
    /// </summary>
    private static void InjectSpecialCharHook(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNext(static instr => instr.MatchCallvirt(typeof(List<FancyText.Node>).GetMethod("Add")!)))
        {
            Logger.Error(LogId, "Could not insert obfuscated char injection!");
            return;
        }
        cursor.EmitLdarg0();
        cursor.EmitDelegate(SwapForSpecialChar);
    }

    /// <summary>
    ///   Replace <see cref="FancyText.Char"/>s for <see cref="ISpecialChar"/> types depending on the command matched in <cee cref="TryMatchObfuscatedCommand"/>
    /// </summary>
    private static FancyText.Char SwapForSpecialChar(FancyText.Char node, FancyText self)
    {
        DynamicData selfData = DynamicData.For(self);

        if (selfData.Get(ObfuscatedDynDataKey) is true)
        {
            return ObfuscatedChar.FromChar(node, (float)(selfData.Get(ObfuscatedDynDataNoOrigChance) ?? 0.5f), (bool)selfData.Get(ObfuscatedDynDataFullObfuscation));
        }
        else if (selfData.Get(CaseShiftDynDataKey) is true)
        {
            return CaseShiftChar.FromChar(node);
        }

        return node;
    }

    /// <summary>
    ///   Return a list of <see cref="char"/>s that have the same width as the passed <paramref name="character"/>
    ///   in the given <see cref="PixelFontSize"/>.
    /// </summary>
    private static List<int> GetSimilarWidthChars(PixelFontSize pixelFontSize, int character)
    {
        Dictionary<int, List<int>> similarWidthChars = CharWidthMap[pixelFontSize];
        return similarWidthChars.Values.FirstOrDefault(chars => chars.Contains(character));
    }

    /// <summary>
    ///   Group all characters of a <see cref="PixelFontSize"/> by width and put them in <see cref="CharWidthMap"/>.
    /// </summary>
    private static void PrepareCharWidthMap(PixelFontSize pixelFontSize)
    {
        if (CharWidthMap.ContainsKey(pixelFontSize))
            return;

        Dictionary<int, List<int>> widthToCharsMap = [];

        foreach ((int charIndex, PixelFontCharacter @char) in pixelFontSize.Characters)
        {
            int width = @char.Texture.Width;

            if (!widthToCharsMap.TryGetValue(width, out List<int> chars))
                widthToCharsMap[width] = chars = [];

            chars.Add(charIndex);
        }

        CharWidthMap[pixelFontSize] = widthToCharsMap;
    }

    public interface ISpecialChar
    {
        public void BeforeDraw(PixelFont font, float baseSize, Vector2 scale);
        public void AfterDraw(PixelFont font, float baseSize, Vector2 scale);
    }

    public class CaseShiftChar : FancyText.Char, ISpecialChar
    {
        private static readonly Random Rng = new();

        /// <summary>
        ///   The true character from the parsed text.
        /// </summary>
        public int ActualCharacter;

        internal static CaseShiftChar FromChar(FancyText.Char character)
        => new()
        {
            Index = character.Index,
            Character = character.Character,
            ActualCharacter = character.Character,
            Position = character.Position,
            Line = character.Line,
            Page = character.Page,
            Delay = character.Delay,
            LineWidth = character.LineWidth,
            Color = character.Color,
            Scale = character.Scale,
            Rotation = character.Rotation,
            YOffset = character.YOffset,
            Fade = character.Fade,
            Shake = character.Shake,
            Wave = character.Wave,
            Impact = character.Impact,
            IsPunctuation = character.IsPunctuation,
        };

        public void BeforeDraw(PixelFont font, float baseSize, Vector2 scale)
        {
            if(Engine.Scene.OnRawInterval(0.05f))Character = Rng.Chance(0.5f) ? char.ToLower((char)ActualCharacter) : char.ToUpper((char)ActualCharacter);
            

            Vector2 vector = scale * ((Impact ? 2f - Fade : 1f) * Scale);
            PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));

            float difference = pixelFontSize.Measure((char)Character).X - pixelFontSize.Measure((char)ActualCharacter).X;

            Position -= difference / 2f;
        }
        public void AfterDraw(PixelFont font, float baseSize, Vector2 scale)
        {
            Vector2 vector = scale * ((Impact ? 2f - Fade : 1f) * Scale);
            PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));

            float difference = pixelFontSize.Measure((char)Character).X - pixelFontSize.Measure((char)ActualCharacter).X;

            Position += difference / 2f;
        }
    }

    public class ObfuscatedChar : FancyText.Char, ISpecialChar
    {
        private static readonly Random Rng = new();


        /// <summary>
        ///   The true character from the parsed text.
        /// </summary>
        public int ActualCharacter;

        /// <summary>
        ///   The chance [0f..1f] that the original character will not be shown when printing the <see cref="ObfuscatedChar"/>.
        /// </summary>
        public float NoOriginalCharacterChance;

        /// <summary>
        ///   Whether the <see cref="ObfuscatedChar"/> can be drawn as any character, rather than only characters with similar width.
        /// </summary>
        /// <remarks>
        ///   Will cause character overlap.
        /// </remarks>
        public bool FullObfuscation;

        /// <summary>
        ///   Clones the passed <see cref="FancyText.Char"/> and turns it into an <see cref="ObfuscatedChar"/>.
        /// </summary>
        internal static ObfuscatedChar FromChar(FancyText.Char character, float chance, bool full)
            => new()
            {
                Index = character.Index,
                Character = character.Character,
                ActualCharacter = character.Character,
                Position = character.Position,
                Line = character.Line,
                Page = character.Page,
                Delay = character.Delay,
                LineWidth = character.LineWidth,
                Color = character.Color,
                Scale = character.Scale,
                Rotation = character.Rotation,
                YOffset = character.YOffset,
                Fade = character.Fade,
                Shake = character.Shake,
                Wave = character.Wave,
                Impact = character.Impact,
                IsPunctuation = character.IsPunctuation,
                NoOriginalCharacterChance = chance,
                FullObfuscation = full,
            };

        /// <summary>
        ///   Pick a random character before getting rendered.
        /// </summary>
        public void BeforeDraw(PixelFont font, float baseSize, Vector2 scale)
        {
            // copy-pasted from orig_Draw
            Vector2 vector = scale * ((Impact ? 2f - Fade : 1f) * Scale);
            PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));

            PrepareCharWidthMap(pixelFontSize);

            List<int> choices = GetSimilarWidthChars(pixelFontSize, ActualCharacter);

            if (FullObfuscation)
            {
                //i love linq (keep emoji out)
                choices = [.. pixelFontSize.Characters.Keys.Where(c => c < '\ue000')];
            }

            if (Rng.Chance(1 - NoOriginalCharacterChance))
            {
                choices.Remove(ActualCharacter);
                Character = Rng.Choose(choices);
                choices.Add(ActualCharacter);
            }
            else
            {
                Character = Rng.Choose(choices);
            }

            float difference = pixelFontSize.Measure((char)Character).X - pixelFontSize.Measure((char)ActualCharacter).X;

            Position -= difference / 2f;
        }

        public void AfterDraw(PixelFont font, float baseSize, Vector2 scale)
        {
            Vector2 vector = scale * ((Impact ? 2f - Fade : 1f) * Scale);
            PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));

            float difference = pixelFontSize.Measure((char)Character).X - pixelFontSize.Measure((char)ActualCharacter).X;

            Position += difference / 2f;
        }
    }
}
