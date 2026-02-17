using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/TouchPlatform")]
public class TouchPlatform : JumpThru
{
    public bool Triggered;

    public readonly float Gravity;
    public readonly Vector2 SpeedOnTrigger;
    public Vector2 Speed;
    public TouchPlatform(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, false)
    {
        Depth = -900;

        string sprite = data.Attr("sprite", "brown");
        string path = $"objects/FemtoHelper/TouchPlatform/{sprite}";

        for (int i = 0; i < Width; i += 8)
        {
            MTexture slice;

            if (i == 0)
            {
                slice = GFX.Game[path].GetSubtexture(0, 0, 8, 16);
            }
            else if (i == (data.Width - 8))
            {
                slice = GFX.Game[path].GetSubtexture(16, 0, 8, 16);
            }
            else
            {
                slice = GFX.Game[path].GetSubtexture(8, 0, 8, 16);
            }
            Add(new Image(slice)
            {
                Position = Vector2.UnitX * i
            });
        }

        SurfaceSoundIndex = data.Int("surfaceIndex", -1);
        if (SurfaceSoundIndex == -1)
        {
            SurfaceSoundIndex = sprite switch
            {
                "gray" => 14,
                "checkeredA" => 18,
                "checkeredB" => 19,
                _ => 5,
            };
        }

        Gravity = data.Float("gravity", 200f);
        SpeedOnTrigger = data.Vector2("speedOnTriggerX", "speedOnTriggerY", Vector2.Zero);
    }

    public override void Update()
    {
        base.Update();

        if (!Triggered)
        {
            if (HasRider())
            {
                Trigger();
            }
        }

        if (Triggered)
        {
            MoveV(Speed.Y * Engine.DeltaTime);
            MoveH(Speed.X * Engine.DeltaTime);

            Speed.Y = Calc.Approach(Speed.Y, 200 * Math.Sign(Gravity), Engine.DeltaTime * Math.Abs(Gravity));
        }

        Level level = Scene as Level;
        if (Top > level.Bounds.Bottom + 8 ||
            Bottom < level.Bounds.Top - 64 ||
            Left > level.Bounds.Right + 8 ||
            Right < level.Bounds.Left - 8)
        {
            RemoveSelf();
        }
    }

    public void Trigger()
    {
        Speed = SpeedOnTrigger;
        Triggered = true;
    }
}
