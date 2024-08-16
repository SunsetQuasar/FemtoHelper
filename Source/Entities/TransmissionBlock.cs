
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Pico8;
using Celeste.Mod.FemtoHelper;
using System;

namespace Celeste.Mod.FemtoHelper.Entities
{
    [CustomEntity("FemtoHelper/TransmissionBlock")]
    public class TransmissionBlock : Solid
    {
        public Vector2[] nodes;

        public MTexture[,] nineSlice;

        public MTexture[,] nineSliceOutline;

        public MTexture nodeTexture;

        public Color color;

        public float timer;

        public int currentNode;

        public float rectOpacity;

        public float outlineOpacity;

        public Vector2 rectSize;

        public Vector2 scale = Vector2.One;

        public TransmissionBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
        {
            MTexture tex = GFX.Game[data.Attr("texture", "objects/FemtoHelper/transmissionBlock/") + "block"];
            MTexture texOutline = GFX.Game[data.Attr("texture", "objects/FemtoHelper/transmissionBlock/") + "outline"];

            nineSlice = new MTexture[3, 3];
            nineSliceOutline = new MTexture[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    nineSlice[i, j] = tex.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                    nineSliceOutline[i, j] = texOutline.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }

            Vector2[] n = data.NodesOffset(offset);

            this.nodes = new Vector2[n.Length + 1];
            this.nodes[0] = Position;
            for (int i = 0; i < n.Length; i++)
            {
                this.nodes[i + 1] = n[i];
            }

            color = Color.White;

            nodeTexture = GFX.Game[data.Attr("texture", "objects/FemtoHelper/transmissionBlock/") + "node"];
            Add(new LightOcclude(0.5f));

            timer = Calc.Random.Range(0, (float)Math.PI * 2);

            currentNode = 0;

            OnDashCollide = OnDashed;

            rectOpacity = 0;
            outlineOpacity = 0;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (StaticMover staticMover in staticMovers)
            {
                staticMover.Entity.Depth = base.Depth + 1;
                Spikes spikes = staticMover.Entity as Spikes;
                if (spikes != null)
                {
                    spikes.VisibleWhenDisabled = true;
                }
                Spring spring = staticMover.Entity as Spring;
                if (spring != null)
                {
                    spring.VisibleWhenDisabled = true;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime;
        }

        public void DrawBlock(Vector2 xy, MTexture[,] ninesl, bool shake)
        {
            Vector2 pos = shake ? (xy + base.Shake) : xy;
            int num = (int)(Width / 8f);
            int num2 = (int)(Height / 8f);
            ninesl[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
            ninesl[2, 0].Draw(pos + new Vector2(Width - 8f, 0f), Vector2.Zero, color);
            ninesl[0, 2].Draw(pos + new Vector2(0f, Height - 8f), Vector2.Zero, color);
            ninesl[2, 2].Draw(pos + new Vector2(Width - 8f, Height - 8f), Vector2.Zero, color);
            for (int i = 1; i < num - 1; i++)
            {
                ninesl[1, 0].Draw(pos + new Vector2(i * 8, 0f), Vector2.Zero, color);
                ninesl[1, 2].Draw(pos + new Vector2(i * 8, Height - 8f), Vector2.Zero, color);
            }
            for (int j = 1; j < num2 - 1; j++)
            {
                ninesl[0, 1].Draw(pos + new Vector2(0f, j * 8), Vector2.Zero, color);
                ninesl[2, 1].Draw(pos + new Vector2(Width - 8f, j * 8), Vector2.Zero, color);
            }
            for (int k = 1; k < num - 1; k++)
            {
                for (int l = 1; l < num2 - 1; l++)
                {
                    ninesl[1, 1].Draw(pos + new Vector2(k, l) * 8f, Vector2.Zero, color);
                }
            }
        }

        public override void Render()
        {
            ActiveFont.Draw("dont use this", Position, Color.Red);

            base.Render();

            Vector2 centerOffset = new Vector2(Width / 2, Height / 2);

            for(int i = 0; i < nodes.Length; i++)
            {
                if(i != nodes.Length - 1)
                {

                    Draw.Line(nodes[i] + centerOffset, nodes[i + 1] + centerOffset, Color.White * 0.3f, (float)(Math.Sin(timer * 5) * 1.5f) + 1.5f);
                }
                
                nodeTexture.DrawOutlineCentered(nodes[i] + centerOffset + (Vector2.UnitY * (float)Math.Sin(timer * 4) * 2));
                nodeTexture.DrawCentered(nodes[i] + centerOffset + (Vector2.UnitY * (float)Math.Sin(timer * 4) * 2));
                if (i == nodes.Length - 1)
                {
                    DrawBlock(nodes[i], nineSliceOutline, false);
                }

            }

            DrawBlock(Position, nineSlice, true);
            Draw.Rect(Position - new Vector2(rectSize.X / 2, rectSize.Y / 2), rectSize.X, rectSize.Y, Color.White * rectOpacity);
        }

        public DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            StartShaking(0.2f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            Add(new Coroutine(Sequence()));
            return direction.Y != -1 ? DashCollisionResults.Rebound : DashCollisionResults.Bounce;
        }

        public IEnumerator Sequence()
        {
            Collidable = false;
            Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.2f, true);
            tween2.OnUpdate = delegate (Tween t)
            {
                float yyy = MathHelper.Lerp(Y, Y + 4, t.Eased);
                MoveTo(new Vector2(X, yyy));
                color = Color.Lerp(Color.White * 1f, Color.White * 0.25f, t.Eased);
                rectOpacity = 1f;
                rectSize = new Vector2(MathHelper.Lerp(Width, 4, t.Eased), MathHelper.Lerp(Height, 4, t.Eased));
                foreach (StaticMover staticMover in staticMovers)
                {
                    staticMover.Disable();
                    Spikes spikes = staticMover.Entity as Spikes;
                    if (spikes != null)
                    {
                        spikes.DisabledColor = color;
                    }
                    Spring spring = staticMover.Entity as Spring;
                    if (spring != null)
                    {
                        spikes.DisabledColor = color;
                    }
                }
                Add(tween2);
            };
            yield return 0.2f;
            if(currentNode == nodes.Length - 1){
                for (int i = nodes.Length - 1; i > 0; i--)
                {
                    currentNode = i;
                    Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.5f, true);
                    tween.OnUpdate = delegate (Tween t)
                    {
                        float xX = MathHelper.Lerp(nodes[i].X, nodes[i - 1].X, t.Eased);
                        float yY = MathHelper.Lerp(nodes[i].Y + 4, nodes[i - 1].Y + 4, t.Eased);
                        MoveTo(new Vector2(xX, yY));
                    };
                    Add(tween);
                    yield return 0.5f;
                    currentNode = i - 1;
                    tween.RemoveSelf();
                }
            } 
            else
            {
                for (int i = 0; i < nodes.Length - 1; i++)
                {
                    currentNode = i;
                    Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.5f, true);
                    tween.OnUpdate = delegate (Tween t)
                    {
                        float xX = MathHelper.Lerp(nodes[i].X, nodes[i + 1].X, t.Eased);
                        float yY = MathHelper.Lerp(nodes[i].Y + 4, nodes[i + 1].Y + 4, t.Eased);
                        MoveTo(new Vector2(xX, yY));
                    };
                    Add(tween);
                    yield return 0.5f;
                    currentNode = i + 1;
                    tween.RemoveSelf();
                }
            }
            Collidable = true;
        }
    }
}
