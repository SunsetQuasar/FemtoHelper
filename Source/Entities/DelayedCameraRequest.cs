

namespace Celeste.Mod.FemtoHelper.Entities;

public class DelayedCameraRequest : Entity
{
    public Player player;
    public bool error;
    public DelayedCameraRequest(Player player, bool error) : base(Vector2.Zero)
    {
        this.player = player;
        this.error = error;
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        (Scene as Level).DoScreenWipe(wipeIn: true);
        (Scene as Level).Camera.Position = player.CameraTarget;
        player.StateMachine.State = 0;
        if (error)
        {
            Scene.Add(new MiniTextbox("FEMTOHELPER_ERRORHANDLER_INVALID_ROOM"));
        }
        RemoveSelf();
    }

    public override void Render()
    {
        base.Render();
    }
}