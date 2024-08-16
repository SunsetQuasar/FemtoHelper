// Celeste.WaveDashPage06
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Celeste.Mod;
using Celeste.Mod.Entities;

public class UltraDashPage09 : UltraDashPage
{
	private ReverseAreaCompleteTitle title;

	private float time = 0f;

	public UltraDashPage09()
	{
		Transition = Transitions.Rotate3D;
		ClearColor = Calc.HexToColor("EB3C50");
	}

	public override IEnumerator Routine()
	{
		yield return 1f;
		Audio.Play("event:/new_content/game/10_farewell/ppt_happy_wavedashing");
		title = new ReverseAreaCompleteTitle(new Vector2((float)Width / 2f, 150f), Dialog.Clean("AliceQuasar_FOOLSPACE_WAVEDASH_PAGE9_TITLE"), 2f, rainbow: true);
		yield return 1.5f;
	}

	public override void Update()
	{
		if (title != null)
		{
			title.Update();
		}
		time += Engine.DeltaTime * 800f;
	}

	public override void Render()
	{
		Presentation.Gfx["FemtoHelper/Swag Clip Art"].DrawCentered(new Vector2(base.Width, base.Height) / 2f + new Vector2(-40 + (float)Math.Sin(time / 12 * Engine.DeltaTime) * 80, (float)Math.Cos(time / 12 * Engine.DeltaTime) * 20), Color.White, 0.7f + (float)Math.Sin(time / 12 * Engine.DeltaTime) * 0.1f);
		if (title != null)
		{
			title.Render();
		}
	}
}
