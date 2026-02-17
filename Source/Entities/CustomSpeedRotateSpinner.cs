// Celeste.CustomSpeedRotateSpinner
using Celeste;
using MonoMod;
using System;

[CustomEntity("FemtoHelper/CustomSpeedRotateSpinner")]

public class CustomSpeedRotateSpinner : RotateSpinner
{
    public readonly Sprite Sprite;

    private readonly DustGraphic dusty;

    private int colorId;

    private bool fixAngle;

    private readonly Vector2 startCenter;

    private readonly Vector2 startPosition;

    private readonly bool noParticles;

    public bool IsDust;

    public bool IsBlade;

    public float RotateTime;

    public readonly Vector2 Scale;

    public CustomSpeedRotateSpinner(EntityData data, Vector2 offset) : base(data, offset)
    {
        Scale = data.Vector2("scaleX", "scaleY", Vector2.One);
        RotateTime = data.Float("rotateTime");
        IsBlade = data.Bool("isBlade");
        IsDust = data.Bool("isDust");

        if (IsBlade)
        {
            Add(Sprite = GFX.SpriteBank.Create("templeBlade"));
            Sprite.Play("idle");
            Depth = -50;
            Add(new MirrorReflection());
        }
        else if (IsDust)
        {
            Add(dusty = new DustGraphic(ignoreSolids: true));
        }
        else
        {
            Add(Sprite = GFX.SpriteBank.Create("moonBlade"));
            colorId = Calc.Random.Choose(0, 1, 2);
            Sprite.Play("idle" + colorId);
            Depth = -50;
            Add(new MirrorReflection());
        }

        if(!data.Bool("attach", true))
        {
            Remove(Get<StaticMover>());
        }
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    public void base_Update() { }

    public override void Update()
    {
        base_Update();
        if (IsBlade)
        {
            if (Scene.OnInterval(0.04f) && !noParticles)
            {
                SceneAs<Level>().ParticlesBG.Emit(BladeTrackSpinner.P_Trail, 2, Position, Vector2.One * 3f);
            }
            if (Scene.OnInterval(1f))
            {
                Sprite.Play("spin");
            }
        }
        else if (IsDust)
        {
            if (Moving)
            {
                dusty.EyeDirection = dusty.EyeTargetDirection = Calc.AngleToVector(Angle + (float)Math.PI / 2f * (Clockwise ? 1 : -1), 1f);
                if (Scene.OnInterval(0.02f) && !noParticles)
                {
                    SceneAs<Level>().ParticlesBG.Emit(DustStaticSpinner.P_Move, 1, Position, Vector2.One * 4f);
                }
            }
        }
        else
        {
            if (Moving && Scene.OnInterval(0.03f) && !noParticles)
            {
                SceneAs<Level>().ParticlesBG.Emit(StarTrackSpinner.P_Trail[colorId], 1, Position, Vector2.One * 3f);
            }
            if (Scene.OnInterval(0.8f))
            {
                colorId++;
                colorId %= 3;
                Sprite.Play("spin" + colorId);
            }
        }


        if (Moving)
        {
            if (Clockwise)
            {
                rotationPercent -= Engine.DeltaTime / RotateTime;
                rotationPercent += 1f;
            }
            else
            {
                rotationPercent += Engine.DeltaTime / RotateTime;
            }
            rotationPercent %= 1f;
            Position = center + Calc.AngleToVector(Angle, length) * Scale;
        }

        if (!fallOutOfScreen) return;
        center.Y += 160f * Engine.DeltaTime;
        if (Y > (Scene as Level).Bounds.Bottom + 32)
        {
            RemoveSelf();
        }
    }

    public override void OnPlayer(Player player)
    {
        base.OnPlayer(player);
        dusty.OnHitPlayer();
    }
}