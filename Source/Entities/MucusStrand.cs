using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/MucusStrand")]
public class MucusStrand : Entity
{
    private Vector2 endpoint;
    private bool playerInside;
    private static ParticleType snotty;
    private static ParticleType snottyDrip;

    private float timer;
    private float rate;
    private float deathpercent;

    private readonly Shaker shakeit;

    private readonly float length;
    private readonly Vector2 tangent;
    private readonly Vector2 normal;
    private readonly float mucusDropCount;
    private readonly static Random random = new();

    private readonly MTexture mucusAtlas;

    public class MucusBall
    {
        public MTexture SubTexture;
        public Vector2 TargetPos;
        public Vector2 CurrentPos;
        public Vector2 Speed;
        public float Width, Height, Stress, MovementFac;

        public const float AccelerationFactor = 95f;
        public const float Deceleration = 280f;

        public void Tick()
        {
            Vector2 force = (TargetPos - CurrentPos) * AccelerationFactor;

            Speed = Calc.Approach(Speed, Vector2.Zero, Deceleration * Engine.DeltaTime);
            Speed += force * Engine.DeltaTime;

            CurrentPos += Speed * Engine.DeltaTime;

            Stress = Vector2.DistanceSquared(TargetPos, CurrentPos);
        }
    }

    public List<MucusBall> Balls = [];

    public MucusStrand(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(8, 8, -4, -4);

        endpoint = data.NodesOffset(offset)[0];

        if (endpoint.X < Position.X || endpoint.Y > Position.Y)
        {
            (endpoint, Position) = (Position, endpoint);
        }

        timer = Calc.Random.NextFloat(100f);
        rate = Calc.Random.Range(0.9f, 1.3f);

        Depth = -100;

        Add(shakeit = new Shaker(false));

        tangent = (endpoint - Position).SafeNormalize();
        normal = tangent.TurnRight();
        length = Vector2.Distance(Position, endpoint);
        mucusDropCount = length / 8f; //every tile of distance, another chance to create a mucus drop

        mucusAtlas = GFX.Game["objects/FemtoHelper/mucusStrand/ball"];

        float mucusBallCount = MathF.Round(length / 3.6f);
        int index = 0;
        for (float i = 0f; i <= 1f; i += 1f / mucusBallCount)
        {
            Vector2 pos = Vector2.Lerp(Position, endpoint, i);
            Balls.Add(new MucusBall
            {
                SubTexture = mucusAtlas.GetSubtexture(Calc.Random.Next(4) * 16, 0, 16, 16),
                TargetPos = pos,
                CurrentPos = pos,
                Speed = Vector2.Zero,
                Width = Calc.Random.Range(12, 16) / 16f,
                Height = Calc.Random.Range(12, 16) / 16f,
            });

            float fac = (0.5f + 0.5f * MathF.Cos(MathF.Tau * i));

            fac = 1 - MathF.Pow(fac, 10) * (0.9f + 0.1f * -MathF.Cos(MathF.Tau * i));

            Balls[index].MovementFac = fac;
            index++;
        }
    }

    public override void Update()
    {
        base.Update();

        foreach (MucusBall b in Balls)
        {
            b.Tick();
        }

        timer += Engine.DeltaTime * rate;

        if (deathpercent > 0)
        {
            deathpercent = Math.Max(deathpercent - Engine.DeltaTime, 0);
        }

        if (Scene.OnInterval(0.5f))
        {
            for (float i = 0f; i < 1f; i += 1f / mucusDropCount)
            {
                if (!random.Chance(0.15f)) continue;

                Vector2 at = Vector2.Lerp(Position, endpoint, random.NextFloat());

                (Scene as Level).ParticlesBG.Emit(snottyDrip, at + new Vector2(Calc.Random.Range(-1, 1), Calc.Random.Range(-1, 1)), random.NextAngle());
            }

            if (random.Chance(0.3f))
            {
                shakeit.ShakeFor(0.06f, false);
            }
        }

        Player player = Scene.Tracker.GetEntity<Player>();
        if (player != null)
        {
            if (player.CollideLine(Position, endpoint) && Collidable)
            {
                if (!player.DashAttacking)
                {
                    Vector2 spd = player.Speed;
                    if (player.Die((player.Position - ((Position + endpoint) / 2)).SafeNormalize()) != null)
                    {
                        float closestPercent = ClosestPointOnLine_PercentEdition(Position, endpoint, player.Center);
                        ApplyStrandForce(closestPercent, spd);

                        EventInstance e = Audio.Play("event:/char/madeline/water_out", player.Position);
                        e.setPitch(0.6f);
                        e.setVolume(2.2f);
                        for (int i = -3; i <= 3; i++)
                        {
                            (Scene as Level).Particles.Emit(snotty, player.Center + new Vector2(Calc.Random.Range(-3, 3), Calc.Random.Range(-3, 3)), Calc.Random.NextAngle());
                        }
                        deathpercent = 1;
                        shakeit.ShakeFor(0.5f, true);
                        return;
                    }
                }
                if (!playerInside)
                {
                    EventInstance e = Audio.Play("event:/char/madeline/water_out", player.Position);
                    e.setPitch(0.5f);
                    e.setVolume(2f);

                    float closestPercent = ClosestPointOnLine_PercentEdition(Position, endpoint, player.Center);

                    ApplyStrandForce(closestPercent, player.Speed / 2.75f);

                    for (int i = -3; i <= 3; i++)
                    {
                        (Scene as Level).Particles.Emit(snotty, player.Center + (tangent * i * 2) + new Vector2(Calc.Random.Range(-2, 2), Calc.Random.Range(-2, 2)), player.Speed.Angle() + Calc.Random.Range(-0.5f, 0.5f));
                    }
                    playerInside = true;
                }
            }
            else playerInside = false;
        }
    }

    public void ApplyStrandForce(float percentAt, Vector2 force)
    {
        int index = (int)(percentAt * (Balls.Count - 1));

        Debug(force.Length());

        // hard cap the force given
        force = force.SafeNormalize() * Math.Min(force.Length(), 180f);

        Debug(force.Length());

        //affects 5 balls behind and 5 ahead

        // applies a sort of soft cap if stress is too high

        for (int i = Math.Max(index - 5, 0); i <= index; i++)
        {
            float cap = 1 - Ease.ExpoIn(Calc.ClampedMap(Balls[i].Speed.LengthSquared(), 20f * 20f, 80f * 80f));

            Balls[i].Speed += force * Balls[i].MovementFac * Ease.SineInOut(Calc.ClampedMap(i, Math.Max(index - 5, 0), index) * cap);
        }

        for (int i = index + 1; i <= Math.Min(index + 5, Balls.Count - 1); i++)
        {
            float cap = 1 - Ease.ExpoIn(Calc.ClampedMap(Balls[i].Speed.LengthSquared(), 20f * 20f, 80f * 80f));

            Balls[i].Speed += force * Balls[i].MovementFac * Ease.SineInOut(Calc.ClampedMap(i, Math.Min(index + 5, Balls.Count - 1), index)) * cap;
        }
    }

    public override void Render()
    {
        base.Render();
        /*
        Vector2 rawoffset = (endpoint - Position);
        Vector2 offset13 = Position + rawoffset * (1f / 3f);
        Vector2 offset23 = Position + rawoffset * (2f / 3f);
        renderoffset += shakeit.Value;
        rawoffset.Normalize();
        WobbleLine(Position - (rawoffset * 3), offset13 + renderoffset + (rawoffset * 3), 0);
        WobbleLine(offset13 + renderoffset - (rawoffset * 3), offset23 + renderoffset + (rawoffset * 3), 0.43f);
        WobbleLine(offset23 + renderoffset - (rawoffset * 3), endpoint + (rawoffset * 3), 0.71f);

        WobbleLine(endpoint + (rawoffset * 3), offset23 + renderoffset - (rawoffset * 3), 0);
        WobbleLine(offset23 + renderoffset + (rawoffset * 3), offset13 + renderoffset - (rawoffset * 3), 0.26f);
        WobbleLine(offset13 + renderoffset + (rawoffset * 3), Position - (rawoffset * 3), 0.84f);
        renderoffset -= shakeit.Value;
        */

        for (int i = 0; i < Balls.Count; i++)
        {
            MucusBall b = Balls[i];

            float scaleFac = MathF.Sin((timer * 1.831f) + i);
            scaleFac *= scaleFac * scaleFac * scaleFac * scaleFac * scaleFac * scaleFac * scaleFac;

            b.SubTexture.DrawOnlyOutlineCentered(b.CurrentPos + (((normal * 2 * MathF.Sin((timer * 2) + (i * 0.75f))) + shakeit.Value) * b.MovementFac), Color.Black * 0.8f, new Vector2(b.Width, b.Height) * (1f + (0.2f * scaleFac)));
        }

        for (int i = 0; i < Balls.Count; i++)
        {
            MucusBall b = Balls[i];
            float colorFac = MathF.Cos((timer * 2.123f) + (i * 0.678f));
            Color ballColor = Color.Lerp(Color.LemonChiffon, Color.White, 0.5f + 0.5f * colorFac);
            ballColor *= 0.6f + 0.1f * colorFac;
            ballColor.A = (byte)(255 * ((ballColor.A / 255f) * (0.4f + (0.2f * colorFac))));

            float scaleFac = MathF.Sin((timer * 1.831f) + i);
            scaleFac *= scaleFac * scaleFac * scaleFac * scaleFac * scaleFac * scaleFac * scaleFac;

            Vector2 pos = b.CurrentPos + (((normal * 2 * MathF.Sin((timer * 2) + (i * 0.75f))) + shakeit.Value) * b.MovementFac);

            b.SubTexture.DrawCentered(pos, ballColor, new Vector2(b.Width, b.Height) * (1f + (0.2f * scaleFac)));

            Color stressColor = Color.Lerp(Color.Transparent, new Color(1f, 0.3f, 0.4f, 0f), b.Stress / (4 * 4));

            b.SubTexture.DrawCentered(pos, stressColor, new Vector2(b.Width, b.Height) * (1f + (0.2f * scaleFac)));
        }
    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        Draw.Line(Position, endpoint, Color.Red);
        if (Scene.Tracker.GetEntity<Player>() is Player player)
        {
            Vector2 closest = Calc.ClosestPointOnLine(Position, endpoint, player.Center);
            Draw.Rect(closest - Vector2.One * 2, 4, 4, Color.Cyan * 0.5f);
        }
    }

    [OnLoadContent]
    public static void LoadContent(bool firstLoad)
    {
        snotty = new ParticleType
        {
            Source = GFX.Game["particles/blob"],
            Color = Calc.HexToColor("51952c") * 0.6f,
            FadeMode = ParticleType.FadeModes.None,
            LifeMin = 0.5f,
            LifeMax = 0.8f,
            Size = 0.7f,
            SizeRange = 0.25f,
            ScaleOut = true,
            Direction = 4.712389f,
            DirectionRange = 0.17453292f,
            SpeedMin = 50f,
            SpeedMax = 100f,
            SpeedMultiplier = 0.01f,
            Acceleration = new Vector2(0f, 90f)
        };

        snottyDrip = new ParticleType(snotty)
        {
            Color = Calc.HexToColor("51952c") * 0.5f,
            Color2 = Calc.HexToColor("51952c") * 0.3f,
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 0.6f,
            LifeMax = 1f,
            SpeedMin = 0.5f,
            SpeedMax = 1f,
            SpeedMultiplier = 1f,
        };
    }

    public static void Load()
    {

    }

    public static void Unload()
    {

    }

    public static float ClosestPointOnLine_PercentEdition(Vector2 lineA, Vector2 lineB, Vector2 closestTo)
    {
        Vector2 vector = lineB - lineA;
        float value = Vector2.Dot(closestTo - lineA, vector) / Vector2.Dot(vector, vector);
        value = MathHelper.Clamp(value, 0f, 1f);
        return value;
    }
}
