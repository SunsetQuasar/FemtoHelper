﻿// Celeste.CustomFakeHeart
using System;
using Celeste;
using Celeste.Mod.FemtoHelper;


[CustomEntity("FemtoHelper/OshiroCaller")]

public class OshiroCaller : Entity
{
	private readonly bool flagSet = true;

	private readonly string customFlag = "custom_flag";

	private Sprite sprite;

	public Wiggler ScaleWiggler;

	private Wiggler moveWiggler;

	private Vector2 moveWiggleDir;

	private readonly HoldableCollider crystalCollider;

	private float timer;

	private float bounceSfxDelay;

	private readonly bool repell;

	private readonly bool justMakeOshiroLeave;

	private readonly ParticleType callerParticle = new ParticleType
	{
		Source = GFX.Game["particles/circle"],
		Color = Calc.HexToColor("8484a5"),
		Color2 = Calc.HexToColor("3f3f74"),
		FadeMode = ParticleType.FadeModes.Linear,
		ColorMode = ParticleType.ColorModes.Choose,
		RotationMode = ParticleType.RotationModes.Random,
		LifeMin = 1f,
		LifeMax = 1.5f,
		Size = 1f,
		SizeRange = 0.5f,
		ScaleOut = true,
		SpeedMin = 80f,
		SpeedMax = 140f,
		Friction = 100f,
		Acceleration = new Vector2(0f, 0f),
		SpinMin = 8f,
		SpinMax = 16f
	};
	private readonly ParticleType callerParticle2 = new ParticleType
	{
		Source = GFX.Game["particles/circle"],
		Color = Calc.HexToColor("8484a5"),
		Color2 = Calc.HexToColor("3f3f74"),
		FadeMode = ParticleType.FadeModes.Linear,
		ColorMode = ParticleType.ColorModes.Choose,
		RotationMode = ParticleType.RotationModes.None,
		LifeMin = 2f,
		LifeMax = 2.5f,
		Size = 0.5f,
		SizeRange = 0.2f,
		ScaleOut = true,
		SpeedMin = 10f,
		SpeedMax = 20f,
		Friction = 10f,
		Acceleration = new Vector2(0f, 0f),
	};
	public OshiroCaller(Vector2 position)
		: base(position)
	{
		Add(crystalCollider = new HoldableCollider(OnHoldable));
		Add(new MirrorReflection());
	}

	public OshiroCaller(EntityData data, Vector2 offset) : this(data.Position + offset)
	{
		repell = data.Bool("repell");
		justMakeOshiroLeave = data.Bool("justMakeOshiroLeave");
	}

	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		Add(sprite = FemtoModule.FemtoSpriteBank.Create("oshiroCaller"));
		sprite.Play(repell ? "repeller" : "caller");
		Collider = new Hitbox(16f, 16f, -8f, -8f);
		Add(new PlayerCollider(OnPlayer));
		Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
		{
			sprite.Scale = Vector2.One * (1f + f * 0.25f);
		}));
		Add(new BloomPoint(0.75f, 16f));
		Add(new VertexLight(Calc.HexToColor("BBBBFF"), 1f, 32, 64));
		moveWiggler = Wiggler.Create(0.8f, 2f);
		moveWiggler.StartZero = true;
		Add(moveWiggler);
	}

	public override void Update()
	{
		bounceSfxDelay -= Engine.DeltaTime;
		timer += Engine.DeltaTime;
		sprite.Position = Vector2.UnitY * (float)Math.Sin(timer * 2f) * 2f + moveWiggleDir * moveWiggler.Value * -8f;
		base.Update();
		if (Visible && Scene.OnInterval(0.1f))
		{
			SceneAs<Level>().Particles.Emit(callerParticle2, 1, Center, Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
		}
	}

	public void OnHoldable(Holdable h)
	{
		Player entity = Scene.Tracker.GetEntity<Player>();
		if (!Visible || !h.Dangerous(crystalCollider)) return;
		if (repell)
		{
			if (Scene.Tracker.GetEntity<AngryOshiro>() != null)
			{
				Level level = SceneAs<Level>();
				(Scene as Level).Session.SetFlag(customFlag, flagSet);
				level.Displacement.AddBurst(Position, 1, 8, 48, 0.5f);
				Celeste.Celeste.Freeze(0.1f);
				level.Flash(Color.White * 0.25f, drawPlayerOver: true);
				SceneAs<Level>().Shake();
				if (justMakeOshiroLeave)
				{
					Scene.Tracker.GetEntity<AngryOshiro>()?.Leave();
					SceneAs<Level>().Particles.Emit(callerParticle, 1, Center, Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
					Audio.Play("event:/FemtoHelper/oshirorepeller_disappear", Scene.Tracker.GetEntity<AngryOshiro>().Position);
				}
				else
				{
					level.Displacement.AddBurst(Scene.Tracker.GetEntity<AngryOshiro>().Position, 0.75f, 8, 64, 1);
					Audio.Play("event:/FemtoHelper/oshirorepeller_disappear", Position);
					for (int i = 0; i < 20; i++)
					{
						SceneAs<Level>().Particles.Emit(callerParticle, 1, Center, Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
						SceneAs<Level>().Particles.Emit(callerParticle, 1, Scene.Tracker.GetEntity<AngryOshiro>().Position, Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
						SceneAs<Level>().Particles.Emit(callerParticle, 1, Scene.Tracker.GetEntity<AngryOshiro>().Position, Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
					}
					Scene.Tracker.GetEntity<AngryOshiro>()?.StopControllingTime();
					Scene.Tracker.GetEntity<AngryOshiro>()?.RemoveSelf();
					Distort.GameRate = 1f;
					Engine.TimeRate = 1f;
					Distort.anxiety = 0f;
				}
				RemoveSelf();
			}
		}
		else
		{
			Level level = SceneAs<Level>();
			(Scene as Level).Session.SetFlag(customFlag, flagSet);
			level.Displacement.AddBurst(Position, 1, 8, 48, 0.5f);
			Audio.Play("event:/FemtoHelper/oshirocaller_hit", Position);
			for (int i = 0; i < 20; i++)
			{
				SceneAs<Level>().Particles.Emit(callerParticle, 1, Center, Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
			}
			Celeste.Celeste.Freeze(0.1f);
			level.Flash(Color.White * 0.25f, drawPlayerOver: true);
			Vector2 position = new Vector2(level.Bounds.Left - 32, level.Bounds.Top + level.Bounds.Height / 2);
			Scene.Add(new AngryOshiro(position, fromCutscene: false));
			SceneAs<Level>().Shake();
			RemoveSelf();
		}
	}

	public void OnPlayer(Player player)
	{
		if (!Visible || (Scene as Level).Frozen) return;
		if (player.DashAttacking)
		{
			if (repell)
			{
				if (Scene.Tracker.GetEntity<AngryOshiro>() != null)
				{
					Level level = SceneAs<Level>();
					(Scene as Level).Session.SetFlag(customFlag, flagSet);
					level.Displacement.AddBurst(Position, 1, 8, 48, 0.5f);
					Celeste.Celeste.Freeze(0.1f);
					level.Flash(Color.White * 0.25f, drawPlayerOver: true);
					SceneAs<Level>().Shake();
					if (justMakeOshiroLeave)
					{
						Scene.Tracker.GetEntity<AngryOshiro>()?.Leave();
						SceneAs<Level>().Particles.Emit(callerParticle, 1, Center, Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
						Audio.Play("event:/FemtoHelper/oshirorepeller_disappear", Scene.Tracker.GetEntity<AngryOshiro>().Position);
					}
					else
					{
						level.Displacement.AddBurst(Scene.Tracker.GetEntity<AngryOshiro>().Position, 0.75f, 8, 64, 1);
						Audio.Play("event:/FemtoHelper/oshirorepeller_disappear", Position);
						for (int i = 0; i < 20; i++)
						{
							SceneAs<Level>().Particles.Emit(callerParticle, 1, Center, Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
							SceneAs<Level>().Particles.Emit(callerParticle, 1, Scene.Tracker.GetEntity<AngryOshiro>().Position, Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
							SceneAs<Level>().Particles.Emit(callerParticle, 1, Scene.Tracker.GetEntity<AngryOshiro>().Position, Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
						}
						Scene.Tracker.GetEntity<AngryOshiro>()?.StopControllingTime();
						Scene.Tracker.GetEntity<AngryOshiro>()?.RemoveSelf();
						Distort.GameRate = 1f;
						Engine.TimeRate = 1f;
						Distort.anxiety = 0f;
					}
					RemoveSelf();
				}
			} else
			{
				Level level = SceneAs<Level>();
				(Scene as Level).Session.SetFlag(customFlag, flagSet);
				level.Displacement.AddBurst(Position, 1, 8, 48, 0.5f);
				Audio.Play("event:/FemtoHelper/oshirocaller_hit", Position);
				for (int i = 0; i < 20; i++)
				{
					SceneAs<Level>().Particles.Emit(callerParticle, 1, Center, Vector2.One * 8f, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
				}
				Celeste.Celeste.Freeze(0.1f);
				level.Flash(Color.White * 0.25f, drawPlayerOver: true);
				Vector2 position = new Vector2(level.Bounds.Left - 32, level.Bounds.Top + level.Bounds.Height / 2);
				Scene.Add(new AngryOshiro(position, fromCutscene: false));
				SceneAs<Level>().Shake();
				RemoveSelf();
			}
		}
		if (bounceSfxDelay <= 0f)
		{
			Audio.Play("event:/game/03_resort/forcefield_bump", Position);
			bounceSfxDelay = 0.1f;
		}
		if (!player.DashAttacking || (repell && Scene.Tracker.GetEntity<AngryOshiro>() == null))
		{
			player.PointBounce(Center);
		}
		moveWiggler.Start();
		ScaleWiggler.Start();
		moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
		Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
	}
}

