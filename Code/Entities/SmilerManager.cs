
using System;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity("FemtoHelper/SmilerManager")]

public class SmilerManager : Entity
{
	private Level level;

	public SmilerManager(EntityData data, Vector2 offset) : base(data.Position + offset)
	{
	}
	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		level = scene as Level;
	}
	public override void Update()
	{
		if (base.Scene.Tracker.GetEntities<SmileGhost>() != null)
		{
			foreach (SmileGhost smiley in base.Scene.Tracker.GetEntities<SmileGhost>())
			{
				foreach (SmileGhost smiley2 in base.Scene.Tracker.GetEntities<SmileGhost>())
				{
					if (Math.Pow(smiley.Position.X - smiley2.Position.X, 2) + Math.Pow(smiley.Position.Y - smiley2.Position.Y, 2) < Math.Pow(32f, 2))
					{
						smiley.vel += (smiley.Position - smiley2.Position) * 0.5f;
					}
					if (smiley.distance > smiley2.distance)
					{
						Audio.SetMusicParam("smileDistance", smiley.distance);
					}
				}
			}
		}
	}
}

