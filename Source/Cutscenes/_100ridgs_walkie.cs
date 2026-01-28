// Celeste.CS10_Gravestone
using System.Collections;
using Celeste;

public class _100ridgs_walkie(Player player) : CutsceneEntity
{
	public override void OnBegin(Level level)
	{
		Add(new Coroutine(Cutscene()));
	}

	private IEnumerator Cutscene()
	{
		player.StateMachine.State = 11;
		player.ForceCameraUpdate = true;
		player.DummyGravity = true;
		player.Speed.Y = 0f;
		yield return player.DummyWalkToExact(480);
		player.Facing = Facings.Right;
		yield return 11f;
		EndCutscene(Level);
	}

	public override void OnEnd(Level level)
	{
		player.Y = 888;
		player.X = 480;
		Level.Camera.Y = 731;
		Level.Camera.X = 391;
		player.Facing = Facings.Right;
		player.DummyAutoAnimate = true;
		player.DummyGravity = true;
		player.StateMachine.State = 0;
		Level.Session.Inventory.Dashes = 1;
		player.Dashes = 1;
		level.ResetZoom();
	}
}
