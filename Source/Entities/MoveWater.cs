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

namespace Celeste.Mod.FemtoHelper.Entities
{
    [TrackedAs(typeof(Water))]
    [CustomEntity("FemtoHelper/MovingWaterBlock")]
    public class MovingWaterBlock : Water
    {
        protected DynData<Water> waterData;

        private SoundSource moveSfx;

        public bool triggered;

        public float targetSpeed;

        public float angle;

        public float Speed;

        public MTexture arrow;
        public MTexture deadsprite;
        public MTexture[,] ninSlice;

        public ParticleType dissipate;
        public ParticleType tinydrops;
        public ParticleType tinydrops2;

        public float shaketimer;
        public Vector2 shake;

        public bool dying;

        public float wvtimer;
        public MovingWaterBlock(EntityData data, Vector2 offset) : base(data.Position + offset, false, false, data.Width, data.Height)
        {
            waterData = new DynData<Water>(this);
            waterData["FillColor"] = Color.Transparent;
            Add(moveSfx = new SoundSource());
            triggered = false;
            targetSpeed = data.Float("maxSpeed", 60f);
            angle = (data.Float("angle", 90f) / 180) * (float)-Math.PI;
            arrow = GFX.Game["objects/FemtoHelper/moveWater/arrow"];
            deadsprite = GFX.Game["objects/FemtoHelper/moveWater/dead"];
            ninSlice = new MTexture[3, 3];
            MTexture nine = GFX.Game["objects/FemtoHelper/moveWater/nineSlice"];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    ninSlice[i, j] = nine.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
            dissipate = Booster.P_Burst;
            dissipate.Color = Color.LightSkyBlue * 0.3f;
            wvtimer = 0f;
            Depth = -51000;
            tinydrops = new ParticleType
            {
                Size = 1f,

                Color = Color.LightSkyBlue * 0.6f,
                DirectionRange = (float)Math.PI / 30f,
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
                DirectionRange = (float)Math.PI / 30f,
                LifeMin = 0.6f,
                LifeMax = 1f,
                SpeedMin = 40f,
                SpeedMax = 50f,
                SpeedMultiplier = 0.25f,
                FadeMode = ParticleType.FadeModes.Late
            };
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
                    Vector2 pos2 = Position + new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                    SceneAs<Level>().ParticlesFG.Emit(tinydrops, pos2, (pos2 - Center).Angle());
                }
            }

            foreach (SeekerBarrier entity in base.Scene.Tracker.GetEntities<SeekerBarrier>())
            {
                entity.Collidable = true;
                bool b = CollideCheck(entity);
                entity.Collidable = false;
                if ((b || Left < (Scene as Level).Bounds.Left || Right > (Scene as Level).Bounds.Right || Top < (Scene as Level).Bounds.Top || Bottom > (Scene as Level).Bounds.Bottom) && !dying) Add(new Coroutine(Destroy()));
            }

            Position += Calc.AngleToVector(angle, Speed) * Engine.DeltaTime;

            if (Scene.OnInterval(0.02f))
            {
                shake = shaketimer > 0 ? new Vector2(Calc.Random.Range(-1f, 1f), Calc.Random.Range(-1f, 1f)) : Vector2.Zero;
            }
            if (shaketimer > 0) shaketimer -= Engine.DeltaTime;
            wvtimer += Engine.DeltaTime;
        }

        public IEnumerator Destroy()
        {

            moveSfx.Param("arrow_stop", 1f);
            dying = true;
            Speed = 0f;

            yield return 0.1f;

            Audio.Play("event:/Codecumber/movewater_break", Position);
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
            RemoveSelf();
        }

        public static void Load()
        {
            On.Celeste.Water.RenderDisplacement += GoodLookingWaterISwear;
        }

        public static void Unload()
        {
            On.Celeste.Water.RenderDisplacement -= GoodLookingWaterISwear;
        }

        public static void GoodLookingWaterISwear(On.Celeste.Water.orig_RenderDisplacement orig, Water self)
        {
            MovingWaterBlock goodWater = self as MovingWaterBlock;
            if (goodWater == null)
            {
                orig(self);
            }
            else
            {
                Vector2 num3 = self.Position;
                self.Position += goodWater.shake - (Calc.AngleToVector(goodWater.angle, goodWater.Speed) * Engine.DeltaTime);
                for (float i = -1; i < self.Width + 1; i++)
                {
                    Draw.Rect(self.Position + new Vector2(i, -1), 1, self.Height + 1, new Color(0.5f, 0.5f + (float)Math.Sin(i / 4f + (goodWater.wvtimer * 4)) * 0.03f, 0.4f, 1f));
                }
                self.Position = num3;
            }
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
            Audio.Play("event:/game/04_cliffside/arrowblock_activate", Position);
            moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
            moveSfx.Param("arrow_stop", 0f);
        }

        public override void Render()
        {
            Vector2 num3 = Position;
            Position += shake;
            base.Render();
            Color color = Color.White;
            color.A = 200;
            int num = (int)(Width / 8f);
            int num2 = (int)(Height / 8f);
            ninSlice[0, 0].Draw(Position + new Vector2(0f, 0f), Vector2.Zero, color);
            ninSlice[2, 0].Draw(Position + new Vector2(Width - 8f, 0f), Vector2.Zero, color);
            ninSlice[0, 2].Draw(Position + new Vector2(0f, Height - 8f), Vector2.Zero, color);
            ninSlice[2, 2].Draw(Position + new Vector2(Width - 8f, Height - 8f), Vector2.Zero, color);
            for (int i = 1; i < num - 1; i++)
            {
                ninSlice[1, 0].Draw(Position + new Vector2(i * 8, 0f), Vector2.Zero, color);
                ninSlice[1, 2].Draw(Position + new Vector2(i * 8, Height - 8f), Vector2.Zero, color);
            }
            for (int j = 1; j < num2 - 1; j++)
            {
                ninSlice[0, 1].Draw(Position + new Vector2(0f, j * 8), Vector2.Zero, color);
                ninSlice[2, 1].Draw(Position + new Vector2(Width - 8f, j * 8), Vector2.Zero, color);
            }
            for (int k = 1; k < num - 1; k++)
            {
                for (int l = 1; l < num2 - 1; l++)
                {
                    ninSlice[1, 1].Draw(Position + new Vector2(k, l) * 8f, Vector2.Zero, color);
                }
            }
            MTexture tex = arrow;

            float ang = angle;
            if (dying)
            {
                tex = deadsprite;
                ang = 0;
            }
            tex.DrawOutlineCentered(Center, Color.Black, 1, ang);
            tex.DrawCentered(Center, Color.White, 1, ang);
            Position = num3;

        }
    }
}
