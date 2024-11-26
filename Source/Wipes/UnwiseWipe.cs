using Microsoft.Xna.Framework.Graphics;
using System;

namespace Celeste.Mod.FemtoHelper.Wipes;
public class UnwiseWipe : ScreenWipe
{
    MTexture texture;

    public UnwiseWipe(Scene scene, bool wipeIn, Action onComplete = null) : base(scene, wipeIn, onComplete)
    {
        texture = GFX.Gui["FemtoHelper/wipes/unwise"];
    }

    public override void Render(Scene scene)
    {
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
        base.Render(scene);

        Vector2 pos;
        pos.X = 0;
        pos.Y = WipeIn ? -1 + ((Engine.Height+2) * Ease.CubeInOut(Percent)) : -Engine.Height - 1 + ((Engine.Height + 2) * Ease.CubeInOut(Percent));

        texture.Draw(pos);
        Draw.SpriteBatch.End();
    }
}
