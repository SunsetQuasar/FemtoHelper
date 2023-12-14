// Celeste.WaveDashPage00
using System;
using System.Collections;
using System.Globalization;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class UltraDashPage00 : UltraDashPage
{
	private Color taskbarColor = Calc.HexToColor("d9d3b1");

	private string time;

	private Vector2 pptIcon;

	private Vector2 cursor;

	private bool selected;

	public UltraDashPage00()
	{
		AutoProgress = true;
		ClearColor = Calc.HexToColor("118475");
		time = DateTime.Now.ToString("h:mm tt", CultureInfo.CreateSpecificCulture("en-US"));
		pptIcon = new Vector2(1080f, 520f);
		cursor = new Vector2(700f, 700f);
	}

	public override IEnumerator Routine()
	{
		yield return 1f;
		yield return MoveCursor(cursor + new Vector2(-20f, -40f), 0.4f);
		yield return 0.2f;
		yield return MoveCursor(pptIcon, 0.8f);
		yield return 0.7f;
		selected = true;
		Audio.Play("event:/new_content/game/10_farewell/ppt_doubleclick");
		yield return 0.1f;
		selected = false;
		yield return 0.1f;
		selected = true;
		yield return 0.08f;
		selected = false;
		yield return 0.5f;
		Presentation.ScaleInPoint = pptIcon;
	}

	private IEnumerator MoveCursor(Vector2 to, float time)
	{
		Vector2 from = cursor;
		for (float t = 0f; t < 1f; t += Engine.DeltaTime / time)
		{
			cursor = from + (to - from) * Ease.SineOut(t);
			yield return null;
		}
	}

	public override void Update()
	{
	}

	public override void Render()
	{
		DrawIcon(new Vector2(160f, 120f), "desktop/mymountain_icon", Dialog.Clean("FEMTOHELPER_ULTRADASH_DESKTOP_MYPC"));
		DrawIcon(new Vector2(160f, 320f), "desktop/recyclebin_icon", Dialog.Clean("FEMTOHELPER_ULTRADASH_DESKTOP_RECYCLEBIN"));
		DrawIcon(new Vector2(160f, 520f), "desktop/FemtoHelper/lm_icon", Dialog.Clean("FEMTOHELPER_ULTRADASH_DESKTOP_LM"));
		DrawIcon(new Vector2(400f, 520f), "desktop/FemtoHelper/pekkaed_icon", Dialog.Clean("FEMTOHELPER_ULTRADASH_DESKTOP_PEKKA"));
		DrawIcon(new Vector2(1800f, 120f), "desktop/FemtoHelper/ahorn_icon", Dialog.Clean("FEMTOHELPER_ULTRADASH_DESKTOP_AHORN"));
		DrawIcon(new Vector2(1800f, 320f), "desktop/FemtoHelper/femto_icon", Dialog.Clean("FEMTOHELPER_ULTRADASH_DESKTOP_FEMTOTECH"));
		DrawIcon(new Vector2(1560f, 520f), "desktop/wavedashing_icon", Dialog.Clean("FEMTOHELPER_ULTRADASH_DESKTOP_WAVEDASH"));
		DrawIcon(new Vector2(1560f, 720f), "desktop/wavedashing_icon", Dialog.Clean("FEMTOHELPER_ULTRADASH_DESKTOP_NYOOM"));
		DrawIcon(pptIcon, "desktop/wavedashing_icon", Dialog.Clean("FEMTOHELPER_ULTRADASH_DESKTOP_POWERPOINT"));
		DrawTaskbar();
		Presentation.Gfx["desktop/cursor"].DrawCentered(cursor);
	}

	public void DrawTaskbar()
	{
		Draw.Rect(0f, (float)base.Height - 80f, base.Width, 80f, taskbarColor);
		Draw.Rect(0f, (float)base.Height - 80f, base.Width, 4f, Color.White * 0.5f);
		MTexture mTexture = Presentation.Gfx["desktop/FemtoHelper/startberry_golden"];
		float num = 64f;
		float num2 = num / (float)mTexture.Height * 0.7f;
		string text = Dialog.Clean("FEMTOHELPER_ULTRADASH_DESKTOP_STARTBUTTON");
		float num3 = 0.6f;
		float width = (float)mTexture.Width * num2 + ActiveFont.Measure(text).X * num3 + 32f;
		Vector2 value = new Vector2(8f, (float)base.Height - 80f + 8f);
		Draw.Rect(value.X, value.Y, width, num, Color.White * 0.5f);
		mTexture.DrawJustified(value + new Vector2(8f, num / 2f), new Vector2(0f, 0.5f), Color.White, Vector2.One * num2);
		ActiveFont.Draw(text, value + new Vector2((float)mTexture.Width * num2 + 16f, num / 2f), new Vector2(0f, 0.5f), Vector2.One * num3, Color.Black * 0.8f);
		ActiveFont.Draw(time, new Vector2((float)base.Width - 24f, (float)base.Height - 40f), new Vector2(1f, 0.5f), Vector2.One * 0.6f, Color.Black * 0.8f);
	}

	private void DrawIcon(Vector2 position, string icon, string text)
	{
		bool flag = cursor.X > position.X - 64f && cursor.Y > position.Y - 64f && cursor.X < position.X + 64f && cursor.Y < position.Y + 80f;
		if (selected && flag)
		{
			Draw.Rect(position.X - 80f, position.Y - 80f, 160f, 200f, Color.White * 0.25f);
		}
		if (flag)
		{
			DrawDottedRect(position.X - 80f, position.Y - 80f, 160f, 200f);
		}
		MTexture mTexture = Presentation.Gfx[icon];
		float scale = 128f / (float)mTexture.Height;
		mTexture.DrawCentered(position, Color.White, scale);
		ActiveFont.Draw(text, position + new Vector2(0f, 80f), new Vector2(0.5f, 0f), Vector2.One * 0.6f, (selected && flag) ? Color.Black : Color.White);
	}

	private void DrawDottedRect(float x, float y, float w, float h)
	{
		float num = 4f;
		Draw.Rect(x, y, w, num, Color.White);
		Draw.Rect(x + w - num, y, num, h, Color.White);
		Draw.Rect(x, y, num, h, Color.White);
		Draw.Rect(x, y + h - num, w, num, Color.White);
		if (!selected)
		{
			for (float num2 = 4f; num2 < w; num2 += num * 2f)
			{
				Draw.Rect(x + num2, y, num, num, ClearColor);
				Draw.Rect(x + w - num2, y + h - num, num, num, ClearColor);
			}
			for (float num3 = 4f; num3 < h; num3 += num * 2f)
			{
				Draw.Rect(x, y + num3, num, num, ClearColor);
				Draw.Rect(x + w - num, y + h - num3, num, num, ClearColor);
			}
		}
	}
}
