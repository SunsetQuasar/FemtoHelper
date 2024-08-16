// Celeste.CS10_Farewell
		using System;
		using System.Collections;
		using Celeste;
		using FMOD.Studio;
		using Microsoft.Xna.Framework;
		using Monocle;

public class CS_FunnyAsHell : CutsceneEntity
	{
		private Player player;

		private NPC granny;

		private float fade;
		private Coroutine grannyWalk4head;
		private Coroutine grannyWalk;

		private EventInstance snapshot;

		private EventInstance dissipate;

		private float width = 0;

		private float darkness = 0;

		private bool spawned = false;

		public CS_FunnyAsHell(Player player)
			: base(fadeInOnSkip: false)
		{
			this.player = player;
			base.Depth = -1000000;
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Level obj = scene as Level;
			obj.TimerStopped = true;
			obj.TimerHidden = true;
			obj.SaveQuitDisabled = true;
			obj.SnapColorGrade("none");
			snapshot = Audio.CreateSnapshot("snapshot:/game_10_granny_clouds_dialogue");
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.Dashes = 1;
			player.StateMachine.State = 11;
			player.Sprite.Play("idle");
			player.Visible = false;
			Audio.SetMusic("event:/new_content/music/lvl10/granny_farewell");
			FadeWipe fadeWipe = new FadeWipe(Level, wipeIn: true);
			fadeWipe.Duration = 2f;
			ScreenWipe.WipeColor = Color.White;
			yield return fadeWipe.Duration;
			yield return 1.5f;
			Add(new Coroutine(Level.ZoomTo(new Vector2(160f, 125f), 2f, 5f)));
			yield return 0.2f;
			Audio.Play("event:/new_content/char/madeline/screenentry_gran");
			yield return 0.3f;
			_ = player.Position;
			player.Position = new Vector2(player.X, level.Bounds.Bottom + 8);
			player.Speed.Y = -160f;
			player.Visible = true;
			player.DummyGravity = false;
			player.MuffleLanding = true;
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			while (!player.OnGround() || player.Speed.Y < 0f)
			{
				float y = player.Speed.Y;
				player.Speed.Y += Engine.DeltaTime * 900f * 0.2f;
				if (y < 0f && player.Speed.Y >= 0f)
				{
					player.Speed.Y = 0f;
					yield return 0.2f;
				}
				yield return null;
			}
			Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
			Audio.Play("event:/new_content/char/madeline/screenentry_gran_landing", player.Position);
			yield return 0.5;
			Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(level.Camera.X + 32, level.Camera.Y), 2f, Ease.CubeInOut, 0.1f)));
			yield return 2.1f;
			SceneAs<Level>().Shake(0.5f);
			player.DummyWalkToExact((int)player.Position.X - 8, walkBackwards: true, 0.4f);
			for (float i = 1.1f; i >= -1.9; i -= 0.02f)
			{
				width += i;
				if (width > 30 && spawned == false)
				{
					spawned = true;
					granny = new NPC(player.Position + new Vector2(80f, 0f));
					granny.IdleAnim = "idle";
					granny.MoveAnim = "walk";
					granny.Maxspeed = 15f;
					granny.Add(granny.Sprite = GFX.SpriteBank.Create("granny"));
					GrannyLaughSfx grannyLaughSfx = new GrannyLaughSfx(granny.Sprite);
					grannyLaughSfx.FirstPlay = false;
					granny.Add(grannyLaughSfx);
					granny.Sprite.OnFrameChange = delegate (string anim)
					{
						int currentAnimationFrame = granny.Sprite.CurrentAnimationFrame;
						if (anim == "walk" && currentAnimationFrame == 2)
						{
							float volume = Calc.ClampedMap((player.Position - granny.Position).Length(), 64f, 128f, 1f, 0f);
							Audio.Play("event:/new_content/char/granny/cane_tap_ending", granny.Position).setVolume(volume);
						}
					};
					Scene.Add(granny);
					grannyWalk4head = new Coroutine(granny.MoveTo(granny.Position + new Vector2(-1, 0)));
					Add(grannyWalk4head);
					grannyWalk = new Coroutine(granny.MoveTo(player.Position + new Vector2(16f, 0f)));
				}
				yield return null;
			}
			Add(grannyWalk);
			yield return Textbox.Say("hello_madeline", Laugh, StopLaughing, StepForward, GrannyDisappear, FadeToWhite, WaitForGranny);
			yield return 0.8f;
			yield return player.DummyWalkToExact((int)player.X + 4, walkBackwards: false, 0.4f);
			yield return 0.8f;
			yield return 2f;
			while (fade < 1f)
			{
				yield return null;
			}
			EndCutscene(level);
		}

		private IEnumerator WaitForGranny()
		{
			while (grannyWalk != null && !grannyWalk.Finished)
			{
				yield return null;
			}
		}

		private IEnumerator Laugh()
		{
			granny.Sprite.Play("laugh");
			yield break;
		}

		private IEnumerator StopLaughing()
		{
			granny.Sprite.Play("idle");
			yield break;
		}

		private IEnumerator StepForward()
		{
			yield return player.DummyWalkToExact((int)player.X + 8, walkBackwards: false, 0.4f);
		}

		private IEnumerator GrannyDisappear()
		{
			Audio.SetMusicParam("end", 1f);
			Add(new Coroutine(player.DummyWalkToExact((int)player.X + 8, walkBackwards: false, 0.4f)));
			yield return 0.1f;
			dissipate = Audio.Play("event:/new_content/char/granny/dissipate", granny.Position);
			MTexture frame = granny.Sprite.GetFrame(granny.Sprite.CurrentAnimationID, granny.Sprite.CurrentAnimationFrame);
			Level.Add(new DisperseImage(granny.Position, new Vector2(1f, -0.1f), granny.Sprite.Origin, granny.Sprite.Scale, frame));
			yield return null;
			granny.Visible = false;
			yield return 3.5f;
		}

		private IEnumerator FadeToWhite()
		{
			Add(new Coroutine(DoFadeToWhite()));
			yield break;
		}

		private IEnumerator DoFadeToWhite()
		{
			Add(new Coroutine(Level.ZoomBack(8f)));
			while (fade < 1f)
			{
				fade = Calc.Approach(fade, 1f, Engine.DeltaTime / 8f);
				yield return null;
			}
		}

		public override void OnEnd(Level level)
		{
			Dispose();
			if (WasSkipped)
			{
				Audio.Stop(dissipate);
			}
			Level.OnEndOfFrame += delegate
			{
				Achievements.Register(Achievement.FAREWELL);
				Level.TeleportTo(player, "end-cinematic", Player.IntroTypes.Transition);
			};
		}

		public override void SceneEnd(Scene scene)
		{
			base.SceneEnd(scene);
			Dispose();
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			Dispose();
		}

		private void Dispose()
		{
			Audio.ReleaseSnapshot(snapshot);
			snapshot = null;
		}

		public override void Render()
		{
			if (fade > 0f)
			{
				Draw.Rect(Level.Camera.X - 1f, Level.Camera.Y - 1f, 322f, 182f, Color.White * fade);
			}
			Draw.Rect(Level.Camera.X - 1f, Level.Camera.Y - 1f, 322f, 182f, Color.Black * darkness);
			Draw.Rect(player.X + 80 - (width / 2), Level.Camera.Y - 3f, (float)Math.Max(width, 0), 185f, Color.White * 0.5f);
			Draw.Rect(player.X + 80 - ((width - 4) / 2), Level.Camera.Y - 3f, (float)Math.Max(width-4, 0), 185f, Color.White * 0.5f);
			Draw.Rect(player.X + 80 - ((width - 8) / 2), Level.Camera.Y - 3f, (float)Math.Max(width-8, 0), 185f, Color.White * 0.5f);
		}
	}