using Celeste.Mod.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Celeste.MoonGlitchBackgroundTrigger;

namespace Celeste.Mod.FemtoHelper.Entities;

[TrackedAs(typeof(Water))]
[CustomEntity("FemtoHelper/WaterZipMover")]
public class WaterZipMover : GenericWaterBlock
{
    public static Color RopeLightColor = Calc.HexToColor("2C5560") * (93f / 255f);

    private class WaterZipMoverPathRenderer : Entity
    {
        public WaterZipMover WaterZipMover;

        private MTexture cog;

        private Vector2 from;

        private Vector2 to;

        private Vector2 sparkAdd;

        private float sparkDirFromA;

        private float sparkDirFromB;

        private float sparkDirToA;

        private float sparkDirToB;

        public Wiggler CogWiggler;
        public Vector2 CogScale = Vector2.One;
        private float timePass;

        public WaterZipMoverPathRenderer(WaterZipMover zipMover, string path)
        {
            base.Depth = 5000;
            WaterZipMover = zipMover;
            from = WaterZipMover.start + new Vector2(WaterZipMover.Width / 2f, WaterZipMover.Height / 2f);
            to = WaterZipMover.target + new Vector2(WaterZipMover.Width / 2f, WaterZipMover.Height / 2f);
            sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
            float num = (from - to).Angle();
            sparkDirFromA = num + MathF.PI / 8f;
            sparkDirFromB = num - MathF.PI / 8f;
            sparkDirToA = num + MathF.PI - MathF.PI / 8f;
            sparkDirToB = num + MathF.PI + MathF.PI / 8f;
            cog = GFX.Game[$"{path}cog"];
            Add(CogWiggler = Wiggler.Create(0.6f, 4f, (t) =>
            {
                CogScale = Vector2.One + new Vector2(t, -t) * 0.5f;
            }));
            timePass = Calc.Random.Range(0, MathF.Tau);
        }

        public override void Update()
        {
            base.Update();
            timePass += Engine.DeltaTime;
        }

        public void CreateSparks()
        {
            SceneAs<Level>().ParticlesBG.Emit(Tinydrops, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
            SceneAs<Level>().ParticlesBG.Emit(Tinydrops, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
            SceneAs<Level>().ParticlesBG.Emit(Tinydrops, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
            SceneAs<Level>().ParticlesBG.Emit(Tinydrops, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
        }

        
        public override void Render()
        {
            //DrawCogs(Vector2.UnitY, Color.Black);
            DrawCogs(Vector2.Zero);
        }

        
        private void DrawCogs(Vector2 offset)
        {
            Vector2 vector = (to - from).SafeNormalize();
            Vector2 vector2 = vector.Perpendicular() * 3f;
            Vector2 vector3 = -vector.Perpendicular() * 4f;
            float rotation = WaterZipMover.percent * MathF.PI * 2f;
            //Draw.Line(from + vector2 + offset, to + vector2 + offset, ropeColor);
            //Draw.Line(from + vector3 + offset, to + vector3 + offset, ropeColor);
            for (float num = 4f - WaterZipMover.percent * MathF.PI * 8f % 4f; num < (to - from).Length(); num += 4f)
            {
                Vector2 vector4 = from + vector2 + vector.Perpendicular() + vector * num;
                Vector2 vector5 = to + vector3 - vector * num;

                vector4 += vector.Perpendicular() * MathF.Sin(((MathF.PI * 320f % 4f) * WaterZipMover.percent) + timePass + num / 5) * 1;
                vector5 += vector.Perpendicular() * MathF.Sin(((MathF.PI * 320f % 4f) * WaterZipMover.percent) + timePass + num / 5) * 1;

                Draw.Line(vector4 + offset, vector4 + vector * 2f + offset, RopeLightColor, 3 + 3 * MathF.Sin(((MathF.PI * 480f % 4f) * WaterZipMover.percent) - (timePass * 0.7f) + num / 4));
                Draw.Line(vector5 + offset, vector5 - vector * 2f + offset, RopeLightColor, 3 + 3 * MathF.Cos(((MathF.PI * 480f % 4f) * WaterZipMover.percent) - (timePass * 0.7f) + num / 4));
            }
            cog.DrawCentered(from + offset, Color.White, CogScale, rotation);
            cog.DrawCentered(to + offset, Color.White, CogScale, rotation);
        }
    }

    private readonly WaterSprite waterSprite;

    private readonly Vector2 start, target;

    private SoundSource sfx = new SoundSource();

    private float percent;

    private WaterZipMoverPathRenderer pathRenderer;

    private string texturePath;

    private Sprite cornerL, cornerR;

    private Wiggler cornerWiggler;

    private BloomPoint bloomL, bloomR, bloomStart, bloomTarget;

    //private readonly Image centerNoReturn, centerPermanent;

    private enum Behaviors
    {
        Default,
        NoReturn,
        Permanent
    }

    private Behaviors behavior;

    public WaterZipMover(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, data.Bool("canCarry", true))
    {
        texturePath = data.Attr("texturePath", "objects/FemtoHelper/waterZipMover/");

        Add(waterSprite = new WaterSprite(texturePath + "nineSlice"));
        start = data.Position + offset;
        target = data.Nodes[0] + offset;
        Add(new Coroutine(Sequence()));
        sfx.Position = new Vector2(base.Width, base.Height) / 2f;
        Add(sfx);

        behavior = data.Enum("behavior", Behaviors.Default);

        Image center = null;
        if (behavior == Behaviors.NoReturn)
        {
            MTexture tex = GFX.Game[$"{texturePath}noReturn"];
            Add(center = new Image(tex)
            {
                Position = new Vector2(Width, Height) / 2,
                Origin = new Vector2(tex.Width, tex.Height) / 2
            });
        }
        else if (behavior == Behaviors.Permanent)
        {
            MTexture tex = GFX.Game[$"{texturePath}permanent"];
            Add(center = new Image(tex)
            {
                Position = new Vector2(Width, Height) / 2,
                Origin = new Vector2(tex.Width, tex.Height) / 2
            });
        }

        Add(cornerL = new Sprite(GFX.Game, texturePath));
        Add(bloomL = new BloomPoint(0.5f, 6f));
        cornerL.Add("frames", "cornerL", 1f);
        cornerL.Play("frames");
        cornerL.Active = false;
        cornerL.SetAnimationFrame(1);

        cornerL.Position = Vector2.Zero;
        bloomL.Position = new Vector2(2, 3);

        Add(cornerR = new Sprite(GFX.Game, texturePath));
        Add(bloomR = new BloomPoint(0.5f, 6f));
        cornerR.Add("frames", "cornerR", 1f);
        cornerR.Active = false;
        cornerR.Play("frames");
        cornerR.SetAnimationFrame(1);

        cornerR.Position = new Vector2(Width, 0) - Vector2.UnitX * 8;
        bloomR.Position = new Vector2(Width, 0) + new Vector2(6, 3) - Vector2.UnitX * 8;

        Add(cornerWiggler = Wiggler.Create(0.5f, 3f, (t) =>
        {
            bloomL.Position = new Vector2(t * 4, -t * 4).Round() + new Vector2(2, 3);
            cornerL.Position = new Vector2(t * 4, -t * 4).Round();

            bloomR.Position = ((new Vector2(Width, 0) - Vector2.UnitX * 8) + new Vector2(-t * 4, -t * 4)).Round() + new Vector2(6, 3);
            cornerR.Position = ((new Vector2(Width, 0) - Vector2.UnitX * 8) + new Vector2(-t * 4, -t * 4)).Round();

            if (center != null)
            {
                center.Scale = Vector2.One + new Vector2(t, -t) * 0.5f;
            }
        }));

        Add(bloomStart = new BloomPoint(0.5f, 12f)
        {
            Position = (start + new Vector2(Width, Height) / 2) - this.Position
        });
        Add(bloomTarget = new BloomPoint(0.5f, 12f)
        {
            Position = (target + new Vector2(Width, Height) / 2) - this.Position
        });
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(pathRenderer = new WaterZipMoverPathRenderer(this, texturePath));
    }

    private void WiggleEverything(float dur = 0.6f, float freq = 4)
    {
        StartShaking(0.2f);
        cornerWiggler.Start();
        waterSprite.Wiggle.Start();
        pathRenderer.CogWiggler.increment = 1f / dur;
        pathRenderer.CogWiggler.sineAdd = MathF.PI * 2f * freq;
        pathRenderer.CogWiggler.Start();
    }

    private IEnumerator Sequence()
    {
        Vector2 start = Position;
    label_start:
        while (true)
        {
            if (!CollideCheck<Player>())
            {
                yield return null;
                continue;
            }
            sfx.Play("event:/FemtoHelper/water_zip_mover");
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            WiggleEverything();
            yield return 0.1f;
            cornerL.SetAnimationFrame(3);
            cornerR.SetAnimationFrame(3);
            float at = 0f;
            while (at < 1f)
            {
                yield return null;
                at = Calc.Approach(at, 1f, 2f * Engine.DeltaTime);
                percent = Ease.SineIn(at);
                Vector2 vector = Vector2.Lerp(start, target, percent);
                //ScrapeParticlesCheck(vector);
                if (Scene.OnInterval(0.1f))
                {
                    //pathRenderer.CreateSparks();
                }
                if (Scene.OnInterval(0.04f))
                {
                    Vector2 pos2 = Position + new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                    SceneAs<Level>().ParticlesFG.Emit(Tinydrops, pos2, (pos2 - Center).Angle());
                }
                MoveTo(vector);
                bloomStart.Position = (this.start + new Vector2(Width, Height) / 2) - this.Position;
                bloomTarget.Position = (target + new Vector2(Width, Height) / 2) - this.Position;
            }
            WiggleEverything();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().Shake();
            yield return 0.5f;
            if (behavior == Behaviors.Permanent)
            {
                cornerL.SetAnimationFrame(1);
                cornerR.SetAnimationFrame(1);
                sfx.Stop();
                yield return 0.5f;
                sfx.Play("event:/FemtoHelper/water_zip_mover_permanent");
                WiggleEverything(0.4f, 2f);
                cornerL.SetAnimationFrame(0);
                cornerR.SetAnimationFrame(0);
                bloomL.Alpha = 0;
                bloomR.Alpha = 0;
                yield break;
            }
            else if (behavior == Behaviors.NoReturn)
            {
                cornerL.SetAnimationFrame(1);
                cornerR.SetAnimationFrame(1);
                sfx.Stop();
                while (!CollideCheck<Player>())
                {
                    yield return null;
                }
                sfx.Play("event:/FemtoHelper/water_zip_mover");
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                WiggleEverything();
                yield return 0.1f;
                cornerL.SetAnimationFrame(3);
                cornerR.SetAnimationFrame(3);
                at = 1f;
                while (at > 0f)
                {
                    yield return null;
                    at = Calc.Approach(at, 0f, 2f * Engine.DeltaTime);
                    percent = Ease.SineOut(at);
                    Vector2 vector = Vector2.Lerp(start, target, percent);
                    //ScrapeParticlesCheck(vector);
                    if (Scene.OnInterval(0.1f))
                    {
                        //pathRenderer.CreateSparks();
                    }
                    if (Scene.OnInterval(0.04f))
                    {
                        Vector2 pos2 = Position + new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                        SceneAs<Level>().ParticlesFG.Emit(Tinydrops, pos2, (pos2 - Center).Angle());
                    }
                    MoveTo(vector);
                    bloomStart.Position = (this.start + new Vector2(Width, Height) / 2) - this.Position;
                    bloomTarget.Position = (target + new Vector2(Width, Height) / 2) - this.Position;
                }
                WiggleEverything();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().Shake();
                yield return 0.5f;
                cornerL.SetAnimationFrame(1);
                cornerR.SetAnimationFrame(1);
                sfx.Stop();
                goto label_start;
            }
            else
            {
                cornerL.SetAnimationFrame(2);
                cornerR.SetAnimationFrame(2);
                at = 0f;
                while (at < 1f)
                {
                    yield return null;
                    at = Calc.Approach(at, 1f, 0.5f * Engine.DeltaTime);
                    percent = 1f - Ease.SineIn(at);
                    Vector2 position = Vector2.Lerp(target, start, Ease.SineIn(at));
                    if (Scene.OnInterval(0.04f))
                    {
                        Vector2 pos2 = Position + new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                        SceneAs<Level>().ParticlesFG.Emit(Tinydrops, pos2, (pos2 - Center).Angle());
                    }
                    MoveTo(position);
                    bloomStart.Position = (this.start + new Vector2(Width, Height) / 2) - this.Position;
                    bloomTarget.Position = (target + new Vector2(Width, Height) / 2) - this.Position;
                }
                WiggleEverything(0.4f, 2f);
                cornerL.SetAnimationFrame(1);
                cornerR.SetAnimationFrame(1);
                yield return 0.5f;
            }
        }
    }

    public override void Render()
    {
        Vector2 pos = Position;
        Position += Shake;
        base.Render();
        Position = pos;
    }
}
