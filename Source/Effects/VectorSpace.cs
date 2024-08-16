
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

		public float xComp;

		public float yComp;

		public float xFreq;

		public float yFreq;

		public Color indColor;

		public float xTimer;

		public float yTimer;
	}

	private Vectand[] vecs;

	private Vector2 lastCamera = Vector2.Zero;

	private Color[] Colors;

	private float scroll;

	private	int howmanyx;
	private int howmanyy;

	private float spacingX;
	private float spacingY;

	private int extras;

	private float g_amplitude;

	private bool scaleTip;
	private bool renderTip;

	public VectorSpace(float spacing_x, float spacing_y, float speed_x, float speed_y, float scroll, string color, float xOffsetMin, float xOffsetMax, float yOffsetMin, float yOffsetMax, float x_freqMin, float x_freqMax, float y_freqMin, float y_freqMax, float amplitude, bool trender, bool tscale, float alpha, bool YFrelX, bool YOrelX)
	{
		extras = Math.Max(
			(int)Math.Ceiling(spacing_x / amplitude),
			(int)Math.Ceiling(spacing_y / amplitude)
			);

		g_amplitude = amplitude;

		this.scroll = scroll;

		howmanyx = (int)Math.Ceiling(320 / spacing_x) + extras;
		howmanyy = (int)Math.Ceiling(180 / spacing_y) + extras;

		spacingX = spacing_x;
		spacingY = spacing_y;

		scaleTip = tscale;
		renderTip = trender;

		vecs = new Vectand[(howmanyx * howmanyy)];
		//Alpha = alpha;
		Colors = color
				.Split(',')
				.Select(str => Calc.HexToColor(str.Trim()) * alpha)
				.ToArray();


		for (int i = 0; i < vecs.Length; i++)
		{
			vecs[i].Position = new Vector2(
				mod(i , howmanyx) * spacing_x, 
				(i / howmanyx) * spacing_y
				);
			vecs[i].Speed = new Vector2(speed_x, speed_y);
			vecs[i].indColor = Colors[Calc.Random.Next(Colors.Length)];

			vecs[i].xFreq = (Calc.Random.Range(x_freqMin, x_freqMax) / 180) * (float)Math.PI;

            if (YFrelX) 
            {
				vecs[i].yFreq = vecs[i].xFreq + (Calc.Random.Range(y_freqMin, y_freqMax) / 180) * (float)Math.PI;
			}
            else
            {
				vecs[i].yFreq = (Calc.Random.Range(y_freqMin, y_freqMax) / 180) * (float)Math.PI;
			}
			

			vecs[i].xTimer = (Calc.Random.Range(xOffsetMin, xOffsetMax) / 180) * (float)Math.PI;

			if(YOrelX)
            {
				vecs[i].yTimer = vecs[i].xTimer + (Calc.Random.Range(yOffsetMin, yOffsetMax) / 180) * (float)Math.PI;
			} 
			else
            {
				vecs[i].yTimer = (Calc.Random.Range(yOffsetMin, yOffsetMax) / 180) * (float)Math.PI;
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
				vecs[i].Position.X = mod(vecs[i].Position.X, howmanyx * spacingX) + 0;
			}
			if (vecs[i].Position.Y < 0 || vecs[i].Position.Y > howmanyy * spacingY)
			{
				vecs[i].Position.Y = mod(vecs[i].Position.Y, howmanyy * spacingY) + 0;
			}

			vecs[i].xTimer += vecs[i].xFreq * Engine.DeltaTime;
			vecs[i].yTimer += vecs[i].yFreq * Engine.DeltaTime;

			vecs[i].xComp = (float)Math.Sin(vecs[i].xTimer) * g_amplitude;
			vecs[i].yComp = (float)Math.Sin(vecs[i].yTimer) * g_amplitude;
		}
		lastCamera = position;
		
	}

	public override void Render(Scene scene)
	{
		for (int i = 0; i < vecs.Length; i++)
		{
			Draw.Line(new Vector2(
				vecs[i].Position.X - ((howmanyx * spacingX - 320) / 2),
				vecs[i].Position.Y - ((howmanyy * spacingY - 180) / 2)
				),
				new Vector2(
				vecs[i].Position.X + vecs[i].xComp - ((howmanyx * spacingX - 320) / 2),
				vecs[i].Position.Y + vecs[i].yComp - ((howmanyy * spacingY - 180) / 2)
				),
				vecs[i].indColor);
			if (renderTip)
			{
				float dir = (float)Math.Atan2(vecs[i].xComp, vecs[i].yComp);

				float amp = (float)Math.Sqrt(Math.Pow(vecs[i].xComp, 2) + Math.Pow(vecs[i].yComp, 2)) / g_amplitude;

				float tipscale;
                if (scaleTip)
                {
					tipscale = (g_amplitude / 2) * amp;
				} 
				else
                {
					tipscale = (g_amplitude / 2);
				}
				

				Draw.Line(new Vector2(
					vecs[i].Position.X + vecs[i].xComp - ((howmanyx * spacingX - 320) / 2),
					vecs[i].Position.Y + vecs[i].yComp - ((howmanyy * spacingY - 180) / 2)
					),
					new Vector2(
					vecs[i].Position.X + vecs[i].xComp - ((howmanyx * spacingX - 320) / 2) + (float)Math.Sin(dir - (9 * Math.PI / 8)) * tipscale,
					vecs[i].Position.Y + vecs[i].yComp - ((howmanyy * spacingY - 180) / 2) + (float)Math.Cos(dir - (9 * Math.PI / 8)) * tipscale
					),
					vecs[i].indColor);

				Draw.Line(new Vector2(
					vecs[i].Position.X + vecs[i].xComp - ((howmanyx * spacingX - 320) / 2) + (float)Math.Sin(dir + (9 * Math.PI / 8)) * tipscale,
					vecs[i].Position.Y + vecs[i].yComp - ((howmanyy * spacingY - 180) / 2) + (float)Math.Cos(dir + (9 * Math.PI / 8)) * tipscale

					),
					new Vector2(
					vecs[i].Position.X + vecs[i].xComp - ((howmanyx * spacingX - 320) / 2),
					vecs[i].Position.Y + vecs[i].yComp - ((howmanyy * spacingY - 180) / 2)
					),
					vecs[i].indColor);
			}
		}
	}

	private float mod(float x, float m)
	{
		return (x % m + m) % m;
	}
}
