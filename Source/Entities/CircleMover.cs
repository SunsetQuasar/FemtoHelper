using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/CircleMover")]
public class CircleMover : Solid
{
    private static Color _ropeColor = Calc.HexToColor("663931");

    private static Color _ropeLightColor = Calc.HexToColor("9b6157");

    private class CircleMoverPathRenderer : Entity
    {
        public CircleMover CircleMover;

        private MTexture cog;

        private List<MTexture> glob;

        private Vector2 from;

        private Vector2 to;

        private Vector2 sparkAdd;

        private float sparkDirFromA;

        private float sparkDirFromB;

        private float sparkDirToA;

        private float sparkDirToB;

        public CircleMoverPathRenderer(CircleMover zipMover)
        {
            Depth = 5000;
            CircleMover = zipMover;

            from = CircleMover.centerNode - Calc.AngleToVector(CircleMover.angleStart, CircleMover.length) + new Vector2(CircleMover.Width / 2f, CircleMover.Height / 2f);
            to = CircleMover.centerNode - Calc.AngleToVector(CircleMover.angleEnd, CircleMover.length) + new Vector2(CircleMover.Width / 2f, CircleMover.Height / 2f);

            sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
            float num = (from - to).Angle();
            sparkDirFromA = num + MathF.PI / 8f;
            sparkDirFromB = num - MathF.PI / 8f;
            sparkDirToA = num + MathF.PI - MathF.PI / 8f;
            sparkDirToB = num + MathF.PI + MathF.PI / 8f;
            cog = GFX.Game[CircleMover.Prefix + "cog" + (CircleMover.behavior == Behaviors.NoReturn ? "NoReturn" : "")];
            glob = GFX.Game.GetAtlasSubtextures(CircleMover.Prefix + "path" + (CircleMover.behavior == Behaviors.NoReturn ? "NoReturn" : ""));
        }

        public void CreateSparks()
        {
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
            SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
        }
        public override void Render()
        {
            DrawCogs(Vector2.UnitY, Color.Black);
            DrawCogs(Vector2.Zero);
            Draw.Rect(new Rectangle((int)(CircleMover.X + CircleMover.Shake.X - 1f), (int)(CircleMover.Y + CircleMover.Shake.Y - 1f), (int)CircleMover.Width + 2, (int)CircleMover.Height + 2), Color.Black);
        }
        private void DrawCogs(Vector2 offset, Color? colorOverride = null)
        {
            float rotation = CircleMover.percent * MathF.PI * 2f;
            float degStart = CircleMover.angleStart * Calc.RadToDeg;
            float degEnd = CircleMover.angleEnd * Calc.RadToDeg;
            float mult = 1;
            if (degEnd - degStart < 0)
            {
                (degEnd, degStart) = (degStart, degEnd);
                mult = -1;
            }
            for (float deg = degStart; deg < degEnd; deg += 400 / CircleMover.length)
            {
                //Draw.Rect(new Vector2(circleMover.Width / 2, circleMover.Height / 2) + circleMover.CenterNode - Calc.AngleToVector(deg * Calc.DegToRad, circleMover.length), 1, 1, Color.Bisque);
                glob[(int)Mod(rotation * 5 * mult, glob.Count)].DrawCentered(offset + new Vector2(CircleMover.Width / 2, CircleMover.Height / 2) + CircleMover.centerNode - Calc.AngleToVector(deg * Calc.DegToRad, CircleMover.length),colorOverride.HasValue ? colorOverride.Value : Color.White, 1, deg * Calc.DegToRad);
            }
            cog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
            cog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
        }
    }

    public class DebugVisual : Entity
    {
        public CircleMover Parent;
        public DebugVisual(CircleMover parent) : base()
        {
            this.Parent = parent;
            AddTag(TagsExt.SubHUD);
        }

        public override void Render()
        {
            base.Render();
            ActiveFont.Draw(Math.Abs(Calc.ToDeg(Parent.angleEnd - Parent.angleStart)).ToString() + "°", ((Parent.Position + (new Vector2(Parent.Width, Parent.Height) / 2) - Vector2.UnitY * 6) - (Scene as Level).Camera.Position) * 6, Vector2.One * 0.5f, Vector2.One, Color.Aquamarine, 4, Color.Black, 2, Color.Black);
            if (Parent.angleEnd - Parent.angleStart < 0)
            {
                ActiveFont.Draw("CCW", ((Parent.Position + (new Vector2(Parent.Width, Parent.Height) / 2) + Vector2.UnitY * 6) - (Scene as Level).Camera.Position) * 6, Vector2.One * 0.5f, Vector2.One, Color.Pink, 4, Color.Black, 2, Color.Black);
            }
            else
            {
                ActiveFont.Draw("CW", ((Parent.Position + (new Vector2(Parent.Width, Parent.Height) / 2) + Vector2.UnitY * 6) - (Scene as Level).Camera.Position) * 6, Vector2.One * 0.5f, Vector2.One, Color.GreenYellow, 4, Color.Black, 2, Color.Black);
            }
        }
    }

    private readonly Vector2 centerNode;

    private readonly float length;

    private readonly float angleStart;

    private readonly float angleEnd;

    private float percent;

    private readonly SoundSource sfx = new SoundSource();

    private readonly MTexture[,] edges = new MTexture[3, 3];
    private readonly MTexture chainTex;

    private readonly Image centerGem;
    private Image centerRing;

    private BloomPoint gemGlow;

    private bool idle = true;
    private float glowPercent = 0;
    private readonly string chainZipperFlag;

    private bool activateSignal = false;
    private Color gemTint => Color.Lerp(Color.DimGray, Color.White, Ease.SineInOut(glowPercent));

    private CircleMoverPathRenderer pathRenderer;

    private bool permanented = false;

    private readonly bool threeSixtyLoop;

    private readonly bool activateFallingBlocks;

    private enum Behaviors
    {
        Default,
        NoReturn,
        Permanent
    }

    private Behaviors behavior;

    public string Prefix;

    public CircleMover(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {
        SurfaceSoundIndex = 9;

        centerNode = data.Nodes[0] + offset;

        angleStart = Calc.Angle(data.Position + offset, centerNode);
        angleStart = Calc.WrapAngle(angleStart);

        angleEnd = angleStart + (data.Float("angle", 180) * Calc.DegToRad) * (data.Bool("counterClockwise", false) ? -1 : 1);

        length = (Position - centerNode).Length();

        Add(new Coroutine(Sequence()));

        sfx.Position = new Vector2(Width, Height) / 2f;
        Add(sfx);

        chainZipperFlag = data.Attr("chainZipperFlag", "");
        behavior = data.Enum("behavior", Behaviors.Default);
        threeSixtyLoop = data.Bool("ThreeSixtyLoops", false);
        activateFallingBlocks = data.Bool("activateFallingBlocks", false);

        Prefix = data.Attr("spritePath", "objects/FemtoHelper/circleMover/");

        string id = Prefix + "block";
        string chain = Prefix + "chain";

        if (behavior == Behaviors.NoReturn)
        {
            id += "NoReturn";
            chain += "NoReturn";
        }

        chainTex = GFX.Game[chain];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                edges[i, j] = GFX.Game[id].GetSubtexture(i * 8, j * 8, 8, 8);
            }
        }

        Add(centerGem = new Image(GFX.Game[Prefix + "centerGem" + (behavior == Behaviors.NoReturn ? "NoReturn" : "")]) 
        { 
            Position = new Vector2(Width, Height) / 2
        });
        centerGem.CenterOrigin();

        Add(centerRing = new Image(GFX.Game[Prefix + "centerRing" + (behavior == Behaviors.NoReturn ? "NoReturn" : "")])
        {
            Position = new Vector2(Width, Height) / 2
        });
        centerRing.CenterOrigin();

        Add(gemGlow = new BloomPoint(0.4f, 8f) 
        { 
            Position = new Vector2(Width, Height) / 2
        });
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        //Scene.Add(new DebugVisual(this));
        Scene.Add(pathRenderer = new CircleMoverPathRenderer(this));
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

        Draw.Rect(X + 1f, Y + 1f, Width - 2f, Height - 2f, Color.Black);

        for (int k = 0; (float)k + 1 < Width / 8f; k++)
        {
            float offset = Mod(4 - (k * 3) + (percent * ((k % 2) == 0 ? -1 : 1)) * 16, 8);
            chainTex.GetSubtexture(0, (int)MathF.Round(8 - offset), 8, (int)MathF.Round(offset)).Draw(new Vector2(4 + X + (float)(k * 8), Y), Vector2.Zero, Calc.HexToColor("666262"));
            for (int l = 0; (float)l < (Height / 8f) - 1f; l++)
            {
                chainTex.Draw(new Vector2(4 + X + (float)(k * 8), offset + Y + (float)(l * 8)), Vector2.Zero, Calc.HexToColor("666262"));
            }
            chainTex.GetSubtexture(0, 0, 8, (int)MathF.Round(8 - offset)).Draw(new Vector2(4 + X + (float)(k * 8), offset + Y + Height - 8), Vector2.Zero, Calc.HexToColor("666262"));
        }

        for (int k = 0; (float)k < Width / 8f; k++)
        {
            float offset = Mod((k * 3) + (percent * ((k % 2) == 0 ? 1 : -1)) * 32, 8);
            chainTex.GetSubtexture(0, (int)MathF.Round(8 - offset), 8, (int)MathF.Round(offset)).Draw(new Vector2(X + (float)(k * 8), Y));
            for (int l = 0; (float)l < (Height / 8f) - 1f; l++)
            {
                chainTex.Draw(new Vector2(X + (float)(k * 8), offset + Y + (float)(l * 8)));
            }
            chainTex.GetSubtexture(0, 0, 8, (int)MathF.Round(8 - offset)).Draw(new Vector2(X + (float)(k * 8), offset + Y + Height - 8));
        }


        for (int k = 0; (float)k < Width / 8f; k++)
        {
            for (int l = 0; (float)l < Height / 8f; l++)
            {
                int num4 = ((k != 0) ? (((float)k != Width / 8f - 1f) ? 1 : 2) : 0);
                int num5 = ((l != 0) ? (((float)l != Height / 8f - 1f) ? 1 : 2) : 0);
                if (num4 != 1 || num5 != 1)
                {
                    edges[num4, num5].Draw(new Vector2(X + (float)(k * 8), Y + (float)(l * 8)));
                }
            }
        }
        base.Render();
        Position = pos;
    }

    private void ScrapeParticlesCheck(Vector2 to)
    {
        if (!Scene.OnInterval(0.03f))
        {
            return;
        }
        bool flag = to.Y != ExactPosition.Y;
        bool flag2 = to.X != ExactPosition.X;
        if (flag && !flag2)
        {
            int num = Math.Sign(to.Y - ExactPosition.Y);
            Vector2 vector = ((num != 1) ? TopLeft : BottomLeft);
            int num2 = 4;
            if (num == 1)
            {
                num2 = Math.Min((int)Height - 12, 20);
            }
            int num3 = (int)Height;
            if (num == -1)
            {
                num3 = Math.Max(16, (int)Height - 16);
            }
            if (Scene.CollideCheck<Solid>(vector + new Vector2(-2f, num * -2)))
            {
                for (int i = num2; i < num3; i += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, TopLeft + new Vector2(0f, (float)i + (float)num * 2f), (num == 1) ? (-MathF.PI / 4f) : (MathF.PI / 4f));
                }
            }
            if (Scene.CollideCheck<Solid>(vector + new Vector2(Width + 2f, num * -2)))
            {
                for (int j = num2; j < num3; j += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, TopRight + new Vector2(-1f, (float)j + (float)num * 2f), (num == 1) ? (MathF.PI * -3f / 4f) : (MathF.PI * 3f / 4f));
                }
            }
        }
        else
        {
            int num4 = Math.Sign(to.X - ExactPosition.X);
            Vector2 vector2 = ((num4 != 1) ? TopLeft : TopRight);
            int num5 = 4;
            if (num4 == 1)
            {
                num5 = Math.Min((int)Width - 12, 20);
            }
            int num6 = (int)Width;
            if (num4 == -1)
            {
                num6 = Math.Max(16, (int)Width - 16);
            }
            if (Scene.CollideCheck<Solid>(vector2 + new Vector2(num4 * -2, -2f)))
            {
                for (int k = num5; k < num6; k += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, TopLeft + new Vector2((float)k + (float)num4 * 2f, -1f), (num4 == 1) ? (MathF.PI * 3f / 4f) : (MathF.PI / 4f));
                }
            }
            if (Scene.CollideCheck<Solid>(vector2 + new Vector2(num4 * -2, Height + 2f)))
            {
                for (int l = num5; l < num6; l += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, BottomLeft + new Vector2((float)l + (float)num4 * 2f, 0f), (num4 == 1) ? (MathF.PI * -3f / 4f) : (-MathF.PI / 4f));
                }
            }
        }
    }

    public override void OnStaticMoverTrigger(StaticMover sm)
    {
        base.OnStaticMoverTrigger(sm);
        activateSignal = true;
    }

    public IEnumerator Sequence()
    {
        while (true)
        {
            if (!(activateSignal || HasPlayerRider()))
            {
                yield return null;
                continue;
            }
            activateSignal = false;
            idle = false;
            sfx.Play("event:/game/01_forsaken_city/zip_mover");
            StartShaking(0.1f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            yield return 0.1f;
            float angle = angleStart;

            float at = 0f;
            while (at < 1f)
            {
                yield return null;
                at = Calc.Approach(at, 1f, 2f * Engine.DeltaTime);
                percent = Ease.SineIn(at);
                centerRing.Rotation = 2 * -MathF.Tau * percent;
                angle = Calc.LerpClamp(angleStart, angleEnd, percent);
                Vector2 pos = centerNode - Calc.AngleToVector(angle, length);
                ScrapeParticlesCheck(pos);
                MoveTo(pos);
                if (Scene.OnInterval(0.1f))
                {
                    pathRenderer.CreateSparks();
                }
            }
            percent = 1;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().Shake();
            StartShaking(0.2f);
            Collider temp = Collider;
            Collider = new Hitbox(Width  + 2, Height + 2, -1, -1);
            if (activateFallingBlocks)
            {
                foreach (FallingBlock f in CollideAll<FallingBlock>())
                {
                    f.Triggered = true;
                }
            }
            if (SceneAs<Level>().Session.GetFlag(chainZipperFlag) && !string.IsNullOrEmpty(chainZipperFlag))
            {
                foreach (CircleMover c in CollideAll<CircleMover>())
                {
                    c.activateSignal = true;
                }
            }
            Collider = temp;
            activateSignal = false;
            yield return 0.5f;
            if (behavior == Behaviors.Permanent)
            {
                sfx.Stop();
                yield return 0.5f;
                StartShaking(0.2f);
                gemGlow.Alpha = 0;
                permanented = idle = true;
                yield break;
            }
            else if (behavior == Behaviors.NoReturn)
            {
                idle = true;
                sfx.Stop();
                if (threeSixtyLoop && Math.Abs(angleEnd - angleStart) == 360) continue;
                activateSignal = false;
                while (!(activateSignal || HasPlayerRider()))
                {
                    yield return null;
                }
                activateSignal = false;
                idle = false;
                sfx.Play("event:/game/01_forsaken_city/zip_mover");
                StartShaking(0.1f);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                yield return 0.1f;
                angle = angleStart;

                at = 1f;
                while (at > 0f)
                {
                    yield return null;
                    at = Calc.Approach(at, 0f, 2f * Engine.DeltaTime);
                    percent = Ease.SineOut(at);
                    centerRing.Rotation = 2 * -MathF.Tau * percent;
                    angle = Calc.LerpClamp(angleStart, angleEnd, percent);
                    Vector2 pos = centerNode - Calc.AngleToVector(angle, length);
                    ScrapeParticlesCheck(pos);
                    MoveTo(pos);
                    if (Scene.OnInterval(0.1f))
                    {
                        pathRenderer.CreateSparks();
                    }
                }
                percent = 0;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().Shake();
                StartShaking(0.2f);
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
                    foreach (CircleMover c in CollideAll<CircleMover>())
                    {
                        c.activateSignal = true;
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
                if (threeSixtyLoop && Math.Abs(angleEnd - angleStart) == 360)
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
                    centerRing.Rotation = 2 * -MathF.Tau * percent;
                    angle = Calc.LerpClamp(angleStart, angleEnd, percent);
                    Vector2 pos = centerNode - Calc.AngleToVector(angle, length);
                    MoveTo(pos);
                }
                percent = 0;
                StartShaking(0.2f);
                idle = true;
                yield return 0.5f;
                activateSignal = false;
            }
        }
    }

    private static float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
