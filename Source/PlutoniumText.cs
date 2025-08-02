﻿using Celeste.Mod.FemtoHelper.Entities;
using Celeste.Mod.FemtoHelper.Utils;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

    public class Slider(string k, bool t) : Node 
    {
        public readonly string Key = k;
        public readonly bool Truncate = t;
    }
    public class Flag(string k, string on, string off) : Node
    {
        public readonly string Key = k;
        public readonly string StrIfOn = on;
        public readonly string StrIfOff = off;
    }

    public class ExpressionAsFlag(string k, string on, string off) : Node
    {
        public string exp = k;
        public string strIfOn = on;
        public string strIfOff = off;
    }
    public class ExpressionAsCounter(string k) : Node
    {
        public string exp = k;
    }

    private static readonly string[] bigNumberNames = ["", "", "million", "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion", "octillion", "nonillion", "decillion", "undecillion"];

    private static string getShorthandNumber(float f) {
        if (f < 1000000) return f.ToString();
        int orderOfMagnitudeTriplets = (int) (MathF.Log10(f) / 3);
        double num = Math.Round(f / Math.Pow(10, 3 * orderOfMagnitudeTriplets), 3);
        string str = num.ToString();
        if (orderOfMagnitudeTriplets < bigNumberNames.Length - 1) return str + " " + bigNumberNames[orderOfMagnitudeTriplets];
        else return f.ToString();
    }

    public static List<Node> Parse(string dialogID, bool truncateSliders)
    {
        List<Node> Nodes = [];

        string[] splitStr = SimpleText.MyRegex().Split(Dialog.Get(dialogID));
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
                        Nodes.Add(new Flag(splitOnceAgain[0], splitOnceAgain[1], splitOnceAgain[2]));
                    }
                    else
                    {
                        if (splitStr2[i][0] == '@')
                        {
                            Nodes.Add(new Slider(splitStr2[i].Remove(0, 1), truncateSliders));
                        }
                        else
                        {
                            Nodes.Add(new Counter(splitStr2[i]));
                        }
                    }
                }
            }
            else
            {
                Nodes.Add(new Text(splitStr2[i]));
            }
        }

        return Nodes;
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
                    result += s.Truncate ? getShorthandNumber(level.Session.GetSlider(s.Key)) : level.Session.GetSlider(s.Key);;  
                    break;
                case Flag f:
                    result += level.Session.GetFlag(f.Key) ? f.StrIfOn : f.StrIfOff;
                    break;
                case ExpressionAsFlag ef:
                    result += Util.EvaluateExpressionAsBool(ef.exp, level.Session) ? ef.strIfOn : ef.strIfOff;
                    break;
                case ExpressionAsCounter ec:
                    result += Util.EvaluateExpressionAsInt(ec.exp, level.Session).ToString();
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

    public readonly bool Empty;
    public TextEffectData(bool wavey, Vector2 amp, float offset, bool shakey, float amount, bool obfs, bool twitchy, float twitchChance, float phaseIncrement, float waveSpeed)
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
    }

    public TextEffectData()
    {
        Empty = true;
    }
}

public class PlutoniumText : Component
{
    public readonly Dictionary<char, int> Charset;
    public readonly List<MTexture> CharTextures;
    public Vector2 FontSize;
    public float Seed;
    public readonly string CharList;
    public PlutoniumText(string fontPath, string charList, Vector2 fontSize) : base(true, true)
    {
        FontSize = fontSize;
        Charset = new Dictionary<char, int>();
        CharTextures = new List<MTexture>();
        string characters = CharList = charList;
        // " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'".,ç"
        // " !"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~"

        for (int i = 0; i < characters.Length; i++)
        {
            Charset.Add(characters[i], i);
            CharTextures.Add(GFX.Game[fontPath].GetSubtexture(i * (int)fontSize.X, 0, (int)fontSize.X, (int)fontSize.Y)); // "PlutoniumHelper/PlutoniumText/font"
        }
    }

    public override void Update()
    {
        base.Update();
        if (Scene.OnInterval(0.04f))
        {
            Seed = Calc.Random.NextFloat() * 12801;
        }
    }
    public void PrintCentered(Vector2 pos, string str, bool shadow, int spacing, Color mainColor, Color outlineColor, float scale = 1, int id = 0)
    {
        float stringlen = spacing * str.Length * scale;
        Print(pos - new Vector2((float)Math.Floor(stringlen / 2f), (float)Math.Floor(FontSize.Y / 2f)), str, shadow, spacing, mainColor, outlineColor, new TextEffectData(), scale, id);
    }

    public void PrintCentered(Vector2 pos, string str, bool shadow, int spacing, Color mainColor, Color outlineColor, TextEffectData data, float scale = 1, int id = 0)
    {
        float stringlen = spacing * str.Length * scale;
        Print(pos - new Vector2((float)Math.Floor(stringlen / 2f), (float)Math.Floor(FontSize.Y / 2f)), str, shadow, spacing, mainColor, outlineColor, data, scale, id);
    }

    public void Print(Vector2 pos, string str, bool shadow, int spacing, Color mainColor, Color outlineColor, float scale = 1, int id = 0)
    {
        Print(pos, str, shadow, spacing, mainColor, outlineColor, new TextEffectData(), scale, id);
    }

    public void Print(Vector2 pos, string str, bool shadow, int spacing, Color mainColor, Color outlineColor, TextEffectData data, float scale = 1, int id = 0)
    {

        SpriteEffects flip = SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        if (SaveData.Instance.Assists.MirrorMode) str = new string(str.Reverse().ToArray());

        List<Vector2> effectOffsets = [];
        List<char> theseChars = [];

        pos = pos.Floor();

        if (!data.Empty)
        {
            for (float i = 0; i < str.Length; i++)
            {
                effectOffsets.Add(Vector2.Zero);
                theseChars.Add(' ');

                float i2 = i + id;

                Calc.PushRandom((int)(Seed + 82 * i2 * i));
                if (data.Shakey || (data.Twitchy && Calc.Random.Chance(data.TwitchChance)))
                {
                    Vector2 num = new Vector2(Calc.Random.NextFloat(2f * data.ShakeAmount) - data.ShakeAmount, Calc.Random.NextFloat(2f * data.ShakeAmount) - data.ShakeAmount) * scale;
                    effectOffsets[(int)i] += num;
                }
                Calc.PopRandom();
                if (data.Wavey)
                {
                    Vector2 num = new Vector2
                        (
                        (float)Math.Sin(Scene.TimeActive * data.WaveSpeed + data.PhaseOffset + i2 * data.PhaseIncrement) * data.WaveAmp.X,
                        (float)Math.Sin(Scene.TimeActive * data.WaveSpeed + i2 * data.PhaseIncrement) * data.WaveAmp.Y
                        ) * scale;
                    effectOffsets[(int)i] += num;
                }

                if (!data.Obfuscated) continue;

                Calc.PushRandom((int)(Seed + 47 * i2));
                theseChars[(int)i] = CharList[Calc.Random.Next(CharList.Length)];
                Calc.PopRandom();
            }
        }

        int index = 0;

        if (outlineColor != Color.Transparent)
        {
            foreach (char c in str) //draw all outlines/shadows
            {
                float offset = index * spacing * scale;
                Vector2 charpos = pos + Vector2.UnitX * offset;
                charpos = new Vector2((float)Math.Floor(charpos.X), (float)Math.Floor(charpos.Y));
                if (!data.Empty && index < effectOffsets.Count) charpos += effectOffsets[index];

                int chr;
                if (!Charset.ContainsKey(c))
                {
                    chr = 0;
                }
                else
                {
                    if (!data.Empty && index < theseChars.Count)
                    {
                        if (theseChars[index] != ' ' && c != ' ')
                        {
                            chr = Charset[theseChars[index]];
                        }
                        else chr = Charset[c];
                    }
                    else chr = Charset[c];
                }
                //Logger.Log(nameof(PlutoniumHelperModule), "outline color: " + outlineColor.ToString());
                if (shadow)
                {
                    CharTextures[chr].Draw(charpos + Vector2.One * scale, Vector2.Zero, outlineColor, scale, 0, flip);
                }
                else
                {
                    DrawOutlineExceptGood(CharTextures[chr], charpos, Vector2.Zero, outlineColor, flip, scale);
                }
                index++;
            }
        }

        if (mainColor == Color.Transparent) return;

        index = 0;

        foreach (char c in str) //draw all characters
        {
            float offset = index * spacing * scale;
            Vector2 charpos = pos + Vector2.UnitX * offset;
            charpos = new Vector2((float)Math.Floor(charpos.X), (float)Math.Floor(charpos.Y));
            if (!data.Empty && index < effectOffsets.Count) charpos += effectOffsets[index];
            int chr;
            if (!Charset.TryGetValue(c, out int value))
            {
                chr = 0;
            }
            else
            {
                if (!data.Empty && index < theseChars.Count)
                {
                    if (theseChars[index] != ' ' && c != ' ')
                    {
                        chr = Charset[theseChars[index]];
                    }
                    else chr = value;
                }
                else chr = value;
            }

            CharTextures[chr].Draw(charpos, Vector2.Zero, mainColor, scale, 0, flip);
            index++;
        }

    }

    public static void Load()
    {
        //On.Celeste.DustEdges.BeforeRender += DustEdges_BeforeRender;
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
    }

    public static void DrawOutlineExceptGood(MTexture t, Vector2 position, Vector2 origin, Color color, SpriteEffects flip, float scale)
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
