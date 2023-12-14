using System;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity("FemtoHelper/BackdropWindController")]

public class BackdropWindController : Entity
{
	public string tag;
	public float strengthMultiplier = 1f;
	private Level level;

	public BackdropWindController(EntityData data, Vector2 offset) : base(data.Position + offset)
	{
		tag = data.Attr("tag", "windbackdrop");
		strengthMultiplier = data.Float("strengthMultiplier", 1f);
	}
	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		level = scene as Level;
	}
	public override void Update()
	{
		foreach (Backdrop item in level.Background.GetEach<Backdrop>(tag))
		{
			item.Position += level.Wind * strengthMultiplier * Engine.DeltaTime / 10;
		}
	}
}
