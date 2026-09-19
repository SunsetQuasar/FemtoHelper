using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/MoverRefill")]
public class MoverRefill : CustomRefill
{
    Switch @switch;
    DirectionalLine line;
    EntityID ID;
    string flagName;
    bool disablePushing;
    private static readonly ParticleType _pShatter = new(P_Shatter)
    {
        Color = Calc.HexToColor("d3fffd"),
        Color2 = Calc.HexToColor("ec57ff")
    };

    private static readonly ParticleType _pRegen = new(P_Regen)
    {
        Color = Calc.HexToColor("a5f6ff"),
        Color2 = Calc.HexToColor("6d98e0")
    };

    private static readonly ParticleType _pGlow = new(P_Glow)
    {
        Color = Calc.HexToColor("a5f6ff"),
        Color2 = Calc.HexToColor("6d98e0")
    };

    private static readonly ParticleType _destination = new()
    {
        /*
        { "particleSize", 0.75f },
        { "attachToPlayerOffsetX", 0f },
        { "particleSizeRange", 0.5f },
        { "attachToPlayerOffsetY", 0f },
        { "particleColor", "00e8ff" },
        { "particleScaleOut", true },
        { "particleFadeMode", 2 },
        { "particleSpeedMin", 20f },
        { "particleColorMode", 1 },
        { "particleSpeedMax", 30f },
        { "particleRotationMode", 1 },
        { "particleSpawnSpread", 0f },
        { "particleAccelX", 0f },
        { "particleFriction", 50f },
        { "attachToPlayer", false },
        { "particleAccelY", 0f },
        { "particleTexture", "particles/triangle" },
        { "bloomAlpha", 0f },
        { "particleFlipChance", true },
        { "spawnInterval", 0.099999994f },
        { "particleSpinSpeedMin", 4f },
        { "spawnChance", 70f },
        { "particleSpinSpeedMax", 8f },
        { "bloomRadius", 6f },
        { "particleAngle", 180f },
        { "particleAngleRange", 360f },
        { "flag", "!MVP/2" },
        { "particleCount", 2 },
        { "particleColor2", "86ffff" },
        { "particleAlpha", 1f },
        { "foreground", false },
        { "noTexture", false },
        { "particleLifespanMin", 0.75f },
        { "tag", "" },
        { "particleLifespanMax", 1.25f }
        */

        Size = 0.75f,
        SizeRange = 0.5f,
        Color = Calc.HexToColor("00e8ff"),
        Color2 = Calc.HexToColor("86ffff"),
        ColorMode = ParticleType.ColorModes.Choose,
        ScaleOut = true,
        FadeMode = ParticleType.FadeModes.Late,
        SpeedMin = 20f,
        SpeedMax = 30f,
        RotationMode = ParticleType.RotationModes.Random,
        Acceleration = Vector2.Zero,
        Friction = 50f,
        Source = GFX.Game["particles/triangle"],
        SpinFlippedChance = true,
        SpinMin = 4f,
        SpinMax = 8f,
        Direction = 180f,
        DirectionRange = 360f,
        LifeMin = 0.75f,
        LifeMax = 1.25f,
    };

    public readonly Vector2 Destination;

    public readonly float MoveDuration = 0.5f;

    public MoverRefill(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, false, data.Bool("oneUse", false))
    {
        ID = id;
        flagName = $"mover_refill_{id.Key}";
        RefillDash = RefillStamina = false;

        Destination = data.FirstNodeNullable(offset) ?? (data.Position + offset);

        string path = "objects/FemtoHelper/moverRefill/";
        this.SetTexture(path)
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetCollectLogic((p) => p.Collidable)
            .SetOnCollect(OnCollect)
            .SetOnRespawn(OnRespawn);

        // make the outline a sprite
        Remove(outline);
        Add(outline = new Sprite(GFX.Game, path + "outline"));
        outline.Visible = false;
        (outline as Sprite)?.AddLoop("idle", "", 0.1f);
        (outline as Sprite)?.Play("idle");
        outline.CenterOrigin();

        VisualOffset = data.Vector2("visualOffsetX", "visualOffsetY", Vector2.Zero);
        MoveDuration = data.Float("moveTime", 0.5f);
        typeof(Tween).GetField("cached", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (data.Bool("oneUse", false) && data.Bool("isSwitch", false))
        {
            Add(@switch = new Switch(groundReset: false));
            @switch.OnActivate = () =>
            {
                for (int i = 0; i < 32; i++)
                {
                    float num = Calc.Random.NextFloat(MathF.PI * 2f);
                    level.Particles.Emit(TouchSwitch.P_FireWhite, Position + Calc.AngleToVector(num, 6f), num);
                }
            };
        }

        disablePushing = !data.Bool("solidsCanPush", true);
        RespawnTime = data.Float("respawnTime", 2.5f);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        SceneAs<Level>().Session.SetFlag($"{flagName}", false);
        SceneAs<Level>().Session.SetFlag($"{flagName}_return", true);
        scene.Add(line = new DirectionalLine(Position, Destination, 4, 2.5f, "SineInOut", "SineIn", "SineOut", 0.5f, 0.1f, "objects/FemtoHelper/moverRefill/smalindicatorarrow", "00ffffff", true, $"{flagName}_return", flagName, 0.25f, 0.25f, 1f, -10000));

        scene.OnEndOfFrame += () =>
        {
            line.Position = Position;
        };
    }

    public override void Update()
    {
        float delayRate = 1f;
        if (respawnTimer <= 0f)
        {
            delayRate = 1f;
        }
        else
        {
            delayRate = RespawnTime == 0 ? 1f : 0.1f + (6f * Ease.QuadIn(respawnTimer / RespawnTime));
        }
        (outline as Sprite)?.currentAnimation.Delay = 0.1f / delayRate;

        base.Update();

        if (SceneAs<Level>().Session.GetFlag($"{flagName}") || !Scene.OnInterval(0.1f)) return;

        for (int i = 0; i < 2; i++)
        {
            if (Calc.Random.Chance(0.7f))
            {
                SceneAs<Level>().ParticlesBG.Emit(_destination, Destination);
            }
        }
    }

    public override void Render()
    {
        base.Render();
    }

    public void TurnOn()
    {
        if (!@switch.Activated)
        {
            Audio.Play("event:/game/general/touchswitch_any", Center);
            if (@switch.Activate())
            {
                SoundEmitter.Play("event:/game/general/touchswitch_last_oneshot");
                Add(new SoundSource("event:/game/general/touchswitch_last_cutoff"));
            }
        }
    }

    public void OnCollect(Player player)
    {
        bool previousActive = player.Active;
        player.Active = false;
        bool previousCollidable = player.Collidable;
        player.Collidable = false;
        bool previousPush = player.AllowPushing;
        if (disablePushing) player.AllowPushing = false;

        player.UpdateHair(applyGravity: true);
        player.Hair.AfterUpdate();

        if (player.DashAttacking)
        {
            player.Speed *= 1.5f;
        }

        if (@switch is not null)
        {
            TurnOn();
        }

        Mover mover;
        Scene.Add(mover = new Mover(MoveDuration, Position, Destination));
        mover.Add(new Coroutine(mover.MovePlayer(player, previousActive, previousCollidable, previousPush, RespawnTime)));

        SceneAs<Level>().Session.SetFlag($"{flagName}", true);
        SceneAs<Level>().Session.SetFlag($"{flagName}_return", false);

        if (oneUse && line is not null)
        {
            Alarm.Set(line, line.DeactivationTime, line.RemoveSelf);
        }
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        (scene as Level)?.Session.SetFlag(flagName, true);
        if (line is not null)
        {
            Alarm.Set(line, line.DeactivationTime, line.RemoveSelf);
        }
    }

    public void OnRespawn()
    {
        SceneAs<Level>().Session.SetFlag($"{flagName}", false);
        SceneAs<Level>().Session.SetFlag($"{flagName}_return", true);
    }

    internal class Mover : Entity
    {
        internal SoundSource openSfx;
        internal Sprite icon;
        internal float MoveDuration;
        internal Vector2 Move;
        private static Color activeColor = Color.White;
        private static Color finishColor = Calc.HexToColor("f141df");
        private readonly Wiggler wiggler;
        private float respawnClock = 0f;
        public Mover(float dur, Vector2 start, Vector2 end) : base()
        {
            Depth = -1;
            Position = start;
            MoveDuration = dur;
            Move = end - start;
            Add(openSfx = new SoundSource());

            Add(icon = new Sprite(GFX.Game, "objects/switchgate/icon"));
            icon.Add("spin", "", 0.1f, "spin");
            icon.Play("spin");
            icon.Rate = 1f;
            icon.Color = activeColor;
            icon.CenterOrigin();
            Add(wiggler = Wiggler.Create(0.5f, 4f, f =>
            {
                icon.Scale = Vector2.One * (1f + f);
            }));
        }

        public IEnumerator MovePlayer(Player player, bool previousActiveState, bool previousCollidableState, bool previousAllowPushing, float respawnTime)
        {
            yield return 0.01f;

            openSfx.Play("event:/game/general/touchswitch_gate_open");
            float rate = 1 / MoveDuration;
            Vector2 prev = Vector2.Zero;
            float t = 0;
            while (t < 1)
            {
                yield return null;

                t = Calc.Approach(t, 1f, Engine.DeltaTime * rate);

                Vector2 curr = Vector2.Lerp(Vector2.Zero, Move, Ease.CubeIn(t));
                Vector2 delta = curr - prev;
                prev = curr;

                if (player is not null && !player.Dead)
                {
                    player.NaiveMove(delta);
                    player.UpdateHair(applyGravity: true);
                    player.Hair.AfterUpdate();

                    Position += delta;
                }
            }

            Audio.Play("event:/game/general/touchswitch_gate_finish", Position);
            Level level = Scene as Level;
            level.Shake();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

            if (player is not null && !player.Dead)
            {
                if (!player.TrySquishWiggleNoPusher(5, 5))
                {
                    player.Die(Vector2.Zero);
                }
                else
                {
                    player.Active = previousActiveState;
                    player.Collidable = previousCollidableState;
                    player.AllowPushing = previousAllowPushing;
                }
            }

            while (icon.Rate > 0f)
            {
                icon.Color = Color.Lerp(activeColor, finishColor, 1f - icon.Rate);
                icon.Rate -= Engine.DeltaTime * 4f;
                yield return null;
            }

            icon.Rate = 0f;
            icon.SetAnimationFrame(0);
            wiggler.Start();

            for (int i = 0; i < 32; i++)
            {
                float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
                SceneAs<Level>().Particles.Emit(TouchSwitch.P_Fire, Center + Calc.AngleToVector(angle, 4f), angle);
            }

            float time = MathF.Max(respawnTime - respawnClock, 0);
            yield return time / 2f;

            wiggler.StopAndClear();
            wiggler.onChange?.Invoke(wiggler.Value);
            Vector2 scale = icon.Scale;

            t = 0;
            while (t < 1)
            {
                yield return null;

                t = Calc.Approach(t, 1f, Engine.DeltaTime * 1f / (time / 2f));
                icon.Scale = Vector2.Lerp(scale, Vector2.Zero, Ease.ExpoIn(t));
            }
            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            respawnClock += Engine.DeltaTime;
        }

        public override void Render()
        {
            icon.DrawOutline();
            base.Render();
        }
    }
}
