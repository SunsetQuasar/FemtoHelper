using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Drawing;

namespace Celeste.Mod.FemtoHelper.Entities;

public class DelayedCameraRequest : Entity
{
    public Player player;
    public DelayedCameraRequest(Player player) : base(Vector2.Zero)
    {
        this.player = player;
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        (Scene as Level).DoScreenWipe(wipeIn: true);
        (Scene as Level).Camera.Position = player.CameraTarget;
        player.StateMachine.State = 0;
        RemoveSelf();
    }

    public override void Render()
    {
        base.Render();
    }
}