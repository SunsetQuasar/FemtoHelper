using Celeste.Mod.FemtoHelper.Utils;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
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
    public readonly PlutoniumTextComponent Text;
    public readonly float Parallax;
    public readonly bool Hud;

    public readonly bool TruncateSliders;
    public readonly int Decimals;
    public readonly float Scale;
    public readonly string VisibilityFlag;

    public readonly Vector2 RenderOffset;

    private readonly bool legacy;
    public SimpleText(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        TruncateSliders = data.Bool("truncateSliderValues", false);
        Decimals = data.Int("decimals", -1);

        Nodes = [];
        if (!Dialog.Has(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example")))
        {
            Nodes.Add(new PlutoniumTextNodes.Text(Dialog.Get(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example"))));
        }
        else
        {
            Nodes = PlutoniumTextNodes.Parse(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example"), TruncateSliders, Decimals);
        }

        Color1 = Calc.HexToColorWithAlpha(data.Attr("mainColor", "ffffffff"));
        Color2 = Calc.HexToColorWithAlpha(data.Attr("outlineColor", "000000ff"));
        Depth = data.Int("depth", -100);
        Shadow = data.Bool("shadow", false);
        string list = data.Attr("charList", "");
        legacy = !string.IsNullOrWhiteSpace(list);
        Vector2 size = new(data.Int("fontWidth", 8), data.Int("fontHeight", 12));
        Hud = data.Bool("hud", false);

        string fontFilePath = data.Attr("fontDataPath", data.Attr("fontPath", "objects/FemtoHelper/PlutoniumText/example"));

        Spacing = legacy ? data.Int("spacing", 7) - (int)size.X : 0;
        Spacing += data.Int("extraSpacing", 0);

        Add(Text = new PlutoniumTextComponent(fontFilePath, Hud ? PlutoniumText.TextLayer.HUD : PlutoniumText.TextLayer.Gameplay, null, RenderCallback, new(), list, size));
        Parallax = data.Float("parallax", 1);
        Scale = data.Float("scale", 1);
        VisibilityFlag = data.Attr("visibilityFlag", "");

        RenderOffset = (data.FirstNodeNullable(offset) ?? Position) - Position;

        if (Hud) Tag |= TagsExt.SubHUD;
    }

    public override void Render()
    {
        if (Text.Layer == PlutoniumText.TextLayer.Gameplay) RenderCallback(SceneAs<Level>());
    }

    public void RenderCallback(Level level)
    {
        MTexture orDefault = GFX.ColorGrades.GetOrDefault(level.lastColorGrade, GFX.ColorGrades["none"]);
        MTexture orDefault2 = GFX.ColorGrades.GetOrDefault(level.Session.ColorGrade, GFX.ColorGrades["none"]);
        if (level.colorGradeEase > 0f && orDefault != orDefault2)
        {
            ColorGrade.Set(orDefault, orDefault2, level.colorGradeEase);
        }
        else
        {
            ColorGrade.Set(orDefault2);
        }

        if (!level.FancyCheckFlag(VisibilityFlag)) return;

        string str2 = PlutoniumTextNodes.ConstructString(Nodes, SceneAs<Level>());
        base.Render();

        if (Hud)
        {
            SubHudRenderer.EndRender();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, Matrix.Identity);
        }

        Vector2 position = level.Camera.Position;
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

    [GeneratedRegex(@"(\{|\})")]
    public static partial Regex MyRegex();
}
