// Celeste.CS10_Gravestone
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class SebsCutscene(Player player) : CutsceneEntity
{
	private float fade = 1f;


	public override void OnBegin(Level level)
	{
		
		if (level.Wipe != null)
		{
			level.Wipe.Cancel();
		}
		level.Flash(Color.Black, true);
		Add(new Coroutine(Cutscene()));
	}

	private IEnumerator Cutscene()
	{
		Level level = Scene as Level;
		player.StateMachine.State = 11;
		player.ForceCameraUpdate = true;
		player.DummyGravity = true;
		player.Speed = Vector2.Zero;
		player.Visible = false;
		Add(new Coroutine(FadeIn(3f)));
		yield return 3.4f;
		yield return Textbox.Say("sebsdialogue", EnterTunnel);
		Add(new Coroutine(FadeOut(2f)));
		yield return 2f;
		EndCutscene(Level);
	}

	public override void OnEnd(Level level)
	{
		
		player.DummyAutoAnimate = true;
		player.DummyGravity = true;
		//player.StateMachine.State = 0;
		player.Visible = false;
		Level.Session.Inventory.Dashes = 1;
		player.Dashes = 1;

		level.ResetZoom();
		EndTele();
		player.StateMachine.State = 0;
	}
	private IEnumerator FadeIn(float duration)
	{
		Level level = Scene as Level;
		while (fade > 0f)
		{
			fade = Calc.Approach(fade, 0f, Engine.DeltaTime / duration);
			yield return null;
		}
	}
	private IEnumerator FadeOut(float duration)
	{
		Level level = Scene as Level;
		while (fade <= 1f)
		{
			fade = Calc.Approach(fade, 1f, Engine.DeltaTime / duration);
			yield return null;
		}
	}
	private IEnumerator VisibilityHack()
	{
		Player player2 = player;
		player2.Visible = false;
		yield return 1f;
		player2.Visible = true;
	}
	private IEnumerator EnterTunnel()
	{
		Level level = Scene as Level;
			foreach (Backdrop item in level.Background.GetEach<Backdrop>("sebtunnel_start"))
			{
				item.ForceVisible = true;
				item.Position.Y -= 320;
				item.Position.X = level.Camera.Position.X + 400;
			}
			foreach (Backdrop item in level.Background.GetEach<Backdrop>("sebtunnel_start2"))
			{
				item.ForceVisible = true;
				item.Position.Y -= 320;
				item.Position.X = level.Camera.Position.X + 720;
			}
			yield return 3.3f;
			level.Session.SetFlag("sebscutscene_tunnel");
	}
	private static readonly System.Reflection.FieldInfo LevelFlash = typeof(Level).GetField("flash", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
	private static readonly System.Reflection.FieldInfo LevelFlashColor = typeof(Level).GetField("flashColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
	public override void Render()
	{
		Level level = Scene as Level;
		
		LevelFlashColor.SetValue(level, Color.Black);
		LevelFlash.SetValue(level, fade);
		//level.Render();
	}

	public void EndTele()
    {
		Level level = Scene as Level;
		Player player2 = player;
		level.OnEndOfFrame += delegate
		{
			new Vector2(level.LevelOffset.X + level.Bounds.Width - player2.X, player2.Y - level.LevelOffset.Y);
			Vector2 levelOffset = level.LevelOffset;
			Facings facing = player2.Facing;
			level.Remove(player2);
			level.UnloadLevel();
			level.Session.Level = "destination";
			Vector2? respos = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
			level.Session.RespawnPoint = respos;
			level.Session.FirstLevel = false;
			level.Camera.Position = level.LevelOffset;
			level.Session.Inventory.Dashes = 1;
			//level.Add(player2);
			
			player2.Facing = facing;
			player2.Hair.MoveHairBy(level.LevelOffset - levelOffset);
			level.Wipe = new SpotlightWipe(level, wipeIn: true);
			level.LoadLevel(Player.IntroTypes.WalkInRight);
			Add(new Coroutine(VisibilityHack()));
		};
	}
}
