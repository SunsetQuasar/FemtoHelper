
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[TrackedAs(typeof(Water))]
[CustomEntity("FemtoHelper/WaterTest")]
public class WaterTest : GenericWaterBlock
{
    public Vector2 anchor;
    public float timer;

    public float multiplier;
    public WaterTest(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height)
    {
        anchor = Position;

        multiplier = data.Bool("backwards", false) ? 1 : -1;
    }

    public override void Update()
    {
        base.Update();
        MoveTo(anchor + new Vector2(MathF.Sin(timer * 2) * 24, MathF.Cos(timer * 2) * 24));
        timer += Engine.DeltaTime * multiplier;
    }

    public override void DrawDisplacement()
    {
        
    }

    public override void Render()
    {
        base.Render();
        Draw.HollowRect(Collider, Color.CadetBlue);
    }
}
