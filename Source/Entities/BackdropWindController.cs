using System;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity("FemtoHelper/BackdropWindController")]

public class BackdropWindController : Entity
{
	public readonly string AffectedTag;
	public readonly float StrengthMultiplier;
	private Level level;

	public BackdropWindController(EntityData data, Vector2 offset) : base(data.Position + offset)
	{
        AffectedTag = data.Attr("tag", "windbackdrop");
		StrengthMultiplier = data.Float("strengthMultiplier", 1f);
	}
	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		level = scene as Level;
	}
	public override void Update()
	{
		foreach (Backdrop item in level.Background.GetEach<Backdrop>(AffectedTag))
		{
			item.Position += level.Wind * StrengthMultiplier * Engine.DeltaTime / 10;
		}
	}
}
