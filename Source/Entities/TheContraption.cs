using Iced.Intel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/TheContraption")]
[Tracked]
public class TheContraption : Actor
{
    [Pooled]
    public class ContraptionDebris : Actor
    {
        private Image image;

        private float lifeTimer;

        private float alpha;

        private Vector2 speed;

        private Collision collideH;

        private Collision collideV;

        private int rotateSign;

        private float fadeLerp;

        private bool playSound = true;

        private bool dreaming;

        private SineWave dreamSine;

        private bool hasHitGround;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ContraptionDebris()
            : base(Vector2.Zero)
        {
            base.Collider = new Hitbox(4f, 4f, -2f, -2f);
            base.Tag = Tags.Persistent;
            base.Depth = 2000;
            Add(image = new Image(null));
            collideH = OnCollideH;
            collideV = OnCollideV;
            Add(dreamSine = new SineWave(0.6f, 0f));
            dreamSine.Randomize();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Added(Scene scene)
        {
            base.Added(scene);
            dreaming = SceneAs<Level>().Session.Dreaming;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ContraptionDebris Init(Vector2 pos, string sprite, bool playSound = true)
        {
            ContraptionDebris debris = orig_Init(pos, sprite, playSound);
            debris.image.Texture = GFX.Game[sprite];
            return debris;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ContraptionDebris BlastFrom(Vector2 from)
        {
            float length = Calc.Random.Range(300, 400);
            speed = (Position - from).SafeNormalize(length);
            speed = speed.Rotate(Calc.Random.Range(-MathF.PI / 12f, MathF.PI / 12f));
            return this;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnCollideH(CollisionData data)
        {
            speed.X *= -0.8f;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnCollideV(CollisionData data)
        {
            if (speed.Y > 0f)
            {
                hasHitGround = true;
            }
            speed.Y *= -0.6f;
            if (speed.Y < 0f && speed.Y > -50f)
            {
                speed.Y = 0f;
            }
            if (speed.Y != 0f || !hasHitGround)
            {
                ImpactSfx(Math.Abs(speed.Y));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ImpactSfx(float spd)
        {
            if (playSound)
            {
                string path = "event:/game/general/debris_stone";
                Audio.Play(path, Position, "debris_velocity", Calc.ClampedMap(spd, 0f, 150f));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            base.Update();
            image.Rotation += Math.Abs(speed.X) * (float)rotateSign * Engine.DeltaTime;
            if (fadeLerp < 1f)
            {
                fadeLerp = Calc.Approach(fadeLerp, 1f, 2f * Engine.DeltaTime);
            }
            MoveH(speed.X * Engine.DeltaTime, collideH);
            MoveV(speed.Y * Engine.DeltaTime, collideV);
            if (dreaming)
            {
                speed.X = Calc.Approach(speed.X, 0f, 50f * Engine.DeltaTime);
                speed.Y = Calc.Approach(speed.Y, 6f * dreamSine.Value, 100f * Engine.DeltaTime);
            }
            else
            {
                bool flag = OnGround();
                speed.X = Calc.Approach(speed.X, 0f, (flag ? 50f : 20f) * Engine.DeltaTime);
                if (!flag)
                {
                    if (speed.Y < 100f) speed.Y = Calc.Approach(speed.Y, 300f, 400f * Engine.DeltaTime);
                }
            }
            if (lifeTimer > 0f)
            {
                lifeTimer -= Engine.DeltaTime;
            }
            else if (alpha > 0f)
            {
                alpha -= 4f * Engine.DeltaTime;
                if (alpha <= 0f)
                {
                    RemoveSelf();
                }
            }
            image.Color = Color.Lerp(Color.White, Color.Gray, fadeLerp) * alpha;
        }

        public ContraptionDebris Init(Vector2 pos, string sprite)
        {
            return Init(pos, sprite, playSound: true);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ContraptionDebris orig_Init(Vector2 pos, string sprite, bool playSound = true)
        {
            Position = pos;
            this.playSound = playSound;
            lifeTimer = Calc.Random.Range(0.6f, 2.6f);
            alpha = 1f;
            hasHitGround = false;
            speed = Vector2.Zero;
            fadeLerp = 0f;
            rotateSign = Calc.Random.Choose(1, -1);
            image.Texture = GFX.Game[sprite];
            image.CenterOrigin();
            image.Color = Color.White * alpha;
            image.Rotation = Calc.Random.NextAngle();
            image.Scale.X = Calc.Random.Range(0.5f, 1f);
            image.Scale.Y = Calc.Random.Range(0.5f, 1f);
            image.FlipX = Calc.Random.Chance(0.5f);
            image.FlipY = Calc.Random.Chance(0.5f);
            return this;
        }

        public override void Render()
        {
            image.DrawOutline(Color.Black * (image.Color.A / 255));
            base.Render();
        }
    }

    public class ChildSolid : Solid
    {
        public TheContraption parent;
        public ChildSolid(TheContraption parent, Vector2 pos, float width, float height) : base(pos, width, height, false)
        {
            SurfaceSoundIndex = 9;
            this.parent = parent;
            Depth = 5000;
            Collider.Position = new Vector2((0f - width) / 2f, 0f - height);
            OnDashCollide = OnDashed;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(TopLeft - Vector2.UnitY + Shake, Width, Height + 2, Color.Black);
            Draw.Rect(TopLeft - Vector2.UnitX + Shake, Width + 2, Height, Color.Black);
        }

        public DashCollisionResults OnDashed(Player player, Vector2 dir)
        {
            // Easier wall bounces. (stole from communal helper)
            if ((player.Left >= Right - 4f || player.Right < Left + 4f) && dir.Y == -1)
            {
                return DashCollisionResults.NormalCollision;
            }

            if (!(player.StateMachine.State != Player.StRedDash && player.StateMachine.State != Player.StSummitLaunch) || parent.Hold.IsHeld || (dir.Y == -1 && Input.Grab.Check) || parent.HitState)
            {
                return DashCollisionResults.NormalCollision;
            }
            parent.Hit(dir);
            return DashCollisionResults.Rebound;
        }

        public override void Update()
        {
            base.Update();
            Collidable = !parent.Hold.IsHeld;
        }

        public override void MoveHExact(int move)
        {
            parent.AllowPushing = false;
            base.MoveHExact(move);
            parent.AllowPushing = true;
        }

        public override void MoveVExact(int move)
        {
            parent.AllowPushing = false;
            base.MoveVExact(move);
            parent.AllowPushing = true;
        }
    }

    public bool HitState;

    public ChildSolid child;

    private Vector2 Speed;

    public Holdable Hold;

    private Vector2 prevLiftSpeed;
    private float prevLiftSpeedTimer;

    private readonly Collision onCollideH;
    private readonly Collision onCollideV;

    private float noGravityTimer;

    private float highFrictionTimer;

    private MTexture[,] edges = new MTexture[4, 4];
    private MTexture cog;
    private float cogRotation;

    private Vector2 lastPosition;

    private SoundSource sfx;

    private string spritePath;

    private static ParticleType Steam2 = new(ParticleTypes.Steam)
    {
        SpeedMin = 15,
        SpeedMax = 40,
        Acceleration = new Vector2(-4f, -12f),
        Friction = 10,
    };

    private Hitbox airHitbox => new Hitbox(Width, 8, -Width / 2, 0);
    private Hitbox groundHitbox => new Hitbox(Width + 4, 10, (-Width / 2) - 2, -5);

    private readonly bool particles;

    private readonly bool reducedDebris;

    public TheContraption(EntityData data, Vector2 offset) : base(data.Position + offset + new Vector2((float)data.Width / 2f, data.Height))
    {
        Depth = -5;
        child = new ChildSolid(this, Position, data.Width, data.Height);
        Collider = new Hitbox(data.Width, data.Height);
        Collider.Position = new Vector2((0f - base.Width) / 2f, 0f - base.Height);
        Add(Hold = new Holdable(0.3f)
        {
            PickupCollider = new Hitbox(Width, 8, -Width / 2, 0),
            SlowFall = true,
            SlowRun = false,
            OnPickup = OnPickup,
            OnRelease = OnRelease,
            SpeedGetter = () => Speed,
            OnCarry = OnCarry,
            OnHitSpring = HitSpring,
            SpeedSetter = (speed) => Speed = speed
        });

        spritePath = data.String("spritePath", "objects/Femtohelper/TheContraption/");

        MTexture slice = GFX.Game[$"{spritePath}nineSlice"];

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                edges[i, j] = slice.GetSubtexture(i * 8, j * 8, 8, 8);
            }
        }
        cog = GFX.Game[$"{spritePath}cog"];

        onCollideH = OnCollideH;
        onCollideV = OnCollideV;
        lastPosition = Position;
        sfx = new SoundSource();
        Add(sfx);
        sfx.Play("event:/FemtoHelper/contraption_idle");

        Add(new Coroutine(ShakeOccasionally()));
        if (particles = data.Bool("particles", true))
        {
            Add(new Coroutine(IdleSteam()));
            Add(new Coroutine(ExaustSteam()));
        }
        reducedDebris = data.Bool("reducedDebris", false);
    }

    public IEnumerator IdleSteam()
    {
        while (true)
        {
            (Scene as Level).ParticlesFG.Emit(ParticleTypes.Steam, TopLeft + new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height)));

            yield return 300f / Width / Height;
        }
    }

    public IEnumerator ShakeOccasionally()
    {
        while (true)
        {
            if (Calc.Random.NextFloat() < 0.4f)
            {
                child.StartShaking(0.06f);
            }
            yield return Calc.Random.Range(0.3f, 0.8f);
        }
    }

    public IEnumerator ExaustSteam()
    {
        while (true)
        {
            for (int i = -1; i < 2; i++)
            {
                (Scene as Level).ParticlesBG.Emit(Steam2, BottomCenter + Vector2.UnitX * 4 * i, 90f * Calc.DegToRad);
            }
            yield return Calc.Random.Range(0.3f, 0.7f);
        }
    }

    public override void Render()
    {
        Vector2 position = Position;
        Position = child.TopLeft + child.Shake;
        for (int k = 0; (float)k < base.Width / 8f; k++)
        {
            for (int l = 0; (float)l < base.Height / 8f; l++)
            {
                int num4 = (int)(base.Width / 8f) == 1 ? 3 : ((k != 0) ? (((float)k != base.Width / 8f - 1f) ? 1 : 2) : 0);
                int num5 = (int)(base.Height / 8f) == 1 ? 3 : ((l != 0) ? (((float)l != base.Height / 8f - 1f) ? 1 : 2) : 0);
                edges[num4, num5].Draw(new Vector2(base.X + (float)(k * 8), base.Y + (float)(l * 8)));
            }
        }

        cog.DrawCentered(child.Center + child.Shake - Vector2.UnitX, Calc.HexToColor("505050"), 1, cogRotation);
        cog.DrawCentered(child.Center + child.Shake - Vector2.UnitY, Calc.HexToColor("505050"), 1, cogRotation);
        cog.DrawCentered(child.Center + child.Shake + Vector2.UnitX, Calc.HexToColor("505050"), 1, cogRotation);
        cog.DrawCentered(child.Center + child.Shake + Vector2.UnitY, Calc.HexToColor("505050"), 1, cogRotation);
        cog.DrawCentered(child.Center + child.Shake, Color.White, 1, cogRotation);

        base.Render();
        Position = position;
    }

    private void OnPickup()
    {
        highFrictionTimer = 0.5f;
        child.Collidable = false;
        Speed = Vector2.Zero;
        AddTag(Tags.Persistent);
        child.AddTag(Tags.Persistent);
        foreach (StaticMover e in child.staticMovers)
        {
            e.Entity.AddTag(Tags.Persistent);
        }
    }

    private void OnCarry(Vector2 target)
    {
        Position = target;
        child.MoveTo(Position);
    }
    public override bool IsRiding(Solid solid)
    {
        return !(solid is ChildSolid) && base.IsRiding(solid);
    }

    public override void OnSquish(CollisionData data)
    {
        if (Collidable)
        {
            if (!TryBigSquishWiggle(data))
            {
                Die();
            }
        }
    }

    private void Hit(Vector2 dir)
    {
        (Scene as Level).Shake();
        Celeste.Freeze(0.05f);
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
        Audio.Play("event:/FemtoHelper/contraption_hit");
        sfx.Play("event:/FemtoHelper/contraption_angry");
        child.StartShaking(0.5f);
        if (particles)
        {
            for (int i = 0; i < Width / 8f; i++)
            {
                for (int j = 0; j < Height / 8f; j++)
                {
                    (Scene as Level).ParticlesFG.Emit(Steam2, TopLeft + new Vector2(i * 8, j * 8), -((TopLeft + new Vector2(i * 8, j * 8) - Center)).Angle());
                }
            }
        }
        Speed.X *= 0.25f;
        Speed.Y *= 0.5f;
        HitState = true;
    }

    private bool TryBigSquishWiggle(CollisionData data)
    {
        bool collidable = Collidable;
        bool collidable2 = child.Collidable;
        Collidable = false;
        child.Collidable = false;
        data.Pusher.Collidable = true;
        for (int i = 0; i <= Math.Max(3, (int)base.Width / 2); i++)
        {
            for (int j = 0; j <= Math.Max(3, (int)base.Height / 2); j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                for (int num = 1; num >= -1; num -= 2)
                {
                    for (int num2 = 1; num2 >= -1; num2 -= 2)
                    {
                        Vector2 vector = new Vector2(i * num, j * num2);
                        if (!CollideCheck<Solid>(Position + vector))
                        {
                            Position += vector;
                            child.MoveTo(Position);
                            data.Pusher.Collidable = false;
                            Collidable = collidable;
                            child.Collidable = collidable2;
                            return true;
                        }
                    }
                }
            }
        }
        data.Pusher.Collidable = false;
        Collidable = collidable;
        child.Collidable = collidable2;
        return false;
    }

    private bool ReleasedSquishWiggle()
    {
        bool collidable = Collidable;
        bool collidable2 = child.Collidable;
        Collidable = false;
        child.Collidable = false;
        for (int i = 0; i <= Math.Max(3, (int)base.Width); i++)
        {
            for (int j = 0; j <= Math.Max(3, (int)base.Height); j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                for (int num = 1; num >= -1; num -= 2)
                {
                    for (int num2 = 1; num2 >= -1; num2 -= 2)
                    {
                        Vector2 vector = new Vector2(i * num, j * num2);
                        if (!CollideCheck<Solid>(Position + vector))
                        {
                            Position += vector;
                            child.MoveTo(Position);
                            Collidable = collidable;
                            child.Collidable = collidable2;
                            return true;
                        }
                    }
                }
            }
        }
        Collidable = collidable;
        child.Collidable = collidable2;
        return false;
    }

    public void Explode()
    {
        for (int i = 0; (float)i < base.Width / 8f; i++)
        {
            for (int j = 0; (float)j < base.Height / 8f; j++)
            {
                bool flag = true;
                if (reducedDebris)
                {
                    if (Calc.Random.NextFloat() < 0.5f)
                    {
                        flag = false;
                    }
                } 

                if (flag) base.Scene.Add(Engine.Pooler.Create<ContraptionDebris>().Init(TopLeft + new Vector2(4 + i * 8, 4 + j * 8), $"{spritePath}debris", true).BlastFrom(Center));
            }
        }
        Collider collider = base.Collider;
        base.Collider = new Hitbox(collider.Width + 48, collider.Height + 48, collider.Left - 24, collider.Top - 24);
        Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position);
        Player player = CollideFirst<Player>();
        if (player != null && !base.Scene.CollideCheck<Solid>(Position, player.Center))
        {
            player.ExplodeLaunch(Position, snapUp: false, sidesOnly: true);
        }
        TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
        if (theoCrystal != null && !base.Scene.CollideCheck<Solid>(Position, theoCrystal.Center))
        {
            theoCrystal.ExplodeLaunch(Position);
        }
        foreach (TempleCrackedBlock entity in base.Scene.Tracker.GetEntities<TempleCrackedBlock>())
        {
            if (CollideCheck(entity))
            {
                entity.Break(Position);
            }
        }
        foreach (TouchSwitch entity2 in base.Scene.Tracker.GetEntities<TouchSwitch>())
        {
            if (CollideCheck(entity2))
            {
                entity2.TurnOn();
            }
        }
        foreach (FloatingDebris entity3 in base.Scene.Tracker.GetEntities<FloatingDebris>())
        {
            if (CollideCheck(entity3))
            {
                entity3.OnExplode(Position);
            }
        }
        base.Collider = collider;
        Level level = SceneAs<Level>();
        level.Shake();
        level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
        level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
        level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
        for (float num = 0f; num < MathF.PI * 2f; num += 0.17453292f)
        {
            Vector2 position = base.Center + Calc.AngleToVector(num + Calc.Random.Range(-MathF.PI / 90f, MathF.PI / 90f), Calc.Random.Range(12, 18));
            level.Particles.Emit(Seeker.P_Regen, position, num);
        }

        Die();
    }

    public void Die()
    {
        RemoveSelf();
        child.DestroyStaticMovers();
        child.RemoveSelf();
    }

    private void OnRelease(Vector2 force)
    {

        if (CollideCheck<Solid>())
        {
            if (!ReleasedSquishWiggle())
            {
                Die();
            }
            force = Vector2.Zero;
        }

        if (force.X == 0f)
        {
            Audio.Play("event:/new_content/char/madeline/glider_drop", Position);
        }
        child.Collidable = true;
        RemoveTag(Tags.Persistent);
        child.RemoveTag(Tags.Persistent);
        foreach (StaticMover e in child.staticMovers)
        {
            e.Entity.RemoveTag(Tags.Persistent);
        }
        force.Y *= 0.5f;
        if (force.X != 0f && force.Y == 0f)
        {
            force.Y = -0.4f;
        }
        Speed = force * 100f;
        MoveTo(Position - Vector2.UnitY * 4);
        child.MoveTo(Position);
    }

    private void OnCollideH(CollisionData data)
    {
        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
        }
        if (Speed.X < 0f)
        {
            //Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_left", Position);
        }
        else
        {
            //Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_right", Position);
        }
        Speed.X *= -1f;
    }

    private void OnCollideV(CollisionData data)
    {
        if (HitState)
        {
            if (Speed.Y < 0)
            {
                Explode();
            }
        }
        else
        {
            if (Math.Abs(Speed.Y) > 8f)
            {
                //Audio.Play("event:/new_content/game/10_farewell/glider_land", Position);
            }
            if (Speed.Y < 0f)
            {
                Speed.Y *= -0.5f;
            }
            else
            {
                Speed.Y = 0f;
            }
        }
    }

    public override void Update()
    {
        Level level = Scene as Level;

        base.Update();

        if (!HitState)
        {
            if (prevLiftSpeedTimer > 0)
            {
                prevLiftSpeedTimer -= Engine.DeltaTime;
            }
            else
            {
                prevLiftSpeed = Vector2.Zero;
            }

            if (Hold.IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
            }

            else
            {
                if (highFrictionTimer > 0f)
                {
                    highFrictionTimer -= Engine.DeltaTime;
                }
                if (OnGround())
                {
                    Hold.PickupCollider = groundHitbox;

                    float target2 = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
                    Speed.X = Calc.Approach(Speed.X, target2, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = base.LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                    {
                        Speed.Y = prevLiftSpeed.Y;
                        if (Math.Abs(prevLiftSpeed.X) > Math.Abs(Speed.X)) Speed.X = prevLiftSpeed.X;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f)
                        {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f)
                        {
                            //noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        prevLiftSpeed = liftSpeed;
                        prevLiftSpeedTimer = 0.18f;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f)
                        {
                            Speed.Y = 0f;
                        }
                    }
                }
                else
                {

                    float num3 = ((Speed.Y < 0f) ? 40f : ((!(highFrictionTimer <= 0f)) ? 10f : 40f));
                    Speed.X = Calc.Approach(Speed.X, 0f, num3 * Engine.DeltaTime);

                    Hold.PickupCollider = airHitbox;
                    if (Hold.ShouldHaveGravity)
                    {
                        float num2 = 200f;
                        if (Speed.Y >= -30f)
                        {
                            num2 *= 0.5f;
                        }
                        if (noGravityTimer > 0f)
                        {
                            noGravityTimer -= Engine.DeltaTime;
                        }
                        else
                        {
                            Speed.Y = Calc.Approach(Speed.Y, 30f, num2 * Engine.DeltaTime);
                        }
                    }
                }

                Move(Speed * Engine.DeltaTime);


                if (base.Left < (float)level.Bounds.Left)
                {
                    base.Left = level.Bounds.Left;
                    child.MoveToX(Position.X);
                    OnCollideH(new CollisionData
                    {
                        Direction = -Vector2.UnitX
                    });
                }
                else if (base.Right > (float)level.Bounds.Right)
                {
                    base.Right = level.Bounds.Right;
                    child.MoveToY(Position.Y);
                    OnCollideH(new CollisionData
                    {
                        Direction = Vector2.UnitX
                    });
                }
                else if (base.Top > (float)(level.Bounds.Bottom + 16))
                {
                    Die();
                    return;
                }
                Hold.CheckAgainstColliders();
            }
        }
        else
        {
            if (Hold.IsHeld)
            {
                Hold.Holder?.Drop();
            }
            Hold.cannotHoldTimer = 1f;

            Speed.Y = Calc.Approach(Speed.Y, -180f, 300f * Engine.DeltaTime);

            Move(Speed * Engine.DeltaTime);
        }




        Scene.OnEndOfFrame += () =>
        {
            cogRotation += Engine.DeltaTime + ((Position - lastPosition).X * 0.1f) + ((Position - lastPosition).Y * 0.1f);
            lastPosition = Position;
        };
    }

    public bool HitSpring(Spring spring)
    {
        if (!Hold.IsHeld)
        {
            if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
            {
                Speed.X *= 0.5f;
                Speed.Y = -160f;
                noGravityTimer = 0.15f;
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = 160f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                Speed.X = -160f;
                Speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            }
        }
        return false;
    }

    public void Move(Vector2 offset)
    {
        MoveH(offset.X, OnCollideH);
        MoveV(offset.Y, OnCollideV);
    }

    public void MoveTo(Vector2 pos)
    {
        MoveToX(pos.X, OnCollideH);
        MoveToY(pos.Y, OnCollideV);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(child);
    }


    public static void Load()
    {
        On.Celeste.Actor.MoveHExact += Actor_MoveHExact;
        On.Celeste.Actor.MoveVExact += Actor_MoveVExact;
        On.Celeste.Actor.OnGround_int += Actor_OnGround_int;
    }

    private static bool Actor_OnGround_int(On.Celeste.Actor.orig_OnGround_int orig, Actor self, int downCheck)
    {
        if (self is TheContraption contr)
        {
            if (contr.child == null) return orig(self, downCheck);
            bool collidable = contr.child.Collidable;
            contr.child.Collidable = false;
            bool result = orig(self, downCheck);
            contr.child.Collidable = collidable;
            return result;
        }
        else return orig(self, downCheck);
    }

    private static bool Actor_MoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher)
    {
        if (self is TheContraption contr)
        {
            if (contr.child == null) return orig(self, moveV, onCollide, pusher);
            bool collidable = contr.child.Collidable;
            contr.child.Collidable = false;
            float oldy = self.Y;
            bool result = orig(self, moveV, onCollide, pusher);
            float moveV2 = self.Y - oldy;
            contr.child.Collidable = collidable;
            contr.child.MoveV(moveV2, contr.Speed.Y);
            return result;
        }
        else return orig(self, moveV, onCollide, pusher);
    }

    private static bool Actor_MoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher)
    {
        if (self is TheContraption contr)
        {
            if (contr.child == null) return orig(self, moveH, onCollide, pusher);
            bool collidable = contr.child.Collidable;
            contr.child.Collidable = false;
            float oldx = self.X;
            bool result = orig(self, moveH, onCollide, pusher);
            float moveH2 = self.X - oldx;
            contr.child.Collidable = collidable;
            contr.child.MoveH(moveH2, contr.Speed.X);
            return result;
        }
        else return orig(self, moveH, onCollide, pusher);
    }

    public static void Unload()
    {
        On.Celeste.Actor.MoveHExact -= Actor_MoveHExact;
        On.Celeste.Actor.MoveVExact -= Actor_MoveVExact;
        On.Celeste.Actor.OnGround_int -= Actor_OnGround_int;
    }
}
