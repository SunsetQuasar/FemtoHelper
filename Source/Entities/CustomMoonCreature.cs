// Celeste.DashBlock
using Celeste;
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

	public readonly TrailNode[] Trail;

	public Vector2 Start;

	public Vector2 Target;

	private float targetTimer;

	private Vector2 speed;

	private Vector2 bump;

	private Player following;

	private Vector2 followingOffset;

	private float followingTime;

	private Color orbColor;

	private readonly Color centerColor;

	private readonly float acceleration = 90f;

	private readonly float followAcceleration = 120f;

	private readonly float maxSpeed = 40f;

	private readonly float maxFollowSpeed = 70f;

	private readonly float maxFollowDistance = 200f;

	private readonly float minFollowTime = 6f;

	private readonly float maxFollowTime = 12f;

	private readonly int spawn;

	private Rectangle originLevelBounds;

	private readonly Color[] colors;

	private readonly bool tintSprite;

	private readonly int trailCount;

	private readonly float trailBaseSize;

	private readonly float trailCatchupSpeed;

	private readonly float trailSpacing;

	private readonly float targetRangeRadius;

	private readonly float minTargetTime;

	private readonly float maxTargetTime;

	private readonly float trailBaseAlpha;

	private readonly float trailTipAlpha;

	private readonly float trailGravity;

	private BloomPoint bloom;

	private VertexLight light;

	public CustomMoonCreature(EntityData data, Vector2 offset) : this(data.Position + offset)
	{
		Sprite sprite;
		spawn = data.Int("number", 1) - 1;
		string spritePath = data.Attr("sprite", "moonCreatureTiny");
		tintSprite = data.Bool("tintSprite", false);
		trailCount = data.Int("trailCount", 10);
		trailBaseSize = data.Float("trailBaseSize", 3f);
		acceleration = data.Float("acceleration", 90f);
		followAcceleration = data.Float("followAcceleration", 120f);
		maxSpeed = data.Float("maxSpeed", 40f);
		maxFollowSpeed = data.Float("maxFollowSpeed", 70f);
		maxFollowDistance = data.Float("maxFollowDistance", 200f);
		minFollowTime = data.Float("minFollowTime", 6f);
		maxFollowTime = data.Float("maxFollowTime", 12f);
		trailCatchupSpeed = data.Float("trailCatchupSpeed", 128f);
		trailSpacing = data.Float("trailSpacing", 2f);
		targetRangeRadius = data.Float("targetRangeRadius", 32f);
		minTargetTime = data.Float("minTargetTime", 0.8f);
		maxTargetTime = data.Float("maxTargetTime", 4f);
		trailBaseAlpha = data.Float("trailBaseAlpha", 1f);
		trailTipAlpha = data.Float("trailTipAlpha", 1f);
		trailGravity = data.Float("trailGravity", 0.05f);
		string[] array = data.Attr("colors", "c34fc7,4f95c7,53c74f").Split(',');
		var colors = new Color[array.Length];
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = Calc.HexToColor(array[i]);
		}
		centerColor = Color.Lerp(this.colors[Calc.Random.Next(this.colors.Length)], Color.White, 0);
		orbColor = Calc.HexToColor("b0e6ff");
		//Logger.Log("a", mainColors[0].ToString());
		Color value = Color.Lerp(centerColor, Calc.HexToColor(data.Attr("trailSubcolor1", "bde4ee")), data.Float("trailSubcolor1LerpAmount", 0.5f));
		Color value2 = Color.Lerp(centerColor, Calc.HexToColor(data.Attr("trailSubcolor2", "2f2941")), data.Float("trailSubcolor2LerpAmount", 0.5f));
		Trail = new TrailNode[trailCount];
		for (int i = 0; i < trailCount; i++)
		{
			Trail[i] = new TrailNode
			{
				Position = Position,
				Color = Color.Lerp(value, value2, (float)i / (trailCount - 1)) * (trailBaseAlpha - ((trailBaseAlpha - trailTipAlpha) * i / trailCount))
			};
		}
		Add(sprite = GFX.SpriteBank.Create(spritePath));
		if (tintSprite)
		{
			sprite.Color = centerColor;
		}
		Add(bloom = new BloomPoint(data.Float("bloomAlpha", 0f), data.Float("bloomRadius", 0)));
		Add(light = new VertexLight(!data.Bool("lightColorIsMainColor") ? Calc.HexToColor(data.Attr("lightColor")) : centerColor, data.Float("lightAlpha", 0f), data.Int("lightStartFade", 12), data.Int("lightEndFade", 24)));
	}
	public CustomMoonCreature(Vector2 position)
	{
		Tag = Tags.TransitionUpdate;
		Depth = -13010;
		Collider = new Hitbox(20f, 20f, -10f, -10f);
		Start = position;
		targetTimer = 0f;
		GetRandomTarget();
		Position = Target;
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
		if (!(vector.LengthSquared() > bump.LengthSquared())) return;
		bump = vector;
		if (!((player.Center - Start).Length() < maxFollowDistance)) return;
		following = player;
		followingTime = Calc.Random.Range(minFollowTime, maxFollowTime);
		GetFollowOffset();
	}
	private void GetFollowOffset()
	{
		followingOffset = new Vector2(Calc.Random.Choose(-1, 1) * Calc.Random.Range(8, 16), Calc.Random.Range(-20f, 0f));
	}

	private void GetRandomTarget()
	{
		Vector2 value = Target;
		do
		{
			float length = Calc.Random.NextFloat(targetRangeRadius);
			float angleRadians = Calc.Random.NextFloat((float)Math.PI * 2f);
			Target = Start + Calc.AngleToVector(angleRadians, length);
		}
		while ((value - Target).Length() < targetRangeRadius / 4f);
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
			Target = following.Center + followingOffset;
			if ((Position - Start).Length() > maxFollowDistance || followingTime <= 0f)
			{
				following = null;
				targetTimer = 0f;
			}
		}
		Vector2 value = (Target - Position).SafeNormalize();
		speed += value * ((following == null) ? acceleration : followAcceleration) * Engine.DeltaTime;
		speed = speed.SafeNormalize() * Math.Min(speed.Length(), (following == null) ? maxSpeed : maxFollowSpeed);
		bump = bump.SafeNormalize() * Calc.Approach(bump.Length(), 0f, Engine.DeltaTime * 80f);
		Position += (speed + bump) * Engine.DeltaTime;
		Vector2 position = Position;
		for (int i = 0; i < Trail.Length; i++)
		{
			Vector2 vector = (Trail[i].Position - position).SafeNormalize();
			if (vector == Vector2.Zero)
			{
				vector = new Vector2(0f, 1f);
			}
			vector.Y += trailGravity;
			Vector2 vector2 = position + vector * trailSpacing;
			Trail[i].Position = Calc.Approach(Trail[i].Position, vector2, trailCatchupSpeed * Engine.DeltaTime);
			position = Trail[i].Position;
		}
		X = Calc.Clamp(X, originLevelBounds.Left + 4, originLevelBounds.Right - 4);
		Y = Calc.Clamp(Y, originLevelBounds.Top + 4, originLevelBounds.Bottom - 4);
	}

	public override void Render()
	{
		Vector2 position = Position;
		Position = Position.Floor();
		for (int num = Trail.Length - 1; num >= 0; num--)
		{
			Vector2 position2 = Trail[num].Position;
			float num2 = Calc.ClampedMap(num, 0f, Trail.Length - 1, trailBaseSize);
			Draw.Rect(position2.X - num2 / 2f, position2.Y - num2 / 2f, num2, num2, Trail[num].Color);
		}
		base.Render();
		Position = position;
	}
	public override void DebugRender(Camera camera)
	{
		base.DebugRender(camera);
		Draw.Line(Position, Target, Color.White * 0.5f);
		Draw.Rect(Target - new Vector2(2, 2), 4, 4, centerColor * 0.5f);
		Draw.Circle(Start, targetRangeRadius, centerColor * 0.5f, 16);
	}
}

