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

		public bool hasDrawn;

		private VertexPositionColor[] vertex = new VertexPositionColor[(bars+4) * 6];

		public CirclerWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
			for (int i = 0; i < vertex.Length; i++)
			{
				vertex[i].Color = Color.Black;
			}
		}

		public override void BeforeRender(Scene scene)
		{
			hasDrawn = true;
			Engine.Graphics.GraphicsDevice.SetRenderTarget(Celeste.WipeTarget);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

			float num = Calc.Clamp((!WipeIn ? 1 - Ease.QuintInOut(Percent) : Ease.QuintInOut(Percent)) * 1.2f, 0, 1);

			for (float i = 1; i < bars + 4; i++)
			{
				float i2 = i - 2;

				float bottom = (i2 + 1) * Engine.Height / bars;
				float top = i2 * Engine.Height / bars;
				vertex[0 + (int)(6 * i)].Position = new Vector3(Engine.Width + 1, top + (Engine.Height / bars / 2 * num) + (Engine.Width * 0.1f), 0f);
				vertex[1 + (int)(6 * i)].Position = new Vector3(Engine.Width + 1, bottom - (Engine.Height / bars / 2 * num) + (Engine.Width * 0.1f), 0f);
				vertex[2 + (int)(6 * i)].Position = new Vector3(0f, bottom - (Engine.Height / bars / 2 * num) - (Engine.Width * 0.1f), 0f);

				vertex[3 + (int)(6 * i)].Position = new Vector3(0f, bottom - (Engine.Height / bars / 2 * num) - (Engine.Width * 0.1f), 0f);
				vertex[4 + (int)(6 * i)].Position = new Vector3(0f, top + (Engine.Height / bars / 2 * num) - (Engine.Width * 0.1f), 0f);
				vertex[5 + (int)(6 * i)].Position = new Vector3(Engine.Width + 1, top + (Engine.Height / bars / 2 * num) + (Engine.Width * 0.1f), 0f);
			}
			GFX.DrawVertices(Matrix.Identity, vertex, vertex.Length);

		}

		public override void Render(Scene scene)
		{
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
			if ((WipeIn && Percent <= 0.01f) || (!WipeIn && Percent >= 0.99f))
			{
				Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black);
			}
			else if (hasDrawn)
			{
				Draw.SpriteBatch.Draw((RenderTarget2D)Celeste.WipeTarget, new Vector2(-1f, -1f), Color.White);
			}
			Draw.SpriteBatch.End();
		}
	}

}
