using Celeste.Mod.FemtoHelper.Utils;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Celeste.Mod.FemtoHelper.Entities
{
    [CustomEntity("FemtoHelper/SimpleText")]
    public class SimpleText : Entity
    {
        public List<PlutoniumTextNodes.Node> nodes;
        public Color color1;
        public Color color2;
        public bool shadow;
        public int spacing;
        public PlutoniumText text;
        public float parallax;
        public bool hud;
        public float scale;
        public string visibilityFlag;
        public SimpleText(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            nodes = new List<PlutoniumTextNodes.Node>();

            string[] split_str = Regex.Split(Dialog.Get(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example")), "(\\s|\\{|\\})");
            string[] split_str2 = new string[split_str.Length];
            int num = 0;
            for (int i = 0; i < split_str.Length; i++)
            {
                if (!string.IsNullOrEmpty(split_str[i]))
                {
                    split_str2[num++] = split_str[i];
                }
            }

            for(int i = 0; i < split_str2.Length; i++)
            {
                if (split_str2[i] == "{")
                {
                    i++;

                    for (; i < split_str2.Length && split_str2[i] != "}"; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(split_str2[i]))
                        {
                            string[] splitOnceAgain = split_str2[i].Split(';');
                            if (splitOnceAgain.Length == 3)
                            {
                                nodes.Add(new PlutoniumTextNodes.Flag(splitOnceAgain[0], splitOnceAgain[1], splitOnceAgain[2]));
                            } 
                            else
                            {
                                nodes.Add(new PlutoniumTextNodes.Counter(split_str2[i]));
                            }
                        }
                    }
                } 
                else
                {
                    nodes.Add(new PlutoniumTextNodes.Text(split_str2[i]));
                }
            }

            color1 = Calc.HexToColorWithAlpha(data.Attr("mainColor", "ffffffff"));
            color2 = Calc.HexToColorWithAlpha(data.Attr("outlineColor", "000000ff"));
            Depth = data.Int("depth", -100);
            shadow = data.Bool("shadow", false);
            spacing = data.Int("spacing", 7);
            string path = data.Attr("fontPath", "objects/FemtoHelper/PlutoniumText/example");
            string list = data.Attr("charList", " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'\".,ç");
            Vector2 size = new Vector2(data.Int("fontWidth", 7), data.Int("fontHeight", 7));
            Add(text = new PlutoniumText(path, list, size));
            parallax = data.Float("parallax", 1);
            hud = data.Bool("hud", false);
            scale = data.Float("scale", 1);
            visibilityFlag = data.Attr("visibilityFlag", "");
            if (hud) Tag |= TagsExt.SubHUD;
        }

        public override void Render()
        {
            if (!(Scene as Level).FancyCheckFlag(visibilityFlag)) return;

            string str2 = PlutoniumTextNodes.ConstructString(nodes, SceneAs<Level>());
            base.Render();

            if (hud)
            {
                SubHudRenderer.EndRender();

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, Matrix.Identity);
            }

            Vector2 position = (Scene as Level).Camera.Position;
            Vector2 vector = position + new Vector2(160f, 90f);
            Vector2 position2 = (Position - position + (Position - vector) * (parallax - 1)) + position;

            float scale2 = scale;

            if (hud)
            {
                position2 -= position;
                position2 *= (Engine.ScreenMatrix.M11 * 6) < 6 ? 6 : (Engine.ScreenMatrix.M11 * 6);
                scale2 *= (Engine.ScreenMatrix.M11 * 6) < 6 ? 6 : (Engine.ScreenMatrix.M11 * 6);
            }

            text.PrintCentered(position2, str2, shadow, spacing, color1, color2, scale2);

            if (hud)
            {

                SubHudRenderer.EndRender();

                SubHudRenderer.BeginRender();
            }
        }
    }
}
