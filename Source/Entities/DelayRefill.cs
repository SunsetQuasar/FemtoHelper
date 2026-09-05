using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/DelayRefill")]
public class DelayRefill : CustomRefill
{
    internal class ClockDisplay : Entity
    {
        private readonly Sprite clock;
        public ClockDisplay(DelayRefill parent) : base(parent.Position) 
        {
            Depth = 8999;

            clock = new(GFX.Game, "objects/FemtoHelper/bumpRefill/");
            clock.AddLoop("idle", "timer", parent.delay / 4f);
            clock.Play("idle");

            clock.CenterOrigin();

            Add(clock);
        }
    }

    public enum Direction
    {
        Up = 0,
        Right = 90,
        Down = 180,
        Left = 270,
    }

    public const float BumpSpeed = 320f;

    private static readonly ParticleType _pShatter = new(P_Shatter)
    {
        Color = Calc.HexToColor("ffd3f7"),
        Color2 = Calc.HexToColor("fc85a2")
    };

    private static readonly ParticleType _pRegen = new(P_Regen)
    {
        Color = Calc.HexToColor("ffa5a5"),
        Color2 = Calc.HexToColor("e06d88")
    };

    private static readonly ParticleType _pGlow = new(P_Glow)
    {
        Color = Calc.HexToColor("ffa5a5"),
        Color2 = Calc.HexToColor("e06d88")
    };

    public Direction dir;
    public float delay;
    ClockDisplay clock;

    public DelayRefill(EntityData data, Vector2 offset) : base(data.Position + offset, false, data.Bool("oneUse", false))
    {
        AlwaysUse = true;
        RefillDash = RefillStamina = false;
        dir = data.Enum("direction", Direction.Up);
        delay = data.Float("delay", 1.2f);

        clock = new(this);

        if (data.FirstNodeNullable(offset).HasValue)
        {
            Vector2 delta = data.FirstNodeNullable(offset).Value - Position;
            clock.Position += delta;
        }

        this.SetTexture("objects/FemtoHelper/bumpRefill/")
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetOnCollect(OnCollect)
            .SetEnumerator(CollectRoutine);

        sprite.Rotation = flash.Rotation = outline.Rotation = (float)dir * Calc.DegToRad;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(clock);
    }

    public void OnCollect(Player player)
    {
        for (int i = 0; i < 5; i++)
        {
            int iRightNow = i;
            Alarm.Set(player, i * (delay / 4f), () =>
            {
                EventInstance instance = Audio.Play("event:/FemtoHelper/clock", player.Position);
                instance.setVolume(0.3f + (0.2f * (iRightNow / 4f)));
                instance.setPitch(Calc.ClampedMap(iRightNow / 4f, 0, 1, 0.9f, 1.1f));
            }, Alarm.AlarmMode.Oneshot);
        }
        Alarm.Set(player, delay, () =>
        {
            if (player is null || player.Dead) return;

            switch (dir)
            {
                case Direction.Up:
                    player.Speed.Y = -BumpSpeed;
                    break;
                case Direction.Right:
                    player.Speed.X = BumpSpeed;
                    break;
                case Direction.Down:
                    player.Speed.Y = BumpSpeed;
                    break;
                case Direction.Left:
                default:
                    player.Speed.X = -BumpSpeed;
                    break;
            }
            Audio.Play("event:/game/general/thing_booped", player.Position);

        }, Alarm.AlarmMode.Oneshot);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Removed(Scene scene)
    {
        clock.RemoveSelf();
        base.Removed(scene);
    }

    public IEnumerator CollectRoutine(Player player)
    {
        // ease the depth a little, jeez
        // so the clock can always appears behind
        Depth = 8990;
        yield break;
    }
}
