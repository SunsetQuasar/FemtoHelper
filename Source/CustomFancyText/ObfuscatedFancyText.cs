using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            InjectObfuscatedCharHook);
    }

    internal static void Unload()
    {
        On.Celeste.FancyText.Char.Draw -= AddBeforeDrawCallHook;
        IL.Celeste.FancyText.Parse -= ParseObfuscatedCommandHook;
        Hook_FancyText_AddWord?.Dispose();
    }

    /// <summary>
    ///   Enable swapping the characters in obfuscated text before they get rendered.
    /// </summary>
    private static void AddBeforeDrawCallHook(
        On.Celeste.FancyText.Char.orig_Draw orig,
        FancyText.Char self,
        PixelFont font,
        float baseSize,
        Vector2 position,
        Vector2 scale,
        float alpha)
    {
        if (self is ObfuscatedChar corruptedChar)
            corruptedChar.BeforeDraw(font, baseSize, scale);
        orig(self, font, baseSize, position, scale, alpha);
    }

    /// <summary>
    ///   Inject the text obfuscation command check.
    /// </summary>
    private static void ParseObfuscatedCommandHook(ILContext il)
    {
        ILCursor cursor = new(il);

        ILLabel @continue = null;

        //Logger.Debug(LogId, "Hi! I'm just an innocent little IL hook! Please don't kill me!");
        

        // pain
        if (!cursor.TryGotoNextBestFit(
            MoveType.AfterLabel,
            static instr => instr.MatchLdloc(7),
            static instr => instr.MatchLdstr("savedata"),
            static instr => instr.MatchCallvirt<string>("Equals"),
            instr => instr.MatchBrfalse(out @continue)))
        {
            Logger.Error(LogId, "Could not insert obfuscated text command check!");
            return;
        }

        cursor.EmitLdarg0();
        cursor.EmitLdloc(7);
        cursor.EmitDelegate(TryMatchObfuscatedCommand);
        cursor.EmitBrtrue(@continue);
        //Logger.Debug(LogId, il.ToString());
    }

    /// <summary>
    ///   Attempt to match the text obfuscation command.
    /// </summary>
    /// <returns>
    ///   Whether the start or end marker for the obfuscated command has been matched.
    /// </returns>
    private static bool TryMatchObfuscatedCommand(FancyText self, string text)
    {
        DynamicData selfData = DynamicData.For(self);
        switch (text)
        {
            case ObfuscatedCommand:
                selfData.Set(ObfuscatedDynDataKey, true);
                return true;
            case "/" + ObfuscatedCommand:
                selfData.Set(ObfuscatedDynDataKey, false);
                return true;
        }
        return false;
    }

    /// <summary>
    ///   Inject <see cref="SwapForObfuscatedChar"/>.
    /// </summary>
    private static void InjectObfuscatedCharHook(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNext(static instr => instr.MatchCallvirt(typeof(List<FancyText.Node>).GetMethod("Add")!)))
        {
            Logger.Error(LogId, "Could not insert obfuscated char injection!");
            return;
        }
        cursor.EmitLdarg0();
        cursor.EmitDelegate(SwapForObfuscatedChar);
    }

    /// <summary>
    ///   Replace <see cref="FancyText.Char"/>s for <see cref="ObfuscatedChar"/>s if text obfuscation is enabled.
    /// </summary>
    private static FancyText.Char SwapForObfuscatedChar(FancyText.Char node, FancyText self)
    {
        DynamicData selfData = DynamicData.For(self);
        return selfData.Get(ObfuscatedDynDataKey) is true
            ? ObfuscatedChar.FromChar(node)
            : node;
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

    public class ObfuscatedChar : FancyText.Char
    {
        private static readonly Random Rng = new();

        /// <summary>
        ///   The true character from the parsed text.
        /// </summary>
        public int ActualCharacter;

        /// <summary>
        ///   Clones the passed <see cref="FancyText.Char"/> and turns it into an <see cref="ObfuscatedChar"/>.
        /// </summary>
        internal static ObfuscatedChar FromChar(FancyText.Char character)
            => new() {
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

        /// <summary>
        ///   Pick a random character before getting rendered.
        /// </summary>
        internal void BeforeDraw(PixelFont font, float baseSize, Vector2 scale)
        {
            // copy-pasted from orig_Draw
            Vector2 vector = scale * ((Impact ? 2f - Fade : 1f) * Scale);
            PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));

            PrepareCharWidthMap(pixelFontSize);

            Character = Rng.Choose(GetSimilarWidthChars(pixelFontSize, ActualCharacter));
        }
    }
}
