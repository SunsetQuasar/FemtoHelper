// Celeste.WaveDashPage01
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class UltraDashPage01 : UltraDashPage
{
	private ReverseAreaCompleteTitle title;

	private float subtitleEase;

	public UltraDashPage01()
	{
		Transition = Transitions.ScaleIn;
		ClearColor = Color.Lerp(Calc.HexToColor("DADB0D"), Color.White, 0.5f);
	}

	public override void Added(UltraDashPresentation presentation)
	{
		base.Added(presentation);
	}

	public override IEnumerator Routine()
	{
		Audio.SetAltMusic("event:/AliceQuasar_FOOL_SPACE/intermission_powerpointUltra");
		yield return 1f;
		title = new ReverseAreaCompleteTitle(new Vector2((float)Width / 2f, (float)Height / 2f - 100f), Dialog.Clean("AliceQuasar_FOOLSPACE_WAVEDASH_PAGE1_TITLE"), 2f, rainbow: true);
		yield return 1f;
		while (subtitleEase < 1f)
		{
			subtitleEase = Calc.Approach(subtitleEase, 1f, Engine.DeltaTime);
			yield return null;
		}
		yield return 0.1f;
	}

	public override void Update()
	{
		if (title != null)
		{
			title.Update();
		}
	}

	public override void Render()
	{
		if (title != null)
		{
			title.Render();
		}
		if (subtitleEase > 0f)
		{
			Vector2 position = new Vector2((float)base.Width / 2f, (float)base.Height / 2f + 80f);
			float x = 1f + Ease.BigBackIn(1f - subtitleEase) * 2f;
			float y = 0.25f + Ease.BigBackIn(subtitleEase) * 0.75f;
			float ypos = Ease.BackOut(subtitleEase) * 48f;
			ActiveFont.Draw(Dialog.Clean("AliceQuasar_FOOLSPACE_WAVEDASH_PAGE1_SUBTITLE"), position, new Vector2(0.5f, 0.5f), new Vector2(x, y), Color.Black * 0.8f);
			ActiveFont.Draw(Dialog.Clean("AliceQuasar_FOOLSPACE_WAVEDASH_PAGE1_CREDITS"), new Vector2((float)base.Width / 2f, (float)base.Height - ypos), new Vector2(0.5f, 0.5f), new Vector2(0.8f, 0.8f), Color.Black * 0.4f);
		}
	}
}
