using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;
public class WaterSprite : GraphicsComponent
{
    public const int Padding = 4;

    public VirtualRenderTarget buffer;

    public MTexture[,] ninSlice;
    public WaterSprite() : base(true)
    {
        ninSlice = new MTexture[3, 3];
        MTexture nine = GFX.Game["objects/FemtoHelper/moveWater/nineSlice"];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                ninSlice[i, j] = nine.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        entity.Add(new BeforeRenderHook(BeforeRender));

        buffer = VirtualContent.CreateRenderTarget("water", (int)Entity.Width + Padding * 2, (int)Entity.Height + Padding * 2);
    }

    public void BeforeRender()
    {
        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);

        Color color = Color.White;
        Vector2 pos = (Vector2.One * Padding);
        int num = (int)(Entity.Width / 8f);
        int num2 = (int)(Entity.Height / 8f);
        ninSlice[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
        ninSlice[2, 0].Draw(pos + new Vector2(Entity.Width - 8f, 0f), Vector2.Zero, color);
        ninSlice[0, 2].Draw(pos + new Vector2(0f, Entity.Height - 8f), Vector2.Zero, color);
        ninSlice[2, 2].Draw(pos + new Vector2(Entity.Width - 8f, Entity.Height - 8f), Vector2.Zero, color);
        for (int i = 1; i < num - 1; i++)
        {
            ninSlice[1, 0].Draw(pos + new Vector2(i * 8, 0f), Vector2.Zero, color);
            ninSlice[1, 2].Draw(pos + new Vector2(i * 8, Entity.Height - 8f), Vector2.Zero, color);
        }
        for (int j = 1; j < num2 - 1; j++)
        {
            ninSlice[0, 1].Draw(pos + new Vector2(0f, j * 8), Vector2.Zero, color);
            ninSlice[2, 1].Draw(pos + new Vector2(Entity.Width - 8f, j * 8), Vector2.Zero, color);
        }
        for (int k = 1; k < num - 1; k++)
        {
            for (int l = 1; l < num2 - 1; l++)
            {
                ninSlice[1, 1].Draw(pos + new Vector2(k, l) * 8f, Vector2.Zero, color);
            }
        }

        Draw.SpriteBatch.End();

        Engine.Graphics.GraphicsDevice.SetRenderTarget(null);
    }

    public override void Render()
    {
        base.Render();

        Color color = new Color(Color.White, 1)
        {
            A = 200
        };

        GameplayRenderer.End();

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, GameplayRenderer.instance.Camera.Matrix);

        Draw.SpriteBatch.Draw(buffer, RenderPosition - (Vector2.One * Padding), color);

        Draw.SpriteBatch.End();

        GameplayRenderer.Begin();
    }
}
