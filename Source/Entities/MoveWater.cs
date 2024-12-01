using System;
using System.Collections;
using MonoMod.Utils;

namespace Celeste.Mod.FemtoHelper.Entities;

public class Droplet : Entity
{
    public Vector2 Start;
    public Vector2 End;
    public Image Image;
    public Droplet Init(Vector2 from, Vector2 to, Color col)
    {
        Add(Image = new Image(GFX.Game["objects/FemtoHelper/moveWater/droplet"]));
        Image.Scale = Vector2.Zero;
        Image.CenterOrigin();
        Image.Color = col;
        Position = Start = from;
        End = to;
        Tween t = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.8f, true);
        t.OnUpdate += (t) =>
        {
            X = Calc.LerpClamp(Start.X, End.X, t.Eased * t.Eased);
            Y = Calc.LerpClamp(Start.Y, End.Y, t.Eased);
            Image.Scale = Vector2.One * t.Eased;
        };
        t.OnComplete += (t) => RemoveSelf();
        Add(t);
        return this;
    }
   
}

[TrackedAs(typeof(Water))]
[CustomEntity("FemtoHelper/MovingWaterBlock")]
public class MovingWaterBlock : GenericWaterBlock
{
    protected readonly DynData<Water> WaterData;

    private readonly SoundSource moveSfx;

    public bool Triggered;

    public readonly float TargetSpeed;

    public readonly float Angle;

    public float Speed;

    public readonly MTexture Arrow;
    public readonly MTexture Deadsprite;

    public readonly ParticleType Dissipate;
    public readonly ParticleType Tinydrops;
    public readonly ParticleType Tinydrops2;

    public float Shaketimer;
    public Vector2 Shake;

    public bool Dying;

    public float Wvtimer;

    public Vector2 Anchor;

    public readonly Wiggler IconWiggler;
    public Vector2 IconScale;

    public readonly WaterSprite Sprite;
    public MovingWaterBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height)
    {
        Anchor = data.Position + offset;
        WaterData = new DynData<Water>(this);
        WaterData["FillColor"] = Color.Transparent;
        Add(moveSfx = new SoundSource());
        Triggered = false;
        TargetSpeed = data.Float("maxSpeed", 60f);
        Angle = (data.Float("angle", 90f) / 180) * MathF.PI;
        Arrow = GFX.Game["objects/FemtoHelper/moveWater/arrow"];
        Deadsprite = GFX.Game["objects/FemtoHelper/moveWater/dead"];
        Dissipate = new ParticleType(Booster.P_Burst)
        {
            Color = Calc.HexToColor("81F4F0") * 0.25f
        };
        Wvtimer = 0f;
        Depth = -51000;
        Tinydrops = new ParticleType
        {
            Size = 1f,

            Color = Color.LightSkyBlue * 0.6f,
            DirectionRange = MathF.PI / 30f,
            LifeMin = 0.3f,
            LifeMax = 0.6f,
            SpeedMin = 5f,
            SpeedMax = 10f,
            SpeedMultiplier = 0.10f,
            FadeMode = ParticleType.FadeModes.Linear,
        };
        Tinydrops2 = new ParticleType
        {
            Size = 1f,
            Color = Color.LightSkyBlue,
            DirectionRange = MathF.PI / 30f,
            LifeMin = 0.6f,
            LifeMax = 1f,
            SpeedMin = 40f,
            SpeedMax = 50f,
            SpeedMultiplier = 0.25f,
            FadeMode = ParticleType.FadeModes.Late
        };

        Add(Sprite = new WaterSprite());

        IconScale = Vector2.One;

        Add(IconWiggler = Wiggler.Create(0.4f, 6f, (t) =>
        {
            IconScale = Vector2.One + (new Vector2(t, -t) * 0.5f);
        }));

        int lig = (int)(MathF.Min(Width, Height) / 2);

        Add(new VertexLight(new Vector2(Width / 2, Height / 2), Color.LightCyan, 0.9f, lig - 8, lig + 8));
    }
    public override void Update()
    {
        base.Update();
        if (!Triggered)
        {
            Player p = Scene.Tracker.GetNearestEntity<Player>(Position);
            if (p != null)
            {
                if (p.CollideCheck(this))
                {
                    TriggerBlock();
                }
            }
        }
        else
        {
            if (!Dying)
            {
                Speed = Calc.Approach(Speed, TargetSpeed, 80f * Engine.DeltaTime);
                if (Scene.OnInterval(0.04f))
                {
                    Vector2 pos2 = Position + new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                    SceneAs<Level>().ParticlesFG.Emit(Tinydrops, pos2, (pos2 - Center).Angle());
                }

            }
        }

        MoveTo(Position + Calc.AngleToVector(Angle, Speed) * Engine.DeltaTime);

        if ((Left < (Scene as Level).Bounds.Left || Right > (Scene as Level).Bounds.Right || Top < (Scene as Level).Bounds.Top || Bottom > (Scene as Level).Bounds.Bottom
          || MoveToCollideBarriers(Position + Calc.AngleToVector(Angle, Speed) * Engine.DeltaTime))
          && !Dying)
        {
            Add(new Coroutine(Destroy()));
        }

        if (Scene.OnInterval(0.02f))
        {
            Shake = Shaketimer > 0 ? new Vector2(Calc.Random.Range(-1f, 1f), Calc.Random.Range(-1f, 1f)) : Vector2.Zero;
        }
        if (Shaketimer > 0) Shaketimer -= Engine.DeltaTime;
        Wvtimer += Engine.DeltaTime;
    }

    public IEnumerator Destroy()
    {
        IconWiggler.Start();
        Sprite.Wiggle.Start();

        moveSfx.Param("arrow_stop", 1f);
        Dying = true;
        Speed = 0f;

        yield return 0.1f;

        Audio.Play("event:/FemtoHelper/movewater_break", Position);
        moveSfx.Stop();
        Shaketimer = 0.2f;

        yield return 0.2f;

        for (int i = 0; i < Width; i += 4)
        {
            for (int j = 0; j < Height; j += 4)
            {
                Vector2 vector = Position + new Vector2(2 + i, 2 + j);
                if (Calc.Random.Chance(0.5f)) SceneAs<Level>().ParticlesFG.Emit(Dissipate, vector + new Vector2(Calc.Random.Range(-2f, 2f), Calc.Random.Range(-2f, 2f)), Color.LightSkyBlue * 0.3f, (vector - Center).Angle());
            }
        }
        Visible = Collidable = false;

        yield return 2.2f;
        Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", Position);

        Position = Anchor;

        for (int i = 0; i < Width; i += 8)
        {
            for (int j = 0; j < Height; j += 8)
            {
                Vector2 vector6 = new Vector2(X + i + 4f, Y + j + 4f);
                Vector2 vec = (vector6 - Center).SafeNormalize();
                Color col = Color.Lerp(Color.CadetBlue * 0.2f, Color.White * 0.3f, vec.LengthSquared());
                Scene.Add(Engine.Pooler.Create<Droplet>().Init(vector6 + vec * 12f, vector6, col));
            }
        }

        yield return 0.8f;
        Audio.Play("event:/game/04_cliffside/greenbooster_reappear", Position).setPitch(0.8f);

        IconWiggler.Start();
        Sprite.Wiggle.Start();
        Collidable = Visible = true;
        Dying = Triggered = false;

    }

    public override void DrawDisplacement()
    {

    }

    public void TriggerBlock()
    {
        for (float i = 0; i < Width; i += 8)
        {
            SceneAs<Level>().ParticlesFG.Emit(Tinydrops2, Position + new Vector2(i, 0) + new Vector2(Calc.Random.Range(-2f, 2f), Calc.Random.Range(-2f, 2f)), Color.LightSkyBlue * 0.3f, (float)-Math.PI / 2f);
            SceneAs<Level>().ParticlesFG.Emit(Tinydrops2, new Vector2(X, Bottom) + new Vector2(i, 0) + new Vector2(Calc.Random.Range(-2f, 2f), Calc.Random.Range(-2f, 2f)), Color.LightSkyBlue * 0.3f, (float)Math.PI / 2f);
        }
        for (float i = 0; i < Height; i += 8)
        {
            SceneAs<Level>().ParticlesFG.Emit(Tinydrops2, Position + new Vector2(0, i) + new Vector2(Calc.Random.Range(-2f, 2f), Calc.Random.Range(-2f, 2f)), Color.LightSkyBlue * 0.3f, (float)Math.PI);
            SceneAs<Level>().ParticlesFG.Emit(Tinydrops2, new Vector2(Right, Y) + new Vector2(0, i) + new Vector2(Calc.Random.Range(-2f, 2f), Calc.Random.Range(-2f, 2f)), Color.LightSkyBlue * 0.3f, 0);
        }
        Triggered = true;
        Shaketimer = 0.2f;
        IconWiggler.Start();
        Sprite.Wiggle.Start();
        Audio.Play("event:/game/04_cliffside/arrowblock_activate", Position);
        moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
        moveSfx.Param("arrow_stop", 0f);
    }

    public override void Render()
    {
        Vector2 num3 = Position;
        Position += Shake;
        base.Render();
        MTexture tex = Arrow;

        float ang = Angle;
        if (Dying)
        {
            tex = Deadsprite;
            ang = 0;
        }
        tex.DrawOutlineCentered(Center, Color.Black, IconScale, ang);
        tex.DrawCentered(Center, Color.White, IconScale, ang);
        Position = num3;

    }
}
