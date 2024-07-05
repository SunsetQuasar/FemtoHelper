// Celeste.DashBlock
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Celeste.Mod.FemtoHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;

[CustomEntity("FemtoHelper/LaCreatura")]
public class LaCreatura : Entity
{
	private struct TrailNode
	{
		public Vector2 Position;

		public Color Color;
	}

	private TrailNode[] trail;

	private Vector2 start;

	private Vector2 target;

	private float targetTimer;

	private Vector2 speed;

	private Vector2 bump;

	private Player following;

	private Vector2 followingOffset;

	private float followingTime;

	private Color OrbColor;

	private Color CenterColor;

	private Sprite Sprite;

	private float Acceleration = 90f;

	private const float FollowAcceleration = 120f;

	private float MaxSpeed = 40f;
	
	private const float MaxFollowSpeed = 70f;

	private const float MaxFollowDistance = 200f;

	//private readonly int spawn;

	private Rectangle originLevelBounds;

	private BloomPoint bloom;

	private VertexLight light;

	private bool facingRight;
	public LaCreatura(EntityData data, Vector2 offset) : this(data.Position + offset)
	{
		Acceleration = data.Float("acceleration", 90f);
		MaxSpeed = data.Float("maxSpeed", 40f);
	}
	public LaCreatura(Vector2 position)
	{

		base.Tag = Tags.TransitionUpdate;
		base.Depth = -13010;
		base.Collider = new Hitbox(20f, 20f, -10f, -10f);
		start = position;
		targetTimer = 0f;
		GetRandomTarget();
		Position = target;
		Add(new PlayerCollider(OnPlayer));
		OrbColor = Calc.HexToColor("b0e6ff");
		CenterColor = Calc.Random.Choose(Calc.HexToColor("74db93"), Calc.HexToColor("dbc874"), Calc.HexToColor("74a1db"), Calc.HexToColor("e0779f"), Calc.HexToColor("9677e0"));
		Color value = Color.Lerp(CenterColor, Calc.HexToColor("bde4ee"), 0.5f);
		Color value2 = Color.Lerp(CenterColor, Calc.HexToColor("2f2941"), 0.5f);
		trail = new TrailNode[6];
		for (int i = 0; i < 6; i++)
		{
			trail[i] = new TrailNode
			{
				Position = Position,
				Color = Color.Lerp(value, value2, (float)i / 4f) * (0.2f - (0.1f * i / 6))
			};
		}
		Add(Sprite = FemtoModule.femtoSpriteBank.Create("butterfly"));
	}

	public override void Added(Scene scene)
	{
		base.Added(scene);
		Sprite.Play("right");
		Sprite.Color = CenterColor;
		Add(bloom = new BloomPoint(0.75f, 16f));
		Add(light = new VertexLight(CenterColor, 1f, 12, 24));

		originLevelBounds = (scene as Level).Bounds;
	}
	private void OnPlayer(Player player)
	{
		Vector2 vector = (Position - player.Center).SafeNormalize(player.Speed.Length() * 0.3f);
		if (vector.LengthSquared() > bump.LengthSquared())
		{
			bump = vector;
			if ((player.Center - start).Length() < 200f)
			{
				following = player;
				followingTime = Calc.Random.Range(6f, 12f);
				GetFollowOffset();
			}
		}
	}
	private void GetFollowOffset()
	{
		followingOffset = new Vector2(Calc.Random.Choose(-1, 1) * Calc.Random.Range(8, 16), Calc.Random.Range(-20f, 0f));
	}

	private void GetRandomTarget()
	{
		Vector2 value = target;
		do
		{
			float length = Calc.Random.NextFloat(32f);
			float angleRadians = Calc.Random.NextFloat((float)Math.PI * 2f);
			target = start + Calc.AngleToVector(angleRadians, length);
		}
		while ((value - target).Length() < 8f);
	}

	public override void Update()
	{
		base.Update();
		if (speed.X >= 0 && facingRight == false)
		{
			Sprite.Play("turn_right");
			facingRight = true;
		}
		else if (speed.X < 0 && facingRight == true)
		{
			Sprite.Play("turn_left");
			facingRight = false;
		}
		if (following == null)
		{
			targetTimer -= Engine.DeltaTime;
			if (targetTimer <= 0f)
			{
				targetTimer = Calc.Random.Range(0.8f, 4f);
				GetRandomTarget();
			}
		}
		else
		{
			followingTime -= Engine.DeltaTime;
			targetTimer -= Engine.DeltaTime;
			if (targetTimer <= 0f)
			{
				targetTimer = Calc.Random.Range(0.8f, 2f);
				GetFollowOffset();
			}
			target = following.Center + followingOffset;
			if ((Position - start).Length() > 200f || followingTime <= 0f)
			{
				following = null;
				targetTimer = 0f;
			}
		}
		Vector2 value = (target - Position).SafeNormalize();
		speed += value * ((following == null) ? Acceleration : Acceleration * (1 + (1/3))) * Engine.DeltaTime;
		speed = speed.SafeNormalize() * Math.Min(speed.Length(), (following == null) ? MaxSpeed : MaxSpeed + 30f);
		bump = bump.SafeNormalize() * Calc.Approach(bump.Length(), 0f, Engine.DeltaTime * 80f);
		Position += (speed + bump) * Engine.DeltaTime;
		Vector2 position = Position;
		for (int i = 0; i < trail.Length; i++)
		{
			Vector2 vector = (trail[i].Position - position).SafeNormalize();
			if (vector == Vector2.Zero)
			{
				vector = new Vector2(0f, 1f);
			}
			vector.Y += 0.05f;
			Vector2 vector2 = position + vector * 5f;
			trail[i].Position = Calc.Approach(trail[i].Position, vector2, 128f * Engine.DeltaTime);
			position = trail[i].Position;
		}
		base.X = Calc.Clamp(base.X, originLevelBounds.Left + 4, originLevelBounds.Right - 4);
		base.Y = Calc.Clamp(base.Y, originLevelBounds.Top + 4, originLevelBounds.Bottom - 4);
	}

	public override void Render()
	{
		Vector2 position = Position;
		Position = Position.Floor();
		for (int num = trail.Length - 1; num >= 0; num--)
		{
			Vector2 position2 = trail[num].Position;
			float num2 = Calc.ClampedMap(num, 0f, trail.Length - 1, 3f);
			Draw.Rect(position2.X - num2 / 2f, position2.Y - num2 / 2f, num2, num2, trail[num].Color);
		}
		base.Render();
		Position = position;
	}
	public override void DebugRender(Camera camera)
	{
		base.DebugRender(camera);
		Draw.Line(Position, target, Color.White * 0.5f);
		Draw.Rect(target - new Vector2(2, 2), 4, 4, CenterColor * 0.5f);
	}
}

