using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.FemtoHelper.Entities;

public class WaterSprite : GraphicsComponent
{
    public const int Padding = 4;

    public VirtualRenderTarget Buffer;

    public readonly MTexture[,] NinSlice;

    public readonly Effect Effect;

    public float Timer = 0f;
    public readonly float Seed;

    public Wiggler Wiggle;
    public Vector2 ExtraSize;

    public bool Drawn;
    public WaterSprite(string path = "objects/FemtoHelper/moveWater/nineSlice") : base(true)
    {
        if (Everest.Content.TryGet($"Effects/FemtoHelper/WaterSprite.cso", out var effectAsset, true))
        {
            Effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
        }
        NinSlice = new MTexture[4, 4];
        MTexture nine = GFX.Game[path];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                NinSlice[i, j] = nine.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }
        Seed = Calc.Random.NextFloat(100);
    }

    public override void Added(Entity entity)
    {
        base.Added(entity);
        entity.Add(new BeforeRenderHook(BeforeRender));

        Buffer = VirtualContent.CreateRenderTarget("water", (int)Entity.Width + Padding * 2, (int)Entity.Height + Padding * 2);

        entity.Add(Wiggle = Wiggler.Create(0.5f, 3f, (t) =>
        {
            ExtraSize = new Vector2(-4 / (Entity.Width + Padding * 2), 4 / (Entity.Height + Padding * 2)) * t;
        }));
    }

    public override void Update()
    {
        base.Update();
        Timer += Engine.DeltaTime;
    }

    public void BeforeRender()
    {
        if (Drawn) return;
        Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);

        Vector2 size = new Vector2(Entity.Width, Entity.Height);

        Color color = Color.White;
        Vector2 pos = Vector2.One * Padding;
        //int num = (int)(size.X / 8f);
        //int num2 = (int)(size.Y / 8f);
        //NinSlice[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
        //NinSlice[2, 0].Draw(pos + new Vector2(size.X - 8f, 0f), Vector2.Zero, color);
        //NinSlice[0, 2].Draw(pos + new Vector2(0f, size.Y - 8f), Vector2.Zero, color);
        //NinSlice[2, 2].Draw(pos + new Vector2(size.X - 8f, size.Y - 8f), Vector2.Zero, color);
        //for (int i = 1; i < num - 1; i++)
        //{
        //    NinSlice[1, 0].Draw(pos + new Vector2(i * 8, 0f), Vector2.Zero, color);
        //    NinSlice[1, 2].Draw(pos + new Vector2(i * 8, size.Y - 8f), Vector2.Zero, color);
        //}
        //for (int j = 1; j < num2 - 1; j++)
        //{
        //    NinSlice[0, 1].Draw(pos + new Vector2(0f, j * 8), Vector2.Zero, color);
        //    NinSlice[2, 1].Draw(pos + new Vector2(size.X - 8f, j * 8), Vector2.Zero, color);
        //}
        //for (int k = 1; k < num - 1; k++)
        //{
        //    for (int l = 1; l < num2 - 1; l++)
        //    {
        //        NinSlice[1, 1].Draw(pos + new Vector2(k, l) * 8f, Vector2.Zero, color);
        //    }
        //}

        int num = NinSlice.GetLength(0);
        int num2 = NinSlice.GetLength(1);
        for (int i = 0; (float)i < Entity.Width; i += 8)
        {
            for (int j = 0; (float)j < Entity.Height; j += 8)
            {
                int num3 = ((i != 0) ? ((!((float)i >= Entity.Width - 8f)) ? Calc.Random.Next(1, num - 1) : (num - 1)) : 0);
                int num4 = ((j != 0) ? ((!((float)j >= Entity.Height - 8f)) ? Calc.Random.Next(1, num2 - 1) : (num2 - 1)) : 0);
                NinSlice[num3, num4].Draw(pos + new Vector2(i, j), Vector2.Zero, color);
            }
        }

        Drawn = true;

        Draw.SpriteBatch.End();

        Engine.Graphics.GraphicsDevice.SetRenderTarget(null);
    }

    public override void Render()
    {
        base.Render();

        Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
        EffectParameterCollection parameters = Effect.Parameters;
        parameters["TransformMatrix"]?.SetValue(Matrix.Identity * projection);
        parameters["ViewMatrix"]?.SetValue(GameplayRenderer.instance.Camera.Matrix);
        parameters["Time"]?.SetValue(Timer);
        parameters["Seed"]?.SetValue(Seed);
        parameters["BlockSize"]?.SetValue(new Vector2(Entity.Width, Entity.Height));

        Texture textemp = Engine.Graphics.GraphicsDevice.Textures[3];
        SamplerState samptemp = Engine.Graphics.GraphicsDevice.SamplerStates[3];
        Engine.Graphics.GraphicsDevice.Textures[3] = Buffer;
        Engine.Graphics.GraphicsDevice.SamplerStates[3] = SamplerState.PointClamp;

        Color color = new Color(Color.White, 1)
        {
            A = 200
        };

        GameplayRenderer.End();

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, Effect, GameplayRenderer.instance.Camera.Matrix);

        Draw.SpriteBatch.Draw(Buffer, RenderPosition - ExtraSize * new Vector2(Buffer.Width, Buffer.Height) - Vector2.One * Padding, null, color, 0f, Vector2.Zero, Vector2.One + ExtraSize * 2, SpriteEffects.None, 0f);

        Draw.SpriteBatch.End();

        GameplayRenderer.Begin();

        Engine.Graphics.GraphicsDevice.Textures[3] = textemp;
        Engine.Graphics.GraphicsDevice.SamplerStates[3] = samptemp;
    }
}
