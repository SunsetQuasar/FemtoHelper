using System;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.FemtoHelper.Wipes;

public class SquareWipe : ScreenWipe
{
    public static readonly BlendState SubtractBlendmode = new BlendState
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    private bool hasDrawn;

    private readonly VertexPositionColor[] vertex = new VertexPositionColor[6];

    public SquareWipe(Scene scene, bool wipeIn, Action onComplete = null)
        : base(scene, wipeIn, onComplete)
    {
        for (int i = 0; i < vertex.Length; i++)
        {
            vertex[i].Color = WipeIn ? Color.Black : Color.White;
        }
    }

    public override void BeforeRender(Scene scene)
    {
        hasDrawn = true;
        Engine.Graphics.GraphicsDevice.SetRenderTarget(Celeste.WipeTarget);
        Engine.Graphics.GraphicsDevice.Clear(WipeIn ? Color.White : Color.Black);
        //if (Percent > 0.8f)
        //{
        //    float num = Calc.Map(Percent, 0.8f, 1f) * 1082f;
        //    Draw.SpriteBatch.Begin();
        //    Draw.Rect(-1f, (1080f - num) * 0.5f, 1922f, num, (!WipeIn) ? Color.White : Color.Black);
        //    Draw.SpriteBatch.End();
        //}
        float num2 = (float)Math.Pow(Percent, 3.5) * 1.4f;
        float num3 = WipeIn ? num2 : - num2;
        vertex[0].Position = new Vector3(new Vector2(Engine.Width / 2, Engine.Height / 2) - Calc.AngleToVector(num3 * 2, Engine.Width / 2 * num2), 0f);
        vertex[1].Position = new Vector3(new Vector2(Engine.Width / 2, Engine.Height / 2) - Calc.AngleToVector(num3 * 2 + (float)(1 * Math.PI / 2), Engine.Width / 2 * num2), 0f);
        vertex[2].Position = new Vector3(new Vector2(Engine.Width / 2, Engine.Height / 2) - Calc.AngleToVector(num3 * 2 + (float)(2 * Math.PI / 2), Engine.Width / 2 * num2), 0f);

        vertex[3].Position = new Vector3(new Vector2(Engine.Width / 2, Engine.Height / 2) - Calc.AngleToVector(num3 * 2 + (float)(2 * Math.PI / 2), Engine.Width / 2 * num2), 0f);
        vertex[4].Position = new Vector3(new Vector2(Engine.Width / 2, Engine.Height / 2) - Calc.AngleToVector(num3 * 2 + (float)(3 * Math.PI / 2), Engine.Width / 2 * num2), 0f);
        vertex[5].Position = new Vector3(new Vector2(Engine.Width / 2, Engine.Height / 2) - Calc.AngleToVector(num3 * 2, Engine.Width / 2 * num2), 0f);

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
            Draw.SpriteBatch.Draw((RenderTarget2D)Celeste.WipeTarget, new Vector2(-1f, -1f), Color.White);
        }
        Draw.SpriteBatch.End();
    }
}