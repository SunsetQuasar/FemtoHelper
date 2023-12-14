// Celeste.CS10_MoonIntro
using System;
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class BB_CS_PLEASEWORK : CutsceneEntity
{
	public const string Flag = "moon_intro";

	private Player player;

	private BadelineDummy badeline;

	private BirdNPC bird;

	private float fade = 1f;

	private float targetX;

	private float width = 0;

	private bool spawned = false;

	private ParticleType spawnParticle = new ParticleType
	{
		Source = GFX.Game["particles/smoke0"],
		Color = Calc.HexToColor("EEFFFF"),
		Color2 = Calc.HexToColor("EEEEFF"),
		FadeMode = ParticleType.FadeModes.Linear,
		ColorMode = ParticleType.ColorModes.Choose,
		RotationMode = ParticleType.RotationModes.Random,
		LifeMin = 1f,
		LifeMax = 1.5f,
		Size = 1f,
		SizeRange = 0.5f,
		ScaleOut = true,
		SpeedMin = 60f,
		SpeedMax = 100f,
		Friction = 180f,
		Acceleration = new Vector2(0f, -200f),
		SpinMin = 8f,
		SpinMax = 16f
	};

	public BB_CS_PLEASEWORK(Player player)
	{
		base.Depth = -10000;
		this.player = player;
		targetX = player.CameraTarget.X;
	}

	public override void OnBegin(Level level)
	{
		bird = base.Scene.Entities.FindFirst<BirdNPC>();
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
		level.Wipe.Duration = 0.5f;
		for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.5f)
		{
			level.Wipe.Percent = 0f;
			yield return null;
		}
		level.Camera.Position = level.LevelOffset + new Vector2(40f, -20f);
		yield return 0.5f;
		ScreenWipe.WipeColor = Color.Black; 
		Add(new Coroutine(FadeIn(3f)));
		yield return 0.5f;
		yield return CutsceneEntity.CameraTo(new Vector2(targetX, level.Camera.Y + 20), 3f, Ease.SineInOut);
		level.Camera.Position = new Vector2(targetX, level.Camera.Y);
		yield return 0.5f;
		player.Speed = Vector2.Zero;
		player.Position = level.GetSpawnPoint(player.Position);
		player.Active = true;
		yield return 0.2f;
		Audio.Play("event:/game/09_core/frontdoor_unlock", player.Position);
		SceneAs<Level>().Shake(0.5f);
		player.Depth = 0;
		for (float i = 1.4f; i >= -1.9; i -= 0.024f)
		{
			width += i;
			if (width > 30 && spawned == false)
			{
				for (int p = 0; p < 50; p++)
				{
					SceneAs<Level>().Particles.Emit(spawnParticle, 1, player.Center + new Vector2(0, Calc.Random.Range(-160, 32)), Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
				}
				spawned = true;
				level.Session.Inventory.Dashes = 1;
				player.Dashes = 1;
				player.Speed = Vector2.Zero;
				player.Position = level.GetSpawnPoint(player.Position) + new Vector2(0f, -32f);
				player.Active = true;
				player.Visible = true;
				player.StateMachine.State = 11;
				player.X = (int)player.X;
				player.Y = (int)player.Y;
				while (!player.OnGround() && player.Bottom < (float)level.Bounds.Bottom)
				{
					player.MoveVExact(16);
				}
			}
			yield return null;
		}
		yield return 0.6f;
		yield return player.DummyWalkToExact((int)player.X + 64, walkBackwards: false, 0.8f);
		yield return 0.2f;
		yield return level.ZoomTo(new Vector2(196f, 120f), 2f, 3f);
		yield return Audio.SetMusic("event:/AliceQuasar_BouncyBash/herBest");
		yield return Textbox.Say("UBC2021_AliceQuasar_intro");
		Audio.SetMusicParam("fade", 0f);
		yield return level.ZoomBack(0.5f);
		Audio.SetMusicParam("fade", 1f);
		yield return Audio.SetMusic("event:/AliceQuasar_BouncyBash/main");
		Audio.SetMusicParam("alicequasar_started", 1f);
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
		player.Position = level.GetSpawnPoint(player.Position) + new Vector2(64f, -32f);
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
		Draw.Rect(player.X - ((width - 4) / 2), Level.Camera.Y - 3f, (float)Math.Max(width - 4, 0), 185f, Color.White * 0.5f);
		Draw.Rect(player.X - ((width - 8) / 2), Level.Camera.Y - 3f, (float)Math.Max(width - 8, 0), 185f, Color.White * 0.5f);
	}
}
