using System;
using System.Runtime.CompilerServices;
using Celeste;
using IL.Celeste.Mod.Registry.DecalRegistryHandlers;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.FemtoHelper.Wipes;
public class SolarWipe : ScreenWipe
{
    public static BlendState SubtractBlendmode = new BlendState
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    private VertexPositionColor[] verts = new VertexPositionColor[num * 3];

    private bool hasDrawn;

    public float spinspeed;
    public float spin = MathF.PI / 4;
    public const int num = 16;
    public SolarWipe(Scene scene, bool wipeIn, Action onComplete = null)
        : base(scene, wipeIn, onComplete)
    {
        for (int k = 0; k < verts.Length; k++)
        {
            verts[k].Color = (WipeIn ? Color.White : Color.Black);
        }
        spinspeed = 0;
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);
        spin += Engine.DeltaTime * spinspeed;
        spinspeed += WipeIn ? -0.2f : 0.2f;
    }

    public override void BeforeRender(Scene scene)
    {
        hasDrawn = true;
        Engine.Graphics.GraphicsDevice.SetRenderTarget(global::Celeste.Celeste.WipeTarget);
        Engine.Graphics.GraphicsDevice.Clear(WipeIn ? Color.Black : Color.White);
        int index = 0;
        for (float i = 0; i < num; i++)
        {
            float factor = i % 2 == 0 ? 1.4f : 1f;
            float antifactor = i % 2 == 0 ? 1f : 1.4f;
            verts[index++].Position = new Vector3(
                new Vector2(
                    Engine.Width / 2,
                    Engine.Height / 2) +
                new Vector2(
                    MathF.Sin(spin + i * MathF.PI * 2 / num) * factor * (1102 * Ease.QuadInOut(1 - Percent)),
                    MathF.Cos(spin + i * MathF.PI * 2 / num) * factor * (1102 * Ease.QuadInOut(1 - Percent)))
                , 0);
            verts[index++].Position = new Vector3(
                new Vector2(
                    Engine.Width / 2,
                    Engine.Height / 2) +
                new Vector2(MathF.Sin(spin + (i + 1) * MathF.PI * 2 / num) * antifactor * (1102 * Ease.QuadInOut(1 - Percent)),
                MathF.Cos(spin + (i + 1) * MathF.PI * 2 / num) * antifactor * (1102 * Ease.QuadInOut(1 - Percent)))
                , 0);
            verts[index++].Position = new Vector3(
                new Vector2(
                    Engine.Width / 2,
                    Engine.Height / 2)
                , 0);
        }

        GFX.DrawVertices(Matrix.Identity, verts, verts.Length);
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
