// Celeste.Petals
using Celeste;
using System;
using System.Linq;

public class WindPetals : Backdrop
{
    public string PetalColor = "66cc22";

    public readonly Color[] Colors;

    public readonly float FallSpeedMin = 6f;

    public readonly float FallSpeedMax = 16f;

    public readonly float XDriftSpeedMin = 0f;

    public readonly float XDriftSpeedMax = 0f;

    public readonly int BlurCount = 15;

    public readonly float BlurDensity = 3;

    public readonly string Sprite = "particles/petal";

    public readonly float Parallax = 1f;

    public readonly float SpinSpeedMultiplier = 1f;

    public readonly float SpinAmount = 8f;

    public readonly float Alpha;

    public readonly float Scale;

    public readonly float WindXMultiplier;
    
    public readonly float WindYMultiplier;

    public readonly float ExtraLoopBorder;
    
    public static int GameplayBufferWidth => GameplayBuffers.Gameplay?.Width ?? 320;
    public static int GameplayBufferHeight => GameplayBuffers.Gameplay?.Height ?? 180;

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

    private readonly Particle[] particles = new Particle[40];

    private float fade;

    public WindPetals(string colors, float fallingSpeedMin, float fallingSpeedMax, int blurCount, float blurDensity, string texture, int count, float scroll, float spinFrequency, float spinAmplitude, float transparency, float size, float xDriftingSpeedMin, float xDriftingSpeedMax, float windXMultiplier, float windYMultiplier, float extraLoopBorder)
    {
        // this is like communal helper!
        Colors = colors
                .Split(',')
                .Select(str => Calc.HexToColorWithAlpha(str.Trim()))
                .ToArray();
        FallSpeedMin = fallingSpeedMin;
        FallSpeedMax = fallingSpeedMax;
        XDriftSpeedMin = xDriftingSpeedMin;
        XDriftSpeedMax = xDriftingSpeedMax;
        BlurCount = blurCount;
        BlurDensity = blurDensity;
        Sprite = texture;
        particles = new Particle[count];
        Parallax = (float)Math.Max(scroll, 0.00001);
        SpinSpeedMultiplier = spinFrequency;
        SpinAmount = spinAmplitude;
        Alpha = transparency;
        Scale = size;
        WindXMultiplier = windXMultiplier;
        WindYMultiplier = windYMultiplier;
        ExtraLoopBorder = extraLoopBorder;
        for (int i = 0; i < particles.Length; i++)
        {
            Reset(i);
        }
    }

    private void Reset(int i)
    {
        particles[i].Position = new Vector2(Calc.Random.Range(0, GameplayBufferWidth + ExtraLoopBorder) / Parallax, Calc.Random.Range(0, GameplayBufferHeight + ExtraLoopBorder) / Parallax);
        particles[i].Speed = Calc.Random.Range(FallSpeedMin, FallSpeedMax);
        particles[i].SpeedX = Calc.Random.Range(XDriftSpeedMin, XDriftSpeedMax);
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
            particles[i].Position.Y += level.Wind.Y * WindYMultiplier * Engine.DeltaTime;
            particles[i].Position.X += level.Wind.X * WindXMultiplier * Engine.DeltaTime;
        }
        fade = Calc.Approach(fade, Visible ? 1f : 0f, Engine.DeltaTime);
    }

    public override void Render(Scene level)
    {
        if (!(fade <= 0f))
        {
            float fade2 = Ease.SineInOut(fade);
            Camera camera = (level as Level).Camera;
            MTexture mTexture = GFX.Game[Sprite];
            for (int i = 0; i < particles.Length; i++)
            {
                Vector2 position = Vector2.Zero;
                position.X = -16f + Mod(particles[i].Position.X - camera.X, (GameplayBufferWidth + (ExtraLoopBorder * Parallax)) / Parallax);
                position.Y = -16f + Mod(particles[i].Position.Y - camera.Y, (GameplayBufferHeight + (ExtraLoopBorder * Parallax)) / Parallax);
                float num = MathF.PI / 2 + MathF.Sin(particles[i].RotationCounter * SpinSpeedMultiplier * particles[i].MaxRotate) * 1.0f;
                position += Calc.AngleToVector(num, 4f);

                float windStrengthX = (level as Level).Wind.X * WindXMultiplier;
                float windStrengthY = (level as Level).Wind.Y * WindYMultiplier;

                for (int n = 1; n < BlurCount; n++)
                {
                    mTexture.DrawCentered(
                        (
                        position -
                        new Vector2(
                            windStrengthX / 300f * (n / BlurDensity),
                            windStrengthY / 300f * (n / BlurDensity)
                            )
                        ) * Parallax,
                        particles[i].Color * Calc.Map(n, 1, BlurCount - 1, 0.5f, 0) * fade * Math.Max(Math.Min(Math.Abs(windStrengthX) / 300, 1), Math.Min(Math.Abs(windStrengthY) / 300, 1)) * Alpha, 1f * Scale, (num - 0.8f) * SpinAmount);
                }
                mTexture.DrawCentered(position * Parallax, particles[i].Color * fade2 * Alpha, 1f * Scale, (num - 0.8f) * SpinAmount);
            }
        }
    }

    private float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
