using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using MonoMod.Utils;
using static Celeste.MoonGlitchBackgroundTrigger;
using System.IO;

namespace Celeste.Mod.FemtoHelper.Entities
{
    public class Droplet : Entity
    {
        public Vector2 start;
        public Vector2 end;
        public Image image;
        public Droplet Init(Vector2 From, Vector2 to, Color col)
        {
            Add(image = new Image(GFX.Game["objects/FemtoHelper/moveWater/droplet"]));
            image.Scale = Vector2.Zero;
            image.CenterOrigin();
            image.Color = col;
            Position = start = From;
            end = to;
            Tween t = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.8f, true);
            t.OnUpdate += (t) =>
            {
                X = Calc.LerpClamp(start.X, end.X, t.Eased * t.Eased);
                Y = Calc.LerpClamp(start.Y, end.Y, t.Eased);
                image.Scale = Vector2.One * t.Eased;
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
        protected DynData<Water> waterData;

        private SoundSource moveSfx;

        public bool triggered;

        public float targetSpeed;

        public float angle;

        public float Speed;

        public MTexture arrow;
        public MTexture deadsprite;

        public ParticleType dissipate;
        public ParticleType tinydrops;
        public ParticleType tinydrops2;

        public float shaketimer;
        public Vector2 shake;

        public bool dying;

        public float wvtimer;

        public Vector2 Anchor;

        public Wiggler iconWiggler;
        public Vector2 iconScale;

        public WaterSprite sprite;
        public MovingWaterBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height)
        {
            Anchor = data.Position + offset;
            waterData = new DynData<Water>(this);
            waterData["FillColor"] = Color.Transparent;
            Add(moveSfx = new SoundSource());
            triggered = false;
            targetSpeed = data.Float("maxSpeed", 60f);
            angle = (data.Float("angle", 90f) / 180) * MathF.PI;
            arrow = GFX.Game["objects/FemtoHelper/moveWater/arrow"];
            deadsprite = GFX.Game["objects/FemtoHelper/moveWater/dead"];
            dissipate = new ParticleType(Booster.P_Burst);
            dissipate.Color = Calc.HexToColor("81F4F0") * 0.25f;
            wvtimer = 0f;
            Depth = -51000;
            tinydrops = new ParticleType
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
            tinydrops2 = new ParticleType
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

            Add(sprite = new WaterSprite());

            iconScale = Vector2.One;

            Add(iconWiggler = Wiggler.Create(0.4f, 6f, (t) =>
            {
                iconScale = Vector2.One + (new Vector2(t, -t) * 0.5f);
            }));

            int lig = (int)(MathF.Min(Width, Height) / 2);

            Add(new VertexLight(new Vector2(Width / 2, Height / 2), Color.LightCyan, 0.9f, lig - 8, lig + 8));
        }
        public override void Update()
        {
            base.Update();
            if (!triggered)
            {
                Player p = Scene.Tracker.GetNearestEntity<Player>(Position);
                if (p != null)
                {
                    if (p.CollideCheck(this))
                    {
                        triggerBlock();
                    }
                }
            }
            else
            {
                if (!dying)
                {
                    Speed = Calc.Approach(Speed, targetSpeed, 80f * Engine.DeltaTime);
                    if (Scene.OnInterval(0.04f))
                    {
                        Vector2 pos2 = Position + new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                        SceneAs<Level>().ParticlesFG.Emit(tinydrops, pos2, (pos2 - Center).Angle());
                    }

                }
            }

            MoveTo(Position + Calc.AngleToVector(angle, Speed) * Engine.DeltaTime);

            if ((Left < (Scene as Level).Bounds.Left || Right > (Scene as Level).Bounds.Right || Top < (Scene as Level).Bounds.Top || Bottom > (Scene as Level).Bounds.Bottom
              || MoveToCollideBarriers(Position + Calc.AngleToVector(angle, Speed) * Engine.DeltaTime))
              && !dying)
            {
                Add(new Coroutine(Destroy()));
            }

            if (Scene.OnInterval(0.02f))
            {
                shake = shaketimer > 0 ? new Vector2(Calc.Random.Range(-1f, 1f), Calc.Random.Range(-1f, 1f)) : Vector2.Zero;
            }
            if (shaketimer > 0) shaketimer -= Engine.DeltaTime;
            wvtimer += Engine.DeltaTime;
        }

        public IEnumerator Destroy()
        {
            iconWiggler.Start();
            sprite.wiggle.Start();

            moveSfx.Param("arrow_stop", 1f);
            dying = true;
            Speed = 0f;

            yield return 0.1f;

            Audio.Play("event:/FemtoHelper/movewater_break", Position);
            moveSfx.Stop();
            shaketimer = 0.2f;

            yield return 0.2f;

            for (int i = 0; (float)i < Width; i += 4)
            {
                for (int j = 0; (float)j < Height; j += 4)
                {
                    Vector2 vector = Position + new Vector2(2 + i, 2 + j);
                    if (Calc.Random.Chance(0.5f)) SceneAs<Level>().ParticlesFG.Emit(dissipate, vector + new Vector2(Calc.Random.Range(-2f, 2f), Calc.Random.Range(-2f, 2f)), Color.LightSkyBlue * 0.3f, (vector - Center).Angle());
                }
            }
            Visible = Collidable = false;

            yield return 2.2f;
            Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", Position);

            Position = Anchor;

            for (int i = 0; (float)i < Width; i += 8)
            {
                for (int j = 0; (float)j < Height; j += 8)
                {
                    Vector2 vector6 = new Vector2(X + (float)i + 4f, Y + (float)j + 4f);
                    Vector2 vec = (vector6 - Center).SafeNormalize();
                    Color col = Color.Lerp(Color.CadetBlue * 0.2f, Color.White * 0.3f, vec.LengthSquared());
                    Scene.Add(Engine.Pooler.Create<Droplet>().Init(vector6 + vec * 12f, vector6, col));
                }
            }

            yield return 0.8f;
            Audio.Play("event:/game/04_cliffside/greenbooster_reappear", Position).setPitch(0.8f);

            iconWiggler.Start();
            sprite.wiggle.Start();
            Collidable = Visible = true;
            dying = triggered = false;

        }

        public override void DrawDisplacement()
        {

        }

        public void triggerBlock()
        {
            for (float i = 0; i < Width; i += 8)
            {
                SceneAs<Level>().ParticlesFG.Emit(tinydrops2, Position + new Vector2(i, 0) + new Vector2(Calc.Random.Range(-2f, 2f), Calc.Random.Range(-2f, 2f)), Color.LightSkyBlue * 0.3f, (float)-Math.PI / 2f);
                SceneAs<Level>().ParticlesFG.Emit(tinydrops2, new Vector2(X, Bottom) + new Vector2(i, 0) + new Vector2(Calc.Random.Range(-2f, 2f), Calc.Random.Range(-2f, 2f)), Color.LightSkyBlue * 0.3f, (float)Math.PI / 2f);
            }
            for (float i = 0; i < Height; i += 8)
            {
                SceneAs<Level>().ParticlesFG.Emit(tinydrops2, Position + new Vector2(0, i) + new Vector2(Calc.Random.Range(-2f, 2f), Calc.Random.Range(-2f, 2f)), Color.LightSkyBlue * 0.3f, (float)Math.PI);
                SceneAs<Level>().ParticlesFG.Emit(tinydrops2, new Vector2(Right, Y) + new Vector2(0, i) + new Vector2(Calc.Random.Range(-2f, 2f), Calc.Random.Range(-2f, 2f)), Color.LightSkyBlue * 0.3f, 0);
            }
            triggered = true;
            shaketimer = 0.2f;
            iconWiggler.Start();
            sprite.wiggle.Start();
            Audio.Play("event:/game/04_cliffside/arrowblock_activate", Position);
            moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
            moveSfx.Param("arrow_stop", 0f);
        }

        public override void Render()
        {
            Vector2 num3 = Position;
            Position += shake;
            base.Render();
            MTexture tex = arrow;

            float ang = angle;
            if (dying)
            {
                tex = deadsprite;
                ang = 0;
            }
            tex.DrawOutlineCentered(Center, Color.Black, iconScale, ang);
            tex.DrawCentered(Center, Color.White, iconScale, ang);
            Position = num3;

        }
    }
}
