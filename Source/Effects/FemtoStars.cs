// Celeste.StarsBG
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;

public class FemtoStars : Backdrop
{
	private struct Star
	{
		public Vector2 Position;

		public int TextureSet;

		public float Timer;

		public float Rate;

		public Color[] Colors;

		public Color MainColor;

		public Vector2 Speed;
	}

	private const int StarCount = 100;

	private readonly Star[] stars;

	private readonly Color[] colors2;

	private readonly List<List<MTexture>> textures;

	//private float falling;

	private readonly Vector2 center;

	private readonly int trailCount;

	private readonly float extraLoopBorderX;

	private readonly float extraLoopBorderY;

	private readonly Color backdropColor;

	private readonly float backdropAlpha;

	private readonly float parallaxX;

	private readonly float parallaxY;

	public float AlphaLegacy;

	public readonly string Alpha;

	private readonly float separation;
	public FemtoStars(int blurCount, string colors3, float minXSpeed, float maxXSpeed, float minYSpeed, float maxYSpeed, float loopBorderX, float loopBorderY, int count, string backgroundColor, float backgroundAlpha, string sprite, float scrollX, float scrollY, float transparency, float trailSeparation, float animationRate, string transp2, float animationRateRandom)
	{
		trailCount = blurCount;
		extraLoopBorderX = loopBorderX;
		extraLoopBorderY = loopBorderY;
		backdropColor = Calc.HexToColor(backgroundColor);
		backdropAlpha = backgroundAlpha;
		parallaxX = scrollX;
		parallaxY = scrollY;
		AlphaLegacy = transparency;
		Alpha = transp2;
		separation = trailSeparation;
		var alphas3 = Array.ConvertAll(
		Alpha.Split(",", StringSplitOptions.RemoveEmptyEntries), double.Parse);
		colors2 = colors3.Split(',').Select(str => Calc.HexToColorWithAlpha(str.Trim())).ToArray();
		textures =
        [
            GFX.Game.GetAtlasSubtextures(sprite + "/a"),
            GFX.Game.GetAtlasSubtextures(sprite + "/b"),
            GFX.Game.GetAtlasSubtextures(sprite + "/c"),
        ];
		center = new Vector2(textures[0][0].Width, textures[0][0].Height) / 2f;
		stars = new Star[count];
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i] = new Star
			{
				Position = new Vector2(Calc.Random.NextFloat(320f + extraLoopBorderX) - extraLoopBorderX / 2, Calc.Random.NextFloat(180f + extraLoopBorderY) - extraLoopBorderY / 2),
				Timer = Calc.Random.NextFloat((float)Math.PI * 2f),
				Rate = animationRate + Calc.Random.NextFloat(animationRateRandom),
				TextureSet = Calc.Random.Next(textures.Count),
				Colors = new Color[trailCount],
			};
			int randomIndex = Calc.Random.Next(colors2.Length);
			int randomAlphas = Calc.Random.Next(alphas3.Length);
			for (int j = 0; j < stars[i].Colors.Length; j++)
			{
				stars[i].Colors[j] = colors2[randomIndex] * (float)alphas3[randomAlphas] * (1f - j / (float)stars[i].Colors.Length);
			}
			//Logger.Log("hi oomfie", alphas3[0].ToString()); //ffs lmao
			stars[i].MainColor = colors2[randomIndex] * (float)alphas3[randomAlphas];
			stars[i].Speed = new Vector2(Calc.Random.Range(minXSpeed, maxXSpeed), Calc.Random.Range(minYSpeed, maxYSpeed));
			//Logger.Log("fun", stars[i].speed.Y.ToString());
		}
	}

	public override void Update(Scene scene)
	{
		base.Update(scene);
		if (Visible)
		{
			Level level = scene as Level;
			for (int i = 0; i < stars.Length; i++)
			{
				stars[i].Timer += Engine.DeltaTime * stars[i].Rate;
				stars[i].Position += stars[i].Speed * Engine.DeltaTime / 60 * 100;
			}
		}
	}

	public override void Render(Scene scene)
	{
		Draw.Rect(0f, 0f, 320f, 180f, backdropColor * backdropAlpha);
		Level level = scene as Level;
		int num = stars.Length;
		for (int i = 0; i < num; i++)
		{
			List<MTexture> list = textures[stars[i].TextureSet];
			int num2 = (int)((Math.Sin(stars[i].Timer) + 1.0) / 2.0 * list.Count);
			num2 %= list.Count;
			Vector2 position = stars[i].Position;
			MTexture mTexture = list[num2];
			position.Y -= level.Camera.Y * parallaxY;
			position.X -= level.Camera.X * parallaxX;
			position.Y %= 180f + extraLoopBorderY;
			position.X %= 320f + extraLoopBorderX;
			if (position.Y < 0f)
			{
				position.Y += 180f + extraLoopBorderY;
			}
			if (position.X < 0f)
			{
				position.X += 320f + extraLoopBorderX;
			}
			Vector2 epic = new Vector2();
			epic = Vector2.Normalize(stars[i].Speed);
			for (int j = 0; j < stars[i].Colors.Length; j++)
			{
				mTexture.Draw(position - new Vector2(extraLoopBorderX / 2, extraLoopBorderY / 2) - epic * (j * separation), center, stars[i].Colors[j] * FadeAlphaMultiplier);
			}
			mTexture.Draw(position - new Vector2(extraLoopBorderX / 2, extraLoopBorderY / 2), center, stars[i].MainColor * FadeAlphaMultiplier);
		}
	}
}
