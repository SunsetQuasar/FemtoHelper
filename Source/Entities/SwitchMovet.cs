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
    public SwitchMovet Target;
    public SwitchMovetBox Parent;
    public float GeneralPercent = 0;
    public float FlashPercent = 0;
    public bool Flash = false;
    public SwitchMovetBeam(SwitchMovet target, SwitchMovetBox parent) : base(Vector2.Zero)
    {
        Depth = -500;
        this.Target = target;
        this.Parent = parent;
        Add(new Coroutine(Sequence()));
    }
    public IEnumerator Sequence()
    {
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.15f, true);
        tween.OnUpdate += t => GeneralPercent = t.Percent;
        Add(tween);
        yield return 0.15f;
        Remove(tween);
        tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.15f, true);
        tween.OnUpdate += t => GeneralPercent = 1 + t.Percent;
        Add(tween);
        Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.3f, true);
        Flash = true;
        tween2.OnUpdate += t => FlashPercent = t.Percent;
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
        Color color = Calc.HexToColor(Parent.Color);
        color.A = (byte)(color.A / 2);
        float thick = 1 + Ease.CubeInOut(GeneralPercent > 1 ? 1 - (GeneralPercent - 1) : GeneralPercent) * 4;
        if (GeneralPercent > 0 && GeneralPercent < 1)
        {
            Draw.Line(Parent.Center, Vector2.Lerp(Parent.Center, Target.Center, Ease.CubeIn(GeneralPercent)), color * 0.5f, thick);
        }
        else if (GeneralPercent >= 1)
        {
            Draw.Line(Target.Center, Vector2.Lerp(Target.Center, Parent.Center, Ease.CubeOut(1 - (GeneralPercent - 1))), color * 0.5f, thick);
        }
        if (Flash)
        {
            float p = Ease.QuadOut(1 - FlashPercent);
            Draw.Rect(Target.Position + Target.Shake, Target.Width, (1 + Target.Height / 2) * p, color * p);
            Draw.Rect(Target.Position + Target.Shake + Vector2.UnitY * (Target.Height - (1 + Target.Height / 2) * p), Target.Width, (1 + Target.Height / 2) * p, color * p);
        }
    }
}

[Tracked]
[CustomEntity("FemtoHelper/SwitchMovetBox")]
public class SwitchMovetBox : Solid
{
    public string Color;
    private SineWave sine;
    private SineWave sine2;
    private float sink;
    private Vector2 bounceDir;
    private Wiggler bounce;
    public Vector2 Start;
    public Sprite Cover;
    public Image Back, BackOneUse, Crystal, Glow;
    public bool DoSmashParticles;
    public ParticleType PSmash;
    private readonly bool floaty;
    public SoundSource Sound;
    public Vector2 halfSize => new Vector2(Width, Height) / 2f;
    public readonly bool OneUse;
    public SwitchMovetBox(EntityData data, Vector2 offset) : base(data.Position + offset, 32, 32, false)
    {
        Depth = -450;
        SurfaceSoundIndex = 9;
        Start = Position;
        OnDashCollide = OnDash;
        Color = data.Attr("color", "FF0000");
        sine = new SineWave(0.5f, 0f);
        Add(sine);
        sine2 = new SineWave(0.5f, -1f);
        Add(sine2);
        bounce = Wiggler.Create(1f, 0.5f);
        bounce.StartZero = false;
        Add(bounce);
        string path = data.Attr("path", "objects/FemtoHelper/switchMovetBox/");
        floaty = data.Bool("floaty", true);
        OneUse = data.Bool("oneUse", false);
        Add(Back = new Image(GFX.Game[path + "back"]));
        BackOneUse = new Image(GFX.Game[path + "back_oneuse"]);
        if (OneUse)
        {
            BackOneUse.Color = Calc.HexToColor(Color);
            Add(BackOneUse);
        }
        Add(Crystal = new Image(GFX.Game[path + "crystal"]));
        Add(Glow = new Image(GFX.Game[path + "glow"]));
        Add(Cover = new Sprite(GFX.Game, path));
        Cover.AddLoop("idle", "cover", 0.1f, [0]);
        Cover.Play("idle");
        Cover.Add("hit", "cover", 0.05f, "idle", [0, 1, 2, 3, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4, 3, 2, 1, 0]);
        Cover.Add("break", "break", 0.05f);
        Cover.OnFinish = (s) =>
        {
            if (s == "break")
            {
                Cover.Visible = false;
            }
        };

        Back.CenterOrigin();
        BackOneUse.CenterOrigin();
        Crystal.CenterOrigin();
        Glow.CenterOrigin();
        Cover.CenterOrigin();

        Cover.Position = BackOneUse.Position = Back.Position = Crystal.Position = Glow.Position = halfSize;

        Color glowColor = Microsoft.Xna.Framework.Color.Lerp(Calc.HexToColor(Color), Microsoft.Xna.Framework.Color.DarkGray * 0.2f, 0.8f);
        Glow.Color = glowColor;
        Crystal.Color = Calc.HexToColor(Color);
        Add(new LightOcclude(0.8f));

        PSmash = new ParticleType(LightningBreakerBox.P_Smash)
        {
            Color = Calc.HexToColor(Color),
            Color2 = Microsoft.Xna.Framework.Color.White,
            SpeedMin = 70,
            SpeedMax = 80
        };
        Add(Sound = new SoundSource()
        {
            Position = halfSize
        });
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
            position = CenterRight - Vector2.UnitX * 12f;
            positionRange = Vector2.UnitY * (Height - 6f) * 0.5f;
            num = (int)(Height / 8f) * 4;
        }
        else if (dir == -Vector2.UnitX)
        {
            direction = MathF.PI;
            position = CenterLeft + Vector2.UnitX * 12f;
            positionRange = Vector2.UnitY * (Height - 6f) * 0.5f;
            num = (int)(Height / 8f) * 4;
        }
        else if (dir == Vector2.UnitY)
        {
            direction = MathF.PI / 2f;
            position = BottomCenter - Vector2.UnitY * 12f;
            positionRange = Vector2.UnitX * (Width - 6f) * 0.5f;
            num = (int)(Width / 8f) * 4;
        }
        else
        {
            direction = -MathF.PI / 2f;
            position = TopCenter + Vector2.UnitY * 12f;
            positionRange = Vector2.UnitX * (Width - 6f) * 0.5f;
            num = (int)(Width / 8f) * 4;
        }
        num += 2;
        SceneAs<Level>().ParticlesBG.Emit(PSmash, num, position, positionRange, -direction);
    }

    public DashCollisionResults OnDash(Player player, Vector2 dir)
    {
        bool flag = false;
        foreach (SwitchMovet s in Scene.Tracker.GetEntities<SwitchMovet>())
        {
            if (s.Color == this.Color)
            {
                if (!s.Activated)
                {
                    flag = true;
                    Scene.Add(new SwitchMovetBeam(s, this));
                    s.Add(new Coroutine(s.Sequence()));
                }
            }
        }
        if (flag)
        {
            Cover.Play("hit");
            (Scene as Level).DirectionalShake(dir);
            bounceDir = dir;
            bounce.Start();
            Cover.Scale = Back.Scale = BackOneUse.Scale = new Vector2(1f + Math.Abs(dir.Y) * 0.2f - Math.Abs(dir.X) * 0.2f, 1f + Math.Abs(dir.X) * 0.2f - Math.Abs(dir.Y) * 0.2f);
            Celeste.Freeze(0.1f);
            DoSmashParticles = true;
            Sound.Play("event:/FemtoHelper/switch_movet_box_hit");
            if (OneUse)
            {
                Alarm Yep = Alarm.Set(this, 0.5f, () =>
                {
                    Sound.Play("event:/new_content/game/10_farewell/fusebox_hit_2");
                    Cover.Play("break");
                    Crystal.Visible = Glow.Visible = Back.Visible = BackOneUse.Visible = Collidable = false;
                    SceneAs<Level>().Flash(new(0.2f, 0.2f, 0.2f, 0f));
                    Celeste.Freeze(0.1f);
                });
            }
        }
        else
        {
            (Scene as Level).DirectionalShake(dir);
            Cover.Scale = Back.Scale = BackOneUse.Scale = new Vector2(1f + Math.Abs(dir.Y) * 0.2f - Math.Abs(dir.X) * 0.2f, 1f + Math.Abs(dir.X) * 0.2f - Math.Abs(dir.Y) * 0.2f);
            bounceDir = dir;
            bounce.Start();
            Sound.Play("event:/FemtoHelper/switch_movet_box_hitfail");
            Celeste.Freeze(0.1f);
        }
        return DashCollisionResults.Rebound;
    }

    public override void Update()
    {
        base.Update();
        if (Collidable)
        {
            if (floaty)
            {
                bool flag = HasPlayerRider();
                sink = Calc.Approach(sink, flag ? 1 : 0, 2f * Engine.DeltaTime);
                sine.Rate = MathHelper.Lerp(1f, 0.5f, sink);
                sine2.Rate = MathHelper.Lerp(1f, 0.5f, sink);
                Vector2 vector = Start;
                vector.Y += sink * 6f + sine.Value * MathHelper.Lerp(4f, 2f, sink);
                vector += bounce.Value * bounceDir * 12f;
                MoveToX(vector.X);
                MoveToY(vector.Y);
            }
            Vector2 vector2 = Start;
            vector2.Y += sink * 6f + sine2.Value * MathHelper.Lerp(4f, 2f, sink);
            vector2 += bounce.Value * bounceDir * (floaty ? 12f : 3f);
            Crystal.Position = halfSize + vector2 - Position;
            Glow.Position = halfSize + vector2 - Position;
            if (DoSmashParticles)
            {
                DoSmashParticles = false;
                SmashParticles(bounceDir.Perpendicular());
                SmashParticles(-bounceDir.Perpendicular());
            }
        }
        Cover.Scale.X = Back.Scale.X = BackOneUse.Scale.X = Calc.Approach(Cover.Scale.X, 1f, Engine.DeltaTime * 4f);
        Cover.Scale.Y = Back.Scale.Y = BackOneUse.Scale.Y = Calc.Approach(Cover.Scale.Y, 1f, Engine.DeltaTime * 4f);
    }

    public override void Render()
    {
        base.Render();
    }
}

public class SwitchMovetPathRenderer : Entity
{
    public SwitchMovet Parent;
    public float Timer;
    public Player Player;

    private ParticleType movetPath = new(Seeker.P_Regen);
    public SwitchMovetPathRenderer(SwitchMovet parent) : base()
    {
        Depth = 5000;
        this.Parent = parent;
        Timer = Calc.Random.NextFloat(MathF.Tau);

        movetPath = new(Seeker.P_Regen)
        {
            Color = Color.Lerp(Calc.HexToColor(parent.Color), Color.White, 0.7f) * 0.4f,
            Color2 = Calc.HexToColor(parent.Color) * 0.3f,
            FadeMode = ParticleType.FadeModes.InAndOut,
            SpeedMin = 5,
            SpeedMax = 15,
            Direction = 0,
            DirectionRange = 360,
            Friction = 30,
            LifeMin = 0.1f,
            LifeMax = 0.3f,
        };
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Player = Scene.Tracker.GetEntity<Player>();
    }

    public override void Update()
    {
        base.Update();
        Timer += Engine.DeltaTime * (3 * (Parent.GemActivation * Parent.SpeedMultiplier) + 1);
        if (Scene.OnInterval(0.1f))
        {
            int count = (int)MathF.Round(Vector2.Distance(Parent.Anchor, Parent.Node) / 32f);
            for (int i = 0; i < count; i++)
            {
                (Scene as Level).ParticlesBG.Emit(movetPath, Vector2.Lerp(Parent.Anchor + Parent.halfSize, Parent.Node + Parent.halfSize, Calc.Random.NextFloat()) + new Vector2(Calc.Random.Range(-0.5f, 0.5f), Calc.Random.Range(-0.5f, 0.5f)));
            }
        }
    }

    public override void Render()
    {
        Color col = Calc.HexToColor(Parent.Color);
        col.A = 0;
        float count = MathF.Round(Vector2.Distance(Parent.Anchor + Parent.halfSize, Parent.Node + Parent.halfSize) / 16f);
        for (float j = 0; j < count; j++)
        {
            float percent = j / count;
            float percentNext = (j + 1) / count;
            Vector2 perp = (Parent.Node + Parent.halfSize - Parent.Anchor + Parent.halfSize).Perpendicular().SafeNormalize(2f);
            Draw.Line(Vector2.Lerp(Parent.Anchor + Parent.halfSize, Parent.Node + Parent.halfSize, percent) + perp * MathF.Sin((Timer * -3) + (j * 1.7f)), Vector2.Lerp(Parent.Anchor + Parent.halfSize, Parent.Node + Parent.halfSize, percentNext) + perp * MathF.Sin((Timer * -3) + ((j + 1) * 1.7f)), col * 0.2f * (0.6f + 0.4f * MathF.Cos((Timer * 2.5f) + (j * 0.2f))), 4);
            Draw.Line(Vector2.Lerp(Parent.Anchor + Parent.halfSize, Parent.Node + Parent.halfSize, percent) - perp * MathF.Sin((Timer * -3) + (j * 1.7f)), Vector2.Lerp(Parent.Anchor + Parent.halfSize, Parent.Node + Parent.halfSize, percentNext) + perp * MathF.Sin((Timer * -3) + ((j - 1) * 1.7f)), col * 0.2f * (0.6f + 0.4f * MathF.Cos((Timer * 2.5f) + (j * 0.2f))), 4);
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

        Color trueGemColor = Parent.gemColor;
        trueGemColor.A = 0;

        Parent.Smallcog.DrawCentered(Parent.Node + Parent.halfSize + Vector2.UnitY, Color.Black, 1, Parent.CogWinding);
        Parent.Smallcog.DrawCentered(Parent.Anchor + Parent.halfSize, Parent.cogColor, 1, Parent.CogWinding);
        Parent.CogGem.DrawCentered(Parent.Anchor + Parent.halfSize, trueGemColor);

        Parent.Smallcog.DrawCentered(Parent.Node + Parent.halfSize + Vector2.UnitY, Color.Black, 1, Parent.CogWinding);
        Parent.Smallcog.DrawCentered(Parent.Node + Parent.halfSize, Parent.cogColor, 1, Parent.CogWinding);
        Parent.CogGem.DrawCentered(Parent.Node + Parent.halfSize, trueGemColor);

        Draw.Rect(new Rectangle((int)(Parent.X + Parent.Shake.X - 1f), (int)(Parent.Y + Parent.Shake.Y - 1f), (int)Parent.Width + 2, (int)Parent.Height + 2), Color.Black);
        base.Render();
    }
}

[Tracked]
[CustomEntity("FemtoHelper/SwitchMovet")]
public class SwitchMovet : Solid
{
    public static ParticleType PScrape = new ParticleType(ZipMover.P_Scrape);
    public string Color = "ffffff";
    public Vector2 Node = Vector2.Zero;
    public Vector2 Anchor = Vector2.Zero;
    public bool Activated = false;
    public bool State = false;
    public float CogWinding = 0;
    public float Distance;
    public Vector2 Step;
    public Vector2 Perp;
    public MTexture Cog;
    public MTexture CogGem;
    public MTexture Smallcog;
    public List<Image> Block;
    public Color cogColor => Microsoft.Xna.Framework.Color.Lerp(Calc.HexToColor(Color), Microsoft.Xna.Framework.Color.White, 0.65f);
    public Color gemColor => Microsoft.Xna.Framework.Color.Lerp(Microsoft.Xna.Framework.Color.Black, Calc.HexToColor(Color), GemActivation);
    public float GemActivation = 0;
    public Vector2 halfSize => new Vector2(Width, Height) / 2;
    public BloomPoint Bloom;
    public BloomPoint Nodebloom;
    public BloomPoint Nodebloom2;

    private readonly float speedMultiplier;
    public float SpeedMultiplier => (speedMultiplier == 0 ? 1f : speedMultiplier);

    public SwitchMovet(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {
        Depth = -400;
        SurfaceSoundIndex = 42;
        Node = data.NodesOffset(offset)[0];
        Anchor = Position;
        Distance = Vector2.Distance(Position, Node);
        Step = (Node - Position).SafeNormalize();
        Perp = Step.Perpendicular();
        Step *= 8;
        Color = data.Attr("color", "FF0000");
        speedMultiplier = data.Float("speedMultiplier", 1f);
        string path = data.Attr("path", "objects/FemtoHelper/switchMovet/");
        Cog = GFX.Game[path + "gear"];
        CogGem = GFX.Game[path + "geargem"];
        Smallcog = GFX.Game[path + "gearsmall"];
        MTexture mainTexture = GFX.Game[path + "block"];
        MTexture mainTextureRust = GFX.Game[path + "rust"];
        Block = new List<Image>();
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
                Block.Add(image);
            }
        }
        Add(Bloom = new BloomPoint(0, 0) { Position = halfSize });
        Add(Nodebloom = new BloomPoint(0, 0) { Position = (Node - Position) + halfSize });
        Add(Nodebloom2 = new BloomPoint(0, 0) { Position = (Anchor - Position) + halfSize });
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
        Nodebloom.Position = (Node - Position) + halfSize;
        Nodebloom2.Position = (Anchor - Position) + halfSize;
    }

    public override void Render()
    {
        Vector2 position = Position;
        Position += Shake;
        base.Render();

        Color trueGemColor = gemColor;
        trueGemColor.A = 0;

        Cog.DrawCentered(Center + Vector2.UnitX, Microsoft.Xna.Framework.Color.Lerp(cogColor, Microsoft.Xna.Framework.Color.Black, 0.6f), 1, CogWinding);
        Cog.DrawCentered(Center - Vector2.UnitX, Microsoft.Xna.Framework.Color.Lerp(cogColor, Microsoft.Xna.Framework.Color.Black, 0.6f), 1, CogWinding);
        Cog.DrawCentered(Center + Vector2.UnitY, Microsoft.Xna.Framework.Color.Lerp(cogColor, Microsoft.Xna.Framework.Color.Black, 0.6f), 1, CogWinding);
        Cog.DrawCentered(Center - Vector2.UnitY, Microsoft.Xna.Framework.Color.Lerp(cogColor, Microsoft.Xna.Framework.Color.Black, 0.6f), 1, CogWinding);

        Cog.DrawCentered(Center, cogColor, 1, CogWinding);
        CogGem.DrawCentered(Center, trueGemColor);
        Position = position;
    }
    public IEnumerator Sequence()
    {
        Vector2 start = State ? Node : Anchor;
        Vector2 end = State ? Anchor : Node;
        Activated = true;
        State = !State;
        StartShaking(0.3f);
        for (float i = 0; i < 1; i += Engine.DeltaTime / 0.3f)
        {
            float eased = Ease.SineInOut(i);
            Bloom.Alpha = eased * 0.7f;
            Bloom.Radius = eased * 12;
            Nodebloom.Alpha = Nodebloom2.Alpha = eased * 0.7f;
            Nodebloom.Radius = Nodebloom2.Alpha = eased * 8;
            GemActivation = eased;
            yield return null;
        }
        Audio.Play("event:/FemtoHelper/switch_movet_accelerate", Position);
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.7f / SpeedMultiplier, true);
        tween.OnUpdate += delegate (Tween t)
        {
            CogWinding = State ? Calc.LerpClamp(0, 2 * MathF.Tau, t.Eased) : Calc.LerpClamp(2 * MathF.Tau, 0, t.Eased);
            Vector2 vector = Vector2.Lerp(start, end, t.Eased);
            ScrapeParticlesCheck(vector);
            MoveTo(vector);
        };
        Add(tween);
        yield return 0.7f / SpeedMultiplier;
        Audio.Play("event:/FemtoHelper/switch_movet_hit", Position);
        StartShaking(0.3f);
        for (float i = 0; i < 1; i += Engine.DeltaTime / 0.3f)
        {
            float eased = Ease.SineInOut(1 - i);
            Bloom.Alpha = eased * 0.7f;
            Bloom.Radius = eased * 12;
            Nodebloom.Alpha = Nodebloom2.Alpha = eased * 0.7f;
            Nodebloom.Radius = Nodebloom2.Alpha = eased * 8;
            GemActivation = eased;
            yield return null;
        }
        Activated = false;
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
                    SceneAs<Level>().ParticlesFG.Emit(PScrape, TopLeft + new Vector2(0f, (float)i + (float)num * 2f), (num == 1) ? (-MathF.PI / 4f) : (MathF.PI / 4f));
                }
            }
            if (Scene.CollideCheck<Solid>(vector + new Vector2(Width + 2f, num * -2)))
            {
                for (int j = num2; j < num3; j += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(PScrape, TopRight + new Vector2(-1f, (float)j + (float)num * 2f), (num == 1) ? (MathF.PI * -3f / 4f) : (MathF.PI * 3f / 4f));
                }
            }
        }
        else
        {
            if (!flag2 || flag)
            {
                return;
            }
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
                    SceneAs<Level>().ParticlesFG.Emit(PScrape, TopLeft + new Vector2((float)k + (float)num4 * 2f, -1f), (num4 == 1) ? (MathF.PI * 3f / 4f) : (MathF.PI / 4f));
                }
            }
            if (Scene.CollideCheck<Solid>(vector2 + new Vector2(num4 * -2, Height + 2f)))
            {
                for (int l = num5; l < num6; l += 8)
                {
                    SceneAs<Level>().ParticlesFG.Emit(PScrape, BottomLeft + new Vector2((float)l + (float)num4 * 2f, 0f), (num4 == 1) ? (MathF.PI * -3f / 4f) : (-MathF.PI / 4f));
                }
            }
        }
    }
}
