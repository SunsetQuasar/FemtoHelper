
// FactoryHelper.Triggers.FactoryEventTrigger
using Celeste;
using System.Collections;

[CustomEntity("FemtoHelper/FrogelineTrigger")]
internal class FrogelineTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
	private readonly string eventName = data.Attr("event");

	public override void OnEnter(Player player)
	{
		base.OnEnter(player);
		Level level = Scene as Level;
		switch (eventName)
		{
			case "tgu_intro" when !level.Session.GetFlag("thegreatunknown_intro"):
				level.Session.SetFlag("thegreatunknown_intro");
				Scene.Add(new TguCs0Intro(player));
				RemoveSelf();
				break;
            case "solards_intro" when !level.Session.GetFlag("solards_intro"):
                level.Session.SetFlag("solards_intro");
                Scene.Add(new S2A12Intro(player, 2));
                RemoveSelf();
                break;
            case "cu_intro" when !level.Session.GetFlag("cu_intro"):
				level.Session.SetFlag("cu_intro");
				Scene.Add(new S2A12Intro(player, 1));
				RemoveSelf();
				break;
			case "fs_hubintro" when !level.Session.GetFlag("foolspace_hubintro"):
				level.Session.SetFlag("foolspace_hubintro");
				Scene.Add(new FsCs0HubIntro(Scene, player));
				RemoveSelf();
				break;
			case "funy_meme" when !level.Session.GetFlag("funy_meme"):
				level.Session.SetFlag("funy_meme");
				Scene.Add(new CsFunnyAsHell(player));
				RemoveSelf();
				break;
			case "fs_hub_out":
				Add(new Coroutine(FS_BackgroundHRT(player)));
				break;
			case "ridgs" when !level.Session.GetFlag("ridgs"):
				level.Session.SetFlag("ridgs");
				Scene.Add(new _100ridgs_walkie(player));
				RemoveSelf();
				break;
			case "sebscutscene" when !level.Session.GetFlag("sebscutscene"):
				level.Session.SetFlag("sebscutscene");
				Scene.Add(new SebsCutscene(player));
				RemoveSelf();
				break;
		}
	}
	private IEnumerator FS_BackgroundHRT(Player player)
	{
		Level level = Scene as Level;
		float start = Left;
		float end = Right;
		while (true)
		{
			if (level.Session.GetFlag("blackhole")) continue;
			
			float fadeAlphaMultiplier = Calc.ClampedMap(player.X, start, end);
			float inversefadeAlphaMultiplier = Calc.ClampedMap(player.X, end, start);
			foreach (Backdrop item in level.Background.GetEach<Backdrop>("fs_bright"))
			{
				item.ForceVisible = true;
				item.FadeAlphaMultiplier = fadeAlphaMultiplier;
			}
			foreach (Backdrop item in level.Background.GetEach<Backdrop>("fs_dark"))
			{
				item.ForceVisible = true;
				item.FadeAlphaMultiplier = inversefadeAlphaMultiplier;
			}
			yield return null;
		}
	} 
}
