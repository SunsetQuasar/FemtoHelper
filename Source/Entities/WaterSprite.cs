using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.FemtoHelper.Entities;

public class WaterSprite : GraphicsComponent
{
    public const int Padding = 4;

    public VirtualRenderTarget buffer;

    public MTexture[,] ninSlice;

    public Effect effect;

    public float timer = 0f;
    public float seed;

    public Wiggler wiggle;
    public Vector2 extraSize;
    public WaterSprite() : base(true)
    {
        if (Everest.Content.TryGet($"Effects/FemtoHelper/WaterSprite.cso", out var effectAsset, true))
        {
            effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
        }
        ninSlice = new MTexture[3, 3];
        MTexture nine = GFX.Game["objects/FemtoHelper/moveWater/nineSlice"];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                ninSlice[i, j] = nine.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }
        seed = Calc.Random.NextFloat(100);
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        entity.Add(new BeforeRenderHook(BeforeRender));

        buffer = VirtualContent.CreateRenderTarget("water", (int)Entity.Width + Padding * 2, (int)Entity.Height + Padding * 2);

        entity.Add(wiggle = Wiggler.Create(0.5f, 3f, (t) =>
        {
            extraSize = new Vector2(-4 / (Entity.Width + Padding * 2), 4 / (Entity.Height + Padding * 2)) * t;
        }));
    }

    public override void Update()
    {
        base.Update();
        timer += Engine.DeltaTime;
    }

    public void BeforeRender()
    {
        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);

        Vector2 size = new Vector2(Entity.Width, Entity.Height);

        Color color = Color.White;
        Vector2 pos = (Vector2.One * Padding);
        int num = (int)(size.X / 8f);
        int num2 = (int)(size.Y / 8f);
        ninSlice[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
        ninSlice[2, 0].Draw(pos + new Vector2(size.X - 8f, 0f), Vector2.Zero, color);
        ninSlice[0, 2].Draw(pos + new Vector2(0f, size.Y - 8f), Vector2.Zero, color);
        ninSlice[2, 2].Draw(pos + new Vector2(size.X - 8f, size.Y - 8f), Vector2.Zero, color);
        for (int i = 1; i < num - 1; i++)
        {
            ninSlice[1, 0].Draw(pos + new Vector2(i * 8, 0f), Vector2.Zero, color);
            ninSlice[1, 2].Draw(pos + new Vector2(i * 8, size.Y - 8f), Vector2.Zero, color);
        }
        for (int j = 1; j < num2 - 1; j++)
        {
            ninSlice[0, 1].Draw(pos + new Vector2(0f, j * 8), Vector2.Zero, color);
            ninSlice[2, 1].Draw(pos + new Vector2(size.X - 8f, j * 8), Vector2.Zero, color);
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

        Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
        EffectParameterCollection parameters = effect.Parameters;
        parameters["TransformMatrix"]?.SetValue(Matrix.Identity * projection);
        parameters["ViewMatrix"]?.SetValue(GameplayRenderer.instance.Camera.Matrix);
        parameters["Time"]?.SetValue(timer);
        parameters["Seed"]?.SetValue(seed);
        parameters["BlockSize"]?.SetValue(new Vector2(Entity.Width, Entity.Height));

        Texture Textemp = Engine.Graphics.GraphicsDevice.Textures[3];
        SamplerState Samptemp = Engine.Graphics.GraphicsDevice.SamplerStates[3];
        Engine.Graphics.GraphicsDevice.Textures[3] = buffer;
        Engine.Graphics.GraphicsDevice.SamplerStates[3] = SamplerState.PointClamp;

        Color color = new Color(Color.White, 1)
        {
            A = 200
        };

        GameplayRenderer.End();

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, GameplayRenderer.instance.Camera.Matrix);

        Draw.SpriteBatch.Draw(buffer, RenderPosition - (extraSize * new Vector2(buffer.Width, buffer.Height)) - (Vector2.One * Padding), null, color, 0f, Vector2.Zero, Vector2.One + (extraSize * 2), SpriteEffects.None, 0f);

        Draw.SpriteBatch.End();

        GameplayRenderer.Begin();

        Engine.Graphics.GraphicsDevice.Textures[3] = Textemp;
        Engine.Graphics.GraphicsDevice.SamplerStates[3] = Samptemp;
    }
}
