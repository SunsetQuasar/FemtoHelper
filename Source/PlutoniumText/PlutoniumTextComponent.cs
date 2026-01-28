using Celeste.Mod.FemtoHelper.Entities;
using Celeste.Mod.FemtoHelper.PlutoniumText;
using Celeste.Mod.FemtoHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Xml;
using static Celeste.Mod.FemtoHelper.PlutoniumFont;

namespace Celeste.Mod.FemtoHelper;
public static class PlutoniumTextNodes
{

    public class Node
    {

    }
    public class Text(string str) : Node
    {
        public readonly string StringText = str;
    }
    public class Counter(string k) : Node
    {
        public readonly string Key = k;
    }

    public class Slider(string k, bool t, int d) : Node
    {
        public readonly string Key = k;
        public readonly bool Truncate = t;
        public readonly int Decimals = d;
    }
    public class Flag(string k, string on, string off) : Node
    {
        public readonly string Key = k;
        public readonly string StrIfOn = on;
        public readonly string StrIfOff = off;
    }

    public class ExpressionAsFlag(string k, string on, string off) : Node
    {
        public string Exp = k;
        public string StrIfOn = on;
        public string StrIfOff = off;
    }
    public class ExpressionAsCounter(string k) : Node
    {
        public string Exp = k;
    }

    private static readonly string[] BigNumberNames = ["", "", "million", "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion", "octillion", "nonillion", "decillion", "undecillion"];

    private static string LimitedDecimals(float f, int decimals)
    {
        if (decimals < 0) return f.ToString();

        string format = "0.";

        for (int i = 0; i < decimals; i++)
        {
            format += "0";
        }

        return f.ToString(format);
    }

    private static string GetShorthandNumber(float f)
    {
        if (f < 1000000) return f.ToString();
        int orderOfMagnitudeTriplets = (int)(MathF.Log10(f) / 3);
        double num = Math.Round(f / Math.Pow(10, 3 * orderOfMagnitudeTriplets), 3);
        string str = num.ToString();
        if (orderOfMagnitudeTriplets < BigNumberNames.Length - 1) return str + " " + BigNumberNames[orderOfMagnitudeTriplets];
        else return f.ToString();
    }

    public static List<Node> Parse(string dialogId, bool truncateSliders, int decimals)
    {
        List<Node> nodes = [];

        string[] splitStr = SimpleText.MyRegex().Split(Dialog.Get(dialogId));
        string[] splitStr2 = new string[splitStr.Length];
        int num = 0;
        foreach (var t in splitStr)
        {
            if (!string.IsNullOrEmpty(t))
            {
                splitStr2[num++] = t;
            }
        }

        for (int i = 0; i < splitStr2.Length; i++)
        {
            if (splitStr2[i] == "{")
            {
                i++;

                for (; i < splitStr2.Length && splitStr2[i] != "}"; i++)
                {
                    if (string.IsNullOrWhiteSpace(splitStr2[i])) continue;

                    string[] splitOnceAgain = splitStr2[i].Split(';');
                    if (splitOnceAgain.Length == 3)
                    {
                        nodes.Add(new Flag(splitOnceAgain[0], splitOnceAgain[1], splitOnceAgain[2]));
                    }
                    else if (splitOnceAgain.Length == 4 && splitOnceAgain[0] == "exp")
                    {
                        nodes.Add(new ExpressionAsFlag(splitOnceAgain[1], splitOnceAgain[2], splitOnceAgain[3]));
                    }
                    else if (splitOnceAgain.Length == 2 && splitOnceAgain[0] == "exp")
                    {
                        nodes.Add(new ExpressionAsCounter(splitOnceAgain[1]));
                    }
                    else
                    {
                        if (splitStr2[i][0] == '@')
                        {
                            nodes.Add(new Slider(splitStr2[i].Remove(0, 1), truncateSliders, decimals));
                        }
                        else
                        {
                            nodes.Add(new Counter(splitStr2[i]));
                        }
                    }
                }
            }
            else
            {
                nodes.Add(new Text(splitStr2[i]));
            }
        }

        return nodes;
    }

    public static string ConstructString(List<Node> nodes, Level level)
    {
        var result = "";
        foreach (var n in nodes)
        {
            switch (n)
            {
                case Text t:
                    result += t.StringText;
                    break;
                case Counter c:
                    result += level.Session.GetCounter(c.Key).ToString();
                    break;
                case Slider s:
                    result += s.Truncate ? GetShorthandNumber(level.Session.GetSlider(s.Key)) : LimitedDecimals(level.Session.GetSlider(s.Key), s.Decimals);
                    break;
                case Flag f:
                    result += level.Session.GetFlag(f.Key) ? f.StrIfOn : f.StrIfOff;
                    break;
                case ExpressionAsFlag ef:
                    result += EvaluateExpressionAsBool(ef.Exp, level.Session) ? ef.StrIfOn : ef.StrIfOff;
                    break;
                case ExpressionAsCounter ec:
                    result += EvaluateExpressionAsInt(ec.Exp, level.Session).ToString();
                    break;
            }
        }
        return result;
    }
}
public class TextEffectData
{
    public readonly bool Wavey;
    public Vector2 WaveAmp;
    public readonly float PhaseOffset;
    public readonly float PhaseIncrement;
    public readonly float WaveSpeed;

    public readonly bool Shakey;
    public readonly float ShakeAmount;

    public readonly bool Obfuscated;

    public readonly bool Twitchy;
    public readonly float TwitchChance;

    public readonly bool Rainbow;
    public Vector2 RainbowAnchor;

    public bool Empty => !(Twitchy || Shakey || Obfuscated || Wavey || Rainbow);
    public TextEffectData(bool wavey = false, Vector2 amp = default, float offset = 0, bool shakey = false, float amount = 0, bool obfs = false, bool twitchy = false, float twitchChance = 0, float phaseIncrement = 0, float waveSpeed = 0, bool rainbow = false, Vector2 rainbowAnchor = default)
    {
        Wavey = wavey;
        if (wavey)
        {
            WaveAmp = amp;
            PhaseOffset = offset;
            PhaseIncrement = phaseIncrement;
            WaveSpeed = waveSpeed;
        }

        Shakey = shakey;
        ShakeAmount = amount;

        Obfuscated = obfs;
        Twitchy = twitchy;
        TwitchChance = twitchChance;
        Rainbow = rainbow;
        RainbowAnchor = rainbowAnchor;
    }

    public TextEffectData()
    {
    }
}

public struct PlutoniumFont
{
    public static Dictionary<string, PlutoniumFont> FontCache { get; private set; } = [];

    public struct Character
    {
        public static BlendState AlphaSubtract = new()
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.ReverseSubtract
        };

        public readonly Rectangle SourceRect;
        public Point PreDrawOffset;
        public Point PostDrawOffset;
        public MTexture Sprite;
        public VirtualRenderTarget Outline;
        public VirtualRenderTarget Shadow;
        public Dictionary<char, Point> KerningData = [];

        public Character(PlutoniumFont parent, char character, MTexture source, Rectangle rect, Point preOffset, Point postOffset)
        {
            Sprite = source.GetSubtexture(rect);
            SourceRect = rect;
            PreDrawOffset = preOffset;
            PostDrawOffset = postOffset;

            Outline = VirtualContent.CreateRenderTarget(parent.SourcePath + "_" + character + "_outline", SourceRect.Width + 2, SourceRect.Height + 2);
            Shadow = VirtualContent.CreateRenderTarget(parent.SourcePath + "_" + character + "_shadow", SourceRect.Width + 2, SourceRect.Height + 2);

            GraphicsDevice g = Engine.Graphics.GraphicsDevice;

            //do outline first

            g.SetRenderTarget(Outline);
            g.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            PlutoniumTextComponent.DrawOutlineExceptGood(Sprite, Vector2.One, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, AlphaSubtract, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            Sprite.Draw(Vector2.One);
            Draw.SpriteBatch.End();

            //then do shadow

            g.SetRenderTarget(Shadow);
            g.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            Sprite.Draw(Vector2.One + Vector2.One);
            Draw.SpriteBatch.End();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, AlphaSubtract, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            Sprite.Draw(Vector2.One);
            Draw.SpriteBatch.End();
        }

        public Point GetKerning(char afterThisChar)
        {
            if (KerningData.TryGetValue(afterThisChar, out var result))
            {
                return result;
            }
            else return Point.Zero;
        }
    }

    public Dictionary<char, Character> Chars = [];

    public string CharList = "";

    public MTexture SourceTexture;

    public string SourcePath = "";

    public int LargestCharWidth = 0;

    public PlutoniumFont(string path, string legacyCharList = "", Vector2 legacySize = default)
    {
        if (!string.IsNullOrWhiteSpace(legacyCharList))
        {
            SourcePath = "";

            SourceTexture = GFX.Game[path];

            CharList = legacyCharList;

            for(int i = 0; i < CharList.Length; i++)
            {
                Chars.Add(CharList[i], new Character(this, CharList[i], SourceTexture, new Rectangle((int)legacySize.X * i, 0, (int)legacySize.X, (int)legacySize.Y), Point.Zero, Point.Zero));
            }

            FontCache.Add(path, this);

            return;
        }

        XmlDocument doc = Calc.LoadContentXML(path);
        if (!doc.HasChildNodes)
        {
            Log($"Missing font XML: {path}", LogLevel.Error);
            return;
        }
        XmlNodeList list = doc.GetElementsByTagName("Font");
        XmlElement font = list.Count == 0 ? null : (XmlElement)list[0];

        if (font is null)
        {
            throw new Exception($"'{path}': file is missing a 'Font' element!");
        }

        SourcePath = path;

        if (font.HasAttr("source"))
        {
            SourceTexture = GFX.Game[font.Attr("source")];
        }
        else
        {
            throw new Exception($"'{path}': 'Font' element does not contain a 'source' attribute!");
        }

        foreach (XmlElement character in font.GetElementsByTagName("Character"))
        {
            char id = default;
            Point pre, post;
            Rectangle source;
            if (
                character.HasAttr("id") &&
                character.HasAttr("x") &&
                character.HasAttr("y") &&
                character.HasAttr("width") &&
                character.HasAttr("height") &&
                character.HasAttr("preOffsetX") &&
                character.HasAttr("preOffsetY") &&
                character.HasAttr("postOffsetX") &&
                character.HasAttr("postOffsetY")
                )
            {
                id = character.AttrChar("id");
                source = character.Rect();
                pre = new(character.AttrInt("preOffsetX"), character.AttrInt("preOffsetY"));
                post = new(character.AttrInt("postOffsetX"), character.AttrInt("postOffsetY"));
            }
            else
            {
                throw new Exception($"'{path}': 'Character' element is missing one or more attributes!");
            }

            Character chr;

            Chars.Add(id, chr = new Character(this, id, SourceTexture, source, pre, post));

            foreach(XmlElement kerning in character.GetElementsByTagName("CharSpecificOffset"))
            {
                if(kerning.HasAttr("after") && kerning.HasAttr("x") && kerning.HasAttr("y"))
                {
                    string set = kerning.Attr("after");
                    Point offset = kerning.Position().ToPoint();
                    foreach (char ch in set)
                    {
                        chr.KerningData.Add(ch, offset);
                    }
                } 
                else
                {
                    throw new Exception($"'{path}': Malformed 'CharSpecificOffset' element!");
                }
            }

            if(chr.Outline.Width > LargestCharWidth)
            {
                LargestCharWidth = chr.Outline.Width;
            }

            CharList += id;
        }

        FontCache.Add(path, this);
    }

    public static PlutoniumFont GetFont(string path, string legacyCharList = "", Vector2 legacyFontSize = default)
    {
        if (FontCache.TryGetValue(path, out PlutoniumFont font))
        {
            return font;
        }
        return new PlutoniumFont(path, legacyCharList, legacyFontSize);
    }

    public readonly Character? GetCharacter(char c)
    {
        if (Chars.TryGetValue(c, out Character character))
        {
            return character;
        }
        return null;
    }

    public readonly Vector2 StringSize(string str, int extraSpacing)
    {
        Vector2 size = Vector2.Zero;
        float tallestChar = 0f;
        char before = ' ';
        bool firstchar = true;
        foreach(char c in str)
        {
            Character? ch = GetCharacter(c);
            if (ch is { } ch2)
            {
                if (!firstchar)
                {
                    size += ch2.GetKerning(before).ToVector2();
                }
                else firstchar = false;
                size += new Vector2(ch2.Sprite.Width, 0) + ch2.PreDrawOffset.ToVector2() + ch2.PostDrawOffset.ToVector2() + (Vector2.UnitX * extraSpacing);

                if(ch2.Sprite.Height > tallestChar)
                {
                    tallestChar = ch2.Sprite.Height;
                }

                before = c;
            }
        }
        return size + (Vector2.UnitY * tallestChar);
    }
}

public class PlutoniumTextComponent : Component
{
    //public readonly List<MTexture> CharTextures;
    public long Seed;
    public string FontPath;
    public readonly Action<Level> BeforeRenderCallback, RenderCallback;
    public readonly TextLayer Layer;
    public PlutoniumFont Font;
    public TextEffectData EffectData;
    public float Timer;
    public PlutoniumTextComponent(string fontPath, TextLayer layer = TextLayer.Gameplay, Action<Level> beforeRender = null, Action<Level> render = null, TextEffectData data = default, string legacyCharList = "", Vector2 legacyFontSize = default) : base(true, true)
    {
        Layer = layer;
        BeforeRenderCallback = beforeRender;
        RenderCallback = render;

        Font = GetFont(fontPath, legacyCharList, legacyFontSize);

        FontPath = fontPath;

        EffectData = data;
    }

    public override void EntityAwake()
    {
        PlutoniumTextRenderer.Track(this);
    }
    public override void EntityRemoved(Scene scene)
    {
        PlutoniumTextRenderer.Untrack(this);
    }
    public override void SceneEnd(Scene scene)
    {
        PlutoniumTextRenderer.Untrack(this);
    }
    public override void Removed(Entity entity)
    {
        PlutoniumTextRenderer.Untrack(this);
    }

    public override void Update()
    {
        base.Update();
        if (Scene.OnInterval(0.04f))
        {
            Seed = Calc.Random.NextFloat().GetHashCode();
        }
        Timer += Engine.DeltaTime;
    }
    public void PrintCentered(Vector2 pos, string str, bool shadow, int extraSpacing, Color mainColor, Color outlineColor, float scale = 1, int id = 0, bool flipped = false)
    {
        Print(pos, str, shadow, extraSpacing, mainColor, outlineColor, new Vector2(0.5f, 0.5f), scale, id, flipped);
    }

    public void Print(Vector2 pos, string str, bool shadow, int extraSpacing, Color mainColor, Color outlineColor, Vector2 justify = default, float scale = 1, int id = 0, bool flipped = false)
    {
        RainbowHelper.GenerateSpinner(Entity.Scene);

        Vector2 stringSize = Font.StringSize(str, extraSpacing) * scale;

        pos.X -= stringSize.X * justify.X;

        int factor = 1;
        SpriteEffects seffect = SpriteEffects.None;
        if(flipped)
        {
            seffect = SpriteEffects.FlipHorizontally;
            factor = -1;
        }

        List<Vector2> effectOffsets = [];
        List<char> useChar = [];

        pos = pos.Round();

        if (!EffectData.Empty)
        {
            for (float i = 0; i < str.Length; i++)
            {
                effectOffsets.Add(Vector2.Zero);
                useChar.Add(' ');

                float i2 = i + id;

                Calc.PushRandom((int)(Seed + (i2 - 5).GetHashCode()));
                if (EffectData.Shakey || (EffectData.Twitchy && Calc.Random.Chance(EffectData.TwitchChance)))
                {
                    Vector2 num = new Vector2(Calc.Random.NextFloat(2f * EffectData.ShakeAmount) - EffectData.ShakeAmount, Calc.Random.NextFloat(2f * EffectData.ShakeAmount) - EffectData.ShakeAmount) * scale;
                    effectOffsets[(int)i] += num;
                }
                Calc.PopRandom();
                if (EffectData.Wavey)
                {
                    Vector2 num = new Vector2
                        (
                        (float)Math.Sin(Timer * EffectData.WaveSpeed + EffectData.PhaseOffset + i2 * EffectData.PhaseIncrement) * EffectData.WaveAmp.X,
                        (float)Math.Sin(Timer * EffectData.WaveSpeed + i2 * EffectData.PhaseIncrement) * EffectData.WaveAmp.Y
                        ) * scale;
                    effectOffsets[(int)i] += num;
                }

                if (!EffectData.Obfuscated) continue;

                Calc.PushRandom((int)(Seed + i2.GetHashCode()));
                useChar[(int)i] = Font.CharList[Calc.Random.Next(Font.CharList.Length)];
                Calc.PopRandom();
            }
        }

        if (outlineColor != Color.Transparent)
        {
            Vector2 offset = flipped ? (Vector2.UnitX * Font.StringSize(str, extraSpacing) * scale) : Vector2.Zero;
            char before = ' ';
            bool firstChar = true;
            for (int i = 0; i < str.Length; i++)
            {
                char origc;
                char c = origc = str[i];

                if (!EffectData.Empty && i < useChar.Count && useChar[i] != ' ' && c != ' ')
                {
                    c = useChar[i];
                }

                Character? origCharNullable = Font.GetCharacter(origc);
                Character? charNullable = Font.GetCharacter(c);
                if ((charNullable is { } @char) && (origCharNullable is { } origChar))
                {
                    if (firstChar)
                    {
                        firstChar = false;
                    }
                    else
                    {
                        offset += origChar.GetKerning(before).ToVector2() * scale * factor;
                    }

                    offset += origChar.PreDrawOffset.ToVector2() * scale * factor;

                    Vector2 charpos = pos + offset;

                    if (flipped) charpos -= Vector2.UnitX * origChar.Sprite.Width;

                    if (!EffectData.Empty && i < effectOffsets.Count) charpos += effectOffsets[i];

                    Color color = outlineColor;

                    if (EffectData.Rainbow)
                    {
                        color = outlineColor.Multiply(RainbowHelper.GetRainbowColorAt(Scene, EffectData.RainbowAnchor + (offset / (scale == 0 ? float.Epsilon : scale))));
                    }

                    if (shadow)
                    {
                        Draw.SpriteBatch.Draw(@char.Shadow, new Rectangle((int)MathF.Floor(charpos.X - scale), (int)(MathF.Floor(charpos.Y - scale) - (justify.Y * @char.Shadow.Height * scale)), (int)(@char.Shadow.Width * scale), (int)(@char.Shadow.Height * scale)), null, color, 0, Vector2.Zero, seffect, 0f);
                    }
                    else
                    {
                        Draw.SpriteBatch.Draw(@char.Outline, new Rectangle((int)MathF.Floor(charpos.X - scale), (int)(MathF.Floor(charpos.Y - scale) - (justify.Y * @char.Shadow.Height * scale)), (int)(@char.Shadow.Width * scale), (int)(@char.Shadow.Height * scale)), null, color, 0, Vector2.Zero, seffect, 0f);
                    }

                    offset += ((Vector2.UnitX * (origChar.Sprite.Width + extraSpacing)) + origChar.PostDrawOffset.ToVector2()) * scale * factor;
                }

                before = origc;
            }
        }

        if (mainColor != Color.Transparent)
        {
            Vector2 offset = flipped ? (Vector2.UnitX * Font.StringSize(str, extraSpacing) * scale) : Vector2.Zero;
            char before = ' ';
            bool firstChar = true;
            for (int i = 0; i < str.Length; i++)
            {
                char origc;
                char c = origc = str[i];

                if (!EffectData.Empty && i < useChar.Count && useChar[i] != ' ' && c != ' ')
                {
                    c = useChar[i];
                }

                Character? charNullable = Font.GetCharacter(c);
                Character? origCharNullable = Font.GetCharacter(origc);
                if (charNullable is { } @char && origCharNullable is { } origChar)
                {
                    if (firstChar)
                    {
                        firstChar = false;
                    }
                    else
                    {
                        offset += origChar.GetKerning(before).ToVector2() * scale * factor;
                    }

                    offset += origChar.PreDrawOffset.ToVector2() * scale * factor;

                    Vector2 charpos = pos + offset;

                    Color color = mainColor;

                    if (EffectData.Rainbow)
                    {
                        color = mainColor.Multiply(RainbowHelper.GetRainbowColorAt(Scene, EffectData.RainbowAnchor + (offset / (scale == 0 ? float.Epsilon : scale))));
                    }

                    if (flipped) charpos -= Vector2.UnitX * origChar.Sprite.Width;

                    if (!EffectData.Empty && i < effectOffsets.Count) charpos += effectOffsets[i];

                    @char.Sprite.Draw((charpos - (justify.Y * @char.Shadow.Height * Vector2.UnitY * scale)).Floor(), Vector2.Zero, color, scale, 0f, seffect);
                    //Draw.Rect((charpos - (justify.Y * @char.Shadow.Height * Vector2.UnitY * scale)).Floor(), 1, 1, Color.Red);

                    offset += ((Vector2.UnitX * (origChar.Sprite.Width + extraSpacing)) + origChar.PostDrawOffset.ToVector2()) * scale * factor;
                }

                before = origc;
            }
        }
    }

    public Rectangle GetVisibilityRectangle(Vector2 pos, string str, int spacing = 0, float scale = 1f, Vector2 justify = default)
    {
        Vector2 size = Font.StringSize(str, spacing) * scale;

        //start off as just the size of the text entity
        Rectangle visRect = new((int)pos.X, (int)pos.Y, (int)MathF.Ceiling(size.X), (int)MathF.Ceiling(size.Y));

        //apply justification
        visRect.X -= (int)MathF.Ceiling(visRect.Width * justify.X);
        visRect.Y -= (int)MathF.Ceiling(visRect.Height * justify.Y);

        //add 1 pixel to each side to account for the outlines
        visRect.Inflate((int)MathF.Ceiling(scale), (int)MathF.Ceiling(scale));

        //now onto the effects:
        //sine wave
        visRect.Inflate((int)MathF.Ceiling(MathF.Abs(EffectData.WaveAmp.X) * scale), (int)MathF.Ceiling(MathF.Abs(EffectData.WaveAmp.Y) * scale));

        //shaking
        visRect.Inflate((int)MathF.Ceiling(MathF.Abs(EffectData.ShakeAmount) * scale), (int)MathF.Ceiling(MathF.Abs(EffectData.ShakeAmount) * scale));

        //in the rare case the last character is very thin and gets replaced by a wider character, expand it by the worst case scenario. better safe than sorry (better underperforming than ugly?)
        if (EffectData.Obfuscated)
        {
            visRect.Width += Font.LargestCharWidth;
        }

        return visRect;
    }

    public static void LoadContent()
    {
        /*
        if (Everest.Content.TryGet("Effects/FemtoHelper/Cutout.cso", out ModAsset value))
        {
            Cutout = new Effect(Engine.Graphics.GraphicsDevice, value.Data);
        }
        else
        {
            Log("Could not find Effect \"Effects/FemtoHelper/Cutout.cso\"!", LogLevel.Error);
        }
        */
    }

    public static void Load()
    {
        //On.Celeste.DustEdges.BeforeRender += DustEdges_BeforeRender;
        Everest.Content.OnUpdate += Content_OnUpdate;
    }

    private static void Content_OnUpdate(ModAsset from, ModAsset to)
    {

        if (to is not null)
        {
            if (to.Format == "xml")
            {
                if (PlutoniumFont.FontCache.TryGetValue(to.PathVirtual + "." + to.Format, out PlutoniumFont found))
                {
                    foreach (var c in found.Chars)
                    {
                        c.Value.Outline.Dispose();
                        c.Value.Shadow.Dispose();
                    }
                    FontCache.Remove(to.PathVirtual + "." + to.Format);
                }
            }
        }
    }

    //private static void DustEdges_BeforeRender(On.Celeste.DustEdges.orig_BeforeRender orig, DustEdges self)
    //{
    //    Texture temp1 = Engine.Graphics.GraphicsDevice.Textures[1];
    //    Texture temp2 = Engine.Graphics.GraphicsDevice.Textures[2];
    //    orig(self);
    //    Engine.Graphics.GraphicsDevice.Textures[1] = temp1;
    //    Engine.Graphics.GraphicsDevice.Textures[2] = temp2;
    //}
    public static void Unload()
    {
        //On.Celeste.DustEdges.BeforeRender -= DustEdges_BeforeRender;
        Everest.Content.OnUpdate -= Content_OnUpdate;
    }

    public static void DrawOutlineExceptGood(MTexture t, Vector2 position, Vector2 origin = default, Color color = default, SpriteEffects flip = default, float scale = 1f)
    {
        float scaleFix = t.ScaleFix * scale;
        Rectangle clipRect = t.ClipRect;
        Vector2 origin2 = (origin - t.DrawOffset) / scaleFix;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i != 0 || j != 0)
                {
                    Draw.SpriteBatch.Draw(t.Texture.Texture_Safe, position + new Vector2(i, j) * scale, clipRect, color, 0f, origin2, scaleFix, flip, 0f);
                }
            }
        }

        //Draw.SpriteBatch.Draw(t.Texture.Texture_Safe, position, clipRect, Color.White, 0f, origin2, scaleFix, SpriteEffects.None, 0f);
    }
}
