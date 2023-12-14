// Celeste.WaveDashPage04
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class UltraDashPage06 : UltraDashPage
{
	private WaveDashPlaybackTutorial tutorial;

	private FancyText.Text list;

	private int listIndex;

	private float time;

	public UltraDashPage06()
	{
		Transition = Transitions.Rotate3D;
		ClearColor = Calc.HexToColor("CCCCCC");
	}

	public override void Added(UltraDashPresentation presentation)
	{
		base.Added(presentation);
		List<MTexture> textures = Presentation.Gfx.GetAtlasSubtextures("FemtoHelper/platformsb");
		tutorial = new WaveDashPlaybackTutorial("FemtoHelper/bhopppt", new Vector2(-126f - 14f, 0f - 21f), new Vector2(1f, 1f), new Vector2(1f, -1f));
		tutorial.OnRender = delegate
		{
			textures[(int)(time % (float)textures.Count)].DrawCentered(Vector2.Zero);
		};
	}

	public override IEnumerator Routine()
	{
		yield return 0.5f;
		list = FancyText.Parse(Dialog.Get("AliceQuasar_FOOLSPACE_WAVEDASH_PAGE6_LIST"), Width, 32, 1f, Color.Black * 0.7f);
		float delay = 0f;
		while (listIndex < list.Nodes.Count)
		{
			if (list.Nodes[listIndex] is FancyText.NewLine)
			{
				yield return PressButton();
			}
			else
			{
				delay += 0.008f;
				if (delay >= 0.016f)
				{
					delay -= 0.016f;
					yield return 0.016f;
				}
			}
			listIndex++;
		}
	}

	public override void Update()
	{
		time += Engine.DeltaTime * 4f;
		tutorial.Update();
	}

	public override void Render()
	{
		ActiveFont.DrawOutline(Dialog.Clean("WAVEDASH_PAGE4_TITLE"), new Vector2(128f, 48f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
		tutorial.Render(new Vector2((float)base.Width / 2f, ((float)base.Height / 2f) - 60f), 3f);
		if (list != null)
		{
			list.Draw(new Vector2(160f, base.Height - 280), new Vector2(0f, 0f), Vector2.One * 0.8f, 1f, 0, listIndex);
		}
	}
}
