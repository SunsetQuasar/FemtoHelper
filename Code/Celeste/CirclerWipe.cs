// Celeste.HeartWipe
using System;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
namespace Celeste.Mod.FemtoHelper.Wipes
{
	public class CirclerWipe : ScreenWipe
	{
		private static int bars = 4;

		public static readonly BlendState SubtractBlendmode = new BlendState
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };

		public bool hasDrawn;

		private VertexPositionColor[] vertex = new VertexPositionColor[(bars * 6) + 6];

		public CirclerWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
			for (int i = 0; i < vertex.Length; i++)
			{
				vertex[i].Color = (WipeIn ? Color.Black : Color.White);
			}
		}

		public override void BeforeRender(Scene scene)
		{
			hasDrawn = true;
			Engine.Graphics.GraphicsDevice.SetRenderTarget(global::Celeste.Celeste.WipeTarget);
			Engine.Graphics.GraphicsDevice.Clear(WipeIn ? Color.White : Color.Black);

			float num = (float)Math.Pow(Percent, 2);

			for (float i = 0; i < bars; i++)
			{
				num = (float)(1-Percent);

				float bottom = (i + 1) * Engine.Height / bars;
				float top = i * Engine.Height / bars;
				vertex[0 + (int)(6 * i)].Position = new Vector3(Engine.Width, top + (Engine.Height / bars / 2 * num), 0f);
				vertex[1 + (int)(6 * i)].Position = new Vector3(Engine.Width, bottom - (Engine.Height / bars / 2 * num), 0f);
				vertex[2 + (int)(6 * i)].Position = new Vector3(0f, bottom - (Engine.Height / bars / 2 * num), 0f);

				vertex[3 + (int)(6 * i)].Position = new Vector3(0f, bottom - (Engine.Height / bars / 2 * num), 0f);
				vertex[4 + (int)(6 * i)].Position = new Vector3(0f, top + (Engine.Height / bars / 2 * num), 0f);
				vertex[5 + (int)(6 * i)].Position = new Vector3(Engine.Width, top + (Engine.Height / bars / 2 * num), 0f);
			}
			num = (float)Math.Pow(Percent, 2) * 1.1f;
			vertex[bars * 6].Position = new Vector3(Engine.Width + 1, 0f, 0f);
			vertex[(bars * 6) + 1].Position = new Vector3(Engine.Width + 1, Engine.Height + 1, 0f);
			vertex[(bars * 6) + 2].Position = new Vector3(Engine.Width - (Engine.Width * num / 2), Engine.Height, 0f);

			vertex[(bars * 6) + 3].Position = new Vector3(0f, 0f, 0f);
			vertex[(bars * 6) + 4].Position = new Vector3(0f, Engine.Height, 0f);
			vertex[(bars * 6) + 5].Position = new Vector3(0 + (Engine.Width * num / 2), 0f, 0f);
			GFX.DrawVertices(Matrix.Identity, vertex, vertex.Length);

		}

		public override void Render(Scene scene)
		{
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, SubtractBlendmode, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
			if ((WipeIn && Percent <= 0.01f) || (!WipeIn && Percent >= 0.99f))
			{
				Draw.Rect(-1f, -1f, 1922f, 1082f, Color.White);
			}
			else if (hasDrawn)
			{
				Draw.SpriteBatch.Draw((RenderTarget2D)global::Celeste.Celeste.WipeTarget, new Vector2(-1f, -1f), Color.White);
			}
			Draw.SpriteBatch.End();
		}
	}

}
