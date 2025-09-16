using System;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity("FemtoHelper/BackdropWindController")]

public class BackdropWindController : Entity
{
	private readonly string affectedTag;
	private readonly float strengthMultiplier;
	private Level level;

	public BackdropWindController(EntityData data, Vector2 offset) : base(data.Position + offset)
	{
        affectedTag = data.Attr("tag", "windbackdrop");
		strengthMultiplier = data.Float("strengthMultiplier", 1f);
	}
	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		level = scene as Level;
	}
	public override void Update()
	{
		foreach (Backdrop item in level.Background.GetEach<Backdrop>(affectedTag))
		{
			item.Position += level.Wind * strengthMultiplier * Engine.DeltaTime / 10;
		}
	}
}
