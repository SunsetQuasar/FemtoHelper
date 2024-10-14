using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Wipes;
public class DissolveWipe : ScreenWipe
{

    private bool hasDrawn;

    public Effect effect;

    public float RandomVal;

    public DissolveWipe(Scene scene, bool wipeIn, Action onComplete = null)
        : base(scene, wipeIn, onComplete)
    {
        if (Everest.Content.TryGet($"Effects/FemtoHelper/DissolveWipe.cso", out var effectAsset, true))
        {
            effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
        }
        Calc.PushRandom((int)Engine.Scene.RawTimeActive);
        RandomVal = Calc.Random.NextFloat();
        Calc.PopRandom();
        
        EffectParameterCollection parameters = effect.Parameters;
        parameters["RandomValue"]?.SetValue(RandomVal);

    }

    public override void Update(Scene scene)
    {
        base.Update(scene);

    }

    public override void BeforeRender(Scene scene)
    {
        hasDrawn = true;
        Engine.Graphics.GraphicsDevice.SetRenderTarget(Celeste.WipeTarget);
        Engine.Graphics.GraphicsDevice.Clear(Color.Black);

        Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
        EffectParameterCollection parameters = effect.Parameters;
        parameters["TransformMatrix"]?.SetValue(Matrix.Identity * projection);
        parameters["ViewMatrix"]?.SetValue(Matrix.Identity);
        parameters["Percent"]?.SetValue(Ease.SineInOut(WipeIn ? 1 - Percent : Percent));
    }

    public override void Render(Scene scene)
    {
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, effect, Engine.ScreenMatrix);
        if (hasDrawn)
        {
            Draw.SpriteBatch.Draw((RenderTarget2D)Celeste.WipeTarget, new Vector2(-0f, -0f), Color.White);
        }
        Draw.SpriteBatch.End();

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
        if ((WipeIn && Percent <= 0.05f) || (!WipeIn && Percent >= 0.95f))
        {
            Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black);
        }
        Draw.SpriteBatch.End();
    }
}
