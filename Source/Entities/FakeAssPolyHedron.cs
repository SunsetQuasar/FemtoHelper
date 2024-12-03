using System;

namespace Celeste.Mod.FemtoHelper;

[CustomEntity("FemtoHelper/PseudoPolyhedron")]
public class PseudoPolyhedron : Entity
{
	public float Timer = 0;

	public readonly float RotationSpeed;

	public readonly float BaseWidth;

	public readonly float BaseHeight;

	public Vector2 TipBaseOffset;

	public Color Color;

	public readonly int BaseSides;

	public readonly bool PrismMode = false;

	public readonly float Alpha;

	public readonly Vector2[,] Positions;

	public static int Samples;

	public Vector2 TopBaseSize;

	public Vector2 TopScale;

	public PseudoPolyhedron(EntityData data, Vector2 offset)
		: base(data.Position + offset)
	{
		RotationSpeed = data.Float("rotationSpeed", 30f);
		Timer = data.Float("rotationOffset", 0f);
			
		BaseWidth = data.Float("baseWidth", 10f);
		BaseHeight = data.Float("baseHeight", 3f);
		TipBaseOffset = new Vector2(data.Float("tipBaseOffsetX", 0f), data.Float("tipBaseOffsetY", -14f));
		TopBaseSize = new Vector2(data.Float("topBaseWidth", BaseWidth), data.Float("topBaseHeight", BaseHeight));
		Color = Calc.HexToColor(data.Attr("color", "ffffff"));
		BaseSides = data.Int("baseSideCount", 4);
		PrismMode = data.Bool("isPrism", false);
		Alpha = data.Float("alpha", 1);
		Samples = data.Int("samples", 128);
		Depth = data.Int("depth", 6000);
		Positions = new Vector2[Samples, BaseSides];
		Vector2 origScale = new Vector2(BaseWidth, BaseHeight);
		origScale.Normalize();
		TopScale = origScale + new Vector2(TopBaseSize.X, TopBaseSize.Y);
		float m = 0;
		for (int ind = 0;ind < Positions.GetLength(0); ind++)
		{
			m += (float)Math.PI * 2 / Samples / BaseSides;
			for (float n = 0; n < BaseSides; n++)
			{
				Positions[ind, (int)n] = new Vector2((float)Math.Sin(m + Math.PI / ((float)BaseSides / 2) * n) * BaseWidth, (float)Math.Cos(m + Math.PI / ((float)BaseSides / 2) * n) * BaseHeight);

			}
		}
	}

	public override void Update()
	{
		Timer += RotationSpeed * Engine.DeltaTime * Samples / 15;
		if (
				
			Position.X > (Scene as Level).Camera.X + 320 + Calc.Max(BaseWidth * 2, Math.Abs(TipBaseOffset.X * 2))
			|| 
			Position.X < (Scene as Level).Camera.X - Calc.Max(BaseWidth * 2, Math.Abs(TipBaseOffset.X * 2))
			|| 
			Position.Y > (Scene as Level).Camera.Y + 180 + Calc.Max(BaseHeight * 2, Math.Abs(TipBaseOffset.Y * 2))
			||
			Position.Y < (Scene as Level).Camera.Y - Calc.Max(BaseHeight * 2, Math.Abs(TipBaseOffset.Y * 2))
				
		)
		{
			Visible = false;
		} else
		{
			Visible = true;
		}
	}

	public override void Render()
	{
		int arrayTimer = (int)(Timer % Samples + Samples) % Samples;
		if (PrismMode)
		{
			float xScale = 1 / BaseWidth * TopBaseSize.X;
			float yScale = 1 / BaseHeight * TopBaseSize.Y;
			for (int i = 0; i < BaseSides; i++)
			{
				Draw.Line(Position + Positions[arrayTimer, i], Position + Positions[arrayTimer, (i + 1) % BaseSides], Color * Alpha);

				Draw.Line(Position + TipBaseOffset + Positions[arrayTimer, i] * new Vector2(xScale, yScale), Position + TipBaseOffset + Positions[arrayTimer, (i + 1) % BaseSides] * new Vector2(xScale, yScale), Color * Alpha);

				Draw.Line(Position + Positions[arrayTimer, i], Position + TipBaseOffset + Positions[arrayTimer, i] * new Vector2(xScale, yScale), Color * Alpha);
			}
		} else
		{
			for (int i = 0; i < BaseSides; i++)
			{
				Draw.Line(Position + Positions[arrayTimer, i], Position + Positions[arrayTimer, (i + 1) % BaseSides], Color * Alpha);
				Draw.Line(
					Position + Positions[arrayTimer, i],
					Position + TipBaseOffset, 
					Color * Alpha);
			}	
		}
	}
}