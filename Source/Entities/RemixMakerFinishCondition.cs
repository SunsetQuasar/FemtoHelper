using Celeste;
using Celeste.Mod;

[CustomEntity("FemtoHelper/RemixMakerFinishCondition")]

public class RemixMakerFinishCondition : Entity
{
	private Level level;
	private int amount;

	public RemixMakerFinishCondition(EntityData data, Vector2 offset) : base(data.Position + offset)
	{
	}
	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		level = scene as Level;
		if (level.Session.GetFlag("remixcomplete_1_2"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_1_3"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_1_4"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_1_5"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_1_6"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_2_1"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_2_3"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_2_4"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_2_5"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_2_6"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_3_1"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_3_2"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_3_4"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_3_5"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_3_6"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_4_1"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_4_2"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_4_3"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_4_5"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_4_6"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_5_1"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_5_2"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_5_3"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_5_4"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_5_6"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_6_1"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_6_2"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_6_3"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_6_4"))
		{
			amount++;
		}
		if (level.Session.GetFlag("remixcomplete_6_5"))
		{
			amount++;
		}
		if (amount >= 5)
		{
			level.Session.SetFlag("remixcompletefull");
		}
	}
	public override void Update()
	{
		if (Calc.Random.NextFloat() > 0.99f)
		{
			Logger.Log("balls", "this is the balls entity transmitting signals through all celeste consoles");
		}
	}
}