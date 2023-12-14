
// FactoryHelper.Triggers.FactoryEventTrigger
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

[CustomEntity("FemtoHelper/SmilerSpawner")]
internal class SmilerSpawner : Trigger
{
	private int sicker;
	public SmilerSpawner(EntityData data, Vector2 offset) : base(data, offset)
	{
		sicker = data.Int("sicker", 0);
	}

	public override void OnEnter(Player player)
	{
		Level level = base.Scene as Level;
		level.Add(new SmileGhost(Position + new Vector2(-500, -100), sicker));
		RemoveSelf();
	}
}
