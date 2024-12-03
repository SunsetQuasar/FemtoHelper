// Celeste.HeartWipe
using System;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.FemtoHelper.Wipes;

public class CirclerWipe : ScreenWipe
{
	private static readonly int Bars = 4;

	public bool HasDrawn;

	private readonly VertexPositionColor[] vertex = new VertexPositionColor[(Bars+4) * 6];

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
		HasDrawn = true;
		Engine.Graphics.GraphicsDevice.SetRenderTarget(Celeste.WipeTarget);
		Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

		float num = Calc.Clamp((!WipeIn ? 1 - Ease.QuintInOut(Percent) : Ease.QuintInOut(Percent)) * 1.2f, 0, 1);

		for (float i = 1; i < Bars + 4; i++)
		{
			float i2 = i - 2;

			float bottom = (i2 + 1) * Engine.Height / Bars;
			float top = i2 * Engine.Height / Bars;
			vertex[0 + (int)(6 * i)].Position = new Vector3(Engine.Width + 1, top + Engine.Height / Bars / 2 * num + Engine.Width * 0.1f, 0f);
			vertex[1 + (int)(6 * i)].Position = new Vector3(Engine.Width + 1, bottom - Engine.Height / Bars / 2 * num + Engine.Width * 0.1f, 0f);
			vertex[2 + (int)(6 * i)].Position = new Vector3(0f, bottom - Engine.Height / Bars / 2 * num - Engine.Width * 0.1f, 0f);

			vertex[3 + (int)(6 * i)].Position = new Vector3(0f, bottom - Engine.Height / Bars / 2 * num - Engine.Width * 0.1f, 0f);
			vertex[4 + (int)(6 * i)].Position = new Vector3(0f, top + Engine.Height / Bars / 2 * num - Engine.Width * 0.1f, 0f);
			vertex[5 + (int)(6 * i)].Position = new Vector3(Engine.Width + 1, top + Engine.Height / Bars / 2 * num + Engine.Width * 0.1f, 0f);
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
		else if (HasDrawn)
		{
			Draw.SpriteBatch.Draw((RenderTarget2D)Celeste.WipeTarget, new Vector2(-1f, -1f), Color.White);
		}
		Draw.SpriteBatch.End();
	}
}