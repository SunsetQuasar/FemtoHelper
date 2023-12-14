// Celeste.WaveDashPage03
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class UltraDashPage07 : UltraDashPage
{
	private string title;

	private string titleDisplayed;

	private MTexture clipArt;

	private float clipArtEase;

	private FancyText.Text infoText;

	private ReverseAreaCompleteTitle easyText;

	private bool complementText = false;

	public UltraDashPage07()
	{
		Transition = Transitions.Spiral;
		ClearColor = Calc.HexToColor("F2B44B");
		title = Dialog.Clean("AliceQuasar_FOOLSPACE_WAVEDASH_PAGE7_TITLE");
		titleDisplayed = "";
	}

	public override void Added(UltraDashPresentation presentation)
	{
		base.Added(presentation);
		clipArt = presentation.Gfx["FemtoHelper/movesetReverse"];
	}

	public override IEnumerator Routine()
	{
		while (titleDisplayed.Length < title.Length)
		{
			titleDisplayed += title[titleDisplayed.Length];
			yield return 0.05f;
		}
		yield return PressButton();
		Audio.Play("event:/new_content/game/10_farewell/ppt_wavedash_whoosh");
		while (clipArtEase < 1f)
		{
			clipArtEase = Calc.Approach(clipArtEase, 1f, Engine.DeltaTime);
			yield return null;
		}
		yield return 0.25f;
		infoText = FancyText.Parse(Dialog.Get("AliceQuasar_FOOLSPACE_WAVEDASH_PAGE7_INFO"), Width - 240, 32, 1f, Color.Black * 0.7f);
		yield return PressButton();
		Audio.Play("event:/new_content/game/10_farewell/ppt_its_easy");
		easyText = new ReverseAreaCompleteTitle(new Vector2((float)Width / 2f, Height - 150), Dialog.Clean("AliceQuasar_FOOLSPACE_WAVEDASH_PAGE7_EASY"), 2f, rainbow: true);
		yield return 1f;
		complementText = true;
	}

	public override void Update()
	{
		if (easyText != null)
		{
			easyText.Update();
		}
	}

	public override void Render()
	{
		ActiveFont.DrawOutline(titleDisplayed, new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
		if (clipArtEase > 0f)
		{
			Vector2 scale = Vector2.One * (1f + (1f - clipArtEase) * 1f) * 0.8f;
			float rotation = (1f - clipArtEase) * 8f;
			Color color = Color.White * clipArtEase;
			clipArt.DrawCentered(new Vector2((float)base.Width / 2f, (float)base.Height / 2f - 90f), color, scale, rotation);
		}
		if (infoText != null)
		{
			infoText.Draw(new Vector2((float)base.Width / 2f, base.Height - 350), new Vector2(0.5f, 0f), Vector2.One, 1f);
		}
		if (easyText != null)
		{
			easyText.Render();
		}
		if (complementText)
		{
			ActiveFont.Draw(Dialog.Clean("AliceQuasar_FOOLSPACE_WAVEDASH_PAGE7_EASY_COMPLEMENT"), new Vector2(20f, (float)base.Height - 40f), new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), Color.Black * 0.4f);
		}
	}
}
