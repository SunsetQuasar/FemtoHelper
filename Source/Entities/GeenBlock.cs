using System;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/GeenBlock")]
public class GeenBlock : Solid
{

    public class AfterImages : Entity
    {
        public GeenBlock Parent;
        public AfterImages(GeenBlock parent)
        {
            Parent = parent;
            Active = false;
            Depth = 5000;
        }

        public override void Render()
        {
            Position += Parent.Shake;
            float angle = Parent.angleTarget - Parent.arrowWiggle.Value / 5;
            Parent.DrawBlock(Calc.AngleToVector(angle, 8) * Mod(Parent.awesomei, 1), Color.White * (1 - Mod(Parent.awesomei, 1)));
            Parent.DrawBlock(Calc.AngleToVector(angle, 8) * Mod(Parent.awesomei + 0.5f, 1), Color.White * (1 - Mod(Parent.awesomei + 0.5f, 1)));
            base.Render();
            Position -= Parent.Shake;
        }
    }

    private readonly float spd;
    private readonly Image arrow;
    private readonly SineWave arrowWiggle;
    private readonly LightOcclude occlude;
    private float angleTarget;
    private readonly MTexture[,] edges = new MTexture[4, 4];
    private float awesomei;
    private readonly bool inverse;

    private AfterImages child;

    private bool deactivated;

    private bool staticMoverSignal;

    private float respawnPercent;

    private readonly bool oneUse;
    private readonly bool doesBreak;

    private readonly float respawnTime;
    private readonly float disappearTime;

    private readonly SoundSource sound;

    private Vector2 boostDir
    {
        get
        {
            Vector2 value;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                value = new Vector2(Input.MoveX, Input.MoveY) == Vector2.Zero ? new Vector2((int)player.Facing, 0) : new Vector2(Input.MoveX, Input.MoveY);
            }
            else
            {
                value = new Vector2(Input.MoveX, Input.MoveY);
            }
            return value * (inverse ? -1 : 1);
        }
    }

    public static float ChooseClosestAngle(float angle, float target)
    {
        return Math.Abs(angle - target) < Math.Abs(angle - target - 360) ? target : target - 360;
    }

    public GeenBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {
        spd = data.Float("speed", 400f);
        inverse = data.Bool("inverse", false);
        Add(arrow = new Image(GFX.Game["objects/FemtoHelper/GeenBlock/arrow"]));
        arrow.Position = new Vector2(Width, Height) / 2;
        arrow.CenterOrigin();
        Add(arrowWiggle = new SineWave(1.2f));
        Add(occlude = new LightOcclude());
        Add(sound = new SoundSource());

        Depth = -9999;

        MTexture slice = GFX.Game["objects/FemtoHelper/GeenBlock/block" + (inverse ? "Inverse" : "")];

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                edges[i, j] = slice.GetSubtexture(i * 8, j * 8, 8, 8);
            }
        }
        SurfaceSoundIndex = 7;
        Add(new Coroutine(Sequence()));

        oneUse = data.Bool("oneUse", true);
        respawnTime = data.Float("respawnTime", 2.65f);
        disappearTime = data.Float("disappearTime", 1.1f);
        doesBreak = data.Bool("disappear", true);
    }

    public IEnumerator Sequence()
    {
        if (!doesBreak) yield break;

        float sliceSmall = respawnTime < 2.65f ? Calc.LerpClamp(0.55f, 0, respawnTime / 2.65f) : 0.55f;
        float sliceSmaller = respawnTime < 2.65f ? Calc.LerpClamp(0.1f, 0, respawnTime / 2.65f) : 0.1f;

        while (true)
        {
            while (!(HasPlayerRider() || staticMoverSignal))
            {
                yield return null;
            }
            int count = 0;
            while (count < 4)
            {
                StartShaking(0.1f);
                count++;
                sound.Play("event:/char/madeline/landing", "surface_index", 7f).instance.setPitch(1 + (count / 3));
                yield return disappearTime / 5f;
            }
            Depth = 8000;
            foreach (StaticMover staticMover in staticMovers)
            {
                staticMover.Entity.Depth = Depth + 1;
            }
            Audio.Play("event:/game/03_resort/fallblock_wood_impact", "surface_index", 7f);
            StartShaking(0.4f);
            Collidable = false;
            deactivated = true;
            occlude.Alpha = 0f;
            arrow.Visible = false;
            DisableStaticMovers();
            child.Visible = false;

            if(oneUse)
            {
                child.RemoveSelf();
                Remove(arrow);
                Remove(occlude);
                yield break;
            }

            respawnPercent = 0f;
            Tween.Set(this, Tween.TweenMode.Oneshot, respawnTime, Ease.CubeInOut, (t) =>
            {
                respawnPercent = t.Eased;
            });
            yield return respawnTime - sliceSmall - sliceSmaller;
            sound.Play("event:/game/04_cliffside/arrowblock_reform_begin");
            yield return sliceSmall;
            StartShaking(sliceSmaller);
            yield return sliceSmaller;
            while (CollideCheck<Actor>())
            {
                yield return null;
            }

            StartShaking(0.4f - sliceSmaller);
            sound.Play("event:/game/04_cliffside/arrowblock_reappear");

            deactivated = false;
            Collidable = true;
            child.Visible = true;
            arrow.Visible = true;
            EnableStaticMovers();
            occlude.Alpha = 1f;
            Depth = -9999;

        }
    }

    public override void OnStaticMoverTrigger(StaticMover sm)
    {
        base.OnStaticMoverTrigger(sm);
        staticMoverSignal = true;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        scene.Add(child = new AfterImages(this));
        foreach (StaticMover staticMover in staticMovers)
        {
            if (staticMover.Entity is Spikes spikes)
            {
                spikes.DisabledColor = Color.Gray;
                spikes.VisibleWhenDisabled = true;
            }
            if (staticMover.Entity is Spring spring)
            {
                spring.DisabledColor = Color.Gray;
                spring.VisibleWhenDisabled = true;
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (deactivated) return;
        MoveHExact(0);
        angleTarget = Calc.AngleApproach(angleTarget, boostDir.Angle(), Engine.DeltaTime * 16);
        arrow.Rotation = angleTarget + arrowWiggle.Value / 5;
        awesomei += Engine.DeltaTime * 3;
    }

    public void DrawBlock(Vector2 offset, Color color)
    {
        Vector2 pos = Position;
        Position += offset;
        for (int k = 0; (float)k < Width / 8f; k++)
        {
            for (int l = 0; (float)l < Height / 8f; l++)
            {
                int num4 = (int)(Width / 8f) == 1 ? 3 : ((k != 0) ? (((float)k != Width / 8f - 1f) ? 1 : 2) : 0);
                int num5 = (int)(Height / 8f) == 1 ? 3 : ((l != 0) ? (((float)l != Height / 8f - 1f) ? 1 : 2) : 0);
                edges[num4, num5].Draw(new Vector2(X + (float)(k * 8), Y + (float)(l * 8)), Vector2.Zero, color);
            }
        }
        Position = pos;
    }

    public override void Render()
    {
        Vector2 pos = Position;
        Position += Shake;
        DrawBlock(Vector2.Zero, deactivated ? Color.Gray : Color.White);
        if (!deactivated)
        {
            arrow.DrawOutline(Calc.HexToColor("070719"));
        } 
        else if (!oneUse)
        {
            int SegmentCount = 128;

            float ww = (Width / 2) - 1;
            float hh = (Height / 2) - 1;

            for (int i = 0; i < SegmentCount; i++)
            {
                if (respawnPercent * 128f < i) break;

                float fi = (float)i / SegmentCount;
                float nextfi = ((float)(i + 1) / SegmentCount) % 1;

                Draw.Line(Center + ParametricRoundedSquare(fi, ww, hh, 4), Center + ParametricRoundedSquare(nextfi, ww, hh, 4), new Color(0.4f, 0.4f, 0.4f, 0f));
            }
        }
        base.Render();
        Position = pos;
    }

    public override void MoveHExact(int move)
    {
        LiftSpeed = (boostDir.EightWayNormal() * spd);
        base.MoveHExact(move);
    }

    public override void MoveVExact(int move)
    {
        LiftSpeed = (boostDir.EightWayNormal() * spd);
        base.MoveVExact(move);
    }

    public static float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
