
using System;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
public class VectorSpace : Backdrop
{
	private struct Vectand
	{
		public Vector2 Position;

		public Vector2 Speed;

		public float XComp;

		public float YComp;

		public float XFreq;

		public float YFreq;

		public Color IndColor;

		public float XTimer;

		public float YTimer;
	}

	private readonly Vectand[] vecs;

	private Vector2 lastCamera = Vector2.Zero;

	private readonly Color[] colors;

	private readonly float scroll;

	private readonly int howmanyx;
	private readonly int howmanyy;

	private readonly float spacingX;
	private readonly float spacingY;

	private readonly int extras;

	private readonly float gAmplitude;

	private readonly bool scaleTip;
	private readonly bool renderTip;

	public VectorSpace(float spacingX, float spacingY, float speedX, float speedY, float scroll, string color, float xOffsetMin, float xOffsetMax, float yOffsetMin, float yOffsetMax, float xFreqMin, float xFreqMax, float yFreqMin, float yFreqMax, float amplitude, bool trender, bool tscale, float alpha, bool yFrelX, bool yOrelX)
	{
		extras = Math.Max(
			(int)Math.Ceiling(spacingX / amplitude),
			(int)Math.Ceiling(spacingY / amplitude)
			);

		gAmplitude = amplitude;

		this.scroll = scroll;

		howmanyx = (int)Math.Ceiling(320 / spacingX) + extras;
		howmanyy = (int)Math.Ceiling(180 / spacingY) + extras;

		this.spacingX = spacingX;
		this.spacingY = spacingY;

		scaleTip = tscale;
		renderTip = trender;

		vecs = new Vectand[howmanyx * howmanyy];
		//Alpha = alpha;
		colors = color
				.Split(',')
				.Select(str => Calc.HexToColor(str.Trim()) * alpha)
				.ToArray();


		for (int i = 0; i < vecs.Length; i++)
		{
			vecs[i].Position = new Vector2(
				Mod(i , howmanyx) * spacingX, 
				i / howmanyx * spacingY
				);
			vecs[i].Speed = new Vector2(speedX, speedY);
			vecs[i].IndColor = colors[Calc.Random.Next(colors.Length)];

			vecs[i].XFreq = Calc.Random.Range(xFreqMin, xFreqMax) / 180 * (float)Math.PI;

            if (yFrelX) 
            {
				vecs[i].YFreq = vecs[i].XFreq + Calc.Random.Range(yFreqMin, yFreqMax) / 180 * (float)Math.PI;
			}
            else
            {
				vecs[i].YFreq = Calc.Random.Range(yFreqMin, yFreqMax) / 180 * (float)Math.PI;
			}
			

			vecs[i].XTimer = Calc.Random.Range(xOffsetMin, xOffsetMax) / 180 * (float)Math.PI;

			if(yOrelX)
            {
				vecs[i].YTimer = vecs[i].XTimer + Calc.Random.Range(yOffsetMin, yOffsetMax) / 180 * (float)Math.PI;
			} 
			else
            {
				vecs[i].YTimer = Calc.Random.Range(yOffsetMin, yOffsetMax) / 180 * (float)Math.PI;
			}
		}
	}

	public override void Update(Scene scene)
	{
		base.Update(scene);
		Vector2 position = (scene as Level).Camera.Position;
		Vector2 value = position - lastCamera;
		for (int i = 0; i < vecs.Length; i++)
		{
			vecs[i].Position += vecs[i].Speed * Engine.DeltaTime - value * scroll;
			//stars[i].Rotation += stars[i].RotationSpeed * Engine.DeltaTime;

			if (vecs[i].Position.X < 0 || vecs[i].Position.X > howmanyx * spacingX)
            {
				vecs[i].Position.X = Mod(vecs[i].Position.X, howmanyx * spacingX) + 0;
			}
			if (vecs[i].Position.Y < 0 || vecs[i].Position.Y > howmanyy * spacingY)
			{
				vecs[i].Position.Y = Mod(vecs[i].Position.Y, howmanyy * spacingY) + 0;
			}

			vecs[i].XTimer += vecs[i].XFreq * Engine.DeltaTime;
			vecs[i].YTimer += vecs[i].YFreq * Engine.DeltaTime;

			vecs[i].XComp = (float)Math.Sin(vecs[i].XTimer) * gAmplitude;
			vecs[i].YComp = (float)Math.Sin(vecs[i].YTimer) * gAmplitude;
		}
		lastCamera = position;
		
	}

	public override void Render(Scene scene)
	{
		for (int i = 0; i < vecs.Length; i++)
		{
			Draw.Line(new Vector2(
				vecs[i].Position.X - (howmanyx * spacingX - 320) / 2,
				vecs[i].Position.Y - (howmanyy * spacingY - 180) / 2
				),
				new Vector2(
				vecs[i].Position.X + vecs[i].XComp - (howmanyx * spacingX - 320) / 2,
				vecs[i].Position.Y + vecs[i].YComp - (howmanyy * spacingY - 180) / 2
				),
				vecs[i].IndColor);
			if (renderTip)
			{
				float dir = (float)Math.Atan2(vecs[i].XComp, vecs[i].YComp);

				float amp = (float)Math.Sqrt(Math.Pow(vecs[i].XComp, 2) + Math.Pow(vecs[i].YComp, 2)) / gAmplitude;

				float tipscale;
                if (scaleTip)
                {
					tipscale = gAmplitude / 2 * amp;
				} 
				else
                {
					tipscale = gAmplitude / 2;
				}
				

				Draw.Line(new Vector2(
					vecs[i].Position.X + vecs[i].XComp - (howmanyx * spacingX - 320) / 2,
					vecs[i].Position.Y + vecs[i].YComp - (howmanyy * spacingY - 180) / 2
					),
					new Vector2(
					vecs[i].Position.X + vecs[i].XComp - (howmanyx * spacingX - 320) / 2 + (float)Math.Sin(dir - 9 * Math.PI / 8) * tipscale,
					vecs[i].Position.Y + vecs[i].YComp - (howmanyy * spacingY - 180) / 2 + (float)Math.Cos(dir - 9 * Math.PI / 8) * tipscale
					),
					vecs[i].IndColor);

				Draw.Line(new Vector2(
					vecs[i].Position.X + vecs[i].XComp - (howmanyx * spacingX - 320) / 2 + (float)Math.Sin(dir + 9 * Math.PI / 8) * tipscale,
					vecs[i].Position.Y + vecs[i].YComp - (howmanyy * spacingY - 180) / 2 + (float)Math.Cos(dir + 9 * Math.PI / 8) * tipscale

					),
					new Vector2(
					vecs[i].Position.X + vecs[i].XComp - (howmanyx * spacingX - 320) / 2,
					vecs[i].Position.Y + vecs[i].YComp - (howmanyy * spacingY - 180) / 2
					),
					vecs[i].IndColor);
			}
		}
	}

	private float Mod(float x, float m)
	{
		return (x % m + m) % m;
	}
}
