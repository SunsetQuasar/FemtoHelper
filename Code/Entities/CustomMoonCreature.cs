// Celeste.DashBlock
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Celeste.Mod.FemtoHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;

[CustomEntity("FemtoHelper/CustomMoonCreature")]
public class CustomMoonCreature : Entity
{
	public struct TrailNode
	{
		public Vector2 Position;

		public Color Color;
	}

	public TrailNode[] trail;

	public Vector2 start;

	public Vector2 target;

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

	private float FollowAcceleration = 120f;

	private float MaxSpeed = 40f;

	private float MaxFollowSpeed = 70f;

	private float MaxFollowDistance = 200f;

	private float MinFollowTime = 6f;

	private float MaxFollowTime = 12f;

	private readonly int spawn;

	private Rectangle originLevelBounds;

	private readonly Color[] colors;

	private string sprite;

	private bool tintSprite;

	private int trailCount;

	private float trailBaseSize;

	private float trailCatchupSpeed;

	private float trailSpacing;

	private float targetRangeRadius;

	private float minTargetTime;

	private float maxTargetTime;

	private float trailBaseAlpha;

	private float trailTipAlpha;

	private float trailGravity;

	private BloomPoint bloom;

	private VertexLight light;

	public CustomMoonCreature(EntityData data, Vector2 offset) : this(data.Position + offset)
	{
		spawn = data.Int("number", 1) - 1;
		sprite = data.Attr("sprite", "moonCreatureTiny");
		tintSprite = data.Bool("tintSprite", false);
		trailCount = data.Int("trailCount", 10);
		trailBaseSize = data.Float("trailBaseSize", 3f);
		Acceleration = data.Float("acceleration", 90f);
		FollowAcceleration = data.Float("followAcceleration", 120f);
		MaxSpeed = data.Float("maxSpeed", 40f);
		MaxFollowSpeed = data.Float("maxFollowSpeed", 70f);
		MaxFollowDistance = data.Float("maxFollowDistance", 200f);
		MinFollowTime = data.Float("minFollowTime", 6f);
		MaxFollowTime = data.Float("maxFollowTime", 12f);
		trailCatchupSpeed = data.Float("trailCatchupSpeed", 128f);
		trailSpacing = data.Float("trailSpacing", 2f);
		targetRangeRadius = data.Float("targetRangeRadius", 32f);
		minTargetTime = data.Float("minTargetTime", 0.8f);
		maxTargetTime = data.Float("maxTargetTime", 4f);
		trailBaseAlpha = data.Float("trailBaseAlpha", 1f);
		trailTipAlpha = data.Float("trailTipAlpha", 1f);
		trailGravity = data.Float("trailGravity", 0.05f);
		string[] array = data.Attr("colors", "c34fc7,4f95c7,53c74f").Split(',');
		colors = (Color[])(object)new Color[array.Length];
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = Calc.HexToColor(array[i]);
		}
		CenterColor = Color.Lerp(this.colors[Calc.Random.Next(this.colors.Length)], Color.White, 0);
		OrbColor = Calc.HexToColor("b0e6ff");
		//Logger.Log("a", mainColors[0].ToString());
		Color value = Color.Lerp(CenterColor, Calc.HexToColor(data.Attr("trailSubcolor1", "bde4ee")), data.Float("trailSubcolor1LerpAmount", 0.5f));
		Color value2 = Color.Lerp(CenterColor, Calc.HexToColor(data.Attr("trailSubcolor2", "2f2941")), data.Float("trailSubcolor2LerpAmount", 0.5f));
		trail = new TrailNode[trailCount];
		for (int i = 0; i < trailCount; i++)
		{
			trail[i] = new TrailNode
			{
				Position = Position,
				Color = Color.Lerp(value, value2, (float)i / (trailCount - 1)) * (trailBaseAlpha - ((trailBaseAlpha - trailTipAlpha) * i / trailCount))
			};
		}
		Add(Sprite = GFX.SpriteBank.Create(sprite));
		if (tintSprite)
		{
			Sprite.Color = CenterColor;
		}
		Add(bloom = new BloomPoint(data.Float("bloomAlpha", 0f), data.Float("bloomRadius", 0)));
		Add(light = new VertexLight(!data.Bool("lightColorIsMainColor") ? Calc.HexToColor(data.Attr("lightColor")) : CenterColor, data.Float("lightAlpha", 0f), data.Int("lightStartFade", 12), data.Int("lightEndFade", 24)));
	}
	public CustomMoonCreature(Vector2 position)
	{
		base.Tag = Tags.TransitionUpdate;
		base.Depth = -13010;
		base.Collider = new Hitbox(20f, 20f, -10f, -10f);
		start = position;
		targetTimer = 0f;
		GetRandomTarget();
		Position = target;
		Add(new PlayerCollider(OnPlayer));
	}

	public override void Added(Scene scene)
	{
		base.Added(scene);
		for (int i = 0; i < spawn; i++)
		{
			scene.Add(new MoonCreature(Position + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4))));
		}
		originLevelBounds = (scene as Level).Bounds;
	}
	private void OnPlayer(Player player)
	{
		Vector2 vector = (Position - player.Center).SafeNormalize(player.Speed.Length() * 0.3f);
		if (vector.LengthSquared() > bump.LengthSquared())
		{
			bump = vector;
			if ((player.Center - start).Length() < MaxFollowDistance)
			{
				following = player;
				followingTime = Calc.Random.Range(MinFollowTime, MaxFollowTime);
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
			float length = Calc.Random.NextFloat(targetRangeRadius);
			float angleRadians = Calc.Random.NextFloat((float)Math.PI * 2f);
			target = start + Calc.AngleToVector(angleRadians, length);
		}
		while ((value - target).Length() < 8f);
	}

	public override void Update()
	{
		base.Update();
		if (following == null)
		{
			targetTimer -= Engine.DeltaTime;
			if (targetTimer <= 0f)
			{
				targetTimer = Calc.Random.Range(minTargetTime, maxTargetTime);
				GetRandomTarget();
			}
		}
		else
		{
			followingTime -= Engine.DeltaTime;
			targetTimer -= Engine.DeltaTime;
			if (targetTimer <= 0f)
			{
				targetTimer = Calc.Random.Range(minTargetTime, Math.Max(maxTargetTime / 2, minTargetTime));
				GetFollowOffset();
			}
			target = following.Center + followingOffset;
			if ((Position - start).Length() > MaxFollowDistance || followingTime <= 0f)
			{
				following = null;
				targetTimer = 0f;
			}
		}
		Vector2 value = (target - Position).SafeNormalize();
		speed += value * ((following == null) ? Acceleration : FollowAcceleration) * Engine.DeltaTime;
		speed = speed.SafeNormalize() * Math.Min(speed.Length(), (following == null) ? MaxSpeed : MaxFollowSpeed);
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
			vector.Y += trailGravity;
			Vector2 vector2 = position + vector * trailSpacing;
			trail[i].Position = Calc.Approach(trail[i].Position, vector2, trailCatchupSpeed * Engine.DeltaTime);
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
			float num2 = Calc.ClampedMap(num, 0f, trail.Length - 1, trailBaseSize);
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
		Draw.Circle(start, targetRangeRadius, CenterColor * 0.5f, 16);
	}
}

