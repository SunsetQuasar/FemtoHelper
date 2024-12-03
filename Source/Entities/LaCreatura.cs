// Celeste.DashBlock
using Celeste;
using Celeste.Mod.FemtoHelper;
using System;
using System.Linq;

[CustomEntity("FemtoHelper/LaCreatura")]
public class LaCreatura : Entity
{
	private struct TrailNode
	{
		public Vector2 Position;

		public Color Color;
	}

	private readonly TrailNode[] trail;

	private readonly Vector2 start;

	private Vector2 target;

	private float targetTimer;

	private Vector2 speed;

	private Vector2 bump;

	private Player following;

	private Vector2 followingOffset;

	private float followingTime;

	private Color orbColor;

	private readonly Color centerColor;

	private readonly Sprite sprite;

	private readonly float acceleration;

	private readonly float maxSpeed;
	
	//private readonly int spawn;

	private Rectangle originLevelBounds;

	private BloomPoint bloom;

	private VertexLight light;

	private bool facingRight;

	public readonly float BloomSize;
	public readonly float BloomAlpha;
	public readonly int LightStartFade;
	public readonly int LightEndFade;
	public readonly float LightAlpha;
	public readonly float TargetRangeRadius;
	public readonly float MinFollowTime;
	public readonly float MaxFollowTime;

	public LaCreatura(EntityData data, Vector2 offset) : base(data.Position + offset)
	{
		acceleration = data.Float("acceleration", 90f);
		maxSpeed = data.Float("maxSpeed", 40f);

		if (data.Has("colors"))
		{
            centerColor = Calc.Random.Choose(data.Attr("colors", "74db93,dbc874,74a1db,e0779f,9677e0")
				.Split(',')
				.Select(str => Calc.HexToColor(str.Trim()))
				.ToArray());
		} 
		else
		{
            centerColor = Calc.Random.Choose(Calc.HexToColor("74db93"), Calc.HexToColor("dbc874"), Calc.HexToColor("74a1db"), Calc.HexToColor("e0779f"), Calc.HexToColor("9677e0"));
        }
        Tag = Tags.TransitionUpdate;
        Depth = -13010;
        Collider = new Hitbox(20f, 20f, -10f, -10f);
        start = data.Position + offset;
        targetTimer = 0f;
		if (data.Bool("randomStartPos", true))
		{
            GetRandomTarget();
            Position = target;
        }
        Add(new PlayerCollider(OnPlayer));
        orbColor = Calc.HexToColor("b0e6ff");
        Color value = Color.Lerp(centerColor, Calc.HexToColor("bde4ee"), 0.5f);
        Color value2 = Color.Lerp(centerColor, Calc.HexToColor("2f2941"), 0.5f);
        trail = new TrailNode[6];
        for (int i = 0; i < 6; i++)
        {
            trail[i] = new TrailNode
            {
                Position = Position,
                Color = Color.Lerp(value, value2, i / 4f) * (0.2f - 0.1f * i / 6)
            };
        }
        Add(sprite = FemtoModule.FemtoSpriteBank.Create("butterfly"));
		BloomSize = data.Float("bloomRadius", 16f);
		BloomAlpha = data.Float("bloomAlpha", 0.75f);
		LightStartFade = data.Int("lightStartFade", 12);
		LightEndFade = data.Int("lightEndFade", 24);
		LightAlpha = data.Float("lightAlpha", 1f);
		TargetRangeRadius = data.Float("targetRangeRadius", 32f);
		MinFollowTime = data.Float("minFollowTime", 6f);
        MaxFollowTime = data.Float("maxFollowTime", 12f);
    }

	public override void Added(Scene scene)
	{
		base.Added(scene);
		sprite.Play("right");
		sprite.Color = centerColor;
		if (BloomAlpha > 0) Add(bloom = new BloomPoint(BloomAlpha, BloomSize));
        if (LightAlpha > 0) Add(light = new VertexLight(centerColor, LightAlpha, LightStartFade, LightEndFade));

		originLevelBounds = (scene as Level).Bounds;
	}
	private void OnPlayer(Player player)
	{
		Vector2 vector = (Position - player.Center).SafeNormalize(player.Speed.Length() * 0.3f);
		if (!(vector.LengthSquared() > bump.LengthSquared())) return;
		bump = vector;
		if (!((player.Center - start).Length() < 200f) || !(MinFollowTime + MaxFollowTime > 0)) return;
		following = player;
		followingTime = Calc.Random.Range(MinFollowTime, MaxFollowTime);
		GetFollowOffset();
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
			float length = Calc.Random.NextFloat(TargetRangeRadius);
			float angleRadians = Calc.Random.NextFloat((float)Math.PI * 2f);
			target = start + Calc.AngleToVector(angleRadians, length);
		}
		while ((value - target).Length() < TargetRangeRadius / 4f);
	}

	public override void Update()
	{
		base.Update();
		switch (speed.X)
		{
			case >= 0 when facingRight == false:
				sprite.Play("turn_right");
				facingRight = true;
				break;
			case < 0 when facingRight == true:
				sprite.Play("turn_left");
				facingRight = false;
				break;
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
		speed += value * (following == null ? acceleration : acceleration * (1 + 1/3)) * Engine.DeltaTime;
		speed = speed.SafeNormalize() * Math.Min(speed.Length(), following == null ? maxSpeed : maxSpeed + 30f);
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
		X = Calc.Clamp(X, originLevelBounds.Left + 4, originLevelBounds.Right - 4);
		Y = Calc.Clamp(Y, originLevelBounds.Top + 4, originLevelBounds.Bottom - 4);
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
		Draw.Rect(target - new Vector2(2, 2), 4, 4, centerColor * 0.5f);
        Draw.Circle(start, TargetRangeRadius, centerColor * 0.5f, 16);
    }
}

