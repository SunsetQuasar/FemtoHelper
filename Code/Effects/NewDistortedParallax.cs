using Celeste.Mod.Backdrops;
using Celeste.Mod.UI;
using FMOD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Code.Effects;

[CustomBackdrop("FemtoHelper/NewDistortedParallax")]
public class NewDistortedParallax : Backdrop
{

    private static BlendState Multiply = new BlendState
    {
        ColorBlendFunction = BlendFunction.Add,
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.Zero
    };

    private static BlendState ReverseSubtract = new BlendState
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.Subtract,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    public static readonly BlendState Subtract = new BlendState
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    public VirtualRenderTarget buffer;

    public string effectID;

    public Effect effect;

    public bool initialized;

    public Texture2D texture;

    public float Time;

    public bool DoFadeIn;
    public float fadeIn = 1f;

    public Vector4 Amplitudes;
    public Vector4 Periods;
    public Vector4 Offsets;
    public Vector4 Speeds;

    public Vector4 ScaleInfo;
    public Vector2 RotationInfo;

    public BlendState blend;
    public SamplerState filter;

    public bool waveFix;

    public NewDistortedParallax(BinaryPacker.Element data) : base()
    {
        Position = new Vector2(data.AttrFloat("offsetX", 0f), data.AttrFloat("offsetY", 0f));

        buffer = VirtualContent.CreateRenderTarget("distortedParallax", 320, 180);
        texture = GFX.Game[data.Attr("texture", "bgs/disperse_clouds")].GetPaddedSubtextureCopy(); //TODO: rewrite the base shader to support textures from atlases rather than doing this
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

        waveFix = data.AttrBool("waveRotationFix", true);
        effectID = data.Attr("shaderPath", "FemtoHelper/DistortedParallax");

        switch (data.Attr("filterMode", "point"))
        {
            default:
                filter = SamplerState.PointWrap;
                break;
            case "linear":
                filter = SamplerState.LinearWrap;
                break;
            case "anisotropic":
                filter = SamplerState.AnisotropicWrap;
                break;
        }

        switch (data.Attr("blendState"))
        {
            default:
                blend = BlendState.AlphaBlend;
                break;
            case "additive":
                blend = BlendState.Additive;
                break;
            case "subtract":
                blend = Subtract;
                break;
            case "reversesubtract":
                blend = ReverseSubtract;
                break;
            case "multiply":
                blend = Multiply;
                break;
        }

        effect = null;
        initialized = false;
        Time = 0;
        Reset();
    }

    public void Reset()
    {
        if (Everest.Content.TryGet($"Effects/{effectID}.cso", out var effectAsset, true))
        {
            effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
        }
        else
        {
            Logger.Log(LogLevel.Error, "FemtoHelper/NewDistortedParallax", $"Failed getting effect: \"Effects/{effectID}.cso\"! Using default shader instead.");
            if(Everest.Content.TryGet($"Effects/FemtoHelper/DistortedParallax.cso", out var effectAsset2, true))
            {
                effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset2.Data);
            }
            else
            {
                Logger.Log(LogLevel.Error, "FemtoHelper/NewDistortedParallax", "Failed getting the default shader?? How??");
            }
        }
        EffectParameterCollection parameters = effect.Parameters;
        parameters["textureSize"]?.SetValue(new Vector2(texture.Width, texture.Height));
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
        parameters["waveFix"]?.SetValue(waveFix);
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);

        Time += Engine.DeltaTime;

        if (DoFadeIn)
        {
            fadeIn = Calc.Approach(fadeIn, Visible ? 1 : 0, Engine.DeltaTime);
        }
        else
        {
            fadeIn = (Visible ? 1 : 0);
        }

    }

    public override void BeforeRender(Scene scene)
    {
        base.BeforeRender(scene);

        GraphicsDevice graphicsDevice = Draw.SpriteBatch.GraphicsDevice;

        BlendState blend = graphicsDevice.BlendState;

        if(buffer == null) buffer = VirtualContent.CreateRenderTarget("distortedParallax", 320, 180);

        Level level = (scene as Level);

        Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
        Matrix halfPixelOffset = (typeof(Game).Assembly.FullName.Contains("FNA")) ? Matrix.Identity : Matrix.CreateTranslation(-0.5f, -0.5f, 0f);

        Color col = Color * FadeAlphaMultiplier * fadeIn;

        if (FadeX != null)
        {
            col *= FadeX.Value(level.Camera.Position.X + 160f);
        }
        if (FadeY != null)
        {
            col *= FadeY.Value(level.Camera.Position.Y + 90f);
        }

        EffectParameterCollection parameters = effect.Parameters;
        parameters["tint"]?.SetValue(col.ToVector4());
        parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
        parameters["Time"]?.SetValue(Time);
        parameters["CamPos"]?.SetValue(level.Camera.Position);
        parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);
        parameters["ViewMatrix"]?.SetValue(Matrix.Identity);

        graphicsDevice.SetRenderTarget(buffer);
        graphicsDevice.Clear(Color.Transparent);

    }

    public override void Render(Scene scene)
    {
        GraphicsDevice graphicsDevice = Draw.SpriteBatch.GraphicsDevice;

        //base.Render(scene);
        BlendState prevBlendState = graphicsDevice.BlendState;

        Renderer.EndSpritebatch();

        graphicsDevice.Textures[2] = texture; // skin mod helper: your greed is ruining the economy
        graphicsDevice.SamplerStates[2] = filter;

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, Matrix.Identity);

        Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Vector2.Zero, Color.White);

        Draw.SpriteBatch.End();
        Renderer.StartSpritebatch(prevBlendState);
    }
}

