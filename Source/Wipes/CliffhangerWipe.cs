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

		public float IsFlipped;
	}

	private readonly int circleColumns = 6;

	private readonly int circleRows = 3;

	private const int CircleSegments = 3;

	private const float CircleFillSpeed = 800f;

	private const float CircleSpinSpeed = 0.06f;

	private static Circle[] _circles;

	private static VertexPositionColor[] _vertexBuffer;

	public CliffhangerWipe(Scene scene, bool wipeIn, Action onComplete = null)
		: base(scene, wipeIn, onComplete)
	{
		_vertexBuffer ??= new VertexPositionColor[(circleColumns + 2) * (circleRows + 2) * 3 * 3];
		_circles ??= new Circle[(circleColumns + 2) * (circleRows + 2)];
		for (int i = 0; i < _vertexBuffer.Length; i++)
		{
			_vertexBuffer[i].Color = WipeColor;
		}
		int num = 1920 / circleColumns;
		int num2 = 1080 / circleRows;
		int j = circleColumns + 2;
		int num3 = 0;
		for (; j > 0; j--)
		{
			for (int k = 0; k < circleRows + 2; k++)
			{
				_circles[num3].Position = new Vector2((j - 1) * (float)num, (k - 1) * (float)num2 + (j % 2 == 1 ? num2/2 : 0));
				_circles[num3].Delay = 0;
				_circles[num3].Radius = WipeIn ? CircleFillSpeed * (Duration - _circles[num3].Delay) : 0f;
				_circles[num3].IsFlipped = k % 2 == 1 ? 1 : -1;
				num3++;
			}
		}
	}

	public override void Update(Scene scene)
	{
		base.Update(scene);
		for (int i = 0; i < _circles.Length; i++)
		{
			if (!WipeIn)
			{
				_circles[i].Delay -= Engine.RawDeltaTime;
				if (_circles[i].Delay <= 0f)
				{
					_circles[i].Radius += Engine.RawDeltaTime * (CircleFillSpeed / 8) + _circles[i].Radius / 7;
					//circles[i].Angle += Engine.RawDeltaTime * circleSpinSpeed;
				}
			}
			else if (_circles[i].Radius > 0f)
			{
				_circles[i].Radius -= Engine.RawDeltaTime * (CircleFillSpeed / 8) + _circles[i].Radius / 7;
				//circles[i].Angle -= Engine.RawDeltaTime * circleSpinSpeed;
			}
			else
			{
				_circles[i].Radius = 0f;
			}
		}
	}

	public override void Render(Scene scene)
	{
		int num = 0;
		foreach (var circle in _circles)
		{
			Vector2 vector = new Vector2(1f, 0f);
			for (float num2 = 0f; num2 < CircleSegments; num2 += 1f)
			{
				Vector2 vector2 = Calc.AngleToVector((num2 + 1f) / CircleSegments * ((float)-Math.PI * 2f), 1f);
				_vertexBuffer[num++].Position = new Vector3(circle.Position, 0f);
				_vertexBuffer[num++].Position = new Vector3(circle.Position + vector * circle.IsFlipped * circle.Radius, 0f);
				_vertexBuffer[num++].Position = new Vector3(circle.Position + vector2 * circle.IsFlipped * circle.Radius, 0f);
				vector = vector2;
			}
		}
		DrawPrimitives(_vertexBuffer);
	}
}
