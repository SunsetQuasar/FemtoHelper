using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.FemtoHelper.Utils;
using Iced.Intel;

namespace Celeste.Mod.FemtoHelper.Entities;

[TrackedAs(typeof(Water))]
[CustomEntity("FemtoHelper/WaterFallingBlock")]
public class WaterFallingBlock : GenericWaterBlock
{
    private WaterSprite waterSprite;

    public float FallDelay;
    public bool HasStartedFalling { get; private set; }

    private readonly Vector2 direction;

    private Shaker shake;

    private readonly List<Tuple<Image, int>> arrows;

    private readonly Wiggler Wiggle;

    public WaterFallingBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, data.Bool("canCarry", true))
    {
        string prefix = data.Attr("spritePath", "objects/FemtoHelper/waterFallingBlock/");

        Add(waterSprite = new WaterSprite(prefix + "nineSlice"));
        Add(new Coroutine(Sequence()));
        direction = data.Int("direction", 0) switch
        {
            1 => new Vector2(1, 0),
            2 => new Vector2(0, -1),
            3 => new Vector2(-1, 0),
            _ => new Vector2(0, 1)
        };
        Add(shake = new Shaker(false));

        arrows = [];

        for(int i = 4; i < Width + 4; i += 8)
        {
            Image arrow = new Image(GFX.Game[prefix + "arrow"]);
            arrow.CenterOrigin();
            arrow.Position = new Vector2(i, Height / 2);
            arrows.Add(new Tuple<Image, int>(arrow, i));
            Add(arrow);
            arrow.Rotation = data.Int("direction", 0) switch
            {
                1 => 0,
                2 => -MathF.PI / 2,
                3 => -MathF.PI,
                _ => MathF.PI / 2
            };
        }

        Tween tween = Tween.Create(Tween.TweenMode.Looping, Ease.Linear, 2.4f, true);
        tween.OnUpdate += (t) =>
        {
            for (float i = 0; i < arrows.Count; i++)
            {
                float anchor = Height / 2;
                arrows[(int)i].Item1.Position.Y = anchor + MathF.Sin((2f * i) + t.Eased * MathF.Tau) * 2f;
            }
        };
        Add(tween);

        Add(Wiggle = Wiggler.Create(0.5f, 3f, (t) =>
        {
            for (float i = 0; i < arrows.Count; i++)
            {
                arrows[(int)i].Item1.Position.X = arrows[(int)i].Item2 + ((arrows[(int)i].Item2 - Width / 2) / Width / 2) * 16 * -t;
            }
        }));
    }

    private void WiggleEverything()
    {
        StartShaking();
        Wiggle.Start();
        waterSprite.Wiggle.Start();
    }

    public override void OnShake(Vector2 amount)
    {
        base.OnShake(amount);
        waterSprite.Position += amount;
    }

    private IEnumerator Sequence()
    {
        while (!CollideCheck<Player>())
        {
            yield return null;
        }
        while (FallDelay > 0f)
        {
            FallDelay -= Engine.DeltaTime;
            yield return null;
        }
        HasStartedFalling = true;
        while (true)
        {
            ShakeSfx();
            WiggleEverything();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return 0.2f;
            float timer = 0.4f;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            StopShaking();
            for (int i = 2; (float)i < Width; i += 4)
            {
                if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                {
                    SceneAs<Level>().Particles.Emit(Tinydrops, 2, new Vector2(X + (float)i, Y), Vector2.One * 4f, MathF.PI / 2f);
                }
                SceneAs<Level>().Particles.Emit(Tinydrops, 2, new Vector2(X + (float)i, Y), Vector2.One * 4f);
            }
            float speed = 0f;
            float maxSpeed = 160f;
            while (true)
            {
                Level level = SceneAs<Level>();
                speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                if (MoveToCollideBarriers(Position + (direction * (speed * Engine.DeltaTime)), thruDashBlocks: true, evenSolids: false))
                {
                    break;
                }
                if (Top > (float)(level.Bounds.Bottom + 16) || (Top > (float)(level.Bounds.Bottom - 1) && (CollideCheck<Solid>(Position + direction) || this.CollideCheckIgnoreCollidable<SeekerBarrier>(Position + direction))))
                {
                    WaterFallingBlock fallingBlock = this;
                    WaterFallingBlock fallingBlock2 = this;
                    bool collidable = false;
                    fallingBlock2.Visible = false;
                    fallingBlock.Collidable = collidable;
                    yield return 0.2f;
                    if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f)))
                    {
                        yield return 0.2f;
                        SceneAs<Level>().Shake();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    }
                    RemoveSelf();
                    yield break;
                }
                yield return null;
            }
            ImpactSfx();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.3f);
            WiggleEverything();
            //LandParticles();
            yield return 0.2f;
            StopShaking();
            if (CollideCheck<SolidTiles>(Position + direction))
            {
                break;
            }
            while (CollideCheck<Platform>(Position + direction) || this.CollideCheckIgnoreCollidable<SeekerBarrier>(Position + direction))
            {
                yield return 0.1f;
            }
        }
        //Safe = true;
    }

    public override void Render()
    {
        base.Render();
    }

    private void ShakeSfx()
    {
        Audio.Play("event:/FemtoHelper/water_fallblock_first_shake", base.Center);
    }

    private void ImpactSfx()
    {
        Audio.Play("event:/FemtoHelper/water_fallblock_first_impact", base.BottomCenter);
    }
}
