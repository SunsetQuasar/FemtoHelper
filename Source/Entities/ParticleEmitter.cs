using System;
using Celeste.Mod.FemtoHelper.Utils;

namespace Celeste.Mod.FemtoHelper;

[CustomEntity("FemtoHelper/ParticleEmitter")]
[Tracked]
public class ParticleEmitter : Entity
{
	public readonly ParticleType EmitterParticle;

	public readonly float ParticleSpawnSpread;

	public float SpawnTimer = 0f;

	public readonly int ParticleCount;

	public readonly float SpawnInterval;

	public readonly float SpawnChance;

	public readonly float ParticleAngle;

	public readonly float ParticleAngleRange;

	public readonly bool IsFg = false;

	public readonly string Flag;

	public readonly string Tag;

	public readonly bool AttachToPlayer;
	public readonly Vector2 AttachToPlayerOffset;

	public ParticleEmitter(EntityData data, Vector2 offset)
		: base(data.Position + offset)
	{
		Add(new BloomPoint(data.Float("BloomAlpha"), data.Float("BloomRadius", 6f)));
		ParticleSpawnSpread = data.Float("particleSpawnSpread");
		ParticleCount = data.Int("particleCount");
		SpawnInterval = data.Float("spawnInterval");
		SpawnChance = data.Float("spawnChance");
		ParticleAngle = data.Float("particleAngle");
		ParticleAngleRange = data.Float("particleAngleRange");
		AttachToPlayer = data.Bool("attachToPlayer", false);
		AttachToPlayerOffset = new(data.Float("attachToPlayerOffsetX", 0f), data.Float("attachToPlayerOffsetY", 0f));
        IsFg = data.Bool("foreground", false);
		Flag = data.Attr("flag", "");
		Tag = data.Attr("tag", "");
		string[] texString = data.Attr("particleTexture").Split(',');
		Chooser<MTexture> texchoice = new Chooser<MTexture>();
		foreach (var t in texString)
		{
			texchoice.Add(GFX.Game[t], 1);
		}
		if (!data.Bool("noTexture", false))
		{
			EmitterParticle = new ParticleType
			{
				SourceChooser = texchoice,
				Color = Calc.HexToColorWithAlpha(data.Attr("particleColor", "40FF9080")) * data.Float("particleAlpha"),
				Color2 = Calc.HexToColorWithAlpha(data.Attr("particleColor2", "20408000")) * data.Float("particleAlpha"),
				FadeMode = data.Int("particleFadeMode") != 0 ? data.Int("particleFadeMode") == 1 ? ParticleType.FadeModes.Linear : data.Int("particleFadeMode") == 2 ? ParticleType.FadeModes.Late : ParticleType.FadeModes.InAndOut : ParticleType.FadeModes.None,
				ColorMode = data.Int("particleColorMode") != 0 ? data.Int("particleColorMode") == 1 ? ParticleType.ColorModes.Choose : data.Int("particleColorMode") == 2 ? ParticleType.ColorModes.Blink : ParticleType.ColorModes.Fade : ParticleType.ColorModes.Static,
				RotationMode = data.Int("particleRotationMode") != 0 ? data.Int("particleRotationMode") == 1 ? ParticleType.RotationModes.Random : ParticleType.RotationModes.SameAsDirection : ParticleType.RotationModes.None,
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
		} 
		else
		{
			EmitterParticle = new ParticleType
			{
				Color = Calc.HexToColorWithAlpha(data.Attr("particleColor", "40FF9080")) * data.Float("particleAlpha"),
				Color2 = Calc.HexToColorWithAlpha(data.Attr("particleColor2", "20408000")) * data.Float("particleAlpha"),
				FadeMode = data.Int("particleFadeMode") != 0 ? data.Int("particleFadeMode") == 1 ? ParticleType.FadeModes.Linear : data.Int("particleFadeMode") == 2 ? ParticleType.FadeModes.Late : ParticleType.FadeModes.InAndOut : ParticleType.FadeModes.None,
				ColorMode = data.Int("particleColorMode") != 0 ? data.Int("particleColorMode") == 1 ? ParticleType.ColorModes.Choose : data.Int("particleColorMode") == 2 ? ParticleType.ColorModes.Blink : ParticleType.ColorModes.Fade : ParticleType.ColorModes.Static,
				RotationMode = data.Int("particleRotationMode") != 0 ? data.Int("particleRotationMode") == 1 ? ParticleType.RotationModes.Random : ParticleType.RotationModes.SameAsDirection : ParticleType.RotationModes.None,
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
		}
	}

	public override void Update()
	{
		Level level = Scene as Level;
		base.Update();
		if (AttachToPlayer)
		{
			Player player = Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				Position = player.Position + AttachToPlayerOffset;
			}
		}
		SpawnTimer += 1f;
		if (!Visible || !Scene.OnInterval(SpawnInterval))
		{
			return;
		}
		
		if ((Scene as Level).FancyCheckFlag(Flag))
		{
			for (int i = 0; i < ParticleCount; i++)
			{
				if (Calc.Random.NextFloat() < SpawnChance / 100f)
				{
					float num = ParticleAngleRange / 360f * ((float)Math.PI * 2f);
					if (IsFg)
					{
						SceneAs<Level>().ParticlesFG.Emit(EmitterParticle, 1, Center, Vector2.One * ParticleSpawnSpread, ParticleAngle / 360f * ((float)Math.PI * 2f) + (Calc.Random.NextFloat() * num - num / 2f));
					}
					else
					{
						SceneAs<Level>().ParticlesBG.Emit(EmitterParticle, 1, Center, Vector2.One * ParticleSpawnSpread, ParticleAngle / 360f * ((float)Math.PI * 2f) + (Calc.Random.NextFloat() * num - num / 2f));
					};
				}
			}
		}
	}

	public override void DebugRender(Camera camera)
	{
		base.DebugRender(camera);
		Draw.HollowRect(Position, 4f, 4f, Color.Purple);
	}
}
