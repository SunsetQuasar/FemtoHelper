// Celeste.CS10_MoonIntro
using System;
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class TguCs0Intro : CutsceneEntity
{
	public const string Flag = "moon_intro";

	private readonly Player player;

	private BirdNPC bird;

	private float fade = 1f;

	private readonly float targetX;

	private Celeste.Mod.FemtoHelper.CustomFakeHeart fakeass;

	public TguCs0Intro(Player player)
	{
		Depth = -10000;
		this.player = player;
		targetX = player.CameraTarget.X + 8f;
	}

	public override void OnBegin(Level level)
	{
		bird = Scene.Entities.FindFirst<BirdNPC>();
		player.StateMachine.State = 11;
		if (level.Wipe != null)
		{
			level.Wipe.Cancel();
		}
		level.Wipe = new FadeWipe(level, wipeIn: true);
		ScreenWipe.WipeColor = Color.Black;
		Add(new Coroutine(Cutscene(level)));
	}

	private IEnumerator Cutscene(Level level)
	{
		player.StateMachine.State = 11;
		player.Visible = false;
		player.Active = false;
		player.Dashes = 1;
		level.Wipe.Duration = 1f;
		for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.5f)
		{
			level.Wipe.Percent = 0f;
			yield return null;
		}
		level.Camera.Position = level.LevelOffset + new Vector2(-100f, 0f);
		yield return 1f;
		ScreenWipe.WipeColor = Color.Black;
		yield return 1f;
		Add(new Coroutine(FadeIn(6f)));
		yield return 0.5f;
		yield return CameraTo(new Vector2(targetX, level.Camera.Y), 6f, Ease.SineInOut);
		level.Camera.Position = new Vector2(targetX, level.Camera.Y);
		if (bird != null)
		{
			yield return bird.StartleAndFlyAway();
			level.Session.DoNotLoad.Add(bird.EntityID);
			bird = null;
		}
		yield return 0.5f;
		player.Speed = Vector2.Zero;
		player.Position = level.GetSpawnPoint(player.Position);
		player.Active = true;
		player.StateMachine.State = 23;
		level.Add(fakeass = new Celeste.Mod.FemtoHelper.CustomFakeHeart(new Vector2(level.Bounds.Right - 96f, level.Bounds.Top + 90f)));
		while (player.Top > level.Bounds.Bottom)
		{
			yield return null;
		}
		yield return 0.2f;
		Audio.Play("event:/new_content/char/madeline/screenentry_lowgrav", player.Position);
		while (player.StateMachine.State == 23)
		{
			yield return null;
		}
		player.X = (int)player.X;
		player.Y = (int)player.Y;
		while (!player.OnGround() && player.Bottom < level.Bounds.Bottom)
		{
			player.MoveVExact(16);
		}
		player.StateMachine.State = 11;
		yield return 1.25f;
		yield return player.DummyWalkToExact((int)player.X - 4, walkBackwards: false, 0.4f);
		yield return 0.25f;
		yield return player.DummyWalkToExact((int)player.X + 8, walkBackwards: false, 0.4f);
		yield return 0.75f;
		yield return CameraTo(new Vector2(level.Bounds.Right - 320f, level.Camera.Y), 2f, Ease.CubeInOut);
		yield return 0.5f;
		Audio.Play("event:/new_content/game/10_farewell/bird_fly_uptonext", fakeass.Position);
		for (float i = -2.5f; i < 8f; i += 0.2f)
		{
			fakeass.Position.X += i;
			if (i >= 0)
			{
				fakeass.Position.Y -= 0.25f * i;
			}
			yield return null;
		}
		yield return 0.25f;
		fakeass.RemoveSelf();
		yield return CameraTo(new Vector2(targetX - 8f, level.Camera.Y), 2f, Ease.CubeInOut);
		yield return 0.5f;
		EndCutscene(level);
	}

	private IEnumerator FadeIn(float duration)
	{
		while (fade > 0f)
		{
			fade = Calc.Approach(fade, 0f, Engine.DeltaTime / duration);
			yield return null;
		}
	}

	public override void OnEnd(Level level)
	{
		level.Session.Inventory.Dashes = 1;
		player.Dashes = 1;
		player.Depth = 0;
		player.Speed = Vector2.Zero;
		player.Position = level.GetSpawnPoint(player.Position) + new Vector2(4f, -32f);
		player.Active = true;
		player.Visible = true;
		player.StateMachine.State = 0;
		player.X = (int)player.X;
		player.Y = (int)player.Y;
		while (!player.OnGround() && player.Bottom < level.Bounds.Bottom)
		{
			player.MoveVExact(16);
		}
		if (bird != null)
		{
			bird.RemoveSelf();
			level.Session.DoNotLoad.Add(bird.EntityID);
		}
		level.Camera.Position = new Vector2(targetX - 6f, level.Camera.Y);
		level.Session.SetFlag("moon_intro");
	}

	public override void Render()
	{
		Camera camera = (Scene as Level).Camera;
		Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * fade);
	}
}
