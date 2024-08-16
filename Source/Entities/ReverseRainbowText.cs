// Celeste.AreaCompleteTitle
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class ReverseAreaCompleteTitle : Entity
{
	public class Letter
	{
		public string Value;

		public Vector2 Position;

		public Color Color = Color.White;

		public Color Shadow = Color.Black;

		private float delay;

		private float ease;

		private Vector2 scale;

		private SimpleCurve curve;

		public Letter(int index, string value, Vector2 position)
		{
			Value = value;
			Position = position;
			delay = 0.2f + (float)index * 0.02f;
			curve = new SimpleCurve(position + Vector2.UnitY * 60f, position, position - Vector2.UnitY * 100f);
			scale = new Vector2(0.75f, 1.5f);
		}

		public void Update()
		{
			scale.X = Calc.Approach(scale.X, 1f, 3f * Engine.DeltaTime);
			scale.Y = Calc.Approach(scale.Y, 1f, 3f * Engine.DeltaTime);
			if (delay > 0f)
			{
				delay -= Engine.DeltaTime;
			}
			else if (ease < 1f)
			{
				ease += 4f * Engine.DeltaTime;
				if (ease >= 1f)
				{
					ease = 1f;
					scale = new Vector2(1.5f, 0.75f);
				}
			}
		}

		public void Render(Vector2 offset, float scale, float alphaMultiplier)
		{
			if (ease > 0f)
			{
				Vector2 vector = offset + curve.GetPoint(ease);
				float num = Calc.LerpClamp(0f, 1f, ease * 3f) * alphaMultiplier;
				Vector2 vector2 = this.scale * scale;
				if (num < 1f)
				{
					ActiveFont.Draw(Value, vector, new Vector2(0.5f, 1f), vector2, Color * num);
					return;
				}
				ActiveFont.Draw(Value, vector + Vector2.UnitY * 3.5f * scale, new Vector2(0.5f, 1f), vector2, Shadow);
				ActiveFont.DrawOutline(Value, vector, new Vector2(0.5f, 1f), vector2, Color, 2f, Shadow);
			}
		}
	}

	public float Alpha = 1f;

	private Vector2 origin;

	private List<Letter> letters = new List<Letter>();

	private float rectangleEase;

	private float scale;

	public ReverseAreaCompleteTitle(Vector2 origin, string text, float scale, bool rainbow = false)
	{
		this.origin = origin;
		this.scale = scale;
		Vector2 vector = ActiveFont.Measure(text) * scale;
		Vector2 value = origin + Vector2.UnitY * vector.Y * 0.5f + Vector2.UnitX * vector.X * -0.5f;
		for (int i = 0; i < text.Length; i++)
		{
			Vector2 vector2 = ActiveFont.Measure(text[i].ToString()) * scale;
			if (text[i] != ' ')
			{
				Letter letter = new Letter(i, text[i].ToString(), value + Vector2.UnitX * vector2.X * 0.5f);
				if (rainbow)
				{
					float hue = (float)i / (float)text.Length;
					letter.Color = Color.Lerp(Calc.HsvToColor(hue, 0.8f, 0.9f), Color.Black, 0.7f);
					letter.Shadow = Calc.HsvToColor(hue, 0.8f, 0.9f);
				}
				letters.Add(letter);
			}
			value += Vector2.UnitX * vector2.X;
		}
		Alarm.Set(this, 2.6f, delegate
		{
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, 0.5f, start: true);
			tween.OnUpdate = delegate (Tween t)
			{
				rectangleEase = t.Eased;
			};
			Add(tween);
		});
	}

	public override void Update()
	{
		base.Update();
		foreach (Letter letter in letters)
		{
			letter.Update();
		}
	}

	public void DrawLineUI()
	{
		Draw.Rect(base.X, base.Y + origin.Y - 40f, 1920f * rectangleEase, 80f, Color.Black * 0.65f);
	}

	public override void Render()
	{
		base.Render();
		foreach (Letter letter in letters)
		{
			letter.Render(Position, scale, Alpha);
		}
	}
}
