// Celeste.CustomParallaxBigWaterfall
using System;
using System.Collections.Generic;
using Celeste;

[CustomEntity("FemtoHelper/CustomParallaxBigWaterfall")]

public class CustomParallaxBigWaterfall : Entity
{
	private enum Layers
	{
		Fg,
		Bg
	}

	private readonly Layers layer;

	private readonly float width;

	private readonly float height;

	private readonly float parallax;

	private readonly float fallSpeedMultiplier;

	private readonly List<float> lines = new List<float>();

	private readonly Color surfaceColor;

	private readonly Color fillColor;

	private float sine;

	private readonly SoundSource loopingSfx;

	private float fade;

	private Vector2 RenderPosition => RenderPositionAtCamera((Scene as Level).Camera.Position + new Vector2(160f, 90f));

	public CustomParallaxBigWaterfall(EntityData data, Vector2 offset) : base(data.Position + offset)
	{
		Tag = Tags.TransitionUpdate;
		layer = data.Enum("layer", Layers.Bg);
		width = data.Width;
		height = data.Height;
		fallSpeedMultiplier = data.Float("fallSpeedMultiplier");
		var surfaceOpacity = data.Float("surfaceOpacity");
		var fillOpacity = data.Float("fillOpacity");
		if (layer == Layers.Fg)
		{
			Depth = -49900;
			parallax = data.Float("parallax");
			surfaceColor = Calc.HexToColor(data.Attr("surfaceColor")) * surfaceOpacity;
			fillColor = Calc.HexToColor(data.Attr("fillColor")) * fillOpacity;
			Add(new DisplacementRenderHook(RenderDisplacement));
			lines.Add(3f);
			lines.Add(width - 4f);
			Add(loopingSfx = new SoundSource());
			loopingSfx.Play("event:/env/local/waterfall_big_main");
		}
		else
		{
			Depth = 10010;
			parallax = data.Float("parallax");
			surfaceColor = Calc.HexToColor(data.Attr("surfaceColor")) * surfaceOpacity;
			fillColor = Calc.HexToColor(data.Attr("fillColor")) * fillOpacity;
			lines.Add(6f);
			lines.Add(width - 7f);
		}
		fade = 1f;
		Add(new TransitionListener
		{
			OnIn = delegate (float f)
			{
				fade = f;
			},
			OnOut = delegate (float f)
			{
				fade = 1f - f;
			}
		});
		if (!(width > 16f)) return;
		int num = Calc.Random.Next((int)(width / 16f));
		for (int i = 0; i < num; i++)
		{
			lines.Add(8f + Calc.Random.NextFloat(width - 16f));
		}
	}

	public override void Added(Scene scene)
	{
		base.Added(scene);
		if ((Scene as Level).Transitioning)
		{
			fade = 0f;
		}
	}

	public Vector2 RenderPositionAtCamera(Vector2 camera)
	{
		Vector2 value = Position + new Vector2(width, height) / 2f - camera;
		Vector2 zero = Vector2.Zero;
		if (layer == Layers.Bg)
		{
			zero -= value * (1f - parallax);
		}
		else if (layer == Layers.Fg)
		{
			zero += value * (parallax - 1f);
		}
		return Position + zero;
	}

	public void RenderDisplacement()
	{
		Draw.Rect(RenderPosition.X, Y, width, height, new Color(0.5f, 0.5f, 1f, 1f));
	}

	public override void Update()
	{
		sine += Engine.DeltaTime * fallSpeedMultiplier;
		if (loopingSfx != null)
		{
			Vector2 position = (Scene as Level).Camera.Position;
			loopingSfx.Position = new Vector2(RenderPosition.X - X, Calc.Clamp(position.Y + 90f, Y, height) - Y);
		}
		base.Update();
	}

	public override void Render()
	{
		float x = RenderPosition.X;
		Color color = fillColor * fade;
		Color color2 = surfaceColor * fade;
		Draw.Rect(x, Y, width, height, color);
		if (layer == Layers.Fg)
		{
			Draw.Rect(x - 1f, Y, 3f, height, color2);
			Draw.Rect(x + width - 2f, Y, 3f, height, color2);
			foreach (float line in lines)
			{
				Draw.Rect(x + line, Y, 1f, height, color2);
			}
			return;
		}
		Vector2 position = (Scene as Level).Camera.Position;
		int num = 3;
		float num2 = Math.Max(Y, (float)Math.Floor(position.Y / num) * num);
		float num3 = Math.Min(Y + height, position.Y + 180f);
		for (float num4 = num2; num4 < num3; num4 += num)
		{
			int num5 = (int)(Math.Sin(num4 / 6f - sine * 8f) * 2.0);
			Draw.Rect(x, num4, 4 + num5, num, color2);
			Draw.Rect(x + width - 4f + num5, num4, 4 - num5, num, color2);
			foreach (float line2 in lines)
			{
				Draw.Rect(x + num5 + line2, num4, 1f, num, color2);
			}
		}
	}
}
