
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Celeste.Mod.FemtoHelper;
using Monocle;
using System;

[CustomEntity("FemtoHelper/ParticleRemoteEmit")]
internal class ParticleRemoteEmit(EntityData data, Vector2 offset) : Trigger(data, offset)
{
	private string tag = data.Attr("tag");

	public override void OnEnter(Player player)
	{
		base.OnEnter(player);
		Level level = Scene as Level;

		foreach (Celeste.Mod.FemtoHelper.ParticleEmitter emitter in Scene.Tracker.GetEntities<Celeste.Mod.FemtoHelper.ParticleEmitter>())
        {
			bool flagge;
			if (emitter.Flag.StartsWith("!"))
			{
				flagge = !level.Session.GetFlag(emitter.Flag.Substring(1));
			}
			else
			{
				flagge = level.Session.GetFlag(emitter.Flag);
			}

			if (emitter.Tag != tag) continue;
			if (flagge != true && emitter.Flag != "") continue;
			
			for (int i = 0; i < emitter.ParticleCount; i++)
			{
				float num = emitter.ParticleAngleRange / 360f * ((float)Math.PI * 2f);
				if (emitter.IsFg)
				{
					SceneAs<Level>().ParticlesFG.Emit(emitter.EmitterParticle, 1, emitter.Center, Vector2.One * emitter.ParticleSpawnSpread, emitter.ParticleAngle / 360f * ((float)Math.PI * 2f) + (Calc.Random.NextFloat() * num - num / 2f));
				}
				else
				{
					SceneAs<Level>().ParticlesBG.Emit(emitter.EmitterParticle, 1, emitter.Center, Vector2.One * emitter.ParticleSpawnSpread, emitter.ParticleAngle / 360f * ((float)Math.PI * 2f) + (Calc.Random.NextFloat() * num - num / 2f));
				};
			}
        }
	}
}
