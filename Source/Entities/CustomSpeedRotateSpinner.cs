// Celeste.CustomSpeedRotateSpinner
using Celeste;
using System;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

[CustomEntity("FemtoHelper/CustomSpeedRotateSpinner")]

public class CustomSpeedRotateSpinner : Entity
{
	public readonly Sprite Sprite;

	private readonly DustGraphic dusty;

	private int colorId;

	public bool Moving;

	private Vector2 center;

	private float rotationPercent;

	private float length;

	private bool fallOutOfScreen;

	private bool fixAngle;

	private readonly Vector2 startCenter;

	private readonly Vector2 startPosition;

	private readonly bool noParticles;
	public float Angle
	{
		get
		{
			return !fixAngle ? MathHelper.Lerp(4.712389f, -(float)Math.PI / 2f, Easer(rotationPercent)) : MathHelper.Lerp((float)Math.PI, -(float)Math.PI, Easer(rotationPercent));
		}
	}

	public bool Clockwise;

	public bool IsDust;

	public bool IsBlade;

	public float RotateTime;

	public CustomSpeedRotateSpinner(EntityData data, Vector2 offset) : base(data.Position + offset)
	{
		orig_ctor(data, offset);
		startCenter = data.Nodes[0] + offset;
		startPosition = data.Position + offset;
		noParticles = data.Bool("noParticles", false);

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
	}

	private float Easer(float v)
	{
		return v;
	}

	private float EaserInverse(float v)
	{
		return v;
	}

	public override void Update()
	{
		base.Update();
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
			Position = center + Calc.AngleToVector(Angle, length);
		}

		if (!fallOutOfScreen) return;
		center.Y += 160f * Engine.DeltaTime;
		if (Y > (Scene as Level).Bounds.Bottom + 32)
		{
			RemoveSelf();
		}
	}

	public virtual void OnPlayer(Player player)
	{
		if (player.Die((player.Position - Position).SafeNormalize()) != null)
		{
			if(dusty != null)
			{
                dusty.OnHitPlayer();
            }
			Moving = false;
		}
	}

	public float orig_get_Angle()
	{
		return MathHelper.Lerp(4.712389f, -(float)Math.PI / 2f, Easer(rotationPercent));
	}

	public void orig_ctor(EntityData data, Vector2 offset)
	{
		Moving = true;
		Depth = -50;
		center = data.Nodes[0] + offset;
		Clockwise = data.Bool("clockwise");
		RotateTime = data.Float("rotateTime");
		IsBlade = data.Bool("isBlade");
		IsDust = data.Bool("isDust");
		Collider = new Circle(6f);
		Add(new PlayerCollider(OnPlayer));
		StaticMover staticMover = new StaticMover
		{
			SolidChecker = s => s.CollidePoint(center),
			JumpThruChecker = jt => jt.CollidePoint(center),
			OnMove = v =>
			{
				center += v;
				Position += v;
			},
			OnDestroy = () =>
			{
				fallOutOfScreen = true;
			}
		};
		Add(staticMover);
		float angleRadians = Calc.Angle(center, Position);
		angleRadians = Calc.WrapAngle(angleRadians);
		rotationPercent = EaserInverse(Calc.Percent(angleRadians, -(float)Math.PI / 2f, 4.712389f));
		length = (Position - center).Length();
		Position = center + Calc.AngleToVector(Angle, length);
	}

	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		fixAngle = (Scene as Level).Session.Area.GetLevelSet() != "Celeste";
		if (!fixAngle) return;
		float angleRadians = Calc.Angle(startCenter, startPosition);
		angleRadians = Calc.WrapAngle(angleRadians);
		rotationPercent = EaserInverse(Calc.Percent(angleRadians, (float)Math.PI, -(float)Math.PI));
		Position = center + Calc.AngleToVector(Angle, length);
	}
}