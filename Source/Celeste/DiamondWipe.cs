using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Wipes
{
    public class DiamondWipe : ScreenWipe
    {
        public static readonly BlendState SubtractBlendmode = new BlendState
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };

        private bool hasDrawn;

        private VertexPositionColor[] vertex = new VertexPositionColor[6];

        public DiamondWipe(Scene scene, bool wipeIn, Action onComplete = null)
            : base(scene, wipeIn, onComplete)
        {
            for (int i = 0; i < vertex.Length; i++)
            {
                vertex[i].Color = (WipeIn ? Color.Black : Color.White);
            }
        }

        public override void BeforeRender(Scene scene)
        {
            hasDrawn = true;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Celeste.WipeTarget);
            Engine.Graphics.GraphicsDevice.Clear(WipeIn ? Color.White : Color.Black);
            //if (Percent > 0.8f)
            //{
            //    float num = Calc.Map(Percent, 0.8f, 1f) * 1082f;
            //    Draw.SpriteBatch.Begin();
            //    Draw.Rect(-1f, (1080f - num) * 0.5f, 1922f, num, (!WipeIn) ? Color.White : Color.Black);
            //    Draw.SpriteBatch.End();
            //}
            float num2 = ((Percent * Percent) + (Percent * Percent * Percent)) / 2;
            float num3 = WipeIn ? num2 : -num2;
            Vector2 Center = new Vector2(Engine.Width / 2f, Engine.Height / 2f);
            vertex[0].Position = new Vector3(Center.X, Center.Y - (num2 * (Center.X + Center.Y)), 0);
            vertex[1].Position = new Vector3(Center.X - (num2 * (Center.X + Center.Y)), Center.Y, 0);
            vertex[2].Position = new Vector3(Center.X, Center.Y + (num2 * (Center.X + Center.Y)), 0);
            vertex[3].Position = new Vector3(Center.X, Center.Y - (num2 * (Center.X + Center.Y)), 0);
            vertex[4].Position = new Vector3(Center.X + (num2 * (Center.X + Center.Y)), Center.Y, 0);
            vertex[5].Position = new Vector3(Center.X, Center.Y + (num2 * (Center.X + Center.Y)), 0);


            GFX.DrawVertices(Matrix.Identity, vertex, vertex.Length);
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
}

