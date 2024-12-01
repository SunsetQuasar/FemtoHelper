using Celeste.Mod.FemtoHelper.Utils;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/SimpleText")]
public class SimpleText : Entity
{
    public readonly List<PlutoniumTextNodes.Node> Nodes;
    public Color Color1;
    public Color Color2;
    public readonly bool Shadow;
    public readonly int Spacing;
    public readonly PlutoniumText Text;
    public readonly float Parallax;
    public readonly bool Hud;
    public readonly float Scale;
    public readonly string VisibilityFlag;
    public SimpleText(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Nodes = new List<PlutoniumTextNodes.Node>();

        string[] splitStr = Regex.Split(Dialog.Get(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example")), "(\\s|\\{|\\})");
        string[] splitStr2 = new string[splitStr.Length];
        int num = 0;
        foreach (var t in splitStr)
        {
            if (!string.IsNullOrEmpty(t))
            {
                splitStr2[num++] = t;
            }
        }

        for(int i = 0; i < splitStr2.Length; i++)
        {
            if (splitStr2[i] == "{")
            {
                i++;

                for (; i < splitStr2.Length && splitStr2[i] != "}"; i++)
                {
                    if (!string.IsNullOrWhiteSpace(splitStr2[i]))
                    {
                        string[] splitOnceAgain = splitStr2[i].Split(';');
                        if (splitOnceAgain.Length == 3)
                        {
                            Nodes.Add(new PlutoniumTextNodes.Flag(splitOnceAgain[0], splitOnceAgain[1], splitOnceAgain[2]));
                        } 
                        else
                        {
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
        if (Hud) Tag |= TagsExt.SubHUD;
    }

    public override void Render()
    {
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
        Vector2 position2 = (Position - position + (Position - vector) * (Parallax - 1)) + position;

        float scale2 = Scale;

        if (Hud)
        {
            position2 -= position;
            position2 *= (Engine.ScreenMatrix.M11 * 6) < 6 ? 6 : (Engine.ScreenMatrix.M11 * 6);
            scale2 *= (Engine.ScreenMatrix.M11 * 6) < 6 ? 6 : (Engine.ScreenMatrix.M11 * 6);
        }

        Text.PrintCentered(position2, str2, Shadow, Spacing, Color1, Color2, scale2);

        if (Hud)
        {

            SubHudRenderer.EndRender();

            SubHudRenderer.BeginRender();
        }
    }
}
