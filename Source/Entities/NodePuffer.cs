
using System;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/NodePuffer")]
public class NodePuffer : Entity
{
    private enum States
    {
        Idle,
        Hit,
        Gone
    }

    private const float BounceSpeed = 200f;

    private const float ExplodeRadius = 40f;

    private const float DetectRadius = 32f;

    private const float StunnedAccel = 320f;

    private const float AlertedRadius = 60f;

    private const float CantExplodeTime = 0.5f;

    private Sprite sprite;

    private States state;

    private Vector2 startPosition;

    private Vector2 anchorPosition;

    private Vector2 lastSpeedPosition;

    private Vector2 lastSinePosition;

    private Circle pushRadius;

    private Circle breakWallsRadius;

    private Circle detectRadius;

    private SineWave idleSine;

    private Vector2 hitSpeed;

    private float goneTimer;

    private float cannotHitTimer;

    private float alertTimer;

    private Wiggler bounceWiggler;

    private Wiggler inflateWiggler;

    private Vector2 scale;

    //private SimpleCurve returnCurve;

    private float cantExplodeTimer;

    private Vector2 lastPlayerPos;

    private float playerAliveFade;

    private Vector2 facing = Vector2.One;

    private float eyeSpin;

    private Vector2[] nodes;
    private float[] nodeLens;

    private int nodeIndex;
    private bool moveSignal;
    private bool moving;

    private bool returnSoundPlayed;
    private float timer;
    private ParticleType pufferPath = new(Seeker.P_Regen)
    {
        Color = Calc.HexToColor("fccbde") * 0.4f,
        Color2 = Calc.HexToColor("d957c1") * 0.3f,
        FadeMode = ParticleType.FadeModes.InAndOut,
        SpeedMin = 10,
        SpeedMax = 40,
        Direction = 0,
        DirectionRange = 360,
        Friction = 10,
    };

    private float sequenceTimer;
    private readonly float speedFactor;

    private readonly Color lineColor1;
    private readonly Color lineColor2;

    public NodePuffer(Vector2 position, bool faceRight, float speed, Color col1, Color col2, Vector2[] nodes)
        : base(position)
    {
        Collider = new Hitbox(14f, 12f, -7f, -7f);
        Add(new PlayerCollider(OnPlayer));
        Add(sprite = GFX.SpriteBank.Create("pufferFish"));
        sprite.Play("idle");
        if (!faceRight)
        {
            facing.X = -1f;
        }
        idleSine = new SineWave(0.5f, 0f);
        idleSine.Randomize();
        Add(idleSine);
        anchorPosition = Position;
        //Position += new Vector2(idleSine.Value * 3f, idleSine.ValueOverTwo * 2f);
        state = States.Idle;
        startPosition = (lastSinePosition = (lastSpeedPosition = Position));
        pushRadius = new Circle(40f);
        detectRadius = new Circle(32f);
        breakWallsRadius = new Circle(16f);
        scale = Vector2.One;
        bounceWiggler = Wiggler.Create(0.6f, 2.5f, (float v) =>
        {
            sprite.Rotation = v * 20f * (MathF.PI / 180f);
        });
        Add(bounceWiggler);
        inflateWiggler = Wiggler.Create(0.6f, 2f);
        Add(inflateWiggler);
        this.nodes = new Vector2[nodes.Length + 1];
        this.nodes[0] = Position;
        for (int i = 0; i < nodes.Length; i++)
        {
            this.nodes[i + 1] = nodes[i];
        }

        nodeLens = new float[nodes.Length];
        for (int i = nodes.Length - 1; i >= 0; i--)
        {
            nodeLens[i] = Vector2.Distance(nodes[i], nodes[Utils.Util.Mod(i + 1, nodes.Length)]);
        }

        timer = Calc.Random.NextFloat(500f);

        Add(new Coroutine(Sequence()));
        speedFactor = speed;

        lineColor1 = col1;
        lineColor2 = col2;

        pufferPath = new(Seeker.P_Regen)
        {
            Color = Color.Lerp(lineColor1, Color.White, 0.7f) * 0.4f,
            Color2 = Color.Lerp(lineColor1, lineColor2, 0.5f) * 0.3f,
            FadeMode = ParticleType.FadeModes.InAndOut,
            SpeedMin = 10,
            SpeedMax = 40,
            Direction = 0,
            DirectionRange = 360,
            Friction = 10,
        };
    }


    public NodePuffer(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("right", true), data.Float("speed", 2), data.HexColor("lineColor", Color.HotPink), data.HexColor("activeLineColor", Color.DeepPink), data.NodesOffset(offset))
    {

    }


    private void GotoIdle()
    {
        if (state == States.Gone)
        {
            cantExplodeTimer = 0.5f;
            sprite.Play("recover");
            Audio.Play("event:/new_content/game/10_farewell/puffer_reform", Position);
        }
        lastSinePosition = (lastSpeedPosition = Position);
        hitSpeed = Vector2.Zero;
        idleSine.Reset();
        state = States.Idle;
    }

    public IEnumerator Sequence()
    {
        while (true)
        {
            while (!moveSignal)
            {
                yield return null;
            }
            moveSignal = false;
            moving = true;
            Collidable = false;
            sequenceTimer = 0;
            Vector2 start = nodes[nodeIndex];
            Vector2 end = nodes[(nodeIndex + 1) % nodes.Length];
            while (sequenceTimer <= 1)
            {
                lastSpeedPosition = Position;
                Position = Vector2.Lerp(start, end, Ease.CubeOut(sequenceTimer));
                sequenceTimer += Engine.DeltaTime * speedFactor;
                yield return null;
            }
            sequenceTimer = 0;
            nodeIndex = (nodeIndex + 1) % (nodes.Length - 1);
            moving = false;
            Collidable = true;
        }
    }

    private void GotoHit(Vector2 from)
    {
        scale = new Vector2(1.2f, 0.8f);
        hitSpeed = Vector2.UnitY * 0; //200f;
        state = States.Hit;
        if (!moving) moveSignal = true;
        bounceWiggler.Start();
        Alert(restart: true, playSfx: false);
        Audio.Play("event:/new_content/game/10_farewell/puffer_boop", Position);
    }

    private void GotoHitSpeed(Vector2 speed)
    {
        hitSpeed = speed;
        state = States.Hit;
    }


    private void GotoGone()
    {
        if (!moving) moveSignal = true;
        goneTimer = (1 / speedFactor) + 0.25f;
        returnSoundPlayed = false;
        state = States.Gone;
    }


    private void Explode()
    {
        Collider collider = Collider;
        Collider = pushRadius;
        Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position);
        sprite.Play("explode");
        Player player = CollideFirst<Player>();
        if (player != null && !Scene.CollideCheck<Solid>(Position, player.Center))
        {
            player.ExplodeLaunch(Position, snapUp: false, sidesOnly: true);
        }
        TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
        if (theoCrystal != null && !Scene.CollideCheck<Solid>(Position, theoCrystal.Center))
        {
            theoCrystal.ExplodeLaunch(Position);
        }
        foreach (TempleCrackedBlock entity in Scene.Tracker.GetEntities<TempleCrackedBlock>())
        {
            if (CollideCheck(entity))
            {
                entity.Break(Position);
            }
        }
        foreach (TouchSwitch entity2 in Scene.Tracker.GetEntities<TouchSwitch>())
        {
            if (CollideCheck(entity2))
            {
                entity2.TurnOn();
            }
        }
        foreach (FloatingDebris entity3 in Scene.Tracker.GetEntities<FloatingDebris>())
        {
            if (CollideCheck(entity3))
            {
                entity3.OnExplode(Position);
            }
        }
        Collider = collider;
        Level level = SceneAs<Level>();
        level.Shake();
        level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
        level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
        level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
        for (float num = 0f; num < MathF.PI * 2f; num += 0.17453292f)
        {
            Vector2 position = Center + Calc.AngleToVector(num + Calc.Random.Range(-MathF.PI / 90f, MathF.PI / 90f), Calc.Random.Range(12, 18));
            level.Particles.Emit(Seeker.P_Regen, position, num);
        }
    }


    public override void Render()
    {
        Vector2 sineOffset = new Vector2(idleSine.Value * 1f, idleSine.ValueOverTwo * 1f);
        Position += sineOffset;

        for (int i = 0; i < nodes.Length - 1; i++)
        {
            float nextFactor = nodeIndex == i ? Ease.SineInOut(1 - sequenceTimer) : nodeIndex == Utils.Util.Mod(i - 1, nodes.Length - 1) ? Ease.SineInOut(sequenceTimer) : 0;
            Color col = Color.Lerp(lineColor1, lineColor2, nextFactor);
            col.A = 0;
            float count = MathF.Round(nodeLens[Utils.Util.Mod(i - 1, nodes.Length - 1)] / 8f);
            for (float j = 0; j < count; j++)
            {
                float percent = j / count;
                float percentNext = (j + 1) / count;
                Vector2 perp = (nodes[i + 1] - nodes[i]).Perpendicular().SafeNormalize(1f + (1f * nextFactor));
                Draw.Line(Vector2.Lerp(nodes[i], nodes[i + 1], percent) + perp * MathF.Sin((timer * -3) + ((j + (i * count)) * 1.7f)), Vector2.Lerp(nodes[i], nodes[i + 1], percentNext) + perp * MathF.Sin((timer * -3) + ((j + 1 + (i * count)) * 1.7f)), col * 0.4f * (0.5f + 0.5f * MathF.Cos((timer * 4) + ((j + (i * count)) * 0.2f))), 2);
            }
            Draw.Circle(nodes[i + 1], (6 + (2 * nextFactor)) + (2 + (1 * nextFactor)) * MathF.Cos((timer * 4) + (i * count * 0.2f)), col * 0.4f * (0.5f + 0.5f * MathF.Cos((timer * 4) + (i * count * 0.2f))), 5);
        }

        sprite.Scale = scale * (1f + inflateWiggler.Value * 0.4f);
        sprite.Scale *= facing;
        bool flag = false;
        if (sprite.CurrentAnimationID != "hidden" && sprite.CurrentAnimationID != "explode" && sprite.CurrentAnimationID != "recover")
        {
            flag = true;
        }
        else if (sprite.CurrentAnimationID == "explode" && sprite.CurrentAnimationFrame <= 1)
        {
            flag = true;
        }
        else if (sprite.CurrentAnimationID == "recover" && sprite.CurrentAnimationFrame >= 4)
        {
            flag = true;
        }
        if (!Collidable && state != States.Gone) flag = false;
        if (flag)
        {
            sprite.DrawSimpleOutline();
        }
        float num = playerAliveFade * Calc.ClampedMap((Position - lastPlayerPos).Length(), 128f, 96f);
        if (num > 0f && state != States.Gone)
        {
            bool flag2 = false;
            Vector2 vector = lastPlayerPos;
            if (vector.Y < Y)
            {
                vector.Y = Y - (vector.Y - Y) * 0.5f;
                vector.X += vector.X - X;
                flag2 = true;
            }
            float radiansB = (vector - Position).Angle();
            for (int i = 0; i < 28; i++)
            {
                float num2 = (float)Math.Sin(Scene.TimeActive * 0.5f) * 0.02f;
                float num3 = Calc.Map((float)i / 28f + num2, 0f, 1f, -MathF.PI / 30f, 3.2463126f);
                num3 += bounceWiggler.Value * 20f * (MathF.PI / 180f);
                Vector2 vector2 = Calc.AngleToVector(num3, 1f);
                Vector2 vector3 = Position + vector2 * 32f;
                float t = Calc.ClampedMap(Calc.AbsAngleDiff(num3, radiansB), MathF.PI / 2f, 0.17453292f);
                t = Ease.CubeOut(t) * 0.8f * num;
                if (!(t > 0f))
                {
                    continue;
                }
                if (i == 0 || i == 27)
                {
                    Draw.Line(vector3, vector3 - vector2 * 10f, Color.White * t);
                    continue;
                }
                Vector2 vector4 = vector2 * (float)Math.Sin(Scene.TimeActive * 2f + (float)i * 0.6f);
                if (i % 2 == 0)
                {
                    vector4 *= -1f;
                }
                vector3 += vector4;
                if (!flag2 && Calc.AbsAngleDiff(num3, radiansB) <= 0.17453292f)
                {
                    Draw.Line(vector3, vector3 - vector2 * 3f, Color.White * t);
                }
                else
                {
                    Draw.Point(vector3, Color.White * t);
                }
            }
        }

        if (!Collidable && state != States.Gone) sprite.Color = Color.White * 0.5f;

        base.Render();

        sprite.Color = Color.White;

        if (sprite.CurrentAnimationID == "alerted")
        {
            Vector2 vector5 = Position + new Vector2(3f, (facing.X < 0f) ? (-5) : (-4)) * sprite.Scale;
            Vector2 to = lastPlayerPos + new Vector2(0f, -4f);
            Vector2 vector6 = Calc.AngleToVector(Calc.Angle(vector5, to) + eyeSpin * (MathF.PI * 2f) * 2f, 1f);
            Vector2 vector7 = vector5 + new Vector2((float)Math.Round(vector6.X), (float)Math.Round(Calc.ClampedMap(vector6.Y, -1f, 1f, -1f, 2f)));
            Draw.Rect(vector7.X, vector7.Y, 1f, 1f, Color.Black);
        }
        sprite.Scale /= facing;

        Position -= sineOffset;
    }


    public override void Update()
    {
        base.Update();
        timer += Engine.DeltaTime;
        if (Scene.OnInterval(0.3f))
        {
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                (Scene as Level).Particles.Emit(pufferPath, Vector2.Lerp(nodes[i], nodes[i + 1], Calc.Random.NextFloat()) + new Vector2(Calc.Random.Range(-2, 2), Calc.Random.Range(-2, 2)));
            }
        }
        eyeSpin = Calc.Approach(eyeSpin, 0f, Engine.DeltaTime * 1.5f);
        scale = Calc.Approach(scale, Vector2.One, 1f * Engine.DeltaTime);
        if (cannotHitTimer > 0f)
        {
            cannotHitTimer -= Engine.DeltaTime;
        }
        if (state != States.Gone && cantExplodeTimer > 0f)
        {
            cantExplodeTimer -= Engine.DeltaTime;
        }
        if (alertTimer > 0f)
        {
            alertTimer -= Engine.DeltaTime;
        }
        Player entity = Scene.Tracker.GetEntity<Player>();
        if (entity == null)
        {
            playerAliveFade = Calc.Approach(playerAliveFade, 0f, 1f * Engine.DeltaTime);
        }
        else
        {
            playerAliveFade = Calc.Approach(playerAliveFade, 1f, 1f * Engine.DeltaTime);
            lastPlayerPos = entity.Center;
        }
        switch (state)
        {
            case States.Idle:
                {
                    if (ProximityExplodeCheck())
                    {
                        Explode();
                        GotoGone();
                        break;
                    }
                    if (AlertedCheck())
                    {
                        Alert(restart: false, playSfx: true);
                    }
                    else if (sprite.CurrentAnimationID == "alerted" && alertTimer <= 0f)
                    {
                        Audio.Play("event:/new_content/game/10_farewell/puffer_shrink", Position);
                        sprite.Play("unalert");
                    }
                    break;
                }
            case States.Hit:
                lastSpeedPosition = Position;
                //MoveH(hitSpeed.X * Engine.DeltaTime, onCollideH);
                //MoveV(hitSpeed.Y * Engine.DeltaTime, OnCollideV);
                Position.X += hitSpeed.X * Engine.DeltaTime;
                Position.Y += hitSpeed.Y * Engine.DeltaTime;
                anchorPosition = Position;
                hitSpeed.X = Calc.Approach(hitSpeed.X, 0f, 150f * Engine.DeltaTime);
                hitSpeed = Calc.Approach(hitSpeed, Vector2.Zero, 320f * Engine.DeltaTime);
                if (ProximityExplodeCheck())
                {
                    Explode();
                    GotoGone();
                    break;
                }
                if (Top >= (float)(SceneAs<Level>().Bounds.Bottom + 5))
                {
                    sprite.Play("hidden");
                    GotoGone();
                    break;
                }
                if (hitSpeed == Vector2.Zero)
                {
                    //ZeroRemainderX();
                    //ZeroRemainderY();
                    GotoIdle();
                }
                break;
            case States.Gone:
                {
                    float num = goneTimer;
                    goneTimer -= Engine.DeltaTime;
                    if (goneTimer <= (1 / speedFactor) && !returnSoundPlayed)
                    {
                        returnSoundPlayed = true;
                        Audio.Play("event:/new_content/game/10_farewell/puffer_return", Position);
                    }
                    if (goneTimer <= 0f)
                    {
                        Visible = (Collidable = true);
                        GotoIdle();
                    }
                    break;
                }
        }
    }



    private bool ProximityExplodeCheck()
    {
        if (cantExplodeTimer > 0f || !Collidable)
        {
            return false;
        }
        bool result = false;
        Collider collider = Collider;
        Collider = detectRadius;
        Player player;
        if ((player = CollideFirst<Player>()) != null && player.CenterY >= Y + collider.Bottom - 4f && !Scene.CollideCheck<Solid>(Position, player.Center))
        {
            result = true;
        }
        Collider = collider;
        return result;
    }


    private bool AlertedCheck()
    {
        Player entity = Scene.Tracker.GetEntity<Player>();
        if (entity != null)
        {
            return (entity.Center - Center).Length() < 60f;
        }
        return false;
    }


    private void Alert(bool restart, bool playSfx)
    {
        if (sprite.CurrentAnimationID == "idle")
        {
            if (playSfx)
            {
                Audio.Play("event:/new_content/game/10_farewell/puffer_expand", Position);
            }
            sprite.Play("alert");
            inflateWiggler.Start();
        }
        else if (restart && playSfx)
        {
            Audio.Play("event:/new_content/game/10_farewell/puffer_expand", Position);
        }
        alertTimer = 2f;
    }


    private void OnPlayer(Player player)
    {
        if (state == States.Gone || !(cantExplodeTimer <= 0f))
        {
            return;
        }
        if (cannotHitTimer <= 0f)
        {
            if (player.Bottom > lastSpeedPosition.Y + 3f)
            {
                Explode();
                GotoGone();
            }
            else
            {
                player.Bounce(Top);
                GotoHit(player.Center);
                //MoveToX(anchorPosition.X);
                idleSine.Reset();
                eyeSpin = 1f;
            }
        }
        cannotHitTimer = 0.1f;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (Depth == 0 && ((AreaKey)(object)(scene as Level).Session.Area).LevelSet != "Celeste")
        {
            Depth = -1;
        }
    }
}
