using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.FemtoHelper.Code.Effects;

[CustomBackdrop("FemtoHelper/NewDistortedParallax")]
public class NewDistortedParallax : Backdrop
{

    private static readonly BlendState Multiply = new BlendState
    {
        ColorBlendFunction = BlendFunction.Add,
        ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.DestinationColor,
        ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.Zero
    };

    private static readonly BlendState ReverseSubtract = new BlendState
    {
        ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
        ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
        ColorBlendFunction = BlendFunction.Subtract,
        AlphaSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
        AlphaDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    public static readonly BlendState Subtract = new BlendState
    {
        ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
        ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
        AlphaDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    public VirtualRenderTarget Buffer;

    public readonly string EffectId;

    public Effect Effect;

    public bool Initialized;

    public readonly Texture2D Texture;

    public float Time;

    public readonly bool DoFadeIn;
    public float FadeIn = 1f;

    public Vector4 Amplitudes;
    public Vector4 Periods;
    public Vector4 Offsets;
    public Vector4 Speeds;

    public Vector4 ScaleInfo;
    public Vector2 RotationInfo;

    public readonly BlendState Blend;
    public readonly SamplerState Filter;

    public readonly bool WaveFix;

    public NewDistortedParallax(BinaryPacker.Element data) : base()
    {
        Position = new Vector2(data.AttrFloat("offsetX", 0f), data.AttrFloat("offsetY", 0f));

        Texture = GFX.Game[data.Attr("texture", "bgs/disperse_clouds")].GetPaddedSubtextureCopy(); //DODO: rewrite the base shader to support textures from atlases rather than doing this
        LoopX = data.AttrBool("loopX", true);
        LoopY = data.AttrBool("loopY", true);
        Scroll = new Vector2(data.AttrFloat("scrollX", 1), data.AttrFloat("scrollY", 1));
        Speed = new Vector2(data.AttrFloat("speedX", 0), data.AttrFloat("speedY", 0));
        Color = Calc.HexToColorWithAlpha(data.Attr("color", "FFFFFFFF")) * data.AttrFloat("alpha", 1f);
        FlipX = data.AttrBool("flipX", false);
        FlipY = data.AttrBool("flipY", false);
        DoFadeIn = data.AttrBool("fadeIn", true);

        Amplitudes = new Vector4(data.AttrFloat("xXAmplitude", 0), data.AttrFloat("xYAmplitude", 10), data.AttrFloat("yXAmplitude", 0), data.AttrFloat("yYAmplitude", 0));
        Periods = new Vector4(data.AttrFloat("xXPeriod", 40), data.AttrFloat("xYPeriod", 40), data.AttrFloat("yXPeriod", 40), data.AttrFloat("yYPeriod", 40));
        Offsets = new Vector4(data.AttrFloat("xXOffset", 0), data.AttrFloat("xYOffset", 0), data.AttrFloat("yXOffset", 0), data.AttrFloat("yYOffset", 0));
        Speeds = new Vector4(data.AttrFloat("xXWaveSpeed", 0), data.AttrFloat("xYWaveSpeed", 180), data.AttrFloat("yXWaveSpeed", 0), data.AttrFloat("yYWaveSpeed", 0));

        ScaleInfo = new Vector4(data.AttrFloat("scale", 1), data.AttrFloat("scaleAmplitude", 0), data.AttrFloat("scaleSpeed", 0), data.AttrFloat("scaleOffset", 0));
        RotationInfo = new Vector2(data.AttrFloat("rotationSpeed", 0), data.AttrFloat("rotationOffset", 0));

        WaveFix = data.AttrBool("waveRotationFix", true);
        EffectId = data.Attr("shaderPath", "FemtoHelper/DistortedParallax");

        switch (data.Attr("filterMode", "point"))
        {
            default:
                Filter = SamplerState.PointWrap;
                break;
            case "linear":
                Filter = SamplerState.LinearWrap;
                break;
            case "anisotropic":
                Filter = SamplerState.AnisotropicWrap;
                break;
        }

        switch (data.Attr("blendState"))
        {
            default:
                Blend = BlendState.AlphaBlend;
                break;
            case "additive":
                Blend = BlendState.Additive;
                break;
            case "subtract":
                Blend = Subtract;
                break;
            case "reversesubtract":
                Blend = ReverseSubtract;
                break;
            case "multiply":
                Blend = Multiply;
                break;
        }

        Effect = null;
        Initialized = false;
        Time = 0;
        Reset();
    }

    public void Reset()
    {
        if (Everest.Content.TryGet($"Effects/{EffectId}.cso", out var effectAsset, true))
        {
            Effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
        }
        else
        {
            Logger.Log(LogLevel.Error, "FemtoHelper/NewDistortedParallax", $"Failed getting effect: \"Effects/{EffectId}.cso\"! Using default shader instead.");
            if(Everest.Content.TryGet($"Effects/FemtoHelper/DistortedParallax.cso", out var effectAsset2, true))
            {
                Effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset2.Data);
            }
            else
            {
                Logger.Log(LogLevel.Error, "FemtoHelper/NewDistortedParallax", "Failed getting the default shader?? How??");
            }
        }
        EffectParameterCollection parameters = Effect.Parameters;
        parameters["textureSize"]?.SetValue(new Vector2(Texture.Width, Texture.Height));
        parameters["Dimensions"]?.SetValue(new Vector2(320f, 180f));
        parameters["offset"]?.SetValue(Position);
        parameters["loopMul"]?.SetValue(new Vector2(LoopX ? 1 : 0, LoopY ? 1 : 0));
        parameters["Speed"]?.SetValue(Speed);
        parameters["Scroll"]?.SetValue(Scroll);
        parameters["flipInfo"]?.SetValue(new Vector4(FlipX ? -1 : 1, FlipY ? -1 : 1, FlipX ? 1 : 0, FlipY ? 1 : 0));
        parameters["Amplitudes"]?.SetValue(Amplitudes);
        parameters["Periods"]?.SetValue(Periods * (float)(1/Math.PI));
        parameters["Offsets"]?.SetValue(Offsets * Calc.DegToRad);
        parameters["WaveSpeeds"]?.SetValue(Speeds * Calc.DegToRad);
        parameters["ScaleInfo"]?.SetValue(ScaleInfo * new Vector4(1, 1, Calc.DegToRad, Calc.DegToRad));
        parameters["RotInfo"]?.SetValue(RotationInfo * Calc.DegToRad);
        parameters["waveFix"]?.SetValue(WaveFix);
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);

        Time += Engine.DeltaTime;

        if (DoFadeIn)
        {
            FadeIn = Calc.Approach(FadeIn, Visible ? 1 : 0, Engine.DeltaTime);
        }
        else
        {
            FadeIn = Visible ? 1 : 0;
        }

    }
    //stolen from ja
    private VirtualRenderTarget EnsureValidBuffer()
    {
        var gpBuffer = GameplayBuffers.Gameplay;

        int targetWidth = gpBuffer?.Width ?? 320;
        int targetHeight = gpBuffer?.Height ?? 180;

        if (gpBuffer is null || gpBuffer.Width == 320 || gpBuffer.Width == 321)
        {
            Buffer ??= VirtualContent.CreateRenderTarget("FemtoHelper/DistoredParallax", targetWidth, targetHeight);
            return Buffer;
        }

        // We need a bigger buffer due to zoomout.
        // We'll keep the 320x180 buffer around, in case some other cloudscape wants to render with ZoomBehavior=StaySame
        if (Buffer is null || Buffer.IsDisposed || Buffer.Width != gpBuffer.Width)
        {
            Buffer?.Dispose();
            Buffer = VirtualContent.CreateRenderTarget("FemtoHelper/DistoredParallax", targetWidth, targetHeight);
        }

        return Buffer;
    }

    public override void BeforeRender(Scene scene)
    {
        base.BeforeRender(scene);

        GraphicsDevice graphicsDevice = Draw.SpriteBatch.GraphicsDevice;

        BlendState blend = graphicsDevice.BlendState;

        Buffer = EnsureValidBuffer();

        Level level = scene as Level;

        Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
        Matrix halfPixelOffset = typeof(Game).Assembly.FullName.Contains("FNA") ? Matrix.Identity : Matrix.CreateTranslation(-0.5f, -0.5f, 0f);

        Color col = Color * FadeAlphaMultiplier * FadeIn;

        if (FadeX != null)
        {
            col *= FadeX.Value(level.Camera.Position.X + 160f);
        }
        if (FadeY != null)
        {
            col *= FadeY.Value(level.Camera.Position.Y + 90f);
        }

        EffectParameterCollection parameters = Effect.Parameters;
        parameters["Dimensions"]?.SetValue(new Vector2(Buffer.Width, Buffer.Height));
        parameters["tint"]?.SetValue(col.ToVector4());
        parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
        parameters["Time"]?.SetValue(Time);
        parameters["CamPos"]?.SetValue(level.Camera.Position);
        parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);
        parameters["ViewMatrix"]?.SetValue(Matrix.Identity);

        graphicsDevice.SetRenderTarget(Buffer);
        graphicsDevice.Clear(Color.Transparent);

    }

    public override void Render(Scene scene)
    {
        GraphicsDevice graphicsDevice = Draw.SpriteBatch.GraphicsDevice;

        BlendState prevBlendState = graphicsDevice.BlendState;

        Renderer.EndSpritebatch();

        Texture t = graphicsDevice.Textures[2];
        SamplerState s = graphicsDevice.SamplerStates[2];

        graphicsDevice.Textures[2] = Texture; // skin mod helper: your greed is ruining the economy
        graphicsDevice.SamplerStates[2] = Filter;

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, Blend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, Effect, Matrix.Identity);

        base.Render(scene);
        Draw.SpriteBatch.Draw((RenderTarget2D)Buffer, Vector2.Zero, Color.White);

        Draw.SpriteBatch.End();
        Renderer.StartSpritebatch(prevBlendState);

        graphicsDevice.Textures[2] = t;
        graphicsDevice.SamplerStates[2] = s;
    }
}

