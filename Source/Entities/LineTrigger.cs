
namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/LineTrigger")]
public class LineTrigger : Entity
{
    public Vector2 Node;
    public readonly bool LeftIsTrue;
    public readonly string Flag;
    public bool LastLeft;
    public Player Player;
    public Hitbox Hitbox;
    public LineTrigger(EntityData data, Vector2 offset) : base(data.Position + offset) 
    {
        Node = data.NodesOffset(offset)[0];
        if (Node.X < Position.X)
        {
            (Node, Position) = (Position, Node);
        }

        if (Node.Y < Position.Y)
        {
            (Node, Position) = (Position, Node);
        }

        Flag = data.Attr("flag", "line_trigger_flag");
        LeftIsTrue = data.Bool("leftIsTrue", true);
        Hitbox = new Hitbox(16, 16);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Player = Scene.Tracker.GetEntity<Player>();
    }

    public override void Update()
    {
        base.Update();

        if (Player != null)
        {
            float percent = Calc.ClampedMap(Player.Y, Position.Y, Node.Y, Position.X, Node.X);
            bool left = (Player.X < percent);
            if (left != LastLeft)
            {
                if((Player.Y >= Position.Y) && (Player.Y <= Node.Y)) (Scene as Level).Session.SetFlag(Flag, LeftIsTrue ? left : !left);
            }
            LastLeft = left;
        }
    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        if (Player == null) return;
        float percent = Calc.ClampedMap(Player.Y, Position.Y, Node.Y, Position.X, Node.X);
        Draw.Line(Position, Node, Color.MistyRose);
        Color col = Color.OrangeRed;
        col.A = 0;
        Draw.Rect(new Vector2(percent, Calc.Clamp(Player.Y, Position.Y, Node.Y)), 5, 5, col);
    }
}
