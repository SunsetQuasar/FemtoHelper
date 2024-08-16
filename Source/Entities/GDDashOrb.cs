using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;

namespace Celeste.Mod.FemtoHelper.Entities; 

[CustomEntity("FemtoHelper/GDDashOrb")]
public class GDDashOrb : Entity
{
    public const float Tau = (float)Math.PI * 2f;

    public float angle;
    public float speed;
    public bool lastJump;
    public bool yeahforsure;
    public bool lastforsure;

    public Player player;

    public float cooldown;

    public MTexture orb;
    public MTexture ring;

    public float pulsepercent;

    public float timer;

    public bool pink;

    public bool additive;
    public Color baseColor => pink ? Calc.HexToColor("FF30F0") : Calc.HexToColor("20EE30");

    public ParticleType DashParticles;
    public ParticleType IdleParticles;
    public class Ring : Entity
    {
        public float life;
        public Ring(Vector2 pos) : base(pos)
        {
            life = 1f;
            Depth = 2000;
        }
        public override void Update()
        {
            base.Update();
            life = Math.Max(life - Engine.DeltaTime * 5, 0);
            if (life <= 0)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            base.Render();
            Color col = Color.White * Ease.SineIn(life);
            col.A = 0;
            Draw.Circle(Position, Ease.SineOut(1 - life) * 20, col, 4);
        }
    }

    public GDDashOrb(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(20, 20, -10, -10);
        Add(new PlayerCollider(OnPlayer));

        angle = data.Float("angle", 0f) * Calc.DegToRad;
        speed = data.Float("speed", 240f);
        pink = data.Bool("pink", false);
        additive = data.Bool("additive", false);

        orb = GFX.Game["objects/FemtoHelper/gddashorb/dashorb"];
        ring = GFX.Game["objects/FemtoHelper/gddashorb/dashring"];
        Add(new VertexLight(baseColor, 0.9f, 20, 40));
        Add(new BloomPoint(0.4f, 18));
        timer = Calc.Random.NextFloat(Tau);

        DashParticles = new ParticleType
        {
            Size = 1f,
            Color = baseColor,
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
            (Scene as Level).Particles.Emit(IdleParticles, Position + new Vector2(Calc.Random.Range(-12, 12), Calc.Random.Range(-12, 12)), Calc.Random.NextFloat(Tau));
        }

        pulsepercent = Math.Max(pulsepercent - Engine.DeltaTime * 1f, 0);

        timer += Engine.DeltaTime;

        if (yeahforsure && !lastforsure)
        {
            Scene.Add(new Ring(Position + (Vector2.UnitY * (float)Math.Sin(timer * 2) * 2)));
        }
        cooldown = Math.Max(cooldown - Engine.DeltaTime, 0);
        if (yeahforsure && (cooldown <= 0))
        {
            if (Input.Jump.Pressed && !lastJump)
            {
                Input.Jump.ConsumeBuffer();
                if (player != null)
                {
                    if (player.StateMachine.state == Player.StDash) player.StateMachine.ForceState(Player.StNormal);
                    player.launched = true;
                    Vector2 spd = Calc.AngleToVector(angle, speed);
                    if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1) spd.Y *= -1;
                    if (additive)
                    {
                        player.Speed += spd;
                    } else player.Speed = spd;
                    if (pink) FemtoModule.GravityHelperSupport.SetPlayerGravity?.Invoke(2, 1);
                    ExtraTrailManager t = player.Get<ExtraTrailManager>();
                    if(t != null)
                    {
                        t.dashTrailTimer = 0.07f;
                        t.dashTrailCounter = 4;
                    }
                    player.CreateTrail();
                }
                (Scene as Level).DirectionalShake(Calc.AngleToVector(angle, 1));
                Audio.Play("event:/char/madeline/jump_superslide", Position).setPitch(0.85f + Calc.Random.Range(-0.1f, 0.1f));
                Audio.Play("event:/char/madeline/dash_pink_right", Position).setPitch(1.2f + Calc.Random.Range(-0.1f, 0.1f));
                pulsepercent = 1f;
                cooldown = 0.1f;
                for (float i = Tau / 12f; i < Tau; i += Tau / 12f)
                {
                    (Scene as Level).Particles.Emit(DashParticles, Position, i);
                }
            }
        }
        lastJump = Input.Jump.Pressed;
        lastforsure = yeahforsure;
        yeahforsure = false;
    }

    public void OnPlayer(Player player)
    {
        if (!player.OnGround()) yeahforsure = true;
        this.player = player;
    }

    public override void Render()
    {
        base.Render();

        Color col = Color.Lerp(baseColor, Color.White, Ease.QuintIn(pulsepercent));

        ring.DrawOutlineCentered(Position + (Vector2.UnitY * (float)Math.Sin(timer * 2) * 2), Color.Black, 1 + (Ease.QuintIn(pulsepercent) * 0.6f));
        orb.DrawOutlineCentered(Position + (Vector2.UnitY * (float)Math.Sin(timer * 2) * 2), Color.Black, 1 + (Ease.QuintIn(pulsepercent) * 0.6f), angle);

        ring.DrawCentered(Position + (Vector2.UnitY * (float)Math.Sin(timer * 2) * 2), Color.White, 1 + (Ease.QuintIn(pulsepercent) * 0.6f));
        orb.DrawCentered(Position + (Vector2.UnitY * (float)Math.Sin(timer * 2) * 2), col, 1 + (Ease.QuintIn(pulsepercent) * 0.6f), angle);


    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        Draw.HollowRect(Position - new Vector2(Width / 2f, Height / 2f), Width, Height, Color.LimeGreen);
        Draw.LineAngle(Position, angle, 24f, Color.White);
    }
}
