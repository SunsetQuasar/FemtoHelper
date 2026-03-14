// Celeste.DreamStars
using System;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;

public class PolygonStars : Backdrop
{
    private struct Stars
    {
        public Vector2 Position;

        public float Speed;

        public float Size;

        public float Rotation;

        public float RotationSpeed;

        public Color Color;
    }

    private readonly Stars[] stars;

    private readonly Vector2 angle;

    private Vector2 lastCamera = Vector2.Zero;

    private readonly int sideCount;

    private readonly float pointinessMultiplier;

    private readonly float loopBorder;

    private readonly Color[] colors;

    private readonly float alpha;

    private readonly float scroll;

    private readonly bool filled;

    public PolygonStars(int sides, float pointiness, float minRotation, float maxRotation, float minSize, float maxSize, float border, string color, float angle, float alpha, float minSpeed, float maxSpeed, int amount, float scroll, bool filled)
    {
        sideCount = Math.Max(sides, 0);
        pointinessMultiplier = pointiness;
        loopBorder = border;
        this.alpha = alpha;
        this.scroll = scroll;
        stars = new Stars[amount];
        colors = color
                .Split(',')
                .Select(str => Calc.HexToColor(str.Trim()) * alpha)
                .ToArray();
        this.angle = new Vector2((float)Math.Sin(angle / 180 * Math.PI), (float)Math.Cos(angle / 180 * Math.PI));
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].Position = new Vector2(Calc.Random.NextFloat(320f + loopBorder) - loopBorder / 2, Calc.Random.NextFloat(180f + loopBorder) - loopBorder / 2);
            stars[i].Speed = Calc.Random.Range(minSpeed, maxSpeed);
            stars[i].Size = Calc.Random.Range(minSize, maxSize);
            stars[i].Rotation = Calc.Random.NextFloat((float)Math.PI * 2);
            stars[i].RotationSpeed = Calc.Random.Range(minRotation, maxRotation);
            stars[i].Color = colors[Calc.Random.Next(colors.Length)];
        }

        this.filled = filled;
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);
        Vector2 position = (scene as Level).Camera.Position;
        Vector2 value = position - lastCamera;
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].Position += angle * stars[i].Speed * Engine.DeltaTime - value * scroll;
            stars[i].Rotation += stars[i].RotationSpeed * Engine.DeltaTime;
        }
        lastCamera = position;
    }

    public override void Render(Scene scene)
    {
        if (filled)
        {
            VertexPositionColor[] verts = new VertexPositionColor[stars.Length * sideCount * 3];

            DynamicData spriteBatchData = DynamicData.For(Draw.SpriteBatch);
            BlendState blend = spriteBatchData.Get<BlendState>("blendState") ?? BlendState.AlphaBlend;

            int index = 0;
            for (int i = 0; i < stars.Length; i++)
            {
                Color color = stars[i].Color * alpha;
                Vector2 center = new(Mod(stars[i].Position.X, 320f + loopBorder) - loopBorder / 2, Mod(stars[i].Position.Y, 180f + loopBorder) - loopBorder / 2);

                for (int j = 0; j < sideCount; j++)
                {
                    Vector2 size = j % 2 == 0 ? new Vector2(stars[i].Size, stars[i].Size) : new Vector2(stars[i].Size * pointinessMultiplier, stars[i].Size * pointinessMultiplier);
                    Vector2 size2 = j % 2 == 0 ? new Vector2(stars[i].Size * pointinessMultiplier, stars[i].Size * pointinessMultiplier) : new Vector2(stars[i].Size, stars[i].Size);

                    verts[index++] = new(new Vector3(center, 0), color);
                    verts[index++] = new(new Vector3(center + new Vector2((float)Math.Sin(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * j), (float)Math.Cos(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * j)) * size, 0), color);
                    verts[index++] = new(new Vector3(center + new Vector2((float)Math.Sin(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * (j + 1)), (float)Math.Cos(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * (j + 1))) * size2, 0), color);
                }
            }

            Renderer.EndSpritebatch();

            GFX.DrawVertices(Renderer.Matrix, verts, verts.Length, null, blend);

            Renderer.StartSpritebatch(blend);
        }
        else
        {
            for (int i = 0; i < stars.Length; i++)
            {
                Color color = stars[i].Color * alpha;
                Vector2 center = new(Mod(stars[i].Position.X, 320f + loopBorder) - loopBorder / 2, Mod(stars[i].Position.Y, 180f + loopBorder) - loopBorder / 2);

                //Draw.HollowRect(new Vector2(Mod(stars[i].Position.X, 320f), Mod(stars[i].Position.Y, 180f)), stars[i].Size, stars[i].Size, Color.Teal);
                for (int j = 0; j < sideCount; j++)
                {
                    Vector2 size = j % 2 == 0 ? new Vector2(stars[i].Size, stars[i].Size) : new Vector2(stars[i].Size * pointinessMultiplier, stars[i].Size * pointinessMultiplier);
                    Vector2 size2 = j % 2 == 0 ? new Vector2(stars[i].Size * pointinessMultiplier, stars[i].Size * pointinessMultiplier) : new Vector2(stars[i].Size, stars[i].Size);
                    Draw.Line(
                        center + new Vector2((float)Math.Sin(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * j), (float)Math.Cos(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * j)) * size,
                        center + new Vector2((float)Math.Sin(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * (j + 1)), (float)Math.Cos(stars[i].Rotation + Math.PI / ((float)sideCount / 2) * (j + 1))) * size2, 
                        color);
                }
            }
        }
    }

    private float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
