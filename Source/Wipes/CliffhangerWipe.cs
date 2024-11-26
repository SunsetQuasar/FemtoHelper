// Celeste.DreamWipe
using System;
using Celeste;
using Microsoft.Xna.Framework.Graphics;

public class CliffhangerWipe : ScreenWipe
{
	private struct Circle
	{
		public Vector2 Position;

		public float Radius;

		public float Delay;

		public float isFlipped;
	}

	private readonly int circleColumns = 6;

	private readonly int circleRows = 3;

	private const int circleSegments = 3;

	private const float circleFillSpeed = 800f;

	private const float circleSpinSpeed = 0.06f;

	private static Circle[] circles;

	private static VertexPositionColor[] vertexBuffer;

	public CliffhangerWipe(Scene scene, bool wipeIn, Action onComplete = null)
		: base(scene, wipeIn, onComplete)
	{
		if (vertexBuffer == null)
		{
			vertexBuffer = new VertexPositionColor[(circleColumns + 2) * (circleRows + 2) * 3 * 3];
		}
		if (circles == null)
		{
			circles = new Circle[(circleColumns + 2) * (circleRows + 2)];
		}
		for (int i = 0; i < vertexBuffer.Length; i++)
		{
			vertexBuffer[i].Color = ScreenWipe.WipeColor;
		}
		int num = 1920 / circleColumns;
		int num2 = 1080 / circleRows;
		int j = circleColumns + 2;
		int num3 = 0;
		for (; j > 0; j--)
		{
			for (int k = 0; k < circleRows + 2; k++)
			{
				circles[num3].Position = new Vector2(((float)(j - 1)) * (float)num, ((float)(k - 1)) * (float)num2 + (j % 2 == 1 ? (num2/2) : 0));
				circles[num3].Delay = 0;
				circles[num3].Radius = (WipeIn ? (circleFillSpeed * (Duration - circles[num3].Delay)) : 0f);
				circles[num3].isFlipped = k % 2 == 1 ? 1 : -1;
				num3++;
			}
		}
	}

	public override void Update(Scene scene)
	{
		base.Update(scene);
		for (int i = 0; i < circles.Length; i++)
		{
			if (!WipeIn)
			{
				circles[i].Delay -= Engine.DeltaTime;
				if (circles[i].Delay <= 0f)
				{
					circles[i].Radius += Engine.DeltaTime * (circleFillSpeed / 8) + (circles[i].Radius / 7);
					//circles[i].Angle += Engine.DeltaTime * circleSpinSpeed;
				}
			}
			else if (circles[i].Radius > 0f)
			{
				circles[i].Radius -= Engine.DeltaTime * (circleFillSpeed / 8) + (circles[i].Radius / 7);
				//circles[i].Angle -= Engine.DeltaTime * circleSpinSpeed;
			}
			else
			{
				circles[i].Radius = 0f;
			}
		}
	}

	public override void Render(Scene scene)
	{
		int num = 0;
		for (int i = 0; i < circles.Length; i++)
		{
			Circle circle = circles[i];
			Vector2 vector = new Vector2(1f, 0f);
			for (float num2 = 0f; num2 < circleSegments; num2 += 1f)
			{
				Vector2 vector2 = Calc.AngleToVector((num2 + 1f) / circleSegments * ((float)-Math.PI * 2f), 1f);
				vertexBuffer[num++].Position = new Vector3(circle.Position, 0f);
				vertexBuffer[num++].Position = new Vector3(circle.Position + vector * circle.isFlipped * circle.Radius, 0f);
				vertexBuffer[num++].Position = new Vector3(circle.Position + vector2 * circle.isFlipped * circle.Radius, 0f);
				vector = vector2;
			}
		}
		ScreenWipe.DrawPrimitives(vertexBuffer);
	}
}
