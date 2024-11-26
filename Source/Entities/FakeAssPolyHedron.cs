using System;

namespace Celeste.Mod.FemtoHelper
{
    [CustomEntity("FemtoHelper/PseudoPolyhedron")]
	public class PseudoPolyhedron : Entity
	{
		public float timer = 0;

		public float rotationSpeed = 30f;

		public float baseWidth = 10f;

		public float baseHeight = 3f;

		public Vector2 tipBaseOffset = new Vector2(0f, -14f);

		public Color Color = Color.White;

		public int BaseSides = 5;

		public bool prismMode = false;

		public float Alpha = 1f;

		public Vector2[,] positions;

		public static int samples;

		public Vector2 topBaseSize;

		public Vector2 TopScale;

		public PseudoPolyhedron(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			rotationSpeed = data.Float("rotationSpeed", 30f);
			timer = data.Float("rotationOffset", 0f);
			
			baseWidth = data.Float("baseWidth", 10f);
			baseHeight = data.Float("baseHeight", 3f);
			tipBaseOffset = new Vector2(data.Float("tipBaseOffsetX", 0f), data.Float("tipBaseOffsetY", -14f));
			topBaseSize = new Vector2(data.Float("topBaseWidth", baseWidth), data.Float("topBaseHeight", baseHeight));
			Color = Calc.HexToColor(data.Attr("color", "ffffff"));
			BaseSides = data.Int("baseSideCount", 4);
			prismMode = data.Bool("isPrism", false);
			Alpha = data.Float("alpha", 1);
			samples = data.Int("samples", 128);
			base.Depth = data.Int("depth", 6000);
			positions = new Vector2[samples, BaseSides];
			Vector2 origScale = new Vector2(baseWidth, baseHeight);
			origScale.Normalize();
			TopScale = origScale + new Vector2(topBaseSize.X, topBaseSize.Y);
			float m = 0;
			for (int ind = 0;ind < positions.GetLength(0); ind++)
            {
				m += (float)Math.PI * 2 / samples / BaseSides;
				for (float n = 0; n < BaseSides; n++)
                {
					positions[ind, (int)n] = new Vector2((float)Math.Sin(m + (Math.PI / ((float)BaseSides / 2) * n)) * baseWidth, (float)Math.Cos(m + (Math.PI / ((float)BaseSides / 2) * n)) * baseHeight);

				}
            }
	}

		public override void Update()
		{
			timer += rotationSpeed * Engine.DeltaTime * samples / 15;
			if (
				
					Position.X > (Scene as Level).Camera.X + 320 + Calc.Max(baseWidth * 2, Math.Abs(tipBaseOffset.X * 2))
				|| 
					Position.X < (Scene as Level).Camera.X - Calc.Max(baseWidth * 2, Math.Abs(tipBaseOffset.X * 2))
				|| 
					Position.Y > (Scene as Level).Camera.Y + 180 + Calc.Max(baseHeight * 2, Math.Abs(tipBaseOffset.Y * 2))
				||
					Position.Y < (Scene as Level).Camera.Y - Calc.Max(baseHeight * 2, Math.Abs(tipBaseOffset.Y * 2))
				
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
			int arrayTimer = (int)(timer % samples + samples) % samples;
			if (prismMode)
			{
				float xScale = 1 / baseWidth * topBaseSize.X;
				float yScale = 1 / baseHeight * topBaseSize.Y;
				for (int i = 0; i < BaseSides; i++)
				{
					Draw.Line(Position + positions[arrayTimer, i], Position + positions[arrayTimer, (i + 1) % BaseSides], Color * Alpha);

					Draw.Line(Position + tipBaseOffset + (positions[arrayTimer, i] * new Vector2(xScale, yScale)), Position + tipBaseOffset + (positions[arrayTimer, (i + 1) % BaseSides] * new Vector2(xScale, yScale)), Color * Alpha);

					Draw.Line(Position + positions[arrayTimer, i], Position + tipBaseOffset + (positions[arrayTimer, i] * new Vector2(xScale, yScale)), Color * Alpha);
				}
			} else
            {
				for (int i = 0; i < BaseSides; i++)
				{
					Draw.Line(Position + positions[arrayTimer, i], Position + positions[arrayTimer, (i + 1) % BaseSides], Color * Alpha);
					Draw.Line(
						Position + positions[arrayTimer, i],
						Position + tipBaseOffset, 
						Color * Alpha);
				}	
			}
		}
	}
}
