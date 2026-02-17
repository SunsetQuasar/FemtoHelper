using Celeste.Mod.FemtoHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.DreamStars;
using static Celeste.Mod.FemtoHelper.Entities.EntityKillZone;
using static Celeste.TrackSpinner;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/SMWShell")]
[Tracked]
public class SmwShell : Actor
{

    public class BounceDisplay : Entity
    {
        SmwShell parent;

        public BounceDisplay(SmwShell parent) : base(Vector2.Zero)
        {
            this.parent = parent;
            //AddTag(TagsExt.SubHUD);
            if (parent.displayConfig == BounceCountDisplay.Hud) AddTag(TagsExt.SubHUD);
            Depth = parent.Depth;
        }

        public override void Update()
        {
            base.Update();
            if(parent == null || Scene.Entities.removing.Contains(parent))
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            base.Render();
            Vector2 pos = parent.gravityState == 0 ? (parent.TopCenter - (Vector2.UnitY * 12)) : (parent.BottomCenter + (Vector2.UnitY * 12));

            if (parent.displayConfig == BounceCountDisplay.SpriteText)
            {
                int i = 0;
                string str = parent.bounceCount == 0 ? parent.initialBounceCount.ToString() : parent.bounceCount.ToString();

                float totalWidth = 0;
                foreach (char c in str)
                {
                    totalWidth += parent.digits.TryGetValue(c, out MTexture tex) ? tex.Width : 0;
                }

                Vector2 start = pos - new Vector2(totalWidth / 2, 0);

                foreach (char c in str)
                {
                    if (parent.digits.TryGetValue(c, out MTexture tex))
                    {
                        tex.Draw((start + new Vector2(i, -0.5f * (float)(tex.Height))).Floor(), Vector2.Zero, parent.bounceCount == 0 ? Color.DimGray : Color.White);
                        i += tex.Width;
                    }
                }
            }
            else if (parent.displayConfig == BounceCountDisplay.Hud)
            {
                if (parent.bounceCount == 0)
                {
                    ActiveFont.Draw(parent.initialBounceCount.ToString(), (pos - (Scene as Level).Camera.Position) * 6, new Vector2(0.5f, 0.5f), Vector2.One, Color.DimGray, 2, Color.Black, 2, Color.Black);
                }
                else
                {
                    ActiveFont.Draw(parent.bounceCount.ToString(), (pos - (Scene as Level).Camera.Position) * 6, new Vector2(0.5f, 0.5f), Vector2.One, Color.White, 2, Color.Black, 2, Color.Black);
                }
            }
        }
    }
    public class SplashEffect : Entity
    {
        internal Image img;
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

    internal enum States
    {
        Dropped = 0,
        Kicked = 1,
        Dead = 2
    }

    internal enum TouchKickBehaviors
    {
        Normal = 0,
        Ignore = 1,
        Kill = 2
    }

    internal enum BounceCountDisplay
    {
        None = 0,
        Hud = 1,
        SpriteText = 2
    }

    internal enum OutlineTextureType
    {
        None = 0,
        Black = 1,
        White = 2,
        Tiny = 3
    }

    internal BounceCountDisplay displayConfig;

    internal States state = States.Dropped;

    internal SmwHoldable hold;
    internal Vector2 speed;
    internal readonly Collision onCollideH;
    internal readonly Collision onCollideV;
    internal float noGravityTimer;
    internal Vector2 prevLiftSpeed;
    internal float prevLiftSpeedTimer;

    internal int bounceCount = 0;
    internal readonly int initialBounceCount;

    internal float dontKillTimer;
    internal float dontTouchKickTimer;
    internal readonly float noInteractionDuration;

    internal readonly float gravity;

    internal readonly Sprite sprite;

    internal readonly MTexture splash;
    internal readonly bool doNotSplash;
    internal readonly bool doFreezeFrames;
    internal readonly string audioPath;

    internal readonly bool isDisco;
    internal readonly string[] animations;
    internal int currentSpriteIndex;
    internal string currentSpriteTrimmedName;
    internal float discoTarget;
    internal readonly bool ignoreOtherShells;

    internal readonly bool canBeBouncedOn;
    internal readonly TouchKickBehaviors touchKickBehavior;
    internal readonly bool isCarriable;

    internal readonly float shellSpeed;
    internal readonly float discoSpeed;
    internal readonly float discoAcceleration;
    internal readonly float airFriction;
    internal readonly float groundFriction;
    internal readonly float maxFallSpeed;
    internal readonly float upwardsThrowSpeed;
    internal readonly bool idleActivateTouchSwitches;
    internal readonly bool capSpeed;
    internal readonly bool dontRefill;

    internal readonly bool discoSleep;

    internal bool playerHasMoved = false;

    internal Coroutine pMoveRoutine;

    internal Component gravityListener;
    internal int gravityState = 0;

    internal PlayerCollider bonkCollider;

    internal Dictionary<char, MTexture> digits;

    internal BounceDisplay counter;

    internal readonly float discoSpriteRate;

    internal bool bubble;

    internal readonly float downwardsLeniencySpeed;

    internal bool useFixedThrowSpeeds;
    internal float fixedNeutralThrowSpeed;
    internal float fixedForwardThrowSpeed;

    internal OutlineTextureType outlineTextureType;

    internal float playerThrownUpTimer;

    public static readonly MTexture OutlineTextureBlack = GFX.Game["objects/FemtoHelper/SMWShell/outline_black"];

    public static readonly MTexture OutlineTextureWhite = GFX.Game["objects/FemtoHelper/SMWShell/outline_white"];

    public static readonly MTexture OutlineTextureTiny = GFX.Game["objects/FemtoHelper/SMWShell/outline_tiny"];

    public readonly bool OneUse;
    public readonly float BounceSpeedMultiplier;
    public readonly float BounceLengthMultiplier;

    public SmwShell(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Position.Y++;   //let's pretend the placement doesn't spawn the shell 1px above the ground
        speed = Vector2.Zero;

        bool legacy = data.Bool("legacy", true);

        Collider = legacy ? new Hitbox(12f, 9f, -6f, -2f) : new Hitbox(12f, 12f, -6f, -5f);
        Add(new PlayerCollider(OnPlayer));

        Add(bonkCollider = new PlayerCollider(OnPlayerBonk, legacy ? new Hitbox(12f, 7f, -6f, -9f) : new Hitbox(12f, 15f, -6f, -8f)));

        Depth = -10;
        Add(hold = new SmwHoldable(data.Int("holdYOffset", -12), data.Int("holdYCrouchOffset", -8)));

        string prefix = data.Attr("texturesPrefix", "objects/FemtoHelper/SMWShell/");

        string digs = "0123456789";

        digits = [];

        foreach (char c in digs)
        {
            digits.Add(c, GFX.Game[$"{prefix}digit{c}"]);
        }

        splash = GFX.Game[$"{prefix}splash"];
        doNotSplash = !data.Bool("doSplashEffect", true);
        doFreezeFrames = data.Bool("freezeFrames", true);
        audioPath = data.Attr("audioPath", "event:/FemtoHelper/");
        ignoreOtherShells = data.Bool("ignoreOtherShells", false);
        canBeBouncedOn = data.Bool("canBeBouncedOn", true);
        touchKickBehavior = data.Enum("touchKickBehavior", TouchKickBehaviors.Normal);

        shellSpeed = data.Float("shellSpeed", 200);
        discoSpeed = data.Float("discoSpeed", 120);
        discoAcceleration = data.Float("discoAcceleration", 700);
        gravity = data.Float("gravity", 800);
        airFriction = data.Float("airFriction", 250);
        groundFriction = data.Float("groundFriction", 800);
        maxFallSpeed = data.Float("maxFallSpeed", 200f);
        upwardsThrowSpeed = data.Float("upwardsThrowSpeed", -300);
        idleActivateTouchSwitches = data.Bool("idleActivateTouchSwitches", true);
        discoSleep = data.Bool("discoSleep", false);
        capSpeed = data.Bool("capSpeed", true);
        dontRefill = data.Bool("dontRefill", false);

        initialBounceCount = data.Int("bounceCount", 1);
        displayConfig = data.Enum("bounceCountDisplay", BounceCountDisplay.SpriteText);
        discoSpriteRate = data.Float("discoSpriteRate", 50f);

        downwardsLeniencySpeed = data.Float("downwardsLeniencySpeed", -1);

        useFixedThrowSpeeds = data.Bool("useFixedThrowSpeeds", false);
        fixedNeutralThrowSpeed = data.Float("fixedNeutralThrowSpeed", 182);
        fixedForwardThrowSpeed = data.Float("fixedForwardThrowSpeed", 182);

        noInteractionDuration = data.Float("noInteractionDuration", 0.1f);

        outlineTextureType = data.Enum("outlineTextureType", OutlineTextureType.None);

        BounceSpeedMultiplier = data.Float("bounceSpeedMultiplier", 1f);
        BounceLengthMultiplier = data.Float("bounceLengthMultiplier", 1f);

        bubble = data.Bool("bubble", false);

        Add(sprite = new Sprite(GFX.Game, prefix));

        if (isDisco = data.Bool("disco", false))
        {
            animations = data.Attr("discoSprites", "yellow,blue,red,green,teal,gray,gold,gray").Split(',');
            foreach (string s in animations)
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
            string key = data.Attr("mainSprite", "green");
            animations = [key];
            sprite.AddLoop($"idle_{key}", $"{key}_idle", 15f);
            sprite.AddLoop($"kicked_{key}", $"{key}_kicked", 0.06f);
            currentSpriteIndex = 0;
            ChangeSprite("idle");
        }

        Add(new Coroutine(DiscoRoutine()));
        Add(pMoveRoutine = new Coroutine(PlayerMovedCheck()));

        sprite.RenderPosition -= new Vector2(sprite.Width / 2, sprite.Height / 2);

        hold.PickupCollider = legacy ? new Hitbox(20f, 14f, -10f, -2f) : new Hitbox(20f, 20f, -10f, -8f);
        hold.SlowFall = false;
        hold.SlowRun = false;
        hold.OnPickup = OnPickup;
        hold.OnRelease = OnRelease;
        hold.SpeedGetter = () => speed;
        hold.OnHitSpring = HitSpring;
        hold.SpeedSetter = (spd) =>
        {
            if (capSpeed && state == States.Kicked) speed = new Vector2(Math.Min(isDisco ? discoSpeed : shellSpeed, spd.X), spd.Y);
            else speed = spd;
        };

        if (!(isCarriable = data.Bool("carriable", true)))
        {
            hold.cannotHoldTimer = 0.02f;
        }

        onCollideH = OnCollideH;
        onCollideV = OnCollideV;
        hold.OnClipDeath = OnClipDeath;

        Add(new HoldableCollider(OnAnotherShell));

        gravityListener = FemtoModule.GravityHelperSupport.CreateGravityListener?.Invoke(this, OnGravityChange);
        if (gravityListener != null) Add(gravityListener);

        OneUse = data.Bool("oneUse", false);
    }

    public void OnGravityChange(Entity self, int newValue, float momentumMultiplier)
    {
        gravityState = newValue;
        if (newValue == 0) //normal gravity
        {
            sprite.Position.Y = -8f;
            bonkCollider.Collider.Position.Y = -8f;
        }
        else // inverted
        {
            sprite.Position.Y = 8f;
            bonkCollider.Collider.Position.Y = 1f;
        }

    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (initialBounceCount > (OneUse ? 0 : 1)) Scene.Add(counter = new BounceDisplay(this));
    }

    internal IEnumerator PlayerMovedCheck()
    {
        while (true)
        {
            Player p = Scene.Tracker.GetEntity<Player>();
            if (p != null && p.Speed != Vector2.Zero && p.StateMachine.state != Player.StIntroRespawn)
            {
                playerHasMoved = true;
                break;
            }
            yield return null;
        }
    }

    internal void OnPlayerBonk(Player p)
    {
        if (!canBeBouncedOn)
        {
            if (state == States.Kicked)
            {
                if (dontKillTimer <= 0)
                {
                    if (p.Speed.Y >= 0 && p.Bottom < Bottom - 2)
                    {
                        return;
                    }
                    else if (!(downwardsLeniencySpeed >= 0 && p.Speed.LengthSquared() >= downwardsLeniencySpeed * downwardsLeniencySpeed && p.Speed.Y > 0))
                    {
                        p.Die((Position - p.Center).SafeNormalize());
                    }
                }
            }
            else if ((!(Input.Grab.Check && !p.Ducking && isCarriable) || p.StateMachine.state == Player.StClimb || p.Holding != null) && p.Holding != hold)
            {
                TouchKick(p);
            }
            return;
        }

        if (isDisco && dontKillTimer <= 0)
        {
            if (bubble) UnBubble();
            p.PointBounceMaybeRefill(Center, !dontRefill);
            p.Speed *= new Vector2(0.5f, 0.75f * BounceSpeedMultiplier);
            p.varJumpSpeed = p.Speed.Y;
            p.varJumpTimer *= 1.25f * BounceLengthMultiplier;
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
        p.BounceMaybeRefill(gravityState == 0 ? Top : Bottom, !dontRefill);
        p.Speed.Y *= BounceSpeedMultiplier;
        p.varJumpSpeed = p.Speed.Y;
        p.varJumpTimer *= 1.25f * BounceLengthMultiplier;
        bounceCount--;
        if (bounceCount == 0)
        {
            hold.cannotHoldTimer = 0.2f;
            Drop();
            if (OneUse)
            {
                Audio.Play("event:/FemtoHelper/stomp_poof", Center);

                Scene.Add(new Poof(Center));

                SceneAs<Level>().ParticlesFG.Emit(EntityKillZone.Stars, Center, (-22.5f).ToRad());
                SceneAs<Level>().ParticlesFG.Emit(EntityKillZone.Stars, Center, (22.5f).ToRad());
                SceneAs<Level>().ParticlesFG.Emit(EntityKillZone.Stars, Center, (180 - 22.5f).ToRad());
                SceneAs<Level>().ParticlesFG.Emit(EntityKillZone.Stars, Center, (180 + 22.5f).ToRad());

                RemoveSelf();
                return;
            }
        }
        else dontKillTimer = 0.15f;
        Splash(gravityState != 0 ? (BottomCenter + Vector2.UnitY * 6) : (TopCenter - Vector2.UnitY * 6));
        Audio.Play($"{audioPath}enemykill", Center);
    }

    internal void OnAnotherShell(Holdable h)
    {
        if (h is not SmwHoldable holdable) return;
        if (holdable.Entity is not SmwShell otherShell) return;
        if (otherShell.ignoreOtherShells || ignoreOtherShells || otherShell.state == States.Dead || state == States.Dead) return;

        float dir = CenterX > otherShell.CenterX ? 1 : CenterX == otherShell.CenterX ? 0 : -1;

        if (otherShell.speed.LengthSquared() > 0)
        {
            if (speed.LengthSquared() > 0) otherShell.Die(-dir);
            Die(dir);
        }
        else
        {
            if (speed.LengthSquared() <= 0) Die(dir);
            otherShell.Die(-dir);
        }
        if (hold.IsHeld) Die(dir);
    }

    internal void OnPlayer(Player p)
    {
        if (state == States.Kicked)
        {
            if (dontKillTimer <= 0)
            {
                if (p.Speed.Y >= 0 && p.Bottom < Bottom - 2)
                {
                    return;
                }
                else if (!(downwardsLeniencySpeed >= 0 && p.Speed.LengthSquared() >= downwardsLeniencySpeed * downwardsLeniencySpeed && p.Speed.Y > 0))
                {
                    p.Die((Position - p.Center).SafeNormalize());
                }
            }
        }
        else if ((!(Input.Grab.Check && !p.Ducking && isCarriable) || p.StateMachine.state == Player.StClimb || p.Holding != null) && p.Holding != hold)
        {
            TouchKick(p);
        }
    }

    internal IEnumerator DiscoRoutine()
    {
        while (true)
        {
            yield return 1 / (discoSpriteRate * (Settings.Instance.DisableFlashes ? 0.25f : 1f));
            currentSpriteIndex = Mod(currentSpriteIndex + 1, animations.Length);
            ChangeSprite($"{currentSpriteTrimmedName}", true);
        }
    }

    internal void ChangeSprite(string path, bool keep = false)
    {
        sprite.PlayDontRestart($"{path}_{animations[currentSpriteIndex]}", !keep);
        currentSpriteTrimmedName = path;
    }

    internal void TouchKick(Player p)
    {
        if (touchKickBehavior == TouchKickBehaviors.Kill)
        {
            p.Die((Position - p.Center).SafeNormalize());
            return;
        }
        if (dontTouchKickTimer > 0 || touchKickBehavior == TouchKickBehaviors.Ignore) return;
        float kickSpeed = 0;
        if (p.CenterX > CenterX)
        {
            kickSpeed = -shellSpeed;
        }
        else if (p.CenterX < CenterX)
        {
            kickSpeed = shellSpeed;
        }
        else
        {
            if (p.Facing == Facings.Left)
            {
                kickSpeed = -shellSpeed;
            }
            else
            {
                kickSpeed = shellSpeed;
            }
        }
        if (doFreezeFrames) Celeste.Freeze(0.05f);
        Kick(new Vector2(1 * kickSpeed, speed.Y));
    }

    internal void Kick(Vector2? spd = null)
    {
        if (bubble) UnBubble();
        if (spd != null) hold.SetSpeed(Vector2.One * spd ?? Vector2.Zero);
        state = States.Kicked;
        bounceCount = initialBounceCount;
        dontKillTimer = noInteractionDuration;
        hold.cannotHoldTimer = 0.02f;
        ChangeSprite("kicked");
        Audio.Play($"{audioPath}shellkick", Center);
        LiftSpeed = prevLiftSpeed = Vector2.Zero;
    }

    public void StunFlip(Vector2 direction)
    {
        Splash(Center);
        if (Math.Abs(direction.X) > 0)
        {
            Kick(direction * shellSpeed);
        }
        else
        {
            if (bubble) UnBubble();
            hold.SetSpeed(direction * 240f);
            sprite.FlipY = true;
            Audio.Play($"{audioPath}enemykill", Center);
            LiftSpeed = prevLiftSpeed = Vector2.Zero;
        }
    }

    internal void OnClipDeath(Vector2 force)
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

    internal void Die(float f)
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
        speed.Y = -Calc.Random.Range(100, 120);
        speed.X = f * -Calc.Random.Range(90, 110);
        hold.cannotHoldTimer = 0.02f;
        Depth = -10000;
        sprite.FlipY = true;
    }

    internal void Splash(Vector2 pos)
    {
        if (doNotSplash) return;
        Scene.Add(new SplashEffect(pos, splash));
    }

    public override void Update()
    {
        base.Update();

        if (state == States.Dead)
        {
            speed.Y = Calc.Approach(speed.Y, maxFallSpeed * 3, 400f * Engine.DeltaTime);
            speed.X = Calc.Approach(speed.X, 0, 100f * Engine.DeltaTime);
            Position += speed * Engine.DeltaTime;
            hold.cannotHoldTimer = 0.02f;
            if (Position.Y > SceneAs<Level>().Bounds.Bottom + 32) RemoveSelf();
            return;
        }
        if (state == States.Kicked || !isCarriable)
        {
            hold.cannotHoldTimer = 0.02f;
        }
        if (prevLiftSpeedTimer > 0)
        {
            prevLiftSpeedTimer -= Engine.DeltaTime;
        }
        else
        {
            prevLiftSpeed = Vector2.Zero;
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
        if (playerThrownUpTimer > 0)
        {
            playerThrownUpTimer -= Engine.DeltaTime;
        }
        if (hold.IsHeld)
        {
            prevLiftSpeed = Vector2.Zero;
        }
        else
        {
            Level level = Scene as Level;
            if (Left < (float)level.Bounds.Left)
            {
                Left = level.Bounds.Left;
                OnCollideH(new CollisionData
                {
                    Direction = -Vector2.UnitX
                });
            }
            else if (Right > (float)level.Bounds.Right)
            {
                Right = level.Bounds.Right;
                OnCollideH(new CollisionData
                {
                    Direction = Vector2.UnitX
                });
            }
            else if (Top > (float)(level.Bounds.Bottom + 16))
            {
                RemoveSelf();
                return;
            }
            TreatNaive = false;
            if (isDisco)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && (!discoSleep || playerHasMoved))
                {
                    if (player.CenterX > CenterX)
                    {
                        discoTarget = 1f;
                    }
                    else
                    {
                        discoTarget = -1f;
                    }
                }
                speed.X = Calc.Approach(speed.X, discoSpeed * discoTarget, discoAcceleration * Engine.DeltaTime);
                if (!OnGround() && !bubble && hold.ShouldHaveGravity)
                {
                    float num = gravity;
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
                        speed.Y = Calc.Approach(speed.Y, maxFallSpeed, num * Engine.DeltaTime);
                    }
                }
            }
            else
            {
                if (OnGround())
                {
                    float target = (!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f));
                    if (state != States.Kicked) speed.X = Calc.Approach(speed.X, target, groundFriction * Engine.DeltaTime);
                    Vector2 liftSpeed = LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                    {
                        speed.Y = prevLiftSpeed.Y;
                        if (Math.Abs(prevLiftSpeed.X) > Math.Abs(speed.X)) speed.X = prevLiftSpeed.X;
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
                        prevLiftSpeedTimer = 0.18f;
                        if (liftSpeed.Y < 0f && speed.Y < 0f)
                        {
                            speed.Y = 0f;
                        }
                    }
                }
                else if (hold.ShouldHaveGravity && !bubble)
                {
                    float num = gravity;
                    if (Math.Abs(speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    float num2 = airFriction;
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
                        speed.Y = Calc.Approach(speed.Y, maxFallSpeed, num * Engine.DeltaTime);
                    }
                }
            }
            MoveH(speed.X * Engine.DeltaTime, onCollideH);
            MoveV(speed.Y * Engine.DeltaTime, onCollideV);
            if (state != States.Dropped || idleActivateTouchSwitches)
            {
                foreach (TouchSwitch entity2 in Scene.Tracker.GetEntities<TouchSwitch>())
                {
                    if (CollideCheck(entity2))
                    {
                        entity2.TurnOn();
                    }
                }
            }
        }
        hold.CheckAgainstColliders();
    }

    internal void OnCollideH(CollisionData data)
    {
        if (speed.Y == 0f && speed.X != 0f)
        {
            for (int i = 1; i <= 4; i++)
            {
                for (int num = 1; num >= -1; num -= 2)
                {
                    Vector2 vector = new(Math.Sign(speed.X), i * num);
                    Vector2 vector2 = Position + vector;
                    if (!CollideCheck<Solid>(vector2) && CollideCheck<Solid>(vector2 - Vector2.UnitY * num) /*&& !DashCorrectCheck(vector)*/) // probably don't care about ledge blockers
                    {
                        MoveVExact(i * num);
                        MoveHExact(Math.Sign(speed.X));
                        return;
                    }
                }
            }
        }

        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(speed.X));
        }
        speed.X *= state == States.Kicked ? -1 : -0.5f;
        Audio.Play($"{audioPath}blockhit", Center);
    }

    internal void OnCollideV(CollisionData data)
    {
        if (speed.Y < 0f)
        {
            int num3 = 4;
            if (speed.X <= 0.01f)
            {
                for (int j = 1; j <= num3; j++)
                {
                    if (!CollideCheck<Solid>(Position + new Vector2(-j, -1f)))
                    {
                        Position += new Vector2(-j, -1f);
                        return;
                    }
                }
            }
            if (speed.X >= -0.01f)
            {
                for (int k = 1; k <= num3; k++)
                {
                    if (!CollideCheck<Solid>(Position + new Vector2(k, -1f)))
                    {
                        Position += new Vector2(k, -1f);
                        return;
                    }
                }
            }
        }

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

    internal bool HitSpring(Spring spring)
    {
        if (hold.IsHeld) return false;
        switch (spring.Orientation)
        {
            case Spring.Orientations.Floor when speed.Y >= 0f:
                if (state != States.Kicked) speed.X *= 0.5f;
                speed.Y = -300f;
                return true;
            case Spring.Orientations.WallLeft when speed.X <= 0f:
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = isDisco ? discoSpeed : shellSpeed;
                speed.Y = Math.Min(speed.Y, -160f);
                return true;
            case Spring.Orientations.WallRight when speed.X >= 0f:
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = isDisco ? -discoSpeed : -shellSpeed;
                speed.Y = Math.Min(speed.Y, -160f);
                return true;
            default:
                return false;
        }
    }

    internal void OnPickup()
    {
        if (bubble) UnBubble();
        hold.SetSpeed(Vector2.Zero);
        Drop();
        AddTag(Tags.Persistent);
        if (counter != null) counter.AddTag(Tags.Persistent);
        TreatNaive = true;
    }

    internal void Drop()
    {
        state = States.Dropped;
        ChangeSprite("idle");
        dontTouchKickTimer = 0.15f;
    }

    internal void OnRelease(Vector2 force)
    {
        Player player = Scene.Tracker.GetNearestEntity<Player>(Position);
        if (player == null) return;

        var shouldKick = false;
        var throwSpeed = Vector2.Zero;

        #region calculate the throw speed
        if (force is { X: 0, Y: 0 })
        {
            // drop
            throwSpeed.X = player.Speed.X * 2f / 3f;
        }
        else if (force.Y == 0 && Input.Aim.Value.Y < 0f)
        {
            // up throw
            playerThrownUpTimer = 4f;
            Splash(Center);
            Audio.Play($"{audioPath}shellkick", Center);
            throwSpeed.X = player.Speed.X * 2f / 3f;
            throwSpeed.Y = upwardsThrowSpeed;
        }
        else if (useFixedThrowSpeeds)
        {
            // sideways throws (using fixed speed setting)
            if (Input.Aim.Value.X == 0)
            {
                // neutral throw
                throwSpeed.X = fixedNeutralThrowSpeed * (float)player.Facing;
            }
            else
            {
                // forward throw
                throwSpeed.X = fixedForwardThrowSpeed * (float)player.Facing;
            }
            shouldKick = true;
        }
        else
        {
            // sideways throws (using momentum based setting)
            if ((float)player.Facing != Math.Sign(player.Speed.X))
            {
                // neutral throws & backshots
                throwSpeed.X = (float)player.Facing * shellSpeed;
            }
            else
            {
                // forward throw
                throwSpeed.X = (Math.Sign(player.Speed.X) * shellSpeed) + (player.Speed.X / 2f);
            }
            shouldKick = true;
        }
        #endregion

        if (shouldKick)
        {
            Splash(Center);
            Kick(throwSpeed);
        }
        else
        {
            hold.cannotHoldTimer = 0.2f;
            hold.SetSpeed(throwSpeed);
            dontTouchKickTimer = 0.15f;
        }
        RemoveTag(Tags.Persistent);
        if (counter != null) counter.RemoveTag(Tags.Persistent);
        Position = new(MathF.Round(Position.X), MathF.Round(Position.Y));
        TreatNaive = false;

        foreach (JumpThru entity in base.Scene.Tracker.GetEntities<JumpThru>())
        {
            if (CollideCheck(entity) && base.Bottom - entity.Top <= 6f)
            {
                MoveVExact((int)(entity.Top - base.Bottom));
            }
        }
    }

    public override void Render()
    {
        base.Render();
        if (bubble)
        {
            for (int i = 0; i < 24; i++)
            {
                Draw.Point(Position + PlatformAdd(i), PlatformColor(i));
            }
        }
        if (outlineTextureType != OutlineTextureType.None && playerThrownUpTimer > 0)
        {
            var camera = SceneAs<Level>().Camera;
            if (Bottom < camera.Top)
            {
                var distance = camera.Top - Bottom;
                var texture = outlineTextureType == OutlineTextureType.Black ? OutlineTextureBlack : outlineTextureType == OutlineTextureType.White ? OutlineTextureWhite : OutlineTextureTiny;
                texture.Draw(new Vector2(Left - 3, camera.Top - (distance / 16)).Floor());
            }
        }
    }

    internal Vector2 PlatformAdd(int num)
    {
        return new Vector2(-12 + num, 2 + (int)Math.Round(Math.Sin(Scene.TimeActive + (float)num * 0.2f) * 1.7999999523162842));
    }

    internal Color PlatformColor(int num)
    {
        if (num <= 1 || num >= 22)
        {
            return Color.White * 0.4f;
        }
        return Color.White * 0.8f;
    }

    internal void UnBubble()
    {
        for (int i = 0; i < 24; i++)
        {
            SceneAs<Level>().Particles.Emit(Glider.P_Platform, Position + PlatformAdd(i), PlatformColor(i));
        }
        bubble = false;
    }

    internal int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}