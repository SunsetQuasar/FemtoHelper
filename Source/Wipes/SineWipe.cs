// Celeste.HeartWipe
using System;
using Microsoft.Xna.Framework.Graphics;
namespace Celeste.Mod.FemtoHelper.Wipes;

public class SineWipe : ScreenWipe
{
    private static int bars = 32;

    private VertexPositionColor[] vertex = new VertexPositionColor[bars * 12];

    public SineWipe(Scene scene, bool wipeIn, Action onComplete = null)
        : base(scene, wipeIn, onComplete)
    {
        for (int i = 0; i < vertex.Length; i++)
        {
            vertex[i].Color = ScreenWipe.WipeColor;
        }
    }

    public override void Render(Scene scene)
    {
        float num = !WipeIn ? (float)Math.Pow(Percent, 2) : (1f - (float)Math.Pow(Percent, 2));

        float num2 = !WipeIn ? (float)Math.Pow(Percent, 2) : (1f - (float)Math.Pow(Percent, 2));

        if (num <= 0f)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
            if ((WipeIn && Percent <= 0.01f) || (!WipeIn && Percent >= 0.99f))
            {
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.White);
            }
            Draw.SpriteBatch.End();
            return;
        }
        for (float i = 0; i < bars; i++)
        {
            num = !WipeIn ? (float)Math.Pow(Percent, 2 + Math.Sin(Math.PI * i / (bars / 2))) : (1f - (float)Math.Pow(Percent, 2 + Math.Sin(Math.PI * i / (bars / 2))));

            num2 = !WipeIn ? (float)Math.Pow(Percent, 2 + Math.Cos(Math.PI * i / (bars / 2))) : (1f - (float)Math.Pow(Percent, 2 + Math.Cos(Math.PI * i / (bars / 2))));

            float bottom = (i + 1) * Engine.Height / bars;
            float top = i * Engine.Height / bars;
            vertex[0 + (int)(12 * i)].Position = new Vector3(Engine.Width - (Engine.Width / 2 * num), top, 0f);
            vertex[1 + (int)(12 * i)].Position = new Vector3(Engine.Width - (Engine.Width / 2 * num), bottom, 0f);
            vertex[2 + (int)(12 * i)].Position = new Vector3(Engine.Width, bottom, 0f);

            vertex[3 + (int)(12 * i)].Position = new Vector3(Engine.Width, bottom, 0f);
            vertex[4 + (int)(12 * i)].Position = new Vector3(Engine.Width, top, 0f);
            vertex[5 + (int)(12 * i)].Position = new Vector3(Engine.Width - (Engine.Width / 2 * num), top, 0f);


            vertex[6 + (int)(12 * i)].Position = new Vector3(0 + (Engine.Width / 2 * num2), top, 0f);
            vertex[7 + (int)(12 * i)].Position = new Vector3(0 + (Engine.Width / 2 * num2), bottom, 0f);
            vertex[8 + (int)(12 * i)].Position = new Vector3(0, bottom, 0f);

            vertex[9 + (int)(12 * i)].Position = new Vector3(0, bottom, 0f);
            vertex[10 + (int)(12 * i)].Position = new Vector3(0, top, 0f);
            vertex[11 + (int)(12 * i)].Position = new Vector3(0 + (Engine.Width / 2 * num2), top, 0f);
        }



        ScreenWipe.DrawPrimitives(vertex);
    }
}
