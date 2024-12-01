

namespace Celeste.Mod.FemtoHelper.Entities;

public class DelayedCameraRequest(Player player, bool error) : Entity(Vector2.Zero)
{
    public readonly Player Player = player;
    public readonly bool Error = error;

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        (Scene as Level)?.DoScreenWipe(wipeIn: true);
        (Scene as Level).Camera.Position = Player.CameraTarget;
        Player.StateMachine.State = 0;
        if (Error)
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