// Celeste.SummitCheckpoint
using System;
using Celeste;

[CustomEntity("FemtoHelper/FoolSpaceCustomCheckpoint")]
public class FoolSpaceCustomCheckpoint : Entity
{
	public class ConfettiRenderer : Entity
	{
		private struct Particle
		{
			public Vector2 Position;

			public Color Color;

			public Vector2 Speed;

			public float Timer;

			public float Percent;

			public float Duration;

			public float Alpha;

			public float Approach;
		}


		private readonly Particle[] particles = new Particle[30];

		public ConfettiRenderer(String color, Vector2 position)
			: base(position)
		{
			Depth = -10010;
			string[] array = color.Split(',');
			var confettiColors = new Color[array.Length];
			for (int i = 0; i < confettiColors.Length; i++)
			{
				confettiColors[i] = Calc.HexToColor(array[i]);
			}
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position = Position + new Vector2(Calc.Random.Range(-3, 3), Calc.Random.Range(-3, 3));
				particles[i].Color = Calc.Random.Choose(confettiColors);
				particles[i].Timer = Calc.Random.NextFloat();
				particles[i].Duration = Calc.Random.Range(2, 4);
				particles[i].Alpha = 1f;
				float angleRadians = -(float)Math.PI / 2f + Calc.Random.Range(-0.5f, 0.5f);
				int num = Calc.Random.Range(140, 220);
				particles[i].Speed = Calc.AngleToVector(angleRadians, num);
			}
		}

		public override void Update()
		{
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position += particles[i].Speed * Engine.DeltaTime;
				particles[i].Speed.X = Calc.Approach(particles[i].Speed.X, 0f, 80f * Engine.DeltaTime);
				particles[i].Speed.Y = Calc.Approach(particles[i].Speed.Y, 20f, 500f * Engine.DeltaTime);
				particles[i].Timer += Engine.DeltaTime;
				particles[i].Percent += Engine.DeltaTime / particles[i].Duration;
				particles[i].Alpha = Calc.ClampedMap(particles[i].Percent, 0.9f, 1f, 1f, 0f);
				if (particles[i].Speed.Y > 0f)
				{
					particles[i].Approach = Calc.Approach(particles[i].Approach, 5f, Engine.DeltaTime * 16f);
				}
			}
		}

		public override void Render()
		{
			for (int i = 0; i < particles.Length; i++)
			{
				float num = 0f;
				Vector2 position = particles[i].Position;
				if (particles[i].Speed.Y < 0f)
				{
					num = particles[i].Speed.Angle();
				}
				else
				{
					num = (float)Math.Sin(particles[i].Timer * 4f) * 1f;
					position += Calc.AngleToVector((float)Math.PI / 2f + num, particles[i].Approach);
				}
				GFX.Game["particles/confetti"].DrawCentered(position + Vector2.UnitY, Color.Black * (particles[i].Alpha * 0.5f), 1f, num);
				GFX.Game["particles/confetti"].DrawCentered(position, particles[i].Color * particles[i].Alpha, 1f, num);
			}
		}
	}

	private const string Flag = "summit_checkpoint_";

	public bool Activated;

	public readonly string Number;

	public readonly string NumberB;

	public readonly string SpriteRoot;

	private Vector2 respawn;

	private readonly MTexture baseEmpty;

	private readonly MTexture baseToggle;

	private readonly MTexture baseActive;

	private readonly MTexture numbersEmpty;

	private readonly MTexture numbersEmptyB;

	private readonly MTexture numbersActive;

	private readonly MTexture numbersActiveB;

	public readonly string Colors;

	public FoolSpaceCustomCheckpoint(EntityData data, Vector2 offset)
		: base(data.Position + offset)
	{
		Colors = data.Attr("colors", "fe2074,205efe,cefe20");
		Number = data.Attr("digitA");
		NumberB = data.Attr("digitB");
		SpriteRoot = data.Attr("spriteRoot", "scenery/foolspace_checkpoint/");
		baseEmpty = GFX.Game[SpriteRoot + "base00"];
		baseToggle = GFX.Game[SpriteRoot + "base01"];
		baseActive = GFX.Game[SpriteRoot + "base02"];
		numbersEmpty = GFX.Game[SpriteRoot + "numberbg0" + Number];
		numbersEmptyB = GFX.Game[SpriteRoot + "numberbg0" + NumberB];
		numbersActive = GFX.Game[SpriteRoot + "number0" + Number];
		numbersActiveB = GFX.Game[SpriteRoot + "number0" + NumberB];
		Collider = new Hitbox(32f, 32f, -16f, -8f);
		Depth = 8999;
	}

	public override void Added(Scene scene)
	{
		base.Added(scene);
		if ((scene as Level).Session.GetFlag("summit_checkpoint_" + Number + NumberB))
		{
			Activated = true;
		}
		respawn = SceneAs<Level>().GetSpawnPoint(Position);
	}

	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		if (Activated || !CollideCheck<Player>()) return;
		Activated = true;
		Level obj = Scene as Level;
		obj.Session.SetFlag("summit_checkpoint_" + Number + NumberB);
		obj.Session.RespawnPoint = respawn;
	}

	public override void Update()
	{
		if (!Activated)
		{
			Player player = CollideFirst<Player>();
			if (player != null && player.OnGround() && player.Speed.Y >= 0f)
			{
				Level obj = Scene as Level;
				Activated = true;
				obj.Session.SetFlag("summit_checkpoint_" + Number + NumberB);
				obj.Session.RespawnPoint = respawn;
				obj.Session.UpdateLevelStartDashes();
				obj.Session.HitCheckpoint = true;
				obj.Displacement.AddBurst(Position, 0.5f, 4f, 24f, 0.5f);
				obj.Add(new ConfettiRenderer(Colors, Position));
				Audio.Play("event:/game/07_summit/checkpoint_confetti", Position);
			}
		}
	}

	public override void Render()
	{
		MTexture obj = Activated ? numbersActive : numbersEmpty;
		MTexture objB = Activated ? numbersActiveB : numbersEmptyB;
		MTexture mTexture = baseActive;
		if (!Activated)
		{
			mTexture = Scene.BetweenInterval(0.25f) ? baseEmpty : baseToggle;
		}
		mTexture.Draw(Position - new Vector2(mTexture.Width / 2 + 1, mTexture.Height / 2));
		obj.DrawJustified(Position + new Vector2(-1f, 1f), new Vector2(1f, 0f));
		objB.DrawJustified(Position + new Vector2(0f, 1f), new Vector2(0f, 0f));
	}
}
