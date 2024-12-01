using System;

namespace Celeste.Mod.FemtoHelper.Entities; 

[CustomEntity("FemtoHelper/GDDashOrb")]
public class GdDashOrb : Entity
{
    public const float Tau = (float)Math.PI * 2f;

    public readonly float Angle;
    public readonly float Speed;
    public bool LastJump;
    public bool Yeahforsure;
    public bool Lastforsure;

    public Player Player;

    public float Cooldown;

    public readonly MTexture OrbTexture;
    public readonly MTexture RingTexture;

    public float PulsePercent;

    public float Timer;

    public readonly bool Pink;

    public readonly bool Additive;
    public Color BaseColor => Pink ? Calc.HexToColor("FF30F0") : Calc.HexToColor("20EE30");

    public readonly ParticleType DashParticles;
    public readonly ParticleType IdleParticles;
    public class Ring : Entity
    {
        public float Life;
        public Ring(Vector2 pos) : base(pos)
        {
            Life = 1f;
            Depth = 2000;
        }
        public override void Update()
        {
            base.Update();
            Life = Math.Max(Life - Engine.DeltaTime * 5, 0);
            if (Life <= 0)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            base.Render();
            Color col = Color.White * Ease.SineIn(Life);
            col.A = 0;
            Draw.Circle(Position, Ease.SineOut(1 - Life) * 20, col, 4);
        }
    }

    public GdDashOrb(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(20, 20, -10, -10);
        Add(new PlayerCollider(OnPlayer));

        Angle = data.Float("angle", 0f) * Calc.DegToRad;
        Speed = data.Float("speed", 240f);
        Pink = data.Bool("pink", false);
        Additive = data.Bool("additive", false);

        OrbTexture = GFX.Game["objects/FemtoHelper/gddashorb/dashorb"];
        RingTexture = GFX.Game["objects/FemtoHelper/gddashorb/dashring"];
        Add(new VertexLight(BaseColor, 0.9f, 20, 40));
        Add(new BloomPoint(0.4f, 18));
        Timer = Calc.Random.NextFloat(Tau);

        DashParticles = new ParticleType
        {
            Size = 1f,
            Color = BaseColor,
            Color2 = Color.White,
            ColorMode = ParticleType.ColorModes.Blink,
            DirectionRange = (float)Math.PI / 50f,
            LifeMin = 0.6f,
            LifeMax = 1f,
            SpeedMin = 40f,
            SpeedMax = 50f,
            SpeedMultiplier = 0.25f,
            FadeMode = ParticleType.FadeModes.Late
        };
        IdleParticles = new ParticleType(DashParticles)
        {
            FadeMode = ParticleType.FadeModes.InAndOut,
            SpeedMin = 10f,
            SpeedMax = 20f,
            Friction = 10f
        };
    }

    public override void Update()
    {
        base.Update();

        if (Scene.OnInterval(0.2f))
        {
            (Scene as Level)?.Particles.Emit(IdleParticles, Position + new Vector2(Calc.Random.Range(-12, 12), Calc.Random.Range(-12, 12)), Calc.Random.NextFloat(Tau));
        }

        PulsePercent = Math.Max(PulsePercent - Engine.DeltaTime * 1f, 0);

        Timer += Engine.DeltaTime;

        if (Yeahforsure && !Lastforsure)
        {
            Scene.Add(new Ring(Position + (Vector2.UnitY * (float)Math.Sin(Timer * 2) * 2)));
        }
        Cooldown = Math.Max(Cooldown - Engine.DeltaTime, 0);
        if (Yeahforsure && (Cooldown <= 0))
        {
            if (Input.Jump.Pressed && !LastJump)
            {
                Input.Jump.ConsumeBuffer();
                if (Player != null)
                {
                    if (Player.StateMachine.state == Player.StDash) Player.StateMachine.ForceState(Player.StNormal);
                    Player.launched = true;
                    Vector2 spd = Calc.AngleToVector(Angle, Speed);
                    if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1) spd.Y *= -1;
                    if (Additive)
                    {
                        Player.Speed += spd;
                    } else Player.Speed = spd;
                    if (Pink) FemtoModule.GravityHelperSupport.SetPlayerGravity?.Invoke(2, 1);
                    ExtraTrailManager t = Player.Get<ExtraTrailManager>();
                    if(t != null)
                    {
                        t.DashTrailTimer = 0.07f;
                        t.DashTrailCounter = 4;
                    }
                    Player.CreateTrail();
                }
                (Scene as Level)?.DirectionalShake(Calc.AngleToVector(Angle, 1));
                Audio.Play("event:/char/madeline/jump_superslide", Position).setPitch(0.85f + Calc.Random.Range(-0.1f, 0.1f));
                Audio.Play("event:/char/madeline/dash_pink_right", Position).setPitch(1.2f + Calc.Random.Range(-0.1f, 0.1f));
                PulsePercent = 1f;
                Cooldown = 0.1f;
                for (float i = Tau / 12f; i < Tau; i += Tau / 12f)
                {
                    (Scene as Level)?.Particles.Emit(DashParticles, Position, i);
                }
            }
        }
        LastJump = Input.Jump.Pressed;
        Lastforsure = Yeahforsure;
        Yeahforsure = false;
    }

    public void OnPlayer(Player player)
    {
        if (!player.OnGround()) Yeahforsure = true;
        Player = player;
    }

    public override void Render()
    {
        base.Render();

        Color col = Color.Lerp(BaseColor, Color.White, Ease.QuintIn(PulsePercent));

        RingTexture.DrawOutlineCentered(Position + (Vector2.UnitY * (float)Math.Sin(Timer * 2) * 2), Color.Black, 1 + (Ease.QuintIn(PulsePercent) * 0.6f));
        OrbTexture.DrawOutlineCentered(Position + (Vector2.UnitY * (float)Math.Sin(Timer * 2) * 2), Color.Black, 1 + (Ease.QuintIn(PulsePercent) * 0.6f), Angle);

        RingTexture.DrawCentered(Position + (Vector2.UnitY * (float)Math.Sin(Timer * 2) * 2), Color.White, 1 + (Ease.QuintIn(PulsePercent) * 0.6f));
        OrbTexture.DrawCentered(Position + (Vector2.UnitY * (float)Math.Sin(Timer * 2) * 2), col, 1 + (Ease.QuintIn(PulsePercent) * 0.6f), Angle);


    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        Draw.HollowRect(Position - new Vector2(Width / 2f, Height / 2f), Width, Height, Color.LimeGreen);
        Draw.LineAngle(Position, Angle, 24f, Color.White);
    }
}
