// Celeste.CS10_MoonIntro
using System;
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class S2A12_Intro : CutsceneEntity
{
	public const string Flag = "moon_intro";

	private Player player;

	private BadelineDummy badeline;

	private BirdNPC bird;

	private float fade = 1f;

	private float targetX;

	public S2A12_Intro(Player player)
	{
		base.Depth = -8500;
		this.player = player;
		targetX = player.CameraTarget.X + 8f;
	}

	public override void OnBegin(Level level)
	{
		bird = base.Scene.Entities.FindFirst<BirdNPC>();
		player.StateMachine.State = 11;
		if (level.Wipe != null)
		{
			level.Wipe.Cancel();
		}
		level.Wipe = new StarfieldWipe(level, wipeIn: true);
		Add(new Coroutine(Cutscene(level)));
	}

	private IEnumerator Cutscene(Level level)
	{
		player.StateMachine.State = 11;
		player.Visible = false;
		player.Active = false;
		player.Dashes = 2;
		for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.9f)
		{
			level.Wipe.Percent = 0f;
			yield return null;
		}
		Add(new Coroutine(FadeIn(5f)));
		level.Camera.Position = level.LevelOffset + new Vector2(-100f, 0f);
		yield return CutsceneEntity.CameraTo(new Vector2(targetX, level.Camera.Y), 6f, Ease.SineOut);
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
		while (player.Top > (float)level.Bounds.Bottom)
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
		while (!player.OnGround() && player.Bottom < (float)level.Bounds.Bottom)
		{
			player.MoveVExact(16);
		}
		player.StateMachine.State = 11;
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
		player.Dashes = 2;
		player.Depth = 0;
		player.Speed = Vector2.Zero;
		player.Position = level.GetSpawnPoint(player.Position) + new Vector2(0f, -32f);
		player.Active = true;
		player.Visible = true;
		player.StateMachine.State = 0;
		player.X = (int)player.X;
		player.Y = (int)player.Y;
		while (!player.OnGround() && player.Bottom < (float)level.Bounds.Bottom)
		{
			player.MoveVExact(16);
		}
		if (badeline != null)
		{
			badeline.RemoveSelf();
		}
		if (bird != null)
		{
			bird.RemoveSelf();
			level.Session.DoNotLoad.Add(bird.EntityID);
		}
		level.Camera.Position = new Vector2(targetX, level.Camera.Y);
		level.Session.SetFlag("moon_intro");
	}

	public override void Render()
	{
		Camera camera = (base.Scene as Level).Camera;
		Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * fade);
	}
}
