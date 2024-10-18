using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper;

public class PlutoniumTextNodes
{
    public class Node
    {

    }
    public class Text : Node
    {
        public string text;
        public Text(string str)
        {
            text = str;
        }
    }
    public class Counter : Node
    {
        public string key;
        public Counter(string k)
        {
            key = k;
        }
    }
    public class Flag : Node
    {
        public string key;
        public string strIfOn;
        public string strIfOff;
        public Flag(string k, string on, string off)
        {
            key = k;
            strIfOn = on;
            strIfOff = off;
        }
    }
    public static string ConstructString(List<Node> nodes, Level level)
    {
        string result = "";
        foreach (Node n in nodes)
        {
            if (n is Text t)
            {
                result += t.text;
            }
            else if (n is Counter c)
            {
                result += level.Session.GetCounter(c.key).ToString();
            }
            else if (n is Flag f)
            {
                result += level.Session.GetFlag(f.key) ? f.strIfOn : f.strIfOff;
            }
        }
        return result;
    }
}
public class TextEffectData
{
    public bool wavey;
    public Vector2 waveAmp;
    public float phaseOffset;
    public float phaseIncrement;
    public float waveSpeed;

    public bool shakey;
    public float shakeAmount;

    public bool obfuscated;

    public bool twitchy;
    public float twitchChance;

    public bool empty;
    public TextEffectData(bool wavey, Vector2 amp, float offset, bool shakey, float amount, bool obfs, bool twitchy, float twitchChance, float phase_increment, float wave_speed)
    {
        this.wavey = wavey;
        if (wavey)
        {
            waveAmp = amp;
            phaseOffset = offset;
            phaseIncrement = phase_increment;
            waveSpeed = wave_speed;
        }

        this.shakey = shakey;
        shakeAmount = amount;

        obfuscated = obfs;
        this.twitchy = twitchy;
        this.twitchChance = twitchChance;
    }

    public TextEffectData()
    {
        empty = true;
    }
}

public class PlutoniumText : Component
{
    public Dictionary<char, int> charset;
    public List<MTexture> charTextures;
    public Vector2 fontSize;
    public float seed;
    public string charList;
    public PlutoniumText(string font_path, string char_list, Vector2 font_size) : base(true, true)
    {
        fontSize = font_size;
        charset = new Dictionary<char, int>();
        charTextures = new List<MTexture>();
        string characters = charList = char_list;
        // " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'".,ç"
        // " !"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~"

        for (int i = 0; i < characters.Length; i++)
        {
            charset.Add(characters[i], i);
            charTextures.Add(GFX.Game[font_path].GetSubtexture(i * (int)font_size.X, 0, (int)font_size.X, (int)font_size.Y)); // "PlutoniumHelper/PlutoniumText/font"
        }
    }

    public override void Update()
    {
        base.Update();
        if (Scene.OnInterval(0.04f))
        {
            seed = Calc.Random.NextFloat() * 12801;
        }
    }
    public void PrintCentered(Vector2 pos, string str, bool shadow, int spacing, Color mainColor, Color outlineColor, float scale = 1, int id = 0)
    {
        float stringlen = spacing * str.Length * scale;
        Print(pos - new Vector2((float)Math.Floor(stringlen / 2f), (float)Math.Floor(fontSize.Y / 2f)), str, shadow, spacing, mainColor, outlineColor, new TextEffectData(), scale, id);
    }

    public void PrintCentered(Vector2 pos, string str, bool shadow, int spacing, Color mainColor, Color outlineColor, TextEffectData data, float scale = 1, int id = 0)
    {
        float stringlen = spacing * str.Length * scale;
        Print(pos - new Vector2((float)Math.Floor(stringlen / 2f), (float)Math.Floor(fontSize.Y / 2f)), str, shadow, spacing, mainColor, outlineColor, data, scale, id);
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

        if (!data.empty)
        {
            for (float i = 0; i < str.Length; i++)
            {
                effectOffsets.Add(Vector2.Zero);
                theseChars.Add(' ');

                float i2 = i + id;

                Calc.PushRandom((int)(seed + (82 * i2 * i)));
                if (data.shakey || (data.twitchy && Calc.Random.Chance(data.twitchChance)))
                {
                    Vector2 num = new Vector2(Calc.Random.NextFloat(2f * data.shakeAmount) - data.shakeAmount, Calc.Random.NextFloat(2f * data.shakeAmount) - data.shakeAmount) * scale;
                    effectOffsets[(int)i] += num;
                }
                Calc.PopRandom();
                if (data.wavey)
                {
                    Vector2 num = new Vector2
                        (
                        (float)Math.Sin((Scene.TimeActive * data.waveSpeed) + data.phaseOffset + (i2 * data.phaseIncrement)) * data.waveAmp.X,
                        (float)Math.Sin((Scene.TimeActive * data.waveSpeed) + (i2 * data.phaseIncrement)) * data.waveAmp.Y
                        ) * scale;
                    effectOffsets[(int)i] += num;
                }

                if (data.obfuscated)
                {
                    Calc.PushRandom((int)(seed + (47 * i2)));
                    theseChars[(int)i] = charList[Calc.Random.Next(charList.Length)];
                    Calc.PopRandom();
                }
            }
        }

        int index = 0;

        if (outlineColor != Color.Transparent)
        {
            foreach (char c in str) //draw all outlines/shadows
            {
                float offset = index * spacing * scale;
                Vector2 charpos = pos + (Vector2.UnitX * offset);
                charpos = new Vector2((float)Math.Floor(charpos.X), (float)Math.Floor(charpos.Y));
                if (!data.empty && index < effectOffsets.Count) charpos += effectOffsets[index];

                int chr;
                if (!charset.ContainsKey(c))
                {
                    chr = 0;
                }
                else
                {
                    if (!data.empty && index < theseChars.Count)
                    {
                        if (theseChars[index] != ' ' && c != ' ')
                        {
                            chr = charset[theseChars[index]];
                        }
                        else chr = charset[c];
                    }
                    else chr = charset[c];
                }
                //Logger.Log(nameof(PlutoniumHelperModule), "outline color: " + outlineColor.ToString());
                if (shadow)
                {
                    charTextures[chr].Draw(charpos + (Vector2.One * scale), Vector2.Zero, outlineColor, scale, 0, flip);
                }
                else
                {
                    DrawOutlineExceptGood(charTextures[chr], charpos, Vector2.Zero, outlineColor, flip, scale);
                }
                index++;
            }
        }

        if (mainColor == Color.Transparent) return;

        index = 0;

        foreach (char c in str) //draw all characters
        {
            float offset = index * spacing * scale;
            Vector2 charpos = pos + (Vector2.UnitX * offset);
            charpos = new Vector2((float)Math.Floor(charpos.X), (float)Math.Floor(charpos.Y));
            if (!data.empty && index < effectOffsets.Count) charpos += effectOffsets[index];
            int chr;
            if (!charset.ContainsKey(c))
            {
                chr = 0;
            }
            else
            {
                if (!data.empty && index < theseChars.Count)
                {
                    if (theseChars[index] != ' ' && c != ' ')
                    {
                        chr = charset[theseChars[index]];
                    }
                    else chr = charset[c];
                }
                else chr = charset[c];
            }

            charTextures[chr].Draw(charpos, Vector2.Zero, mainColor, scale, 0, flip);
            index++;
        }

    }

    public static void Load()
    {
        On.Celeste.DustEdges.BeforeRender += DustEdges_BeforeRender;
    }
    private static void DustEdges_BeforeRender(On.Celeste.DustEdges.orig_BeforeRender orig, DustEdges self)
    {
        Texture temp1 = Engine.Graphics.GraphicsDevice.Textures[1];
        Texture temp2 = Engine.Graphics.GraphicsDevice.Textures[2];
        orig(self);
        Engine.Graphics.GraphicsDevice.Textures[1] = temp1;
        Engine.Graphics.GraphicsDevice.Textures[2] = temp2;
    }
    public static void Unload()
    {
        On.Celeste.DustEdges.BeforeRender -= DustEdges_BeforeRender;
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
                    Draw.SpriteBatch.Draw(t.Texture.Texture_Safe, position + (new Vector2(i, j) * scale), clipRect, color, 0f, origin2, scaleFix, flip, 0f);
                }
            }
        }

        //Draw.SpriteBatch.Draw(t.Texture.Texture_Safe, position, clipRect, Color.White, 0f, origin2, scaleFix, SpriteEffects.None, 0f);
    }
}
