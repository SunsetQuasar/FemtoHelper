﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[TrackedAs(typeof(Water))]
[CustomEntity("FemtoHelper/WaterCircleMover")]
public class WaterCircleMover : GenericWaterBlock
{
    private class WaterCircleMoverPathRenderer : Entity
    {
        public WaterCircleMover circleMover;

        private MTexture cog;

        private List<MTexture> glob;

        private Vector2 from;

        private Vector2 to;

        private Vector2 sparkAdd;

        private float sparkDirFromA;

        private float sparkDirFromB;

        private float sparkDirToA;

        private float sparkDirToB;

        public Wiggler cogWiggler;
        public Vector2 cogScale = Vector2.One;

        public WaterCircleMoverPathRenderer(WaterCircleMover zipMover)
        {
            base.Depth = 5000;
            circleMover = zipMover;

            from = circleMover.CenterNode - Calc.AngleToVector(circleMover.angleStart, circleMover.length) + new Vector2(circleMover.Width / 2f, circleMover.Height / 2f);
            to = circleMover.CenterNode - Calc.AngleToVector(circleMover.angleEnd, circleMover.length) + new Vector2(circleMover.Width / 2f, circleMover.Height / 2f);

            sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
            float num = (from - to).Angle();
            sparkDirFromA = num + MathF.PI / 8f;
            sparkDirFromB = num - MathF.PI / 8f;
            sparkDirToA = num + MathF.PI - MathF.PI / 8f;
            sparkDirToB = num + MathF.PI + MathF.PI / 8f;
            cog = GFX.Game[circleMover.prefix + "cog" + (circleMover.behavior == Behaviors.NoReturn ? "NoReturn" : "")];
            glob = GFX.Game.GetAtlasSubtextures(circleMover.prefix + "path" + (circleMover.behavior == Behaviors.NoReturn ? "NoReturn" : ""));

            Add(cogWiggler = Wiggler.Create(0.6f, 4f, (t) =>
            {
                cogScale = Vector2.One + new Vector2(t, -t) * 0.5f;
            }));
        }
        public override void Render()
        {
            DrawCogs(Vector2.Zero);
        }
        private void DrawCogs(Vector2 offset, Color? colorOverride = null)
        {
            float rotation = circleMover.percent * MathF.PI * 2f;
            float degStart = circleMover.angleStart * Calc.RadToDeg;
            float degEnd = circleMover.angleEnd * Calc.RadToDeg;
            float mult = 1;
            if (degEnd - degStart < 0)
            {
                (degEnd, degStart) = (degStart, degEnd);
                mult = -1;
            }
            for (float deg = degStart; deg < degEnd; deg += 400 / circleMover.length)
            {
                //Draw.Rect(new Vector2(circleMover.Width / 2, circleMover.Height / 2) + circleMover.CenterNode - Calc.AngleToVector(deg * Calc.DegToRad, circleMover.length), 1, 1, Color.Bisque);
                glob[(int)mod(rotation * 5 * mult, glob.Count)].DrawCentered(offset + new Vector2(circleMover.Width / 2, circleMover.Height / 2) + circleMover.CenterNode - Calc.AngleToVector(deg * Calc.DegToRad, circleMover.length), colorOverride.HasValue ? colorOverride.Value : Color.White, 1, deg * Calc.DegToRad);
            }
            cog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, cogScale, rotation);
            cog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, cogScale, rotation);
        }
    }

    public class DebugVisual : Entity
    {
        public WaterCircleMover parent;
        public DebugVisual(WaterCircleMover Parent) : base()
        {
            parent = Parent;
            AddTag(TagsExt.SubHUD);
        }

        public override void Render()
        {
            base.Render();
            ActiveFont.Draw(Math.Abs(Calc.ToDeg(parent.angleEnd - parent.angleStart)).ToString() + "°", ((parent.Position + (new Vector2(parent.Width, parent.Height) / 2) - Vector2.UnitY * 6) - (Scene as Level).Camera.Position) * 6, Vector2.One * 0.5f, Vector2.One, Color.Aquamarine, 4, Color.Black, 2, Color.Black);
            if (parent.angleEnd - parent.angleStart < 0)
            {
                ActiveFont.Draw("CCW", ((parent.Position + (new Vector2(parent.Width, parent.Height) / 2) + Vector2.UnitY * 6) - (Scene as Level).Camera.Position) * 6, Vector2.One * 0.5f, Vector2.One, Color.Pink, 4, Color.Black, 2, Color.Black);
            }
            else
            {
                ActiveFont.Draw("CW", ((parent.Position + (new Vector2(parent.Width, parent.Height) / 2) + Vector2.UnitY * 6) - (Scene as Level).Camera.Position) * 6, Vector2.One * 0.5f, Vector2.One, Color.GreenYellow, 4, Color.Black, 2, Color.Black);
            }
        }
    }

    private readonly Vector2 CenterNode;

    private readonly float length;

    private readonly float angleStart;

    private readonly float angleEnd;

    private float percent;

    private readonly SoundSource sfx = new SoundSource();

    private readonly Image centerGem;
    private Image centerRing;

    private BloomPoint gemGlow;

    private bool idle = true;
    private float glowPercent = 0;
    private readonly string chainZipperFlag;

    private bool activateSignal = false;
    private Color gemTint => Color.Lerp(Color.DimGray, Color.White, Ease.SineInOut(glowPercent));

    private WaterCircleMoverPathRenderer pathRenderer;

    private bool permanented = false;

    private readonly bool ThreeSixtyLoop;

    private readonly bool activateFallingBlocks;
    private readonly WaterSprite waterSprite;

    private readonly Wiggler bubbleWiggler;

    private BloomPoint bloomStart, bloomTarget;

    public string prefix;

    private enum Behaviors
    {
        Default,
        NoReturn,
        Permanent
    }

    private Behaviors behavior;

    public WaterCircleMover(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, data.Bool("canCarry", true))
    {
        prefix = data.Attr("spritePath", "objects/FemtoHelper/circleMover/water/");

        CenterNode = data.Nodes[0] + offset;

        angleStart = Calc.Angle(data.Position + offset, CenterNode);
        angleStart = Calc.WrapAngle(angleStart);

        angleEnd = angleStart + (data.Float("angle", 180) * Calc.DegToRad) * (data.Bool("counterClockwise", false) ? -1 : 1);

        length = (Position - CenterNode).Length();

        Add(waterSprite = new WaterSprite(prefix + "nineSlice"));

        Add(new Coroutine(Sequence()));

        sfx.Position = new Vector2(base.Width, base.Height) / 2f;
        Add(sfx);

        chainZipperFlag = data.Attr("chainZipperFlag", "");
        behavior = data.Enum("behavior", Behaviors.Default);
        ThreeSixtyLoop = data.Bool("ThreeSixtyLoops", false);
        activateFallingBlocks = data.Bool("activateFallingBlocks", false);

        Add(centerGem = new Image(GFX.Game[prefix + "centerGem" + (behavior == Behaviors.NoReturn ? "NoReturn" : "")])
        {
            Position = new Vector2(Width, Height) / 2
        });
        centerGem.CenterOrigin();

        Add(centerRing = new Image(GFX.Game[prefix + "centerRing" + (behavior == Behaviors.NoReturn ? "NoReturn" : "")])
        {
            Position = new Vector2(Width, Height) / 2
        });
        centerRing.CenterOrigin();

        Add(gemGlow = new BloomPoint(0.4f, 8f)
        {
            Position = new Vector2(Width, Height) / 2
        });

        Add(bubbleWiggler = Wiggler.Create(0.6f, 4f, (t) =>
        {
            centerRing.Scale = Vector2.One + new Vector2(t, -t) * 0.5f;
        }));

        Add(bloomStart = new BloomPoint(0.5f, 12f)
        {
            Position = ((CenterNode - Calc.AngleToVector(angleStart, length)) + new Vector2(Width, Height) / 2) - this.Position
        });
        Add(bloomTarget = new BloomPoint(0.5f, 12f)
        {
            Position = ((CenterNode - Calc.AngleToVector(angleEnd, length)) + new Vector2(Width, Height) / 2) - this.Position
        });
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        //Scene.Add(new DebugVisual(this));
        Scene.Add(pathRenderer = new WaterCircleMoverPathRenderer(this));
    }

    private void WiggleEverything(float dur = 0.6f, float freq = 4)
    {
        StartShaking(0.2f);
        waterSprite.Wiggle.Start();
        pathRenderer.cogWiggler.increment = 1f / dur;
        pathRenderer.cogWiggler.sineAdd = MathF.PI * 2f * freq;
        pathRenderer.cogWiggler.Start();

        bubbleWiggler.increment = 1f / dur;
        bubbleWiggler.sineAdd = MathF.PI * 2f * freq;
        bubbleWiggler.Start();
    }

    public override void Update()
    {
        centerGem.SetColor(gemTint);
        if (!permanented)
        {
            gemGlow.Alpha = Calc.LerpClamp(0.4f, 1f, Ease.SineInOut(glowPercent));
            gemGlow.Radius = Calc.LerpClamp(8, 12, Ease.SineInOut(glowPercent));
        }
        base.Update();
        if (idle)
        {
            glowPercent = Calc.Approach(glowPercent, 0, Engine.DeltaTime * 4);
        }
        else
        {
            glowPercent = Calc.Approach(glowPercent, 1, Engine.DeltaTime * 4);
        }
    }

    public override void Render()
    {
        Vector2 pos = Position;
        Position += Shake;
        base.Render();
        Position = pos;
    }
    public IEnumerator Sequence()
    {
        while (true)
        {
            if (!(activateSignal || CollideCheck<Player>()))
            {
                yield return null;
                continue;
            }
            activateSignal = false;
            idle = false;
            sfx.Play("event:/FemtoHelper/water_zip_mover");
            WiggleEverything();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            yield return 0.1f;
            float angle = angleStart;

            float at = 0f;
            while (at < 1f)
            {
                yield return null;
                at = Calc.Approach(at, 1f, 2f * Engine.DeltaTime);
                percent = Ease.SineIn(at);
                angle = Calc.LerpClamp(angleStart, angleEnd, percent);
                if (Scene.OnInterval(0.04f))
                {
                    Vector2 pos2 = Position + new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                    SceneAs<Level>().ParticlesFG.Emit(Tinydrops, pos2, (pos2 - Center).Angle());
                }
                Vector2 pos = CenterNode - Calc.AngleToVector(angle, length);
                MoveTo(pos);
                bloomStart.Position = ((CenterNode - Calc.AngleToVector(angleStart, length)) + new Vector2(Width, Height) / 2) - this.Position;
                bloomTarget.Position = ((CenterNode - Calc.AngleToVector(angleEnd, length)) + new Vector2(Width, Height) / 2) - this.Position;
            }
            percent = 1;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().Shake();
            WiggleEverything();
            Collider temp = Collider;
            Collider = new Hitbox(Width + 2, Height + 2, -1, -1);
            if (activateFallingBlocks)
            {
                foreach (FallingBlock f in CollideAll<FallingBlock>())
                {
                    f.Triggered = true;
                }
            }
            if (SceneAs<Level>().Session.GetFlag(chainZipperFlag) && !string.IsNullOrEmpty(chainZipperFlag))
            {
                foreach (Water c in CollideAll<Water>())
                {
                    if (c is WaterCircleMover wcm) wcm.activateSignal = true;
                }
            }
            Collider = temp;
            activateSignal = false;
            yield return 0.5f;
            if (behavior == Behaviors.Permanent)
            {
                sfx.Stop();
                yield return 0.5f;
                WiggleEverything(0.4f, 2f);
                gemGlow.Alpha = 0;
                permanented = idle = true;
                yield break;
            }
            else if (behavior == Behaviors.NoReturn)
            {
                idle = true;
                sfx.Stop();
                if (ThreeSixtyLoop && Math.Abs(angleEnd - angleStart) == 360) continue;
                activateSignal = false;
                while (!(activateSignal || CollideCheck<Player>()))
                {
                    yield return null;
                }
                activateSignal = false;
                idle = false;
                sfx.Play("event:/FemtoHelper/water_zip_mover");
                WiggleEverything();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                yield return 0.1f;
                angle = angleStart;

                at = 1f;
                while (at > 0f)
                {
                    yield return null;
                    at = Calc.Approach(at, 0f, 2f * Engine.DeltaTime);
                    percent = Ease.SineOut(at);
                    angle = Calc.LerpClamp(angleStart, angleEnd, percent);
                    if (Scene.OnInterval(0.04f))
                    {
                        Vector2 pos2 = Position + new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                        SceneAs<Level>().ParticlesFG.Emit(Tinydrops, pos2, (pos2 - Center).Angle());
                    }
                    Vector2 pos = CenterNode - Calc.AngleToVector(angle, length);
                    MoveTo(pos);
                    bloomStart.Position = ((CenterNode - Calc.AngleToVector(angleStart, length)) + new Vector2(Width, Height) / 2) - this.Position;
                    bloomTarget.Position = ((CenterNode - Calc.AngleToVector(angleEnd, length)) + new Vector2(Width, Height) / 2) - this.Position;
                }
                percent = 0;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().Shake();
                WiggleEverything();
                temp = Collider;
                Collider = new Hitbox(Width + 2, Height + 2, -1, -1);
                if (activateFallingBlocks)
                {
                    foreach (FallingBlock f in CollideAll<FallingBlock>())
                    {
                        f.Triggered = true;
                    }
                }
                if (SceneAs<Level>().Session.GetFlag(chainZipperFlag) && !string.IsNullOrEmpty(chainZipperFlag))
                {
                    foreach (Water c in CollideAll<Water>())
                    {
                        if (c is WaterCircleMover wcm) wcm.activateSignal = true;
                    }
                }
                Collider = temp;
                yield return 0.5f;
                activateSignal = false;
                idle = true;
                sfx.Stop();
            }
            else
            {
                if (ThreeSixtyLoop && Math.Abs(angleEnd - angleStart) == 360)
                {
                    sfx.Stop();
                    idle = true;
                    continue;
                }
                at = 0f;
                while (at < 1f)
                {
                    yield return null;
                    at = Calc.Approach(at, 1f, 0.5f * Engine.DeltaTime);
                    percent = 1f - Ease.SineIn(at);
                    angle = Calc.LerpClamp(angleStart, angleEnd, percent);
                    Vector2 pos = CenterNode - Calc.AngleToVector(angle, length);
                    MoveTo(pos);
                    bloomStart.Position = ((CenterNode - Calc.AngleToVector(angleStart, length)) + new Vector2(Width, Height) / 2) - this.Position;
                    bloomTarget.Position = ((CenterNode - Calc.AngleToVector(angleEnd, length)) + new Vector2(Width, Height) / 2) - this.Position;
                }
                percent = 0;
                WiggleEverything(0.4f, 2f);
                idle = true;
                yield return 0.5f;
                activateSignal = false;
            }
        }
    }

    private static float mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
