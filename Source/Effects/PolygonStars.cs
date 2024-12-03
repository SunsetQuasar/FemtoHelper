// Celeste.DreamStars
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.Entities;
using FMOD;
using Microsoft.Xna.Framework;
using Monocle;

public class PolygonStars : Backdrop
{
	private struct Stars
	{
		public Vector2 Position;

		public float Speed;

		public float Size;

		public float Rotation;

		public float RotationSpeed;

		public Color Color;
	}

	private readonly Stars[] stars;

	private readonly Vector2 angle;

	private Vector2 lastCamera = Vector2.Zero;

	private readonly int sideCount;

	private readonly float pointinessMultiplier;

	private readonly float loopBorder;

	private readonly Color[] colors;

	private readonly float alpha;

	private readonly float scroll;

	public PolygonStars(int sides, float pointiness, float minRotation, float maxRotation, float minSize, float maxSize, float border, string color, float angle, float alpha, float minSpeed, float maxSpeed, int amount, float scroll)
	{
		sideCount = sides;
		pointinessMultiplier = pointiness;
		loopBorder = border;
		this.alpha = alpha;
		this.scroll = scroll;
		stars = new Stars[amount];
        colors = color
				.Split(',')
				.Select(str => Calc.HexToColor(str.Trim()) * alpha)
				.ToArray();
        this.angle = new Vector2((float)Math.Sin(angle / 180 * Math.PI), (float)Math.Cos(angle / 180 * Math.PI));
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i].Position = new Vector2(Calc.Random.NextFloat(320f + loopBorder) - loopBorder / 2, Calc.Random.NextFloat(180f + loopBorder) - loopBorder / 2);
			stars[i].Speed = Calc.Random.Range(minSpeed, maxSpeed);
			stars[i].Size = Calc.Random.Range(minSize, maxSize);
			stars[i].Rotation = Calc.Random.NextFloat((float)Math.PI * 2);
			stars[i].RotationSpeed = Calc.Random.Range(minRotation, maxRotation);
			stars[i].Color = colors[Calc.Random.Next(colors.Length)];
		}
	}

	public override void Update(Scene scene)
	{
		base.Update(scene);
		Vector2 position = (scene as Level).Camera.Position;
		Vector2 value = position - lastCamera;
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i].Position += angle * stars[i].Speed * Engine.DeltaTime - value * scroll;
			stars[i].Rotation += stars[i].RotationSpeed * Engine.DeltaTime;
		}
		lastCamera = position;
	}

	public override void Render(Scene scene)
	{
		for (int i = 0; i < stars.Length; i++)
		{
			//Draw.HollowRect(new Vector2(mod(stars[i].Position.X, 320f), mod(stars[i].Position.Y, 180f)), stars[i].Size, stars[i].Size, Color.Teal);
			for (int j = 0; j < sideCount; j++)
			{
				Vector2 size = j % 2 == 0 ? new Vector2(stars[i].Size, stars[i].Size) : new Vector2(stars[i].Size * pointinessMultiplier, stars[i].Size * pointinessMultiplier);
				Vector2 size2 = j % 2 == 0 ? new Vector2(stars[i].Size * pointinessMultiplier, stars[i].Size * pointinessMultiplier) : new Vector2(stars[i].Size, stars[i].Size);
				Draw.Line(new Vector2(Mod(stars[i].Position.X, 320f + loopBorder) - loopBorder / 2, Mod(stars[i].Position.Y, 180f + loopBorder) - loopBorder / 2) + new Vector2((float)Math.Sin(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * j), (float)Math.Cos(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * j)) * size, new Vector2(Mod(stars[i].Position.X, 320f + loopBorder) - loopBorder / 2, Mod(stars[i].Position.Y, 180f + loopBorder) - loopBorder / 2) + new Vector2((float)Math.Sin(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * (j + 1)), (float)Math.Cos(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * (j + 1))) * size2, stars[i].Color * alpha);
			}
		}
	}

	private float Mod(float x, float m)
	{
		return (x % m + m) % m;
	}
}
