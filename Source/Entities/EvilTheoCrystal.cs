using System;
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.FemtoHelper.Entities;


[Tracked(false)]
[CustomEntity("FemtoHelper/EvilTheoCrystal")]
public class EvilTheoCrystal : Actor
{
    public static readonly ParticleType PImpact = new ParticleType(TheoCrystal.P_Impact);

    public Vector2 Speed;

    public bool OnPedestal;

    public readonly Holdable Hold;

    private readonly Sprite sprite;

    private bool dead;

    private Level level;

    private readonly Collision onCollideH;

    private readonly Collision onCollideV;

    private float noGravityTimer;

    private Vector2 prevLiftSpeed;

    private Vector2 previousPosition;

    private HoldableCollider hitSeeker;

    private float swatTimer;

    private bool shattering;

    private float hardVerticalHitSoundCooldown;

    private BirdTutorialGui tutorialGui;

    private float tutorialTimer;

    public readonly VertexLight Light;

    public EvilTheoCrystal(Vector2 position)
        : base(position)
    {
        previousPosition = position;
        Depth = 100;
        Collider = new Hitbox(8f, 10f, -4f, -10f);
        Add(sprite = FemtoModule.FemtoSpriteBank.Create("theo_crystal_evil"));
        sprite.Scale.X = -1f;
        Add(Hold = new Holdable());
        Hold.PickupCollider = new Hitbox(16f, 22f, -8f, -16f);
        Hold.SlowFall = false;
        Hold.SlowRun = true;
        Hold.OnPickup = OnPickup;
        Hold.OnRelease = OnRelease;
        Hold.DangerousCheck = Dangerous;
        Hold.OnHitSeeker = HitSeeker;
        Hold.OnSwat = Swat;
        Hold.OnHitSpring = HitSpring;
        Hold.OnHitSpinner = HitSpinner;
        Hold.SpeedGetter = () => Speed;
        onCollideH = OnCollideH;
        onCollideV = OnCollideV;
        LiftSpeedGraceTime = 0.1f;
        Add(Light = new VertexLight(Collider.Center, Color.Red, 1f, 32, 64));
        Add(new MirrorReflection());
    }

    public EvilTheoCrystal(EntityData e, Vector2 offset)
        : this(e.Position + offset)
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
        if (level.Session.Level == "e-00")
        {
            tutorialGui = new BirdTutorialGui(this, new Vector2(0f, -24f), Dialog.Clean("tutorial_carry"), Dialog.Clean("tutorial_hold"), BirdTutorialGui.ButtonPrompt.Grab);
            tutorialGui.Open = false;
            Scene.Add(tutorialGui);
        }
    }

    public override void Update()
    {
        base.Update();
        if (shattering || dead)
        {
            Hold.cannotHoldTimer = 9999f;
            return;
        }
        if (swatTimer > 0f)
        {
            swatTimer -= Engine.DeltaTime;
        }
        hardVerticalHitSoundCooldown -= Engine.DeltaTime;
        if (OnPedestal)
        {
            Depth = 8999;
            return;
        }
        Depth = 100;
        if (Hold.IsHeld)
        {
            prevLiftSpeed = Vector2.Zero;
        }
        else
        {
            if (OnGround())
            {
                float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
                Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                Vector2 liftSpeed = LiftSpeed;
                if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                {
                    Speed = prevLiftSpeed;
                    prevLiftSpeed = Vector2.Zero;
                    Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                    if (Speed.X != 0f && Speed.Y == 0f)
                    {
                        Speed.Y = -60f;
                    }
                    if (Speed.Y < 0f)
                    {
                        noGravityTimer = 0.15f;
                    }
                }
                else
                {
                    prevLiftSpeed = liftSpeed;
                    if (liftSpeed.Y < 0f && Speed.Y < 0f)
                    {
                        Speed.Y = 0f;
                    }
                }
            }
            else if (Hold.ShouldHaveGravity)
            {
                float num = 800f;
                if (Math.Abs(Speed.Y) <= 30f)
                {
                    num *= 0.5f;
                }
                float num2 = 350f;
                if (Speed.Y < 0f)
                {
                    num2 *= 0.5f;
                }
                Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                if (noGravityTimer > 0f)
                {
                    noGravityTimer -= Engine.DeltaTime;
                }
                else
                {
                    Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                }
            }
            previousPosition = ExactPosition;
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            if (Right > level.Bounds.Right)
            {
                Right = level.Bounds.Right;
                Speed.X *= -0.4f;
            }
            else if (Left < level.Bounds.Left)
            {
                Left = level.Bounds.Left;
                Speed.X *= -0.4f;
            }
            else if (Top < level.Bounds.Top - 4)
            {
                Top = level.Bounds.Top + 4;
                Speed.Y = 0f;
            }
            else if (Bottom > level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
            {
                Bottom = level.Bounds.Bottom;
                Speed.Y = -300f;
                Audio.Play("event:/game/general/assist_screenbottom", Position);
            }
            else if (Top > level.Bounds.Bottom)
            {
                Die();
            }
            if (X < level.Bounds.Left + 10)
            {
                MoveH(32f * Engine.DeltaTime);
            }
            Player entity = Scene.Tracker.GetEntity<Player>();
            TempleGate templeGate = CollideFirst<TempleGate>();
            if (templeGate != null && entity != null)
            {
                templeGate.Collidable = false;
                MoveH(Math.Sign(entity.X - X) * 32 * Engine.DeltaTime);
                templeGate.Collidable = true;
            }
        }
        if (!dead)
        {
            Hold.CheckAgainstColliders();
        }
        if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold))
        {
            hitSeeker = null;
        }
        if (tutorialGui != null)
        {
            if (!OnPedestal && !Hold.IsHeld && OnGround() && level.Session.GetFlag("foundTheoInCrystal"))
            {
                tutorialTimer += Engine.DeltaTime;
            }
            else
            {
                tutorialTimer = 0f;
            }
            tutorialGui.Open = tutorialTimer > 0.25f;
        }
    }

    public IEnumerator Shatter()
    {
        shattering = true;
        BloomPoint bloom = new BloomPoint(0f, 32f);
        VertexLight light = new VertexLight(Color.AliceBlue, 0f, 64, 200);
        Add(bloom);
        Add(light);
        for (float p = 0f; p < 1f; p += Engine.DeltaTime)
        {
            Position += Speed * (1f - p) * Engine.DeltaTime;
            level.ZoomFocusPoint = TopCenter - level.Camera.Position;
            light.Alpha = p;
            bloom.Alpha = p;
            yield return null;
        }
        yield return 0.5f;
        level.Shake();
        sprite.Play("shatter");
        yield return 1f;
        level.Shake();
    }

    public void ExplodeLaunch(Vector2 from)
    {
        if (Hold.IsHeld) return;
        Speed = (Center - from).SafeNormalize(120f);
        SlashFx.Burst(Center, Speed.Angle());
    }

    public void Swat(HoldableCollider hc, int dir)
    {
        if (!Hold.IsHeld || hitSeeker != null) return;
        swatTimer = 0.1f;
        hitSeeker = hc;
        Hold.Holder.Swat(dir);
    }

    public bool Dangerous(HoldableCollider holdableCollider)
    {
        if (!Hold.IsHeld && Speed != Vector2.Zero)
        {
            return hitSeeker != holdableCollider;
        }
        return false;
    }

    public void HitSeeker(Seeker seeker)
    {
        if (!Hold.IsHeld)
        {
            Speed = (Center - seeker.Center).SafeNormalize(120f);
        }
        Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
    }

    public void HitSpinner(Entity spinner)
    {
        if (Hold.IsHeld || !(Speed.Length() < 0.01f) || !(LiftSpeed.Length() < 0.01f) ||
            !((previousPosition - ExactPosition).Length() < 0.01f) || !OnGround()) return;
        int num = Math.Sign(X - spinner.X);
        if (num == 0)
        {
            num = 1;
        }
        Speed.X = num * 120f;
        Speed.Y = -30f;
    }

    public bool HitSpring(Spring spring)
    {
        if (Hold.IsHeld) return false;
        switch (spring.Orientation)
        {
            case Spring.Orientations.Floor when Speed.Y >= 0f:
                Speed.X *= 0.5f;
                Speed.Y = -160f;
                noGravityTimer = 0.15f;
                return true;
            case Spring.Orientations.WallLeft when Speed.X <= 0f:
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = 220f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            case Spring.Orientations.WallRight when Speed.X >= 0f:
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = -220f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            default:
                return false;
        }
    }

    private void OnCollideH(CollisionData data)
    {
        if (data.Hit is DashSwitch @switch)
        {
            @switch.OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
        }
        Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
        if (Math.Abs(Speed.X) > 100f)
        {
            ImpactParticles(data.Direction);
        }
        Speed.X *= -0.4f;
    }

    private void OnCollideV(CollisionData data)
    {
        if (data.Hit is DashSwitch @switch)
        {
            @switch.OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        }
        if (Speed.Y > 0f)
        {
            if (hardVerticalHitSoundCooldown <= 0f)
            {
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                hardVerticalHitSoundCooldown = 0.5f;
            }
            else
            {
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
            }
        }
        if (Speed.Y > 160f)
        {
            ImpactParticles(data.Direction);
        }
        if (Speed.Y > 140f && data.Hit is not SwapBlock && data.Hit is not DashSwitch)
        {
            Speed.Y *= -0.6f;
        }
        else
        {
            Speed.Y = 0f;
        }
    }

    private void ImpactParticles(Vector2 dir)
    {
        float direction;
        Vector2 position;
        Vector2 positionRange;
        switch (dir.X)
        {
            case > 0f:
                direction = (float)Math.PI;
                position = new Vector2(Right, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
                break;
            case < 0f:
                direction = 0f;
                position = new Vector2(Left, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
                break;
            default:
            {
                if (dir.Y > 0f)
                {
                    direction = -(float)Math.PI / 2f;
                    position = new Vector2(X, Bottom);
                    positionRange = Vector2.UnitX * 6f;
                }
                else
                {
                    direction = (float)Math.PI / 2f;
                    position = new Vector2(X, Top);
                    positionRange = Vector2.UnitX * 6f;
                }

                break;
            }
        }
        level.Particles.Emit(PImpact, 12, position, positionRange, direction);
    }

    public override bool IsRiding(Solid solid)
    {
        return Speed.Y == 0f && base.IsRiding(solid);
    }

    public override void OnSquish(CollisionData data)
    {
        if (!TrySquishWiggle(data) && !SaveData.Instance.Assists.Invincible)
        {
            Die();
        }
    }

    private void OnPickup()
    {
        Speed = Vector2.Zero;
        AddTag(Tags.Persistent);
    }

    private void OnRelease(Vector2 force)
    {
        RemoveTag(Tags.Persistent);
        Player p = Scene.Tracker.GetNearestEntity<Player>(Position);
        if (p != null)
        {
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
                if (Input.Aim.Value.Y < 0f)
                {
                    force.Y = -1f;
                    force.X = p.Speed.X / 800f;
                    Speed = force * 200f;
                    goto skipit;
                }
            }

            if (force.X == 0)
            {
                force.X = (float)p.Facing;
                force.Y = -0.4f;
                Speed = force * 200f;
            }
            else
            {
                force.Y *= 1.5f;
                Speed.Y -= 80;
                p.Speed = force * 400f;
            }
        }
        skipit:
        if (Speed != Vector2.Zero)
        {
            noGravityTimer = 0.1f;
        }
    }

    public void Die()
    {
        if (dead) return;
        Add(new Coroutine(DimLights()));
        if (Hold.IsHeld) Hold.Release(Vector2.Zero);
        Hold.cannotHoldTimer = 9999f;
        dead = true;
        Audio.Play("event:/char/madeline/death", Position);
        Add(new DeathEffect(Calc.HexToColor("FF7063"), Center - Position));
        sprite.Visible = false;
        Depth = -1000000;
        AllowPushing = false;
    }

    public IEnumerator DimLights()
    {
        for(float p = 0; p < 1; p += Engine.DeltaTime)
        {
            Light.Alpha = Ease.SineInOut(1 - p);
            Light.HandleGraphicsReset();
            yield return null;
        }
    }

    public static void Load()
    {
        IL.Celeste.Player.Throw += PlayerThrowNoKb;
    }

    public static void Unload()
    {
        IL.Celeste.Player.Throw -= PlayerThrowNoKb;
    }

    private static void PlayerThrowNoKb(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdindR4()))
        {
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(80f)))
            {
                //Logger.Log(LogLevel.Info, "FemtoHelper/EvilTheoCrystal", $"doing the things at {cursor.Index} in CIL code for {cursor.Method.FullName}");

                cursor.Emit(OpCodes.Ldarg_0);

                cursor.EmitDelegate(Upthrowcheck);
                cursor.Emit(OpCodes.Mul);
            }

        }
    }
    private static float Upthrowcheck(Player player)
    {
        if (player is { Holding.Entity: EvilTheoCrystal } && Input.Aim.Value.Y < 0f)
        {
            return 0f;
        }
        return 1f;
    }
}

