// Celeste.StarsBG
using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

public class FemtoStars : Backdrop
{
	private struct Star
	{
		public Vector2 Position;

		public int TextureSet;

		public float Timer;

		public float Rate;

		public Color[] colors;

		public Color mainColor;

		public Vector2 speed;

		public float alpha;
	}

	private const int StarCount = 100;

	private Star[] stars;

	private Color[] colors2;

	private List<List<MTexture>> textures;

	private float falling;

	private Vector2 center;

	private int trailCount;

	private float extraLoopBorderX;

	private float extraLoopBorderY;

	private Color backdropColor;

	private float backdropAlpha;

	private float parallaxX;

	private float parallaxY;

	public float alphaLegacy;

	public string alpha;

	private float separation;

	private string[] alphas;
	private float[] alphas2;
	public FemtoStars(int blurCount, string colors3, float minXSpeed, float maxXSpeed, float minYSpeed, float maxYSpeed, float loopBorderX, float loopBorderY, int count, string backgroundColor, float backgroundAlpha, string sprite, float scrollX, float scrollY, float transparency, float trailSeparation, float animationRate, string transp2)
	{
		trailCount = blurCount;
		extraLoopBorderX = loopBorderX;
		extraLoopBorderY = loopBorderY;
		backdropColor = Calc.HexToColor(backgroundColor);
		backdropAlpha = backgroundAlpha;
		parallaxX = scrollX;
		parallaxY = scrollY;
		alphaLegacy = transparency;
		alpha = transp2;
		separation = trailSeparation;
		string[] array = colors3.Split(',');
		var alphas3 = Array.ConvertAll(
		alpha.Split(new[] { ',', }, StringSplitOptions.RemoveEmptyEntries),
		Double.Parse);
		colors2 = (Color[])(object)new Color[array.Length];
		for (int i = 0; i < colors2.Length; i++)
		{
			colors2[i] = Calc.HexToColor(array[i]);
		}
		textures = new List<List<MTexture>>();
		textures.Add(GFX.Game.GetAtlasSubtextures(sprite + "/a"));
		textures.Add(GFX.Game.GetAtlasSubtextures(sprite + "/b"));
		textures.Add(GFX.Game.GetAtlasSubtextures(sprite + "/c"));
		center = new Vector2(textures[0][0].Width, textures[0][0].Height) / 2f;
		stars = new Star[count];
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i] = new Star
			{
				Position = new Vector2(Calc.Random.NextFloat(320f + extraLoopBorderX) - (extraLoopBorderX / 2), Calc.Random.NextFloat(180f + extraLoopBorderY) - (extraLoopBorderY / 2)),
				Timer = Calc.Random.NextFloat((float)Math.PI * 2f),
				Rate = animationRate + Calc.Random.NextFloat(2f),
				TextureSet = Calc.Random.Next(textures.Count),
				colors = new Color[trailCount],
			};
			int randomIndex = Calc.Random.Next(colors2.Length);
			int randomAlphas = Calc.Random.Next(alphas3.Length);
			for (int j = 0; j < stars[i].colors.Length; j++)
			{
				stars[i].colors[j] = colors2[randomIndex] * (float)(alphas3[randomAlphas]) * (1f - (float)j / (float)stars[i].colors.Length);
			}
			//Logger.Log("hi oomfie", alphas3[0].ToString()); //ffs lmao
			stars[i].mainColor = colors2[randomIndex] * (float)(alphas3[randomAlphas]);
			stars[i].speed = new Vector2(Calc.Random.Range(minXSpeed, maxXSpeed), Calc.Random.Range(minYSpeed, maxYSpeed));
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
				stars[i].Position += stars[i].speed * Engine.DeltaTime / 60 * 100;
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
			int num2 = (int)((Math.Sin(stars[i].Timer) + 1.0) / 2.0 * (double)list.Count);
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
			epic = Vector2.Normalize(stars[i].speed);
			for (int j = 0; j < stars[i].colors.Length; j++)
			{
				mTexture.Draw(position - new Vector2(extraLoopBorderX / 2, extraLoopBorderY / 2) - epic * (j * separation), center, stars[i].colors[j] * FadeAlphaMultiplier);
			}
			mTexture.Draw(position - new Vector2(extraLoopBorderX / 2, extraLoopBorderY / 2), center, stars[i].mainColor * FadeAlphaMultiplier);
		}
	}
}
