using Celeste.Mod.FemtoHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/SMWShell")]
[Tracked]
public class SMWShell : Actor
{
    public class SplashEffect : Entity
    {
        private Image img;
        public SplashEffect(Vector2 pos, MTexture texture) : base(pos)
        {
            Add(img = new Image(texture));
            img.JustifyOrigin(new(0.5f, 0.5f));
            Add(Alarm.Set(this, 0.20f, RemoveSelf));
            Depth = -25000;
            Add(new Coroutine(Routine()));
        }

        public override void Update()
        {
            base.Update();
        }

        public IEnumerator Routine()
        {
            while (true)
            {
                yield return 0.03f;
                img.FlipX = !img.FlipX;
            }
        }
    }

    private enum States
    {
        Dropped = 0,
        Kicked = 1,
        Dead = 2
    }

    private States state = States.Dropped;

    private SMWHoldable hold;
    private Vector2 speed;
    private readonly Collision onCollideH;
    private readonly Collision onCollideV;
    private float noGravityTimer;
    private Vector2 prevLiftSpeed;

    private float dontKillTimer;
    private float dontTouchKickTimer;

    private const float HorizontalFriction = 60f;
    private const float MaxFallSpeed = 200f;
    private const float Gravity = 450f;

    private readonly Sprite sprite;

    private readonly MTexture splash;
    private readonly bool doNotSplash;
    private readonly bool doFreezeFrames;
    private readonly string audioPath;

    private readonly bool isDisco;
    private readonly string[] Animations;
    private int currentSpriteIndex;
    private string currentSpriteTrimmedName;
    private float discoTarget;
    private bool ignoreOtherShells;

    public SMWShell(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        speed = Vector2.Zero;

        Collider = new Hitbox(12f, 9f, -6f, -2f);
        Add(new PlayerCollider(OnPlayer));

        Add(new PlayerCollider(OnPlayerBonk, new Hitbox(12f, 7f, -6f, -9f)));

        Depth = -10;
        Add(hold = new SMWHoldable());

        string prefix = data.Attr("texturesPrefix", "objects/FemtoHelper/SMWShell/");

        splash = GFX.Game[$"{prefix}splash"];
        doNotSplash = !data.Bool("doSplashEffect", true);
        doFreezeFrames = data.Bool("freezeFrames", true);
        audioPath = data.Attr("audioPath", "event:/FemtoHelper/");
        ignoreOtherShells = data.Bool("ignoreOtherShells", false);

        Add(sprite = new Sprite(GFX.Game, prefix));

        if (isDisco = data.Bool("disco", false))
        {
            Animations = data.Attr("discoSprites", "yellow,blue,red,green,teal,gray,gold,gray").Split(',');
            foreach(string s in Animations)
            {
                sprite.AddLoop($"idle_{s}", $"{s}_idle", 15f);
                sprite.AddLoop($"kicked_{s}", $"{s}_kicked", 0.06f);
            }
            currentSpriteIndex = 0;
            ChangeSprite("kicked");
            state = States.Kicked;
            hold.cannotHoldTimer = 0.2f;
        }
        else
        {
            string key = data.Attr("sprite", "green");
            Animations = [key];
            sprite.AddLoop($"idle_{key}", $"{key}_idle", 15f);
            sprite.AddLoop($"kicked_{key}", $"{key}_kicked", 0.06f);
            currentSpriteIndex = 0;
            ChangeSprite("idle");
        }
        Add(new Coroutine(DiscoRoutine()));

        sprite.RenderPosition -= new Vector2(sprite.Width / 2, sprite.Height / 2);

        hold.PickupCollider = new Hitbox(20f, 14f, -10f, -2f);
        hold.SlowFall = false;
        hold.SlowRun = false;
        hold.OnPickup = OnPickup;
        hold.OnRelease = OnRelease;
        hold.SpeedGetter = () => speed;
        hold.OnHitSpring = HitSpring;
        hold.SpeedSetter = (spd) => speed = spd;

        onCollideH = OnCollideH;
        onCollideV = OnCollideV;
        hold.OnClipDeath = OnClipDeath;

        Add(new HoldableCollider(OnAnotherShell));
    }

    private void OnPlayerBonk(Player p)
    {
        if (isDisco && dontKillTimer <= 0)
        {
            p.PointBounce(Center);
            p.Speed *= new Vector2(0.5f, 0.75f);
            p.varJumpSpeed = p.Speed.Y;
            p.varJumpTimer = 0.25f;
            p.jumpGraceTimer = 0f;
            p.AutoJump = true;
            p.AutoJumpTimer = 0.1f;
            Audio.Play($"{audioPath}stomp_bounce", Center);
            Splash(TopCenter - Vector2.UnitY * 6);
            if (doFreezeFrames) Celeste.Freeze(0.05f);
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            dontKillTimer = 0.1f;
            return;
        }
        if (state == States.Dropped && (!Input.Grab.Check || p.StateMachine.state == Player.StClimb || p.Holding != null) && p.Holding != hold) TouchKick(p);
        if (state != States.Kicked || dontKillTimer > 0) return;
        if (doFreezeFrames) Celeste.Freeze(0.05f);
        Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        p.Bounce(Top);
        Drop();
        Splash(TopCenter - Vector2.UnitY * 6);
        Audio.Play($"{audioPath}enemykill", Center);
    }

    private void OnAnotherShell(Holdable h)
    {
        if (h is not SMWHoldable holdable) return;
        if (holdable.Entity is not SMWShell otherShell) return;
        if (otherShell.ignoreOtherShells || ignoreOtherShells || otherShell.state == States.Dead || state == States.Dead) return;

        float dir = CenterX > otherShell.CenterX ? 1 : CenterX == otherShell.CenterX ? 0 : -1;

        if(otherShell.speed.LengthSquared() > 0)
        {
            if (speed.LengthSquared() > 0) otherShell.Die(-dir);
            Die(dir);
        } else
        {
            if (speed.LengthSquared() <= 0) Die(dir);
            otherShell.Die(-dir);
        }
        if (hold.IsHeld) Die(dir);
    }

    private void OnPlayer(Player p)
    {
        if (state == States.Kicked)
        {
            if (dontKillTimer <= 0)
            {
                p.Die((Position - p.Center).SafeNormalize());
            }
        }
        else if ((!Input.Grab.Check || p.StateMachine.state == Player.StClimb || p.Holding != null) && p.Holding != hold)
        {
            TouchKick(p);
        }
    }

    private IEnumerator DiscoRoutine()
    {
        while (true)
        {
            yield return 0.02f;
            currentSpriteIndex = Mod(currentSpriteIndex + 1, Animations.Length);
            ChangeSprite($"{currentSpriteTrimmedName}", true);
        }
    }

    private void ChangeSprite(string path, bool keep = false)
    {
        sprite.PlayDontRestart($"{path}_{Animations[currentSpriteIndex]}", !keep);
        currentSpriteTrimmedName = path;
    }

    private void TouchKick(Player p)
    {
        if (dontTouchKickTimer > 0) return;
        float kickSpeed = 0;
        if (p.CenterX > CenterX)
        {
            kickSpeed = -200;
        }
        else if (p.CenterX < CenterX)
        {
            kickSpeed = 200;
        }
        else
        {
            if (p.Facing == Facings.Left)
            {
                kickSpeed = -200;
            }
            else
            {
                kickSpeed = 200;
            }
        }
        if (doFreezeFrames) Celeste.Freeze(0.05f);
        Kick(Vector2.UnitX * kickSpeed);
    }

    private void Kick(Vector2? spd = null)
    {
        if (spd != null) speed = Vector2.UnitX * spd ?? Vector2.Zero;
        state = States.Kicked;
        dontKillTimer = 0.1f;
        hold.cannotHoldTimer = 0.02f;
        ChangeSprite("kicked");
        Audio.Play($"{audioPath}shellkick", Center);
        base.LiftSpeed = prevLiftSpeed = Vector2.Zero;
    }

    private void OnClipDeath(Vector2 force)
    {
        Die(force.X);
    }

    public override void OnSquish(CollisionData data)
    {
        //base.OnSquish(data);
        if (!TrySquishWiggle(data, 3, 3))
        {
            Die(data.Direction.X);
        }
    }

    public override bool IsRiding(JumpThru jumpThru)
    {
        if (state == States.Dead) return false;
        return base.IsRiding(jumpThru);
    }

    public override bool IsRiding(Solid solid)
    {
        if (state == States.Dead) return false;
        return base.IsRiding(solid);
    }

    private void Die(float f)
    {
        if (state == States.Dead) return;
        state = States.Dead;
        if (hold.IsHeld)
        {
            if (hold.Holder is { } player)
            {
                player.Drop();
            }
        }
        Splash(Center);
        ChangeSprite("idle");
        Audio.Play($"{audioPath}enemykill", Center);
        Collidable = false;
        TreatNaive = true;
        speed.Y = -Calc.Random.Range(100,120);
        speed.X = f * -Calc.Random.Range(90, 110);
        hold.cannotHoldTimer = 0.02f;
        Depth = -10000;
        sprite.FlipY = true;
    }

    private void Splash(Vector2 pos)
    {
        if (doNotSplash) return;
        Scene.Add(new SplashEffect(pos, splash));
    }

    public override void Update()
    {
        base.Update();
        if (state == States.Dead)
        {
            speed.Y = Calc.Approach(speed.Y, MaxFallSpeed * 3, 400f * Engine.DeltaTime);
            speed.X = Calc.Approach(speed.X, 0, 100f * Engine.DeltaTime);
            Position += speed * Engine.DeltaTime;
            hold.cannotHoldTimer = 0.02f;
            if (Position.Y > SceneAs<Level>().Bounds.Bottom + 32) RemoveSelf();
            return;
        }
        if (state == States.Kicked)
        {
            hold.cannotHoldTimer = 0.02f;
        }
        if (dontKillTimer > 0)
        {
            dontKillTimer -= Engine.DeltaTime;
            if (hold.IsHeld) dontKillTimer = 0;
        }
        if (dontTouchKickTimer > 0)
        {
            dontTouchKickTimer -= Engine.DeltaTime;
            if (hold.IsHeld) dontTouchKickTimer = 0;
        }
        if (hold.IsHeld)
        {
            prevLiftSpeed = Vector2.Zero;
        }
        else
        {
            TreatNaive = false;
            if (isDisco)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if(player != null)
                {
                    if(player.CenterX > CenterX)
                    {
                        discoTarget = 1f;
                    } 
                    else
                    {
                        discoTarget = -1f;
                    }
                }
                speed.X = Calc.Approach(speed.X, 120 * discoTarget, 700 * Engine.DeltaTime);
                if (!OnGround() && hold.ShouldHaveGravity)
                {
                    float num = 800f;
                    if (Math.Abs(speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    if (noGravityTimer > 0f)
                    {
                        noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        speed.Y = Calc.Approach(speed.Y, MaxFallSpeed, num * Engine.DeltaTime);
                    }
                }
            }
            else
            {
                if (OnGround())
                {
                    float target = (!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f));
                    if (state != States.Kicked) speed.X = Calc.Approach(speed.X, target, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = base.LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                    {
                        speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        speed.Y = Math.Min(speed.Y * 0.6f, 0f);
                        if (speed.X != 0f && speed.Y == 0f)
                        {
                            speed.Y = -60f;
                        }
                        if (speed.Y < 0f)
                        {
                            noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && speed.Y < 0f)
                        {
                            speed.Y = 0f;
                        }
                    }
                }
                else if (hold.ShouldHaveGravity)
                {
                    float num = 800f;
                    if (Math.Abs(speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    float num2 = 250f;
                    if (speed.Y < 0f)
                    {
                        num2 *= 0.5f;
                    }
                    if (state != States.Kicked) speed.X = Calc.Approach(speed.X, 0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0f)
                    {
                        noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        speed.Y = Calc.Approach(speed.Y, MaxFallSpeed, num * Engine.DeltaTime);
                    }
                }
            }
            MoveH(speed.X * Engine.DeltaTime, onCollideH);
            MoveV(speed.Y * Engine.DeltaTime, onCollideV);

            foreach (TouchSwitch entity2 in base.Scene.Tracker.GetEntities<TouchSwitch>())
            {
                if (CollideCheck(entity2))
                {
                    entity2.TurnOn();
                }
            }
        }
        hold.CheckAgainstColliders();
    }

    private void OnCollideH(CollisionData data)
    {
        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(speed.X));
        }
        speed.X *= state == States.Kicked ? -1 : -0.5f;
        Audio.Play($"{audioPath}blockhit", Center);
    }

    private void OnCollideV(CollisionData data)
    {
        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(speed.Y));
        }
        if (Math.Abs(speed.Y) > 40 && state != States.Kicked)
        {
            speed.Y *= Math.Sign(speed.Y) == 1 ? -0.3f : -0.1f;
        }
        else
        {
            speed.Y = 0f;
        }
        if (state != States.Kicked) speed.X *= 0.5f;
    }

    private bool HitSpring(Spring spring)
    {
        if (hold.IsHeld) return false;
        switch (spring.Orientation)
        {
            case Spring.Orientations.Floor when speed.Y >= 0f:
                speed.X *= 0.5f;
                speed.Y = -160f;
                return true;
            case Spring.Orientations.WallLeft when speed.X <= 0f:
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = 160f;
                speed.Y = -80f;
                return true;
            case Spring.Orientations.WallRight when speed.X >= 0f:
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = -160f;
                speed.Y = -80f;
                return true;
            default:
                return false;
        }
    }

    private void OnPickup()
    {
        speed = Vector2.Zero;
        Drop();
        AddTag(Tags.Persistent);
        TreatNaive = true;
    }

    private void Drop()
    {
        state = States.Dropped;
        ChangeSprite("idle");
        dontTouchKickTimer = 0.15f;
    }

    private void OnRelease(Vector2 force)
    {
        bool kicked = true;
        Player player = Scene.Tracker.GetNearestEntity<Player>(Position);
        if (player == null) return;
        force.Y *= 0.5f;
        if (force.X != 0f)
        {
            if (force.Y == 0)
            {
                //force.Y = -0.4f;
                if (Input.Aim.Value.Y < 0f)
                {
                    Splash(Center);
                    Audio.Play($"{audioPath}shellkick", Center);
                    kicked = false;
                    force.Y = -3f;
                    force.X = player.Speed.X / 200f;
                }
            }
        }

        if (force is { X: 0, Y: 0 })
        {
            kicked = false;
            force.X = player.Speed.X / 400f;
            force.X += player.Facing == Facings.Right ? 0.25f : -0.25f;
        }
        if (kicked)
        {
            Splash(Center);
            Kick();
        }
        hold.cannotHoldTimer = 0.2f;
        speed = force * new Vector2(200, 100);
        RemoveTag(Tags.Persistent);
        Position = new(MathF.Round(Position.X), MathF.Round(Position.Y));
        dontTouchKickTimer = 0.15f;
        TreatNaive = false;
    }

    public override void Render()
    {
        base.Render();
    }

    private int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}