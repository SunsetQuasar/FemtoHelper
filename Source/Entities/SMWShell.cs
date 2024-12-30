using System;
using System.Collections;
using static Celeste.TrackSpinner;

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

    public SMWShell(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        speed = Vector2.Zero;

        Collider = new Hitbox(12f, 9f, -6f, -2f);
        Add(new PlayerCollider(OnPlayer));

        Add(new PlayerCollider(OnPlayerBonk, new Hitbox(12f, 7f, -6f, -9f)));

        Depth = -10;
        Add(hold = new SMWHoldable());

        string prefix = data.Attr("texturesPrefix", "objects/FemtoHelper/SMWShell/");
        string key = data.Attr("sprite", "green");

        splash = GFX.Game[$"{prefix}splash"];
        doNotSplash = !data.Bool("doSplashEffect", true);
        doFreezeFrames = data.Bool("freezeFrames", true);
        audioPath = data.Attr("audioPath", "event:/FemtoHelper/");

        Add(sprite = new Sprite(GFX.Game, prefix));

        sprite.AddLoop("idle", $"{key}_idle", 15f);
        sprite.AddLoop("kicked", $"{key}_kicked", 0.1f);

        sprite.Play("idle");

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
    }

    private void OnPlayerBonk(Player p)
    {
        if (state == States.Dropped && !(Input.Grab.Check && p.StateMachine.state != Player.StClimb)) TouchKick(p);
        if (state != States.Kicked || dontKillTimer > 0) return;
        if (doFreezeFrames) Celeste.Freeze(0.05f);
        Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        p.Bounce(Top);
        Drop();
        Splash(TopCenter - Vector2.UnitY * 6);
        Audio.Play($"{audioPath}enemykill", Center);
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
        else if (!(Input.Grab.Check && p.StateMachine.state != Player.StClimb))
        {
            TouchKick(p);
        }
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
        sprite.Play("kicked");
        Audio.Play($"{audioPath}enemykill", Center);
    }

    private void OnClipDeath(Vector2 force)
    {
        Die(force.X);
    }


    private void Die(float f)
    {
        Splash(Center);
        Audio.Play($"{audioPath}enemykill", Center);
        Collidable = false;
        state = States.Dead;
        speed.Y = -120;
        speed.X = f * -Calc.Random.Range(100, 200);
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
            if (OnGround())
            {
                float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
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
            MoveH(speed.X * Engine.DeltaTime, onCollideH);
            MoveV(speed.Y * Engine.DeltaTime, onCollideV);
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            TempleGate templeGate = CollideFirst<TempleGate>();
            if (templeGate != null && entity != null)
            {
                templeGate.Collidable = false;
                MoveH((float)(Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime);
                templeGate.Collidable = true;
            }
        }
        hold.CheckAgainstColliders();
    }

    private void OnCollideH(CollisionData data)
    {
        if (TrySquishWiggle(data)) return;
        speed.X *= state == States.Kicked ? -1 : -0.5f;
        Audio.Play($"{audioPath}blockhit", Center);
        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(speed.X));
        }
    }

    private void OnCollideV(CollisionData data)
    {
        if (TrySquishWiggle(data)) return;
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
    }

    private void Drop()
    {
        state = States.Dropped;
        sprite.Play("idle");
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
                    Audio.Play($"{audioPath}enemykill", Center);
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
        speed = force * new Vector2(240, 100);
        RemoveTag(Tags.Persistent);
        Position = new(MathF.Round(Position.X), MathF.Round(Position.Y));
        dontTouchKickTimer = 0.15f;
    }

    public override void Render()
    {
        base.Render();
    }
}