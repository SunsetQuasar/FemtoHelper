using Celeste.Mod.FemtoHelper.Utils;
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
    public readonly PlutoniumTextComponent Text;
    public readonly float Parallax;
    public readonly bool Hud;

    public readonly bool TruncateSliders;
    public readonly int Decimals;
    public readonly float Scale;
    public readonly string VisibilityFlag;

    public readonly Vector2 RenderOffset;

    private readonly bool legacy;

    public readonly Vector2 Justify;
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

        Justify = data.Vector2("justifyX", "justifyY", new Vector2(0.5f, 0.5f));

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
        base.Render();
        if (Text.Layer == PlutoniumText.TextLayer.Gameplay) RenderCallback(SceneAs<Level>());
    }

    public void RenderCallback(Level level)
    {
        if (!level.FancyCheckFlag(VisibilityFlag)) return;

        bool flip = (SaveData.Instance?.Assists.MirrorMode ?? false);

        bool hudFlip = flip && Text.Layer == PlutoniumText.TextLayer.HUD;

        string str2 = PlutoniumTextNodes.ConstructString(Nodes, SceneAs<Level>());

        float scale2 = Scale;

        Vector2 camPos = level.Camera.Position;
        Vector2 camCenter = camPos + new Vector2(160f, 90f);
        Vector2 position2 = ((Position - camCenter) * Parallax * new Vector2(hudFlip ? -1 : 1, 1)) + camCenter;

        if (Text.Layer == PlutoniumText.TextLayer.HUD)
        {
            position2 *= 6f;
            scale2 *= 6;
        }

        Text.Print(position2 + RenderOffset, str2, Shadow, Spacing, Color1, Color2, Justify, scale2, 0, flip && (Text.Layer != PlutoniumText.TextLayer.HUD));
    }

    [GeneratedRegex(@"(\{|\})")]
    public static partial Regex MyRegex();
}
