using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

public class SwitchMovetBeam : Entity
{
    public SwitchMovet target;
    public SwitchMovetBox parent;
    public float generalPercent = 0;
    public float flashPercent = 0;
    public bool flash = false;
    public SwitchMovetBeam(SwitchMovet target, SwitchMovetBox parent) : base(Vector2.Zero)
    {
        Depth = -500;
        this.target = target;
        this.parent = parent;
        Add(new Coroutine(Sequence()));
    }
    public IEnumerator Sequence()
    {
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.15f, true);
        tween.OnUpdate += t => generalPercent = t.Percent;
        Add(tween);
        yield return 0.15f;
        Remove(tween);
        tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.15f, true);
        tween.OnUpdate += t => generalPercent = 1 + t.Percent;
        Add(tween);
        Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.3f, true);
        flash = true;
        tween2.OnUpdate += t => flashPercent = t.Percent;
        Add(tween2);
        yield return 0.15f;
        Remove(tween);
        yield return 0.35f;
        Remove(tween2);
        RemoveSelf();
    }

    public override void Render()
    {
        base.Render();
        Color color = Calc.HexToColor(parent.color);
        color.A = (byte)(color.A / 2);
        float thick = 1 + Ease.CubeInOut(generalPercent > 1 ? 1 - (generalPercent - 1) : generalPercent) * 4;
        if (generalPercent > 0 && generalPercent < 1)
        {
            Draw.Line(parent.Center, Vector2.Lerp(parent.Center, target.Center, Ease.CubeIn(generalPercent)), color * 0.5f, thick);
        }
        else if (generalPercent >= 1)
        {
            Draw.Line(target.Center, Vector2.Lerp(target.Center, parent.Center, Ease.CubeOut(1 - (generalPercent - 1))), color * 0.5f, thick);
        }
        if (flash)
        {
            float p = Ease.QuadOut(1 - flashPercent);
            Draw.Rect(target.Position + target.Shake, target.Width, (1 + target.Height / 2) * p, color * p);
            Draw.Rect(target.Position + target.Shake + Vector2.UnitY * (target.Height - (1 + target.Height / 2) * p), target.Width, (1 + target.Height / 2) * p, color * p);
        }
    }
}

[Tracked]
[CustomEntity("FemtoHelper/SwitchMovetBox")]
public class SwitchMovetBox : Solid
{
    public string color;
    private SineWave sine;
    private SineWave sine2;
    private float sink;
    private Vector2 bounceDir;
    private Wiggler bounce;
    public Vector2 start;
    public Sprite cover;
    public Image back;
    public Image crystal;
    public Image glow;
    public bool smashParticles;
    public ParticleType P_Smash;
    public Vector2 halfSize => new Vector2(Width, Height) / 2f;
    public SwitchMovetBox(EntityData data, Vector2 offset) : base(data.Position + offset, 32, 32, false)
    {
        Depth = -450;
        SurfaceSoundIndex = 9;
        start = Position;
        OnDashCollide = OnDash;
        color = data.Attr("color", "FF0000");
        sine = new SineWave(0.5f, 0f);
        Add(sine);
        sine2 = new SineWave(0.5f, -1f);
        Add(sine2);
        bounce = Wiggler.Create(1f, 0.5f);
        bounce.StartZero = false;
        Add(bounce);
        string path = data.Attr("path", "objects/FemtoHelper/switchMovetBox/");
        Add(back = new Image(GFX.Game[path + "back"]));
        Add(crystal = new Image(GFX.Game[path + "crystal"]));
        Add(glow = new Image(GFX.Game[path + "glow"]));
        Add(cover = new Sprite(GFX.Game, path + "cover"));
        cover.AddLoop("idle", "", 0.1f, [0]);
        cover.Play("idle");
        cover.Add("hit", "", 0.05f, "idle", [0, 1, 2, 3, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4, 3, 2, 1, 0]);

        back.CenterOrigin();
        crystal.CenterOrigin();
        glow.CenterOrigin();
        cover.CenterOrigin();

        cover.Position = back.Position = crystal.Position = glow.Position = halfSize;

        Color glowColor = Color.Lerp(Calc.HexToColor(color), Color.DarkGray * 0.2f, 0.8f);
        glow.Color = glowColor;
        crystal.Color = Calc.HexToColor(color);
        Add(new LightOcclude(0.8f));

        P_Smash = new ParticleType(LightningBreakerBox.P_Smash)
        {
            Color = Calc.HexToColor(color),
            Color2 = Color.White,
            SpeedMin = 70,
            SpeedMax = 80
        };
    }

    private void SmashParticles(Vector2 dir)
    {
        float direction;
        Vector2 position;
        Vector2 positionRange;
        int num;
        if (dir == Vector2.UnitX)
        {
            direction = 0f;
            position = base.CenterRight - Vector2.UnitX * 12f;
            positionRange = Vector2.UnitY * (base.Height - 6f) * 0.5f;
            num = (int)(base.Height / 8f) * 4;
        }
        else if (dir == -Vector2.UnitX)
        {
            direction = MathF.PI;
            position = base.CenterLeft + Vector2.UnitX * 12f;
            positionRange = Vector2.UnitY * (base.Height - 6f) * 0.5f;
            num = (int)(base.Height / 8f) * 4;
        }
        else if (dir == Vector2.UnitY)
        {
            direction = MathF.PI / 2f;
            position = base.BottomCenter - Vector2.UnitY * 12f;
            positionRange = Vector2.UnitX * (base.Width - 6f) * 0.5f;
            num = (int)(base.Width / 8f) * 4;
        }
        else
        {
            direction = -MathF.PI / 2f;
            position = base.TopCenter + Vector2.UnitY * 12f;
            positionRange = Vector2.UnitX * (base.Width - 6f) * 0.5f;
            num = (int)(base.Width / 8f) * 4;
        }
        num += 2;
        SceneAs<Level>().ParticlesBG.Emit(P_Smash, num, position, positionRange, -direction);
    }

    public DashCollisionResults OnDash(Player player, Vector2 dir)
    {
        bool flag = false;
        foreach (SwitchMovet s in Scene.Tracker.GetEntities<SwitchMovet>())
        {
            if (s.color == this.color)
            {
                if (!s.activated)
                {
                    flag = true;
                    Scene.Add(new SwitchMovetBeam(s, this));
                    s.Add(new Coroutine(s.Sequence()));
                }
            }
        }
        if (flag)
        {
            cover.Play("hit");
            (Scene as Level).DirectionalShake(dir);
            cover.Scale = back.Scale = new Vector2(1f + Math.Abs(dir.Y) * 0.4f - Math.Abs(dir.X) * 0.4f, 1f + Math.Abs(dir.X) * 0.4f - Math.Abs(dir.Y) * 0.4f);
            bounceDir = dir;
            bounce.Start();
            smashParticles = true;
            Audio.Play("event:/FemtoHelper/switch_movet_box_hit", Position);
            Celeste.Freeze(0.1f);
        }
        else
        {
            (Scene as Level).DirectionalShake(dir);
            cover.Scale = back.Scale = new Vector2(1f + Math.Abs(dir.Y) * 0.2f - Math.Abs(dir.X) * 0.2f, 1f + Math.Abs(dir.X) * 0.2f - Math.Abs(dir.Y) * 0.2f);
            bounceDir = dir;
            bounce.Start();
            Audio.Play("event:/FemtoHelper/switch_movet_box_hitfail", Position);
            Celeste.Freeze(0.1f);
        }
        return DashCollisionResults.Rebound;
    }

    public override void Update()
    {
        base.Update();
        if (Collidable)
        {
            bool flag = HasPlayerRider();
            sink = Calc.Approach(sink, flag ? 1 : 0, 2f * Engine.DeltaTime);
            sine.Rate = MathHelper.Lerp(1f, 0.5f, sink);
            sine2.Rate = MathHelper.Lerp(1f, 0.5f, sink);
            Vector2 vector = start;
            vector.Y += sink * 6f + sine.Value * MathHelper.Lerp(4f, 2f, sink);
            vector += bounce.Value * bounceDir * 12f;
            MoveToX(vector.X);
            MoveToY(vector.Y);
            Vector2 vector2 = start;
            vector2.Y += sink * 6f + sine2.Value * MathHelper.Lerp(4f, 2f, sink);
            vector2 += bounce.Value * bounceDir * 12f;
            crystal.Position = halfSize + vector2 - Position;
            glow.Position = halfSize + vector2 - Position;
            if (smashParticles)
            {
                smashParticles = false;
                SmashParticles(bounceDir.Perpendicular());
                SmashParticles(-bounceDir.Perpendicular());
            }
        }
        cover.Scale.X = back.Scale.X = Calc.Approach(cover.Scale.X, 1f, Engine.DeltaTime * 4f);
        cover.Scale.Y = back.Scale.Y = Calc.Approach(cover.Scale.Y, 1f, Engine.DeltaTime * 4f);
    }

    public override void Render()
    {
        base.Render();
    }
}

public class SwitchMovetPathRenderer : Entity
{
    public SwitchMovet parent;
    public float timer;
    public Player player;
    public SwitchMovetPathRenderer(SwitchMovet parent) : base()
    {
        base.Depth = 5000;
        this.parent = parent;
        timer = Calc.Random.NextFloat(MathF.Tau);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        player = Scene.Tracker.GetEntity<Player>();
    }

    public override void Update()
    {
        base.Update();
        timer += Engine.DeltaTime * (3 * parent.gemActivation + 1);
    }

    public override void Render()
    {
        Color col = Calc.HexToColor(parent.color);
        col.A = 0;
        float count = MathF.Round(Vector2.Distance(parent.anchor + parent.halfSize, parent.node + parent.halfSize) / 16f);
        for (float j = 0; j < count; j++)
        {
            float percent = j / count;
            float percentNext = (j + 1) / count;
            Vector2 perp = (parent.node + parent.halfSize - parent.anchor + parent.halfSize).Perpendicular().SafeNormalize(2f);
            Draw.Line(Vector2.Lerp(parent.anchor + parent.halfSize, parent.node + parent.halfSize, percent) + perp * MathF.Sin((timer * -3) + (j * 1.7f)), Vector2.Lerp(parent.anchor + parent.halfSize, parent.node + parent.halfSize, percentNext) + perp * MathF.Sin((timer * -3) + ((j + 1) * 1.7f)), col * 0.2f * (0.6f + 0.4f * MathF.Cos((timer * 2.5f) + (j * 0.2f))), 4);
            Draw.Line(Vector2.Lerp(parent.anchor + parent.halfSize, parent.node + parent.halfSize, percent) - perp * MathF.Sin((timer * -3) + (j * 1.7f)), Vector2.Lerp(parent.anchor + parent.halfSize, parent.node + parent.halfSize, percentNext) + perp * MathF.Sin((timer * -3) + ((j - 1) * 1.7f)), col * 0.2f * (0.6f + 0.4f * MathF.Cos((timer * 2.5f) + (j * 0.2f))), 4);
        }

        //Vector2 pos = parent.anchor + parent.halfSize;
        //Color color;
        //for (float i = 0; i < parent.distance; i += 8, pos += parent.step)
        //{
        //    float d2 = Vector2.DistanceSquared(pos, player.Position);
        //    float size = Calc.ClampedMap(d2, 6400, 0, 2, 8);
        //    float alpha = Calc.ClampedMap(d2, 6400, 0, 0.5f, 0.9f);

        //    color = Calc.HexToColor(parent.color) * alpha;
        //    color.A = 0;

        //    Draw.Rect(pos - (new Vector2(size) / 2) + (parent.perp * MathF.Sin((i / 12) + timer * 1.5f) * 2), size, size, color);
        //}
        //color = Calc.HexToColor(parent.color) * 0.3f;
        //color.A = 0;
        //Draw.Line(parent.anchor + parent.halfSize, parent.node + parent.halfSize, color, 4);

        Color trueGemColor = parent.gemColor;
        trueGemColor.A = 0;

        parent.smallcog.DrawCentered(parent.node + parent.halfSize + Vector2.UnitY, Color.Black, 1, parent.cogWinding);
        parent.smallcog.DrawCentered(parent.anchor + parent.halfSize, parent.cogColor, 1, parent.cogWinding);
        parent.cogGem.DrawCentered(parent.anchor + parent.halfSize, trueGemColor);

        parent.smallcog.DrawCentered(parent.node + parent.halfSize + Vector2.UnitY, Color.Black, 1, parent.cogWinding);
        parent.smallcog.DrawCentered(parent.node + parent.halfSize, parent.cogColor, 1, parent.cogWinding);
        parent.cogGem.DrawCentered(parent.node + parent.halfSize, trueGemColor);

        Draw.Rect(new Rectangle((int)(parent.X + parent.Shake.X - 1f), (int)(parent.Y + parent.Shake.Y - 1f), (int)parent.Width + 2, (int)parent.Height + 2), Color.Black);
        base.Render();
    }
}

[Tracked]
[CustomEntity("FemtoHelper/SwitchMovet")]
public class SwitchMovet : Solid
{
    public static ParticleType P_Scrape = new ParticleType(ZipMover.P_Scrape);
    public string color = "ffffff";
    public Vector2 node = Vector2.Zero;
    public Vector2 anchor = Vector2.Zero;
    public bool activated = false;
    public bool state = false;
    public float cogWinding = 0;
    public float distance;
    public Vector2 step;
    public Vector2 perp;
    public MTexture cog;
    public MTexture cogGem;
    public MTexture smallcog;
    public List<Image> block;
    public Color cogColor => Color.Lerp(Calc.HexToColor(color), Color.White, 0.65f);
    public Color gemColor => Color.Lerp(Color.Black, Calc.HexToColor(color), gemActivation);
    public float gemActivation = 0;
    public Vector2 halfSize => new Vector2(Width, Height) / 2;
    public BloomPoint bloom;
    public BloomPoint nodebloom;
    public BloomPoint nodebloom2;
    public SwitchMovet(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {
        Depth = -400;
        SurfaceSoundIndex = 42;
        node = data.NodesOffset(offset)[0];
        anchor = Position;
        distance = Vector2.Distance(Position, node);
        step = (node - Position).SafeNormalize();
        perp = step.Perpendicular();
        step *= 8;
        color = data.Attr("color", "FF0000");
        string path = data.Attr("path", "objects/FemtoHelper/switchMovet/");
        cog = GFX.Game[path + "gear"];
        cogGem = GFX.Game[path + "geargem"];
        smallcog = GFX.Game[path + "gearsmall"];
        MTexture mainTexture = GFX.Game[path + "block"];
        MTexture mainTextureRust = GFX.Game[path + "rust"];
        block = new List<Image>();
        for (int i = 0; i < Width; i += 8)
        {
            for (int j = 0; j < Height; j += 8)
            {
                Point cutout = Point.Zero;
                if (i < 8)
                {
                    if (j < 8)
                    {
                        cutout = new Point(0, 0);
                    }
                    else if (j >= Height - 8)
                    {
                        cutout = new Point(0, 16);
                    }
                    else
                    {
                        cutout = new Point(0, 8);
                    }
                }
                else if (i >= Width - 8)
                {
                    if (j < 8)
                    {
                        cutout = new Point(16, 0);
                    }
                    else if (j >= Height - 8)
                    {
                        cutout = new Point(16, 16);
                    }
                    else
                    {
                        cutout = new Point(16, 8);
                    }
                }
                else
                {
                    if (j < 8)
                    {
                        cutout = new Point(8, 0);
                    }
                    else if (j >= Height - 8)
                    {
                        cutout = new Point(8, 16);
                    }
                    else
                    {
                        cutout = new Point(8, 8);
                    }
                }
                Image image = new Image((Calc.Random.Chance(0.5f) ? mainTexture : mainTextureRust).GetSubtexture(cutout.X, cutout.Y, 8, 8)) { Position = new Vector2(i, j) };
                Add(image);
                block.Add(image);
            }
        }
        Add(bloom = new BloomPoint(0, 0) { Position = halfSize });
        Add(nodebloom = new BloomPoint(0, 0) { Position = (node - Position) + halfSize });
        Add(nodebloom2 = new BloomPoint(0, 0) { Position = (anchor - Position) + halfSize });
        Add(new LightOcclude(0.8f));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Scene.Add(new SwitchMovetPathRenderer(this));
    }

    public override void Update()
    {
        base.Update();
        nodebloom.Position = (node - Position) + halfSize;
        nodebloom2.Position = (anchor - Position) + halfSize;
    }

    public override void Render()
    {
        Vector2 position = Position;
        Position += base.Shake;
        base.Render();

        Color trueGemColor = gemColor;
        trueGemColor.A = 0;

        cog.DrawCentered(Center + Vector2.UnitX, Color.Lerp(cogColor, Color.Black, 0.6f), 1, cogWinding);
        cog.DrawCentered(Center - Vector2.UnitX, Color.Lerp(cogColor, Color.Black, 0.6f), 1, cogWinding);
        cog.DrawCentered(Center + Vector2.UnitY, Color.Lerp(cogColor, Color.Black, 0.6f), 1, cogWinding);
        cog.DrawCentered(Center - Vector2.UnitY, Color.Lerp(cogColor, Color.Black, 0.6f), 1, cogWinding);

        cog.DrawCentered(Center, cogColor, 1, cogWinding);
        cogGem.DrawCentered(Center, trueGemColor);
        Position = position;
    }
    public IEnumerator Sequence()
    {
        Vector2 start = state ? node : anchor;
        Vector2 end = state ? anchor : node;
        activated = true;
        state = !state;
        StartShaking(0.3f);
        for (float i = 0; i < 1; i += Engine.DeltaTime / 0.3f)
        {
            float eased = Ease.SineInOut(i);
            bloom.Alpha = eased * 0.7f;
            bloom.Radius = eased * 12;
            nodebloom.Alpha = nodebloom2.Alpha = eased * 0.7f;
            nodebloom.Radius = nodebloom2.Alpha = eased * 8;
            gemActivation = eased;
            yield return null;
        }
        Audio.Play("event:/FemtoHelper/switch_movet_accelerate", Position);
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.7f, true);
        tween.OnUpdate += delegate (Tween t)
        {
            cogWinding = state ? Calc.LerpClamp(0, 2 * MathF.Tau, t.Eased) : Calc.LerpClamp(2 * MathF.Tau, 0, t.Eased);
            Vector2 vector = Vector2.Lerp(start, end, t.Eased);
            ScrapeParticlesCheck(vector);
            MoveTo(vector);
        };
        Add(tween);
        yield return 0.7f;
        Audio.Play("event:/FemtoHelper/switch_movet_hit", Position);
        StartShaking(0.3f);
        for (float i = 0; i < 1; i += Engine.DeltaTime / 0.3f)
        {
            float eased = Ease.SineInOut(1 - i);
            bloom.Alpha = eased * 0.7f;
            bloom.Radius = eased * 12;
            nodebloom.Alpha = nodebloom2.Alpha = eased * 0.7f;
            nodebloom.Radius = nodebloom2.Alpha = eased * 8;
            gemActivation = eased;
            yield return null;
        }
        activated = false;
    }
    private void ScrapeParticlesCheck(Vector2 to)
    {
        if (!base.Scene.OnInterval(0.03f))
        {
            return;
        }
        bool flag = to.Y != base.ExactPosition.Y;
        bool flag2 = to.X != base.ExactPosition.X;
        if (flag && !flag2)
        {
            int num = Math.Sign(to.Y - base.ExactPosition.Y);
            Vector2 vector = ((num != 1) ? base.TopLeft : base.BottomLeft);
            int num2 = 4;
            if (num == 1)
            {
                num2 = Math.Min((int)base.Height - 12, 20);
            }
            int num3 = (int)base.Height;
            if (num == -1)
            {
                num3 = Math.Max(16, (int)base.Height - 16);
            }
            if (base.Scene.CollideCheck<Solid>(vector + new Vector2(-2f, num * -2)))
            {
                for (int i = num2; i < num3; i += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2(0f, (float)i + (float)num * 2f), (num == 1) ? (-MathF.PI / 4f) : (MathF.PI / 4f));
                }
            }
            if (base.Scene.CollideCheck<Solid>(vector + new Vector2(base.Width + 2f, num * -2)))
            {
                for (int j = num2; j < num3; j += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopRight + new Vector2(-1f, (float)j + (float)num * 2f), (num == 1) ? (MathF.PI * -3f / 4f) : (MathF.PI * 3f / 4f));
                }
            }
        }
        else
        {
            if (!flag2 || flag)
            {
                return;
            }
            int num4 = Math.Sign(to.X - base.ExactPosition.X);
            Vector2 vector2 = ((num4 != 1) ? base.TopLeft : base.TopRight);
            int num5 = 4;
            if (num4 == 1)
            {
                num5 = Math.Min((int)base.Width - 12, 20);
            }
            int num6 = (int)base.Width;
            if (num4 == -1)
            {
                num6 = Math.Max(16, (int)base.Width - 16);
            }
            if (base.Scene.CollideCheck<Solid>(vector2 + new Vector2(num4 * -2, -2f)))
            {
                for (int k = num5; k < num6; k += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2((float)k + (float)num4 * 2f, -1f), (num4 == 1) ? (MathF.PI * 3f / 4f) : (MathF.PI / 4f));
                }
            }
            if (base.Scene.CollideCheck<Solid>(vector2 + new Vector2(num4 * -2, base.Height + 2f)))
            {
                for (int l = num5; l < num6; l += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.BottomLeft + new Vector2((float)l + (float)num4 * 2f, 0f), (num4 == 1) ? (MathF.PI * -3f / 4f) : (-MathF.PI / 4f));
                }
            }
        }
    }
}
