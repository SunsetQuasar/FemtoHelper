
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

public class DigitalCascade : Backdrop
{
	private struct Cascatee
	{
		public Vector2 Position;

		public Vector2 Speed;

		public float Size;

		public Color Color;

		public MTexture Texture;

		public float Lifetime;

		public float initLifetime;

		public int spriteIndex;

		public float AItimer;
    }
    private struct Afterimage
    {
        public Vector2 Position;

        public float Size;

        public Color Color;

        public MTexture Texture;

        public float Age;
    }
	private Cascatee[] cascatees;

	private Afterimage[] afterimages;

	private Vector2 lastCamera = Vector2.Zero;

	private Color[] possibleColors;

	private float halfloopborderX;
	private float halfloopborderY;

	private List<MTexture> textures;

	private float minLife;
	private float maxLife;

	private float fadeTime;
	private int fadeScaleMode;

	private bool randomSymbol;

	private int afterimageCount = 0;

	private float AIfadeTime;

	private float AIemitTime;

	private float scrolling;

	private bool infiniteLifetime;

	private float alpha_AI;

	public DigitalCascade(int symbolAmount, int afterimagesCap, string colors, float alpha, float minspeed, float maxspeed, float angle, float extraLoopBorderX, float extraLoopBorderY, string spritePath, float minlife, float maxlife, float fadetime, int doScale, bool randomsym, float aifade, float aiemit, float scroll, bool inflife, float AIalpha)
	{
		string[] colorArray = colors.Split(',');
		possibleColors = (Color[])(object)new Color[colorArray.Length];
		for (int i = 0; i < possibleColors.Length; i++)
		{
			possibleColors[i] = Calc.HexToColor(colorArray[i]) * alpha;
		}

		halfloopborderX = extraLoopBorderX;
		halfloopborderY = extraLoopBorderY;

		minLife = minlife;
		maxLife = maxlife;

		scrolling = scroll;

		cascatees = new Cascatee[symbolAmount];

		afterimages = new Afterimage[afterimagesCap];

		float angleRad = Calc.DegToRad * angle;

		textures = new List<MTexture>();
		textures = GFX.Game.GetAtlasSubtextures(spritePath);

		fadeTime = fadetime;
		fadeScaleMode = doScale;
		randomSymbol = randomsym;

		AIfadeTime = aifade;
		AIemitTime = aiemit;
		alpha_AI = AIalpha;

		infiniteLifetime = inflife;

		for (int i = 0; i < cascatees.Length; i++)
		{
			float chosenSpeed = Calc.Random.Range(minspeed, maxspeed);
			cascatees[i].Position = new Vector2(
				Calc.Random.Range(0 - halfloopborderX, 320f + halfloopborderX),
				Calc.Random.Range(0 - halfloopborderY, 180f + halfloopborderY)
				);
			cascatees[i].Speed = new Vector2((float)Math.Sin(angleRad) * chosenSpeed, (float)Math.Cos(angleRad) * chosenSpeed);
			cascatees[i].Size = 1f;
			cascatees[i].Color = possibleColors[Calc.Random.Next(possibleColors.Length)];
			cascatees[i].spriteIndex = Calc.Random.Next(textures.Count);
			cascatees[i].Texture = textures[cascatees[i].spriteIndex];
			cascatees[i].Lifetime = Calc.Random.Range(minLife, maxLife);
			cascatees[i].initLifetime = cascatees[i].Lifetime;
			cascatees[i].AItimer = Calc.Random.NextFloat(AIemitTime);
		}
	}

	public override void Update(Scene scene)
	{
		base.Update(scene);
		Vector2 position = (scene as Level).Camera.Position;
		Vector2 vector = position - lastCamera;
		for (int ai = 0; ai < afterimageCount; ai++)
		{
			afterimages[ai].Age += Engine.DeltaTime;
			afterimages[ai].Position += Vector2.Zero * Engine.DeltaTime - vector * scrolling;
			if (afterimages[ai].Age >= AIfadeTime)
			{
				afterimages[ai].Age = afterimages[afterimageCount - 1].Age;
				afterimages[ai].Position = afterimages[afterimageCount - 1].Position;
				afterimages[ai].Texture = afterimages[afterimageCount - 1].Texture;
				afterimages[ai].Size = afterimages[afterimageCount - 1].Size;
				afterimages[ai].Color = afterimages[afterimageCount - 1].Color;
				afterimageCount--;
				ai--;
			}

		}
		for (int i = 0; i < cascatees.Length; i++)
		{
			cascatees[i].Position += cascatees[i].Speed * Engine.DeltaTime - vector * scrolling;

			//if (cascatees[i].Position.X < 0 - halfloopborderX || cascatees[i].Position.X > 320 + halfloopborderX)
			//{
			//	cascatees[i].Position.X = mod(cascatees[i].Position.X, 320 + halfloopborderX) - halfloopborderX;
			//}
			//if (cascatees[i].Position.Y < 0 - halfloopborderY || cascatees[i].Position.Y > 180 + halfloopborderY)
			//{
			//	cascatees[i].Position.Y = mod(cascatees[i].Position.Y, 180 + halfloopborderY) - halfloopborderY;
			//}

			cascatees[i].Lifetime -= Engine.DeltaTime;

			Logger.Log("digital cascade", cascatees[i].Lifetime.ToString());
			if (!infiniteLifetime)
			{
				if (cascatees[i].Lifetime <= 0)
				{
					cascatees[i].Lifetime = Calc.Random.Range(minLife, maxLife);
					cascatees[i].initLifetime = cascatees[i].Lifetime;
					if (randomSymbol)
					{
						cascatees[i].Texture = textures[Calc.Random.Next(textures.Count)];
					}
					else
					{
						int nextIndex = cascatees[i].spriteIndex + 1;
						if (nextIndex >= textures.Count)
						{
							nextIndex = 0;
						}
						cascatees[i].spriteIndex = nextIndex;
						cascatees[i].Texture = textures[cascatees[i].spriteIndex];
					}

				}


				if (fadeScaleMode != 0)
				{
					if (cascatees[i].Lifetime <= fadeTime)
					{
						cascatees[i].Size = cascatees[i].Lifetime / fadeTime;

					}
					else if (cascatees[i].initLifetime - cascatees[i].Lifetime <= fadeTime)
					{
						cascatees[i].Size = (cascatees[i].initLifetime - cascatees[i].Lifetime) / fadeTime;

					}
					else
					{
						cascatees[i].Size = 1;
					}
				}
			}
                cascatees[i].AItimer -= Engine.DeltaTime;
			if (cascatees[i].AItimer <= 0)
            {
				cascatees[i].AItimer = AIemitTime;
                {
					afterimages[afterimageCount].Age = 0;
					afterimages[afterimageCount].Position = cascatees[i].Position;
					afterimages[afterimageCount].Texture = cascatees[i].Texture;
					afterimages[afterimageCount].Size = cascatees[i].Size;
					afterimages[afterimageCount].Color = cascatees[i].Color;
					afterimageCount = Math.Min(afterimageCount + 1, afterimages.Length - 1);
						
				}
			}
		}

		lastCamera = position;
	}

	public override void Render(Scene scene)
	{
		for (int i = 0; i < cascatees.Length; i++)
		{
			cascatees[i].Texture.Draw(
				new Vector2(
					mod(cascatees[i].Position.X, 320 + (halfloopborderX * 2)) - halfloopborderX,
					mod(cascatees[i].Position.Y, 180 + (halfloopborderY * 2)) - halfloopborderY
					),
				new Vector2(
					cascatees[i].Texture.Width, 
					cascatees[i].Texture.Height
				) / 2f, 
				cascatees[i].Color,
				fadeScaleMode == 0 ? Vector2.One : // no scaling
				fadeScaleMode == 1 ? new Vector2( // X and Y scaling
					cascatees[i].Size,
					cascatees[i].Size
				) :
				fadeScaleMode == 2 ? new Vector2( // X scaling only
					cascatees[i].Size,
					1
				) : new Vector2( // Y scaling only
					1,
					cascatees[i].Size
				)
			);
		}
		for (int ai = 0; ai < afterimageCount; ai++)
        {
						afterimages[ai].Texture.Draw(
				new Vector2(
					mod(afterimages[ai].Position.X, 320 + (halfloopborderX * 2)) - halfloopborderX,
					mod(afterimages[ai].Position.Y, 180 + (halfloopborderY * 2)) - halfloopborderY
					),
				new Vector2(
					afterimages[ai].Texture.Width,
					afterimages[ai].Texture.Height
				) / 2f,
				afterimages[ai].Color * (1 - (afterimages[ai].Age / AIfadeTime)) * alpha_AI,
				fadeScaleMode == 0 ? Vector2.One : // no scaling
				fadeScaleMode == 1 ? new Vector2( // X and Y scaling
					afterimages[ai].Size,
					afterimages[ai].Size
				) :
				fadeScaleMode == 2 ? new Vector2( // X scaling only
					afterimages[ai].Size,
					1
				) : new Vector2( // Y scaling only
					1,
					afterimages[ai].Size
				)
			);
        }
	}

	private float mod(float x, float m)
	{
		return (x % m + m) % m;
	}
}
