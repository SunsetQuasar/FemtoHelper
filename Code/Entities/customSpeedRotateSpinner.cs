// Celeste.CustomSpeedRotateSpinner
using Celeste;
using System;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

[CustomEntity("FemtoHelper/CustomSpeedRotateSpinner")]

public class CustomSpeedRotateSpinner : Entity
{
	public Sprite Sprite;

	private DustGraphic dusty;

	private int colorID;

	public bool Moving;

	private Vector2 center;

	private float rotationPercent;

	private float length;

	private bool fallOutOfScreen;

	private bool fixAngle;

	private Vector2 startCenter;

	private Vector2 startPosition;

	private bool noParticles;
	public float Angle
	{
		get
		{
			if (!fixAngle)
			{
				return MathHelper.Lerp(4.712389f, -(float)Math.PI / 2f, Easer(rotationPercent));
			}
			return MathHelper.Lerp((float)Math.PI, -(float)Math.PI, Easer(rotationPercent));
		}
	}

	public bool Clockwise;

	public bool isDust;

	public bool isBlade;

	public float rotateTime;

	public CustomSpeedRotateSpinner(EntityData data, Vector2 offset) : base(data.Position + offset)
	{
		orig_ctor(data, offset);
		startCenter = data.Nodes[0] + offset;
		startPosition = data.Position + offset;
		noParticles = data.Bool("noParticles", false);

		if (isBlade)
		{
			Add(Sprite = GFX.SpriteBank.Create("templeBlade"));
			Sprite.Play("idle");
			base.Depth = -50;
			Add(new MirrorReflection());
		} 
		else if (isDust)
		{
			Add(dusty = new DustGraphic(ignoreSolids: true));
		}
		else
		{
			Add(Sprite = GFX.SpriteBank.Create("moonBlade"));
			colorID = Calc.Random.Choose(0, 1, 2);
			Sprite.Play("idle" + colorID);
			base.Depth = -50;
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
		if (isBlade)
		{
			if (base.Scene.OnInterval(0.04f) && !noParticles)
			{
				SceneAs<Level>().ParticlesBG.Emit(BladeTrackSpinner.P_Trail, 2, Position, Vector2.One * 3f);
			}
			if (base.Scene.OnInterval(1f))
			{
				Sprite.Play("spin");
			}
		}
		else if (isDust)
		{
			if (Moving)
			{
				dusty.EyeDirection = dusty.EyeTargetDirection = Calc.AngleToVector(Angle + ((float)Math.PI / 2f * (float)(Clockwise ? 1 : (-1))), 1f);
				if (base.Scene.OnInterval(0.02f) && !noParticles)
				{
					SceneAs<Level>().ParticlesBG.Emit(DustStaticSpinner.P_Move, 1, Position, Vector2.One * 4f);
				}
			}
		}
		else
		{
			if (Moving && base.Scene.OnInterval(0.03f) && !noParticles)
			{
				SceneAs<Level>().ParticlesBG.Emit(StarTrackSpinner.P_Trail[colorID], 1, Position, Vector2.One * 3f);
			}
			if (base.Scene.OnInterval(0.8f))
			{
				colorID++;
				colorID %= 3;
				Sprite.Play("spin" + colorID);
			}
		}


		if (Moving)
		{
			if (Clockwise)
			{
				rotationPercent -= Engine.DeltaTime / rotateTime;
				rotationPercent += 1f;
			}
			else
			{
				rotationPercent += Engine.DeltaTime / rotateTime;
			}
			rotationPercent %= 1f;
			Position = center + Calc.AngleToVector(Angle, length);
		}
		if (fallOutOfScreen)
		{
			center.Y += 160f * Engine.DeltaTime;
			if (base.Y > (float)((base.Scene as Level).Bounds.Bottom + 32))
			{
				RemoveSelf();
			}
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
		base.Depth = -50;
		center = data.Nodes[0] + offset;
		Clockwise = data.Bool("clockwise");
		rotateTime = data.Float("rotateTime");
		isBlade = data.Bool("isBlade");
		isDust = data.Bool("isDust");
		base.Collider = new Circle(6f);
		Add(new PlayerCollider(OnPlayer));
		StaticMover staticMover = new StaticMover();
		staticMover.SolidChecker = (Solid s) => s.CollidePoint(center);
		staticMover.JumpThruChecker = (JumpThru jt) => jt.CollidePoint(center);
		staticMover.OnMove = delegate (Vector2 v)
		{
			center += v;
			Position += v;
		};
		staticMover.OnDestroy = delegate
		{
			fallOutOfScreen = true;
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
		fixAngle = (base.Scene as Level).Session.Area.GetLevelSet() != "Celeste";
		if (fixAngle)
		{
			float angleRadians = Calc.Angle(startCenter, startPosition);
			angleRadians = Calc.WrapAngle(angleRadians);
			rotationPercent = EaserInverse(Calc.Percent(angleRadians, (float)Math.PI, -(float)Math.PI));
			Position = center + Calc.AngleToVector(Angle, length);
		}
	}
}