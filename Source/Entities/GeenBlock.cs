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
            this.Parent = parent;
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

    private float spd;
    private Image arrow;
    private SineWave arrowWiggle;
    private float angleTarget;
    private MTexture[,] edges = new MTexture[4, 4];
    private float awesomei;
    private readonly bool inverse;

    private AfterImages child;

    private bool deactivated;

    private bool staticMoverSignal;

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

    public float ChooseClosestAngle(float angle, float target)
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
        Add(new LightOcclude());

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
    }

    public IEnumerator Sequence()
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
            Audio.Play("event:/char/madeline/landing", Center, "surface_index", 7f).setPitch(1 + (count / 3));
            yield return 0.22f;
        }
        Depth = 8000;
        foreach (StaticMover staticMover in staticMovers)
        {
            staticMover.Entity.Depth = Depth + 1;
        }
        Audio.Play("event:/game/03_resort/fallblock_wood_impact", Center, "surface_index", 7f);
        StartShaking(0.4f);
        Collidable = false;
        deactivated = true;
        Remove(Get<LightOcclude>());
        Remove(arrow);
        DisableStaticMovers();
        child.RemoveSelf();
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
        Position += Shake;
        DrawBlock(Vector2.Zero, deactivated ? Color.Gray : Color.White);
        if (!deactivated) arrow.DrawOutline(Calc.HexToColor("070719"));
        base.Render();
        Position -= Shake;
    }

    public override void MoveHExact(int move)
    {
        LiftSpeed = (boostDir.EightWayNormal() * spd);
        base.MoveHExact(move);
    }

    public override void MoveVExact(int move)
    {
        LiftSpeed = (new Vector2(Input.MoveX, Input.MoveY).EightWayNormal() * spd);
        base.MoveVExact(move);
    }

    public static float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
