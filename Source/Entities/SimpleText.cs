using Celeste.Mod.FemtoHelper.Utils;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/SimpleText")]
public partial class SimpleText : Entity
{
    public readonly List<PlutoniumTextNodes.Node> Nodes;
    public Color Color1;
    public Color Color2;
    public readonly bool Shadow;
    public readonly int Spacing;
    public readonly PlutoniumText Text;
    public readonly float Parallax;
    public readonly bool Hud;

    public readonly bool TruncateSliders;
    public readonly float Scale;
    public readonly string VisibilityFlag;

    public readonly Vector2 RenderOffset;
    public SimpleText(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        TruncateSliders = data.Bool("truncateSliderValues", false);
        
        Nodes = [];
        if(!Dialog.Has(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example")))
        {
            Nodes.Add(new PlutoniumTextNodes.Text(Dialog.Get(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example"))));
        } 
        else
        {
            string[] splitStr = MyRegex().Split(Dialog.Get(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example")));
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
                            Nodes.Add(new PlutoniumTextNodes.Flag(splitOnceAgain[0], splitOnceAgain[1], splitOnceAgain[2]));
                        }
                        else
                        {
                            if (splitStr2[i][0] == '@') {
                                Nodes.Add(new PlutoniumTextNodes.Slider(splitStr2[i].Remove(0,1), TruncateSliders));
                            } else {
                                Nodes.Add(new PlutoniumTextNodes.Counter(splitStr2[i]));
                            }
                        }
                    }
                }
                else
                {
                    Nodes.Add(new PlutoniumTextNodes.Text(splitStr2[i]));
                }
            }
        }

        Color1 = Calc.HexToColorWithAlpha(data.Attr("mainColor", "ffffffff"));
        Color2 = Calc.HexToColorWithAlpha(data.Attr("outlineColor", "000000ff"));
        Depth = data.Int("depth", -100);
        Shadow = data.Bool("shadow", false);
        Spacing = data.Int("spacing", 7);
        string path = data.Attr("fontPath", "objects/FemtoHelper/PlutoniumText/example");
        string list = data.Attr("charList", " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'\".,ç");
        Vector2 size = new Vector2(data.Int("fontWidth", 7), data.Int("fontHeight", 7));
        Add(Text = new PlutoniumText(path, list, size));
        Parallax = data.Float("parallax", 1);
        Hud = data.Bool("hud", false);
        Scale = data.Float("scale", 1);
        VisibilityFlag = data.Attr("visibilityFlag", "");
        
        RenderOffset = (data.FirstNodeNullable(offset) ?? Position) - Position;
        
        if (Hud) Tag |= TagsExt.SubHUD;
    }

    public override void Render()
    {
        MTexture orDefault = GFX.ColorGrades.GetOrDefault((Scene as Level).lastColorGrade, GFX.ColorGrades["none"]);
        MTexture orDefault2 = GFX.ColorGrades.GetOrDefault((Scene as Level).Session.ColorGrade, GFX.ColorGrades["none"]);
        if ((Scene as Level).colorGradeEase > 0f && orDefault != orDefault2)
        {
            ColorGrade.Set(orDefault, orDefault2, (Scene as Level).colorGradeEase);
        }
        else
        {
            ColorGrade.Set(orDefault2);
        }

        if (!(Scene as Level).FancyCheckFlag(VisibilityFlag)) return;

        string str2 = PlutoniumTextNodes.ConstructString(Nodes, SceneAs<Level>());
        base.Render();

        if (Hud)
        {
            SubHudRenderer.EndRender();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, Matrix.Identity);
        }

        Vector2 position = (Scene as Level).Camera.Position;
        Vector2 vector = position + new Vector2(160f, 90f);
        Vector2 position2 = Position - position + (Position - vector) * (Parallax - 1) + position;

        float scale2 = Scale;

        if (Hud)
        {
            position2 -= position;
            position2 *= Engine.ScreenMatrix.M11 * 6 < 6 ? 6 : Engine.ScreenMatrix.M11 * 6;
            scale2 *= Engine.ScreenMatrix.M11 * 6 < 6 ? 6 : Engine.ScreenMatrix.M11 * 6;
        }

        Text.PrintCentered(position2 + RenderOffset * (Hud ? 6 : 1), str2, Shadow, Spacing, Color1, Color2, scale2);

        if (Hud)
        {
            SubHudRenderer.EndRender();

            SubHudRenderer.BeginRender();
        }
    }

    [GeneratedRegex(@"(\s|\{|\})")]
    private static partial Regex MyRegex();
}
