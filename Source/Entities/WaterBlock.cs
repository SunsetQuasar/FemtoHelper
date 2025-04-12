using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[TrackedAs(typeof(Water))]
[CustomEntity("FemtoHelper/WaterBlock")]
public class WaterBlock : GenericWaterBlock
{
    public WaterBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true)
    {
        Add(new WaterSprite(data.Attr("spritePath", "objects/FemtoHelper/moveWater/nineSlice")));
    }
}
