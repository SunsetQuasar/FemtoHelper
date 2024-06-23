
using System;
using System.Reflection;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;


namespace Celeste.Mod.FemtoHelper.Effects
{

	public class DistortedParallax : Parallax
    {
        public enum sliceModes { 
		TransLong,
		LongTrans,
		TransTrans,
		LongLong,
		}


        private float fadein = 1f;

		private MTexture[] slices;

		private MTexture[,] slices2;

		private Vector2 timer = new Vector2(0, 0);

		private Vector2 frequency = new Vector2(40, 10);

		private Vector2 amplitude = new Vector2(10, 0);

		private Vector2 waveAnimSpeed = new Vector2(3, 3);

		private sliceModes sliceMode = sliceModes.TransLong;

		public DistortedParallax(string texture, float X, float Y, float spdX, float spdY, float scrollX, float scrollY, bool doesLoopX, bool doesLoopY, Color color, BlendState blend, float freqX, float freqY, float ampX, float ampY, float wavespdX, float wavespdY, Vector2 offset, bool flX, bool flY, sliceModes vert) : base(GFX.Game[texture])
		{
			Texture = GFX.Game[texture];
			Name = Texture.AtlasPath;
			slices = new MTexture[Texture.Height];
			Color = color;
			LoopX = doesLoopX;
			LoopY = doesLoopY;
			Position = new Vector2(X, Y);
			Speed = new Vector2(spdX, spdY);
			Scroll = new Vector2(scrollX, scrollY);
			timer = new Vector2(0f, 0f) + offset;
			BlendState = blend;
			frequency = new Vector2(freqX, freqY);
			amplitude = new Vector2(ampX, ampY);
			waveAnimSpeed = new Vector2(wavespdX, wavespdY);
			FlipX = flX;
			FlipY = flY;
			sliceMode = vert;
            if (sliceMode == sliceModes.LongTrans)
            {
				slices = new MTexture[Texture.Width];
				for (int i = 0; i < slices.Length; i++)
				{
					slices[i] = Texture.GetSubtexture(i, 0, 1, Texture.Height);
				}
			} else if (sliceMode == sliceModes.TransLong)
            {
				slices = new MTexture[Texture.Height];
				for (int i = 0; i < slices.Length; i++)
				{
					slices[i] = Texture.GetSubtexture(0, i, Texture.Width, 1);
				}
			} else if (sliceMode == sliceModes.LongLong || sliceMode == sliceModes.TransTrans)
            {
				slices2 = new MTexture[Texture.Width, Texture.Height];
				for (int i = 0; i < Texture.Width; i++)
				{
					for (int j = 0; j < Texture.Height; j++)
					{
						slices2[i,j] = Texture.GetSubtexture(i, j, 1, 1);
					}
				}
			}
			
		}

		public override void Update(Scene scene)
		{
			timer += waveAnimSpeed * Engine.DeltaTime;
			base.Update(scene);
			Position += Speed * Engine.DeltaTime;
			Position += WindMultiplier * (scene as Level).Wind * Engine.DeltaTime;
			if (DoFadeIn)
			{
                fadein = Calc.Approach(fadein, Visible ? 1 : 0, Engine.DeltaTime);
			}
			else
			{
				fadein = (Visible ? 1 : 0);
			}
		}

		public override void Render(Scene scene)
		{
			Vector2 vector = ((scene as Level).Camera.Position + CameraOffset).Floor();
			Vector2 vector2 = (Position - vector * Scroll).Floor();
			float num = fadein * Alpha * FadeAlphaMultiplier;
			if (FadeX != null)
			{
				num *= FadeX.Value(vector.X + 160f);
			}
			if (FadeY != null)
			{
				num *= FadeY.Value(vector.Y + 90f);
			}
			Color color = Color;
			if (num < 1f)
			{
				color *= num;
			}
			if (color.A <= 1)
			{
				return;
			}
			if (LoopX)
			{
				while (vector2.X < 0f)
				{
					vector2.X += Texture.Width * 2;
				}
				while (vector2.X > 0f)
				{
					vector2.X -= Texture.Width * 2;
				}
			}
			if (LoopY)
			{
				while (vector2.Y < 0f)
				{
					vector2.Y += Texture.Height * 2;
				}
				while (vector2.Y > 0f)
				{
					vector2.Y -= Texture.Height * 2;
				}
			}
			SpriteEffects flip = SpriteEffects.None;
			if (FlipX && FlipY)
			{
				flip = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
			}
			else if (FlipX)
			{
				flip = SpriteEffects.FlipHorizontally;
			}
			else if (FlipY)
			{
				flip = SpriteEffects.FlipVertically;
			}
            if (sliceMode == sliceModes.LongTrans)
            {

				float sliceID;
				int sliceID2;
				for (float num2 = vector2.X; num2 < 320f + amplitude.X + Texture.Width; num2 += (float)Texture.Width)
				{
					for (float num3 = vector2.Y; num3 < 180f + amplitude.Y + Texture.Height; num3 += (float)Texture.Height)
					{

						for (float i = 0; i < slices.Length; i++)
						{
							Vector2 newnum2 = new Vector2(num2, num3);
							newnum2 += (vector * Scroll);
							if (num2 == vector2.X && LoopX)
                            {
								sliceID = (float)Math.Round(i + (Math.Sin(((newnum2.X + i) / frequency.X) + timer.X) * amplitude.X));
								sliceID2 = FlipX ? (int)mod(slices.Length - sliceID, slices.Length) : (int)mod(sliceID, slices.Length);
								slices[sliceID2].Draw(new Vector2(num2 - Texture.Width + i, num3) + new Vector2(0f, (float)(Math.Sin(((newnum2.X + i) / frequency.Y) + timer.Y) * amplitude.Y)), Vector2.Zero, color, 1f, 0f, flip);
							}
							if(num3 == vector2.Y && LoopY)
                            {
								sliceID = (float)Math.Round(i + (Math.Sin(((newnum2.X + i) / frequency.X) + timer.X) * amplitude.X));
								sliceID2 = FlipX ? (int)mod(slices.Length - sliceID, slices.Length) : (int)mod(sliceID, slices.Length);
								slices[sliceID2].Draw(new Vector2(num2 + i, num3 - Texture.Height) + new Vector2(0f, (float)(Math.Sin(((newnum2.X + i) / frequency.Y) + timer.Y) * amplitude.Y)), Vector2.Zero, color, 1f, 0f, flip);
							}
							if(num2 == vector2.X && num3 == vector2.Y && LoopX && LoopY)
                            {
								sliceID = (float)Math.Round(i + (Math.Sin(((newnum2.X + i) / frequency.X) + timer.X) * amplitude.X));
								sliceID2 = FlipX ? (int)mod(slices.Length - sliceID, slices.Length) : (int)mod(sliceID, slices.Length);
								slices[sliceID2].Draw(new Vector2(num2 - Texture.Width + i, num3 - Texture.Height) + new Vector2(0f, (float)(Math.Sin(((newnum2.X + i) / frequency.Y) + timer.Y) * amplitude.Y)), Vector2.Zero, color, 1f, 0f, flip);
							}
							sliceID = (float)Math.Round(i + (Math.Sin(((newnum2.X + i) / frequency.X) + timer.X) * amplitude.X));
							sliceID2 = FlipX ? (int)mod(slices.Length - sliceID, slices.Length) : (int)mod(sliceID, slices.Length);
							slices[sliceID2].Draw(new Vector2(num2 + i, num3) + new Vector2(0f, (float)(Math.Sin(((newnum2.X + i) / frequency.Y) + timer.Y) * amplitude.Y)), Vector2.Zero, color, 1f, 0f, flip);
						}
						if (!LoopY)
						{
							break;
						}

					}
					if (!LoopX)
					{
						break;
					}
				}
			} else if (sliceMode == sliceModes.TransLong)
            {
				float sliceID;
				int sliceID2;
				for (float num2 = vector2.X; num2 < 320f + amplitude.X + Texture.Width; num2 += (float)Texture.Width)
				{
					for (float num3 = vector2.Y; num3 < 180f + amplitude.Y + Texture.Height; num3 += (float)Texture.Height)
					{

						for (float i = 0; i < slices.Length; i++)
						{

							Vector2 newnum2 = new Vector2(num2, num3);
							newnum2 += (vector * Scroll);
							if (num2 == vector2.X && LoopX)
							{
								sliceID = (float)Math.Round(i + (Math.Sin(((newnum2.Y + i) / frequency.Y) + timer.Y) * amplitude.Y));
								sliceID2 = FlipY ? (int)mod(slices.Length - sliceID, slices.Length) : (int)mod(sliceID, slices.Length);
								slices[sliceID2].Draw(new Vector2(num2 - Texture.Width, num3 + i) + new Vector2((float)(Math.Sin(((newnum2.Y + i) / frequency.X) + timer.X) * amplitude.X), 0f), Vector2.Zero, color, 1f, 0f, flip);
							}
							if (num3 == vector2.Y && LoopY)
							{
								sliceID = (float)Math.Round(i + (Math.Sin(((newnum2.Y + i) / frequency.Y) + timer.Y) * amplitude.Y));
								sliceID2 = FlipY ? (int)mod(slices.Length - sliceID, slices.Length) : (int)mod(sliceID, slices.Length);
								slices[sliceID2].Draw(new Vector2(num2, num3 + i - Texture.Height) + new Vector2((float)(Math.Sin(((newnum2.Y + i) / frequency.X) + timer.X) * amplitude.X), 0f), Vector2.Zero, color, 1f, 0f, flip);
							}
							if (num2 == vector2.X && num3 == vector2.Y && LoopX && LoopY)
							{
								sliceID = (float)Math.Round(i + (Math.Sin(((newnum2.Y + i) / frequency.Y) + timer.Y) * amplitude.Y));
								sliceID2 = FlipY ? (int)mod(slices.Length - sliceID, slices.Length) : (int)mod(sliceID, slices.Length);
								slices[sliceID2].Draw(new Vector2(num2 - Texture.Width, num3 + i - Texture.Height) + new Vector2((float)(Math.Sin(((newnum2.Y + i) / frequency.X) + timer.X) * amplitude.X), 0f), Vector2.Zero, color, 1f, 0f, flip);
							}
							sliceID = (float)Math.Round(i + (Math.Sin(((newnum2.Y + i) / frequency.Y) + timer.Y) * amplitude.Y));
							sliceID2 = FlipY ? (int)mod(slices.Length - sliceID, slices.Length) : (int)mod(sliceID, slices.Length);
							slices[sliceID2].Draw(new Vector2(num2, num3 + i) + new Vector2((float)(Math.Sin(((newnum2.Y + i) / frequency.X) + timer.X) * amplitude.X), 0f), Vector2.Zero, color, 1f, 0f, flip);
						}

						if (!LoopY)
						{
							break;
						}
					}
					if (!LoopX)
					{
						break;
					}
				}
			} else if (sliceMode == sliceModes.TransTrans) {
				for (float num2 = vector2.X - amplitude.X - Texture.Width; num2 < 320f + amplitude.X + Texture.Width; num2 += (float)Texture.Width)
				{
					for (float num3 = vector2.Y - amplitude.Y - Texture.Height; num3 < 180f + amplitude.Y + Texture.Height; num3 += (float)Texture.Height)
					{

						for (float i = 0; i < Texture.Width; i++)
						{
							for (float j = 0; j < Texture.Height; j++)
							{
								slices2[(int)i, (int)j].Draw(new Vector2(num2 + i, num3 + j) + new Vector2((float)(Math.Sin(((num3 + j) / frequency.X) + timer.X) * amplitude.X), (float)(Math.Sin(((num2 + i) / frequency.Y) + timer.Y) * amplitude.Y)), Vector2.Zero, color, 1f, 0f, flip);
							}
							
						}

						if (!LoopY)
						{
							break;
						}
					}
					if (!LoopX)
					{
						break;
					}
				}
			}
			else if (sliceMode == sliceModes.LongLong)
			{
				for (float num2 = vector2.X - amplitude.X - Texture.Width; num2 < 320f + amplitude.X + Texture.Width; num2 += (float)Texture.Width)
				{
					for (float num3 = vector2.Y - amplitude.Y - Texture.Height; num3 < 180f + amplitude.Y + Texture.Height; num3 += (float)Texture.Height)
					{

						for (float i = 0; i < Texture.Width; i++)
						{
							for (float j = 0; j < Texture.Height; j++)
							{
								float sliceID = (float)Math.Round(j + (Math.Sin(((num3 + j) / frequency.Y) + timer.Y) * amplitude.Y));
								int sliceID2 = (int)mod(sliceID, Texture.Height);
								float sliceID3 = (float)Math.Round(i + (Math.Sin(((num2 + i) / frequency.X) + timer.X) * amplitude.X));
								int sliceID4 = (int)mod(sliceID, Texture.Width);
								slices2[sliceID4, sliceID2].Draw(new Vector2(num2 + i, num3 + j) + new Vector2((float)(Math.Sin(((num3 + j) / frequency.X) + timer.X) * amplitude.X), (float)(Math.Sin(((num2 + i) / frequency.Y) + timer.Y) * amplitude.Y)), Vector2.Zero, color, 1f, 0f, flip);
							}
							

						}

						if (!LoopY)
						{
							break;
						}
					}
					if (!LoopX)
					{
						break;
					}
				}
			}

		}
		private float mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}

}
