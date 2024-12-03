// Celeste.DreamStars
using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using FMOD;
using Microsoft.Xna.Framework;
using Monocle;

public class VaporWave : Backdrop
{
	private readonly float lines = 32;
	private float timer = 0;
	private readonly float height = 64;

	public VaporWave(float lineCount, float horizon)
	{
		lines = lineCount;
		height = horizon;
	}

	public override void Update(Scene scene)
	{
		timer += Engine.DeltaTime * 20;
	}

	public override void Render(Scene scene)
	{
		for (int i = 0; i < lines; i++)
		{
			Draw.Line(new Vector2((timer + i * (320/lines)) % 320 - 0, 180 - height), new Vector2((timer * 3 + i * (320 / lines * 3)) % 960 - 320, 180), Color.White);
		}
		for (int i = 0; i < 9; i++)
		{
			Draw.Line(new Vector2(0, 179 - height + i*i), new Vector2(320, 179 - height + i * i), Color.White);
		}
		for (float i = 0; i < 44; i++)
        {
			Draw.Rect(
				new Vector2(
				160f + (i % 2 == 0 ? -((float)Math.Sin(timer / 5 + i / 8) * 2f) : (float)Math.Sin(timer/5 + i / 8) * 2f)
				- (float)Math.Pow(Math.Cos((i - 12) / 64 * Math.PI),1 / 2.2f) * 32,
				178 - height - i),
				(float)Math.Pow(Math.Cos((i - 12) / 64 * Math.PI), 1 / 2.2f) * 64,
				1,
				Calc.HsvToColor(0 + (i-12)/420, 0.9f - i/1000, 1)
				);
        }
	}

	private float Mod(float x, float m)
	{
		return (x % m + m) % m;
	}
}
