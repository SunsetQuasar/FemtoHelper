// Celeste.CS10_HubIntro
using System.Collections;
using System.Collections.Generic;
using Celeste;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

public class FS_CS0_HubIntro : CutsceneEntity
{
	private Celeste.Mod.FemtoHelper.CustomFakeHeart fakeass;

	public const string Flag = "hub_intro";

	public const float BirdOffset = 190f;

	private Player player;

	private List<LockBlock> locks;

	private Booster booster;

	private Vector2 spawn;

	private List<EventInstance> sfxs = new List<EventInstance>();

	public FS_CS0_HubIntro(Scene scene, Player player)
	{
		this.player = player;
		spawn = (scene as Level).GetSpawnPoint(player.Position);
		locks = scene.Entities.FindAll<LockBlock>();
		locks.Sort((LockBlock a, LockBlock b) => (int)(a.X + b.X));
		foreach (LockBlock @lock in locks)
		{
			@lock.Visible = false;
		}
		booster = scene.Entities.FindFirst<Booster>();
		if (booster != null)
		{
			booster.Visible = false;
		}
	}

	public override void OnBegin(Level level)
	{
		Add(new Coroutine(Cutscene(level)));
	}

	private IEnumerator Cutscene(Level level)
	{
		if (player.Holding != null)
		{
			player.Throw();
		}
		player.StateMachine.State = 11;
		player.ForceCameraUpdate = true;
		while (!player.OnGround())
		{
			yield return null;
			if (player.StateMachine.State == 19)
			{
				player.StateMachine.State = 11;
			}
		}
		player.ForceCameraUpdate = false;
		CutsceneEntity.CameraTo(new Vector2(level.Camera.X, level.Bounds.Center.Y), 1f, Ease.CubeInOut);
		yield return 0.25f;
		yield return player.DummyWalkToExact((int)player.X + 32, walkBackwards: false, 0.75f);
		player.DummyAutoAnimate = false;
		player.Sprite.Play("idleB");
		yield return 0.25f;
		level.Add(fakeass = new Celeste.Mod.FemtoHelper.CustomFakeHeart(new Vector2((float)level.Bounds.Right - 202f, level.Bounds.Center.Y)));
		Audio.Play("event:/FemtoHelper/CrystalHeart_PanUp", fakeass.Position);
		yield return CutsceneEntity.CameraTo(new Vector2((float)level.Bounds.Right - 100f - 320f, level.Bounds.Center.Y - 90f), 2.5f, Ease.CubeInOut);
		//yield return bird.IdleRoutine();
		yield return 0.6f;
		Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(level.Bounds.Right - 320f, level.Bounds.Center.Y - 90f), 0.8f, Ease.CubeInOut, 0.1f)));
		Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
		//yield return bird.FlyAwayRoutine();
		Audio.Play("event:/new_content/game/10_farewell/bird_fly_uptonext", fakeass.Position);
		for (float i = -2.5f; i < 12f; i += 0.195f)
		{
			fakeass.Position.X += i;
			fakeass.Position.Y += 0.02f * i;
			yield return null;
		}
		yield return 0.5f;
		//bird.RemoveSelf();
		//bird = null;
		yield return 0.5f;
		float duration = 7.2f;
		string sfx = "event:/FemtoHelper/LockBlock_Close";
		int doorIndex = 1;
		Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(level.Bounds.Left + 180, level.Bounds.Center.Y - 90f), duration, Ease.SineInOut)));
		Add(new Coroutine(Level.ZoomTo(new Vector2(160f, 90f), 1.5f, duration)));
		for (float t = 0f; t < duration; t += Engine.DeltaTime)
		{
			foreach (LockBlock @lock in locks)
			{
				if (!@lock.Visible && level.Camera.X + 70f < @lock.X - 20f)
				{
					sfxs.Add(Audio.Play(sfx, @lock.Center, "lockid", doorIndex - 1));
					@lock.Appear();
					Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
					doorIndex++;
				}
			}
			yield return null;
		}
		yield return 0.5f;
		if (booster != null)
		{
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			booster.Appear();
		}
		yield return 0.3f;
		yield return Level.ZoomBack(0.7f);
		Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(level.Bounds.Left, level.Bounds.Center.Y - 90f), 0.5f, Ease.SineInOut)));
		yield return 0.5f;
		EndCutscene(level);
	}

	public override void OnEnd(Level level)
	{
		if (WasSkipped)
		{
			foreach (EventInstance sfx in sfxs)
			{
				Audio.Stop(sfx);
			}
		}
		foreach (LockBlock @lock in locks)
		{
			@lock.Visible = true;
		}
		if (booster != null)
		{
			booster.Visible = true;
		}
		if (fakeass != null)
		{
			fakeass.RemoveSelf();
		}
		if (WasSkipped)
		{
			player.Position = spawn;
		}
		player.Speed = Vector2.Zero;
		player.DummyAutoAnimate = true;
		player.ForceCameraUpdate = false;
		player.StateMachine.State = 0;
		level.Camera.Position = new Vector2(level.Bounds.Left, level.Bounds.Center.Y - 90f);
		level.Session.SetFlag("hub_intro");
		level.ResetZoom();
	}
}
