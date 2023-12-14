using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.FemtoHelper
{
	[CustomEntity("FemtoHelper/CustomFakeHeart")]
	public class CustomFakeHeart : Entity
	{
		private float RespawnTime = 3f;

		private int heartBehavior = 0;

		private bool flagSet = true;

		private string customFlag = "custom_flag";

		private bool fakeSounds = false;

		private bool doesPulse = true;

		private float freezeTime = 0.1f;

		private string audioEvent = "event:/game/06_reflection/boss_spikes_burst";

		private float flashOpacity = 1f;

		private string flashColor = "ffffff";

		private int particleCount = 12;

		private float particleSpawnSpread = 8f;

		private float burstDuration = 1.5f;

		private float burstRadiusFrom = 4f;

		private float burstRadiusTo = 40f;

		private float burstAlpha = 0.5f;

		private Sprite sprite;

		private ParticleType shineParticle;

		private ParticleType customParticle;

		public Wiggler ScaleWiggler;

		private Wiggler moveWiggler;

		private Vector2 moveWiggleDir;

		private BloomPoint bloom;

		private HoldableCollider crystalCollider;

		private float timer;

		private float bounceSfxDelay;

		private float respawnTimer;

		private bool miniMode;

		private string shineParticleColor;

		private string spriteDirectory;

		private bool removeBloomOnShatter;

		private bool nobloom;

		private bool breakOnContact;

		private bool noWiggle;

		private bool silentPulse;

		public bool collectFlash;

		public bool flashOverPlayer;

		// Legacy
		private int heartColor = 0;

		public CustomFakeHeart(Vector2 position)
			: base(position)
		{
			Add(crystalCollider = new HoldableCollider(OnHoldable));
			Add(new MirrorReflection());
		}

		public CustomFakeHeart(EntityData data, Vector2 offset) : this(data.Position + offset)
		{
			RespawnTime = data.Float("respawnTime");
			heartBehavior = data.Int("heartBehavior");
			flagSet = data.Bool("setFlag");
			customFlag = data.Attr("flag");
			fakeSounds = data.Bool("useFakeSounds");
			doesPulse = data.Bool("pulse");
			freezeTime = data.Float("freezeTime");
			audioEvent = data.Attr("audioEvent");
			flashOpacity = data.Float("flashOpacity");
			flashColor = data.Attr("flashColor");
			particleCount = data.Int("particleCount");
			particleSpawnSpread = data.Float("particleSpawnSpread");
			burstDuration = data.Float("burstDuration");
			burstRadiusFrom = data.Float("burstRadiusFrom");
			burstRadiusTo = data.Float("burstRadiusTo");
			burstAlpha = data.Float("burstAlpha");
			miniMode = data.Bool("miniHeartMode", false);
			nobloom = data.Bool("noBloom", false);
			breakOnContact = data.Bool("breakOnContact", false);
			noWiggle = data.Bool("noWiggle", false);
			silentPulse = data.Bool("silentPulse", false);
            collectFlash = data.Bool("collectFlash", true);
			flashOverPlayer = data.Bool("flashOverPlayer", false);

            customParticle = new ParticleType
			{
				Source = GFX.Game[data.Attr("particleTexture")],
				Color = Calc.HexToColor(data.Attr("particleColor")),
				Color2 = Calc.HexToColor(data.Attr("particleColor2")),
				FadeMode =
				data.Int("particleFadeMode") == 0 ? ParticleType.FadeModes.None :
				data.Int("particleFadeMode") == 1 ? ParticleType.FadeModes.Linear :
				data.Int("particleFadeMode") == 2 ? ParticleType.FadeModes.Late :
				ParticleType.FadeModes.InAndOut,
				ColorMode =
				data.Int("particleColorMode") == 0 ? ParticleType.ColorModes.Static :
				data.Int("particleColorMode") == 1 ? ParticleType.ColorModes.Choose :
				data.Int("particleColorMode") == 2 ? ParticleType.ColorModes.Blink :
				ParticleType.ColorModes.Fade,
				RotationMode =
				data.Int("particleRotationMode") == 0 ? ParticleType.RotationModes.None :
				data.Int("particleRotationMode") == 1 ? ParticleType.RotationModes.Random :
				ParticleType.RotationModes.SameAsDirection,
				LifeMin = data.Float("particleLifespanMin"),
				LifeMax = data.Float("particleLifespanMax"),
				Size = data.Float("particleSize"),
				SizeRange = data.Float("particleSizeRange"),
				ScaleOut = data.Bool("particleScaleOut"),
				SpeedMin = data.Float("particleSpeedMin"),
				SpeedMax = data.Float("particleSpeedMax"),
				Friction = data.Float("particleFriction"),
				Acceleration = new Vector2(data.Float("particleAccelX"), data.Float("particleAccelY")),
				SpinFlippedChance = data.Bool("particleFlipChance"),
				SpinMin = data.Float("particleSpinSpeedMin"),
				SpinMax = data.Float("particleSpinSpeedMax")
			};

			// New
			removeBloomOnShatter = data.Bool("removeBloomOnShatter", false);
			spriteDirectory = data.Attr("spriteDirectory", "collectables/heartGem/0");
			shineParticleColor = data.Attr("shineParticleColor", "5caefa");

			// Legacy
			heartColor = data.Int("heartColor", -1);
			if (heartColor != -1)
			{
				heartColor = Calc.Clamp(heartColor, 0, 2);
			}
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (heartColor == -1)
			{
				/*
				 <heartgem0 path="collectables/heartGem/0/">
					<Center />
					<Loop id="idle" path="" frames="0" />
					<Loop id="spin" path="" frames="0*10,1-13" delay="0.1"/>
					<Loop id="fastspin" path="" delay="0.1"/>
				 </heartgem0>
				*/
				Add(sprite = new Sprite(GFX.Game, spriteDirectory + "/"));
				sprite.AddLoop("idle", "", 0f, 0);
				sprite.AddLoop("spin", "", 0.1f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13);
				sprite.AddLoop("idle", "", 0.1f);
				sprite.CenterOrigin();
			}
			else
			{
				Add(sprite = GFX.SpriteBank.Create("heartgem" + heartColor));
			}
			sprite.Play("spin");
			sprite.OnLoop = delegate (string anim)
			{
				if (Visible && anim == "spin")
				{
					if (doesPulse)
					{
						if (!silentPulse)
                        {
							if (fakeSounds)
							{
								Audio.Play("event:/new_content/game/10_farewell/fakeheart_pulse", Position);
							}
							else
							{
								if (miniMode)
								{
									Audio.Play("event:/game/general/crystalheart_pulse", Position);
								}
								else
								{
									Audio.Play("event:/SC2020_heartShard_pulse", Position);
								}
							}
						}
						ScaleWiggler.Start();
						(base.Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
					}
				}
			};
			Color value;
			switch ((AreaMode)heartColor)
			{
				case AreaMode.Normal:
					value = Color.Aqua;
					shineParticle = HeartGem.P_BlueShine;
					break;
				case AreaMode.BSide:
					value = Color.Red;
					shineParticle = HeartGem.P_RedShine;
					break;
				case AreaMode.CSide:
					value = Color.Gold;
					shineParticle = HeartGem.P_GoldShine;
					break;
				default:
					value = Calc.HexToColor(shineParticleColor);
					shineParticle = new ParticleType(HeartGem.P_BlueShine);
					shineParticle.Color = value;
					break;
			}
			value = Color.Lerp(value, Color.White, 0.5f);

			if (miniMode)
			{
				base.Collider = new Hitbox(12f, 12f, -6f, -6f);
				Add(bloom = new BloomPoint(0.75f, 12f));
				Add(new VertexLight(value, 1f, 24, 48));
			}
			else
			{
				base.Collider = new Hitbox(16f, 16f, -8f, -8f);
				Add(bloom = new BloomPoint(0.75f, 16f));
				Add(new VertexLight(value, 1f, 32, 64));
			}
			Add(new PlayerCollider(OnPlayer));
			Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
			{
				sprite.Scale = Vector2.One * (1f + f * 0.25f);
			}));
			moveWiggler = Wiggler.Create(0.8f, 2f);
			moveWiggler.StartZero = true;
			Add(moveWiggler);
			if (nobloom)
            {
				bloom.Visible = false;
            }
		}

		public override void Update()
		{
			bounceSfxDelay -= Engine.DeltaTime;
			timer += Engine.DeltaTime;
			if (noWiggle)
			{
				sprite.Position = Vector2.UnitY * 0 * 2f + moveWiggleDir * moveWiggler.Value * -8f;
			}
			else
            {
				sprite.Position = Vector2.UnitY * (float)Math.Sin(timer * 2f) * 2f + moveWiggleDir * moveWiggler.Value * -8f;

			}
			if (respawnTimer > 0f)
			{
				respawnTimer -= Engine.DeltaTime;
				if (respawnTimer <= 0f)
				{
					Collidable = (Visible = true);
					if (!nobloom){
						bloom.Visible = true;
					}
					ScaleWiggler.Start();
					Audio.Play("event:/char/badeline/booster_reappear", Position);
					SceneAs<Level>().Particles.Emit(shineParticle, 25, base.Center, Vector2.One * 8f);
				}
			}
			base.Update();
			if (Visible && base.Scene.OnInterval(0.1f))
			{
				SceneAs<Level>().Particles.Emit(shineParticle, 1, base.Center, Vector2.One * 8f);
			}
		}

		public void OnHoldable(Holdable h)
		{
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			if (Visible && h.Dangerous(crystalCollider))
			{
				Collect(entity, h.GetSpeed().Angle());
			}
		}

		public void OnPlayer(Player player)
		{
			if (!Visible || (base.Scene as Level).Frozen)
			{
				return;
			}
			if (player.DashAttacking || breakOnContact)
			{
				if (heartBehavior == 0)
				{
					(base.Scene as Level).Session.SetFlag(customFlag, flagSet);
					SceneAs<Level>().Displacement.AddBurst(Position, burstDuration, burstRadiusFrom, burstRadiusTo, burstAlpha);
					Audio.Play(audioEvent, Position);
					Celeste.Freeze(freezeTime);
					if (collectFlash)
					{
						SceneAs<Level>().Flash(Calc.HexToColor(flashColor) * flashOpacity, !flashOverPlayer);
					}
					for (int i = 0; i < particleCount; i++)
					{
						SceneAs<Level>().Particles.Emit(customParticle, 1, base.Center, Vector2.One * particleSpawnSpread, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
					}
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
					Collect(player, player.Speed.Angle());
					if (removeBloomOnShatter)
					{
						bloom.Visible = false;
					}
					return;
				}
				else if (heartBehavior == 1)
				{
					(base.Scene as Level).Session.SetFlag(customFlag, flagSet);
					player.Die(player.Position - Position, false, true);
					SceneAs<Level>().Displacement.AddBurst(Position, burstDuration, burstRadiusFrom, burstRadiusTo, burstAlpha); Audio.Play(audioEvent, Position);
					Celeste.Freeze(freezeTime);
					if (collectFlash)
					{
						SceneAs<Level>().Flash(Calc.HexToColor(flashColor) * flashOpacity, !flashOverPlayer);
					}
						for (int i = 0; i < particleCount; i++)
					{
						SceneAs<Level>().Particles.Emit(customParticle, 1, base.Center, Vector2.One * particleSpawnSpread, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
					}
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
					Collect(player, player.Speed.Angle());
					if (removeBloomOnShatter)
					{
						bloom.Visible = false;
					}
					return;
				}
				else if (heartBehavior == 2 || heartBehavior == 3)
				{
					(base.Scene as Level).Session.SetFlag(customFlag, flagSet);
					SceneAs<Level>().Displacement.AddBurst(Position, burstDuration, burstRadiusFrom, burstRadiusTo, burstAlpha);
					Audio.Play(audioEvent, Position);
					Celeste.Freeze(freezeTime);
					if (collectFlash)
					{
						SceneAs<Level>().Flash(Calc.HexToColor(flashColor) * flashOpacity, !flashOverPlayer);
					}
						for (int i = 0; i < particleCount; i++)
					{
						SceneAs<Level>().Particles.Emit(customParticle, 1, base.Center, Vector2.One * particleSpawnSpread, Calc.Random.NextFloat() * ((float)Math.PI * 2f));
					}
				}
			}
			if (bounceSfxDelay <= 0f)
			{
				if (fakeSounds)
				{
					Audio.Play("event:/new_content/game/10_farewell/fakeheart_bounce", Position);
				}
				else
				{
					Audio.Play("event:/game/general/crystalheart_bounce", Position);
				}
				bounceSfxDelay = 0.1f;
			}
			if (heartBehavior != 3)
			{
				player.PointBounce(base.Center);
			}
			else if (player.DashAttacking == false && !breakOnContact)
			{
				player.PointBounce(base.Center);
			}
			moveWiggler.Start();
			ScaleWiggler.Start();
			moveWiggleDir = (base.Center - player.Center).SafeNormalize(Vector2.UnitY);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
		}

		private void Collect(Player player, float angle)
		{
			if (Collidable)
			{
				Collidable = (Visible = false);
				respawnTimer = RespawnTime;
				Celeste.Freeze(0.05f);
				SceneAs<Level>().Shake();
				SlashFx.Burst(Position, angle);
				player?.RefillDash();
			}
		}
	}
}