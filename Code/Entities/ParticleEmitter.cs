using System;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.FemtoHelper
{
	[CustomEntity("FemtoHelper/ParticleEmitter")]
	[Tracked]
	public class ParticleEmitter : Entity
	{
		public ParticleType emitterParticle;

		public float particleSpawnSpread;

		public float spawnTimer = 0f;

		public int particleCount;

		public float spawnInterval;

		public float spawnChance;

		public float particleAngle;

		public float particleAngleRange;

		public bool isFG = false;

		public string flag;

		public string tag;

		public ParticleEmitter(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(new BloomPoint(data.Float("BloomAlpha"), data.Float("BloomRadius", 6f)));
			particleSpawnSpread = data.Float("particleSpawnSpread");
			particleCount = data.Int("particleCount");
			spawnInterval = data.Float("spawnInterval");
			spawnChance = data.Float("spawnChance");
			particleAngle = data.Float("particleAngle");
			particleAngleRange = data.Float("particleAngleRange");
			isFG = data.Bool("foreground", false);
			flag = data.Attr("flag", "");
			tag = data.Attr("tag", "");
			string[] texString = data.Attr("particleTexture").Split(',');
			Chooser<MTexture> texchoice = new Chooser<MTexture>();
			for (int i = 0; i < texString.Length; i++)
            {
				texchoice.Add(GFX.Game[texString[i]], 1);
			}
			if (!data.Bool("noTexture", false))
            {
				emitterParticle = new ParticleType
				{
					SourceChooser = texchoice,
					Color = Calc.HexToColorWithAlpha(data.Attr("particleColor", "40FF9080")) * data.Float("particleAlpha"),
					Color2 = Calc.HexToColorWithAlpha(data.Attr("particleColor2", "20408000")) * data.Float("particleAlpha"),
					FadeMode = ((data.Int("particleFadeMode") != 0) ? ((data.Int("particleFadeMode") == 1) ? ParticleType.FadeModes.Linear : ((data.Int("particleFadeMode") == 2) ? ParticleType.FadeModes.Late : ParticleType.FadeModes.InAndOut)) : ParticleType.FadeModes.None),
					ColorMode = ((data.Int("particleColorMode") != 0) ? ((data.Int("particleColorMode") == 1) ? ParticleType.ColorModes.Choose : ((data.Int("particleColorMode") == 2) ? ParticleType.ColorModes.Blink : ParticleType.ColorModes.Fade)) : ParticleType.ColorModes.Static),
					RotationMode = ((data.Int("particleRotationMode") != 0) ? ((data.Int("particleRotationMode") == 1) ? ParticleType.RotationModes.Random : ParticleType.RotationModes.SameAsDirection) : ParticleType.RotationModes.None),
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
				emitterParticle = new ParticleType
				{
					Color = Calc.HexToColorWithAlpha(data.Attr("particleColor", "40FF9080")) * data.Float("particleAlpha"),
					Color2 = Calc.HexToColorWithAlpha(data.Attr("particleColor2", "20408000")) * data.Float("particleAlpha"),
					FadeMode = ((data.Int("particleFadeMode") != 0) ? ((data.Int("particleFadeMode") == 1) ? ParticleType.FadeModes.Linear : ((data.Int("particleFadeMode") == 2) ? ParticleType.FadeModes.Late : ParticleType.FadeModes.InAndOut)) : ParticleType.FadeModes.None),
					ColorMode = ((data.Int("particleColorMode") != 0) ? ((data.Int("particleColorMode") == 1) ? ParticleType.ColorModes.Choose : ((data.Int("particleColorMode") == 2) ? ParticleType.ColorModes.Blink : ParticleType.ColorModes.Fade)) : ParticleType.ColorModes.Static),
					RotationMode = ((data.Int("particleRotationMode") != 0) ? ((data.Int("particleRotationMode") == 1) ? ParticleType.RotationModes.Random : ParticleType.RotationModes.SameAsDirection) : ParticleType.RotationModes.None),
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
			bool flagge;
			if (flag.StartsWith("!")) {
				flagge = !level.Session.GetFlag(flag.Substring(1));
			} else
            {
				flagge = level.Session.GetFlag(flag);
			}
			base.Update();
			spawnTimer += 1f;
			if (!Visible || !base.Scene.OnInterval(spawnInterval))
			{
				return;
			}
			
			if (flagge || flag == "")
            {
				for (int i = 0; i < particleCount; i++)
				{
					if (Calc.Random.NextFloat() < spawnChance / 100f)
					{
						float num = particleAngleRange / 360f * ((float)Math.PI * 2f);
						if (isFG)
						{
							SceneAs<Level>().ParticlesFG.Emit(emitterParticle, 1, base.Center, Vector2.One * particleSpawnSpread, particleAngle / 360f * ((float)Math.PI * 2f) + (Calc.Random.NextFloat() * num - num / 2f));
						}
						else
						{
							SceneAs<Level>().ParticlesBG.Emit(emitterParticle, 1, base.Center, Vector2.One * particleSpawnSpread, particleAngle / 360f * ((float)Math.PI * 2f) + (Calc.Random.NextFloat() * num - num / 2f));
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
}
