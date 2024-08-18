// Celeste.Petals
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;

public class WindPetals : Backdrop
{
    public string petalColor = "66cc22";

    public Color[] Colors;

    public float fallSpeedMin = 6f;

    public float fallSpeedMax = 16f;

    public float xDriftSpeedMin = 0f;

    public float xDriftSpeedMax = 0f;

    public int blurCount = 15;

    public float blurDensity = 3;

    public string sprite = "particles/petal";

    public float parallax = 1f;

    public float spinSpeedMultiplier = 1f;

    public float spinAmount = 8f;

    public float alpha;

    public float scale;

    private struct Particle
    {
        public Vector2 Position;

        public float Speed;

        public float SpeedX;

        public float Spin;

        public float MaxRotate;

        public float RotationCounter;

        public Color Color;
    }

    private Particle[] particles = new Particle[40];

    private float fade;

    public WindPetals(string colors, float fallingSpeedMin, float fallingSpeedMax, int blurCount_, float blurDensity_, string texture, int count, float scroll, float spinFrequency, float spinAmplitude, float transparency, float size, float xDriftingSpeedMin, float xDriftingSpeedMax)
    {
        // this is like communal helper!
        Colors = colors
                .Split(',')
                .Select(str => Calc.HexToColorWithAlpha(str.Trim()))
                .ToArray();
        fallSpeedMin = fallingSpeedMin;
        fallSpeedMax = fallingSpeedMax;
        xDriftSpeedMin = xDriftingSpeedMin;
        xDriftSpeedMax = xDriftingSpeedMax;
        blurCount = blurCount_;
        blurDensity = blurDensity_;
        sprite = texture;
        particles = new Particle[count];
        parallax = (float)Math.Max(scroll, 0.00001);
        spinSpeedMultiplier = spinFrequency;
        spinAmount = spinAmplitude;
        alpha = transparency;
        scale = size;
        for (int i = 0; i < particles.Length; i++)
        {
            Reset(i);
        }
    }

    private void Reset(int i)
    {
        particles[i].Position = new Vector2(Calc.Random.Range(0, 352) / parallax, Calc.Random.Range(0, 212) / parallax);
        particles[i].Speed = Calc.Random.Range(fallSpeedMin, fallSpeedMax);
        particles[i].SpeedX = Calc.Random.Range(xDriftSpeedMin, xDriftSpeedMax);
        particles[i].Spin = Calc.Random.Range(8f, 12f) * 0.2f;
        particles[i].RotationCounter = Calc.Random.NextAngle();
        particles[i].MaxRotate = Calc.Random.Range(0.3f, 0.6f) * ((float)Math.PI / 2f);
        particles[i].Color = Colors[Calc.Random.Next(Colors.Length)];
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);
        Level level = scene as Level;
        for (int i = 0; i < particles.Length; i++)
        {
            Vector2 zero = Vector2.Zero;
            particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
            particles[i].Position.X += particles[i].SpeedX * Engine.DeltaTime;
            particles[i].RotationCounter += particles[i].Spin * Engine.DeltaTime;
            particles[i].Position += level.Wind * Engine.DeltaTime;
        }
        fade = Calc.Approach(fade, Visible ? 1f : 0f, Engine.DeltaTime);
    }

    public override void Render(Scene level)
    {
        if (!(fade <= 0f))
        {
            float fade2 = Ease.SineInOut(fade);
            Camera camera = (level as Level).Camera;
            MTexture mTexture = GFX.Game[sprite];
            for (int i = 0; i < particles.Length; i++)
            {
                Vector2 position = Vector2.Zero;
                position.X = -16f + Mod(particles[i].Position.X - camera.X, 352f / parallax);
                position.Y = -16f + Mod(particles[i].Position.Y - camera.Y, 212f / parallax);
                float num = (MathF.PI / 2) + MathF.Sin(particles[i].RotationCounter * spinSpeedMultiplier * particles[i].MaxRotate) * 1.0f;
                position += Calc.AngleToVector(num, 4f);
                for (int n = 1; n < blurCount; n++)
                {
                    mTexture.DrawCentered(
                        (
                        position -
                        new Vector2(
                            ((level as Level).Wind.X / 300f) * (n / blurDensity),
                            ((level as Level).Wind.Y / 300f) * (n / blurDensity)
                            )
                        ) * parallax,
                        (particles[i].Color * Calc.Map(n, 1, blurCount - 1, 0.5f, 0) * fade) * Math.Max(Math.Min(Math.Abs((level as Level).Wind.X) / 300, 1), Math.Min(Math.Abs((level as Level).Wind.Y) / 300, 1)) * alpha, 1f * scale, (num - 0.8f) * spinAmount);
                }
                mTexture.DrawCentered(position * parallax, (particles[i].Color * fade2) * alpha, 1f * scale, (num - 0.8f) * spinAmount);
            }
        }
    }

    private float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
