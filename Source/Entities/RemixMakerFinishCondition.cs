using Celeste;

[CustomEntity("FemtoHelper/RemixMakerFinishCondition")]

public class RemixMakerFinishCondition(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{
	private Level level;
	private int amount;

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

		RemoveSelf();
	}
}