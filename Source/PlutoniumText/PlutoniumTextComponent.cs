using Celeste.Mod.FemtoHelper.PlutoniumText;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using static Celeste.Mod.FemtoHelper.PlutoniumFont;

namespace Celeste.Mod.FemtoHelper;

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

        pos = pos.Floor();

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

                    if (flipped) charpos -= Vector2.UnitX * origChar.Width;

                    if (!EffectData.Empty && i < effectOffsets.Count) charpos += effectOffsets[i];

                    Color color = outlineColor;

                    if (EffectData.Rainbow)
                    {
                        color = outlineColor.Multiply(RainbowHelper.GetRainbowColorAt(Scene, EffectData.RainbowAnchor + (offset / (scale == 0 ? float.Epsilon : scale))));
                    }

                    if (shadow)
                    {
                        Draw.SpriteBatch.Draw(@char.Shadow, new Rectangle((int)MathF.Round(charpos.X - scale), (int)(MathF.Round(charpos.Y - scale) - (justify.Y * @char.Shadow.Height * scale)), (int)(@char.Shadow.Width * scale), (int)(@char.Shadow.Height * scale)), null, color, 0, Vector2.Zero, seffect, 0f);
                    }
                    else
                    {
                        Draw.SpriteBatch.Draw(@char.Outline, new Rectangle((int)MathF.Round(charpos.X - scale), (int)(MathF.Round(charpos.Y - scale) - (justify.Y * @char.Shadow.Height * scale)), (int)(@char.Shadow.Width * scale), (int)(@char.Shadow.Height * scale)), null, color, 0, Vector2.Zero, seffect, 0f);
                    }

                    offset += ((Vector2.UnitX * (origChar.Width + extraSpacing)) + origChar.PostDrawOffset.ToVector2()) * scale * factor;
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

                    if (flipped) charpos -= Vector2.UnitX * origChar.Width;

                    if (!EffectData.Empty && i < effectOffsets.Count) charpos += effectOffsets[i];

                    @char.DrawCharacter((charpos - (justify.Y * @char.Shadow.Height * Vector2.UnitY * scale)).Round(), color, scale, seffect);
                    //Draw.Rect((charpos - (justify.Y * @char.Shadow.Height * Vector2.UnitY * scale)).Floor(), 1, 1, Color.Red);

                    offset += ((Vector2.UnitX * (origChar.Width + extraSpacing)) + origChar.PostDrawOffset.ToVector2()) * scale * factor;
                }

                before = origc;
            }
        }
    }

    public Rectangle GetVisibilityRectangle(Vector2 pos, string str, int spacing = 0, float scale = 1f, Vector2 justify = default, float zoomFactor = 1)
    {
        Vector2 size = Font.StringSize(str, spacing) * scale;

        //start off as just the size of the text entity
        Rectangle visRect = new((int)pos.X, (int)pos.Y, (int)MathF.Ceiling(size.X), (int)MathF.Ceiling(size.Y));

        //apply justification
        visRect.X -= (int)MathF.Ceiling(visRect.Width * justify.X);
        visRect.Y -= (int)MathF.Ceiling(visRect.Height * justify.Y);

        //add 1 pixel to each side to account for the outlines
        visRect.Inflate((int)MathF.Ceiling(scale), (int)MathF.Ceiling(scale));

        if (!EffectData.Empty)
        {
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
        }

        //account for zoom when Hud Zoom Support is off;
        Point delta = new(visRect.Width - (int)((float)visRect.Width / zoomFactor), visRect.Height - (int)((float)visRect.Height / zoomFactor));
        visRect.X += (int)MathF.Round((float)delta.X  * justify.X);
        visRect.Y += (int)MathF.Round((float)delta.Y * justify.Y);
        visRect.Width = (int)MathF.Round((float)visRect.Width / zoomFactor);
        visRect.Height = (int)MathF.Round((float)visRect.Height / zoomFactor);

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
                if (FontCache.TryGetValue(to.PathVirtual + "." + to.Format, out PlutoniumFont found))
                {
                    foreach (var c in found.Chars)
                    {
                        c.Value.Outline.Dispose();
                        c.Value.Shadow.Dispose();
                    }
                    FontCache.Remove(to.PathVirtual + "." + to.Format);
                    Info($"Font {to.PathVirtual + "." + to.Format} was changed, removing from cache.");
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
