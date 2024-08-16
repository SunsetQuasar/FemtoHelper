
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Celeste.Mod.FemtoHelper;
using Monocle;
using System;

[CustomEntity("FemtoHelper/ParticleRemoteEmit")]
internal class ParticleRemoteEmit : Trigger
{
	private string tag_;

    public ParticleRemoteEmit(EntityData data, Vector2 offset) : base(data, offset)
	{
		tag_ = data.Attr("tag");
	}

	public override void OnEnter(Player player)
	{
		base.OnEnter(player);
		Level level = base.Scene as Level;

		foreach (Celeste.Mod.FemtoHelper.ParticleEmitter emitter in base.Scene.Tracker.GetEntities<Celeste.Mod.FemtoHelper.ParticleEmitter>())
        {
			bool flagge;
			if (emitter.flag.StartsWith("!"))
			{
				flagge = !level.Session.GetFlag(emitter.flag.Substring(1));
			}
			else
			{
				flagge = level.Session.GetFlag(emitter.flag);
			}
			if (emitter.tag_ == tag_)
            {
				if (flagge == true || emitter.flag == "")
				{
					for (int i = 0; i < emitter.particleCount; i++)
					{
						float num = emitter.particleAngleRange / 360f * ((float)Math.PI * 2f);
						if (emitter.isFG)
						{
							SceneAs<Level>().ParticlesFG.Emit(emitter.emitterParticle, 1, emitter.Center, Vector2.One * emitter.particleSpawnSpread, emitter.particleAngle / 360f * ((float)Math.PI * 2f) + (Calc.Random.NextFloat() * num - num / 2f));
						}
						else
						{
							SceneAs<Level>().ParticlesBG.Emit(emitter.emitterParticle, 1, emitter.Center, Vector2.One * emitter.particleSpawnSpread, emitter.particleAngle / 360f * ((float)Math.PI * 2f) + (Calc.Random.NextFloat() * num - num / 2f));
						};
					}
				}
			}
        }
	}
}
