
namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/LineTrigger")]
public class LineTrigger : Entity
{
    public Vector2 node;
    public bool leftIsTrue;
    public string flag;
    public bool lastLeft;
    public Player player;
    public Hitbox hitbox;
    public LineTrigger(EntityData data, Vector2 offset) : base(data.Position + offset) 
    {
        node = data.NodesOffset(offset)[0];
        if (node.X < Position.X)
        {
            Vector2 num = node;
            node = Position;
            Position = num;
        }

        if (node.Y < Position.Y)
        {
            Vector2 num = node;
            node = Position;
            Position = num;
        }

        flag = data.Attr("flag", "line_trigger_flag");
        leftIsTrue = data.Bool("leftIsTrue", true);
        hitbox = new Hitbox(16, 16);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        player = Scene.Tracker.GetEntity<Player>();
    }

    public override void Update()
    {
        base.Update();

        if (player != null)
        {
            float percent = Calc.ClampedMap(player.Y, Position.Y, node.Y, Position.X, node.X);
            bool left = (player.X < percent);
            if (left != lastLeft)
            {
                if((player.Y >= Position.Y) && (player.Y <= node.Y)) (Scene as Level).Session.SetFlag(flag, leftIsTrue ? left : !left);
            }
            lastLeft = left;
        }
    }

    public override void Render()
    {
        base.Render();

    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        if (player != null)
        {
            float percent = Calc.ClampedMap(player.Y, Position.Y, node.Y, Position.X, node.X);
            Draw.Line(Position, node, Color.MistyRose);
            Color col = Color.OrangeRed;
            col.A = 0;
            Draw.Rect(new Vector2(percent, Calc.Clamp(player.Y, Position.Y, node.Y)), 5, 5, col);
        }
    }
}
