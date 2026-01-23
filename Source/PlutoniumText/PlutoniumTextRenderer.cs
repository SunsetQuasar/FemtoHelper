using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.PlutoniumText;

public class PlutoniumTextRenderer : Entity
{
    private TextLayer layer;
    public PlutoniumTextRenderer(TextLayer layer) : base()
    {
        this.layer = layer;
        AddTag(Tags.Global);
        if (layer == TextLayer.HUD) AddTag(TagsExt.SubHUD);
    }
}
