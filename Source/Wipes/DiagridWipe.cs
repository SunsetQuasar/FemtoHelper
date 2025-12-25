using Microsoft.Xna.Framework.Graphics;
using System;

namespace Celeste.Mod.FemtoHelper.Wipes;

public class DiagridWipe : ScreenWipe
{
    private struct Circle
    {
        public Vector2 Position;

        public float Radius;

        public float Delay;
    }

    private readonly int circleColumns = 15;

    private readonly int circleRows = 8;

    private const int CircleSegments = 4;

    private const float CircleFillSpeed = 800f;

    private static Circle[] _circles;

    private static VertexPositionColor[] _vertexBuffer;

    private bool hasDrawn;

    public static readonly BlendState SubtractBlendmode = new BlendState
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    public DiagridWipe(Scene scene, bool wipeIn, Action onComplete = null)
        : base(scene, wipeIn, onComplete)
    {
        _vertexBuffer ??= new VertexPositionColor[(circleColumns + 2) * (circleRows + 2) * CircleSegments * 3];
        _circles ??= new Circle[(circleColumns + 2) * (circleRows + 2)];
        for (int i = 0; i < _vertexBuffer.Length; i++)
        {
            _vertexBuffer[i].Color = WipeIn ? Color.Black : Color.White;
        }
        int num = 1920 / circleColumns;
        int num2 = 1080 / circleRows;
        int num3 = 0;
        for (int j = 0; j < circleColumns + 2; j++)
        {
            for (int k = 0; k < circleRows + 2; k++)
            {
                _circles[num3].Position = new Vector2((j - 1 + 0.2f) * num, (k - 1 + 0.2f) * num2);
                _circles[num3].Delay = (j + (circleRows - k)) * 0.012f;
                _circles[num3].Radius = 0f;
                num3++;
            }
        }
    }
    public override void BeforeRender(Scene scene)
    {
        hasDrawn = true;
        Engine.Graphics.GraphicsDevice.SetRenderTarget(Celeste.WipeTarget);
        Engine.Graphics.GraphicsDevice.Clear(WipeIn ? Color.White : Color.Black);

        int num = 0;
        foreach (var circle in _circles)
        {
            Vector2 vector = new Vector2(1f, 0f);
            for (float num2 = 0f; num2 < CircleSegments; num2 += 1f)
            {
                Vector2 vector2 = Calc.AngleToVector((num2 + 1f) / CircleSegments * ((float)Math.PI * 2f), 1f);
                _vertexBuffer[num++].Position = new Vector3(circle.Position, 0f);
                _vertexBuffer[num++].Position = new Vector3(circle.Position + vector * circle.Radius, 0f);
                _vertexBuffer[num++].Position = new Vector3(circle.Position + vector2 * circle.Radius, 0f);
                vector = vector2;
            }
        }
        GFX.DrawVertices(Matrix.Identity, _vertexBuffer, _vertexBuffer.Length);
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);
        for (int i = 0; i < _circles.Length; i++)
        {
            _circles[i].Delay -= Engine.RawDeltaTime;
            if (_circles[i].Delay <= 0f)
            {
                _circles[i].Radius += Engine.RawDeltaTime * CircleFillSpeed;
            }
        }
    }

    public override void Render(Scene scene)
    {
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, SubtractBlendmode, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
        if ((WipeIn && Percent <= 0.01f) || (!WipeIn && Percent >= 0.99f))
        {
            Draw.Rect(-1f, -1f, 1922f, 1082f, Color.White);
        }
        if (hasDrawn)
        {
            Draw.SpriteBatch.Draw((RenderTarget2D)Celeste.WipeTarget, new Vector2(-1f, -1f), Color.White);
        }
        Draw.SpriteBatch.End();
    }
}
