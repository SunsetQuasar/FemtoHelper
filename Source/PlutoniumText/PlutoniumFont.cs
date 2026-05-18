using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Celeste.Mod.FemtoHelper;

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
