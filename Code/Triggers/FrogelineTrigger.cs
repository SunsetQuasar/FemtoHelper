
// FactoryHelper.Triggers.FactoryEventTrigger
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Pico8;
using Celeste.Mod.FemtoHelper;

[CustomEntity("FemtoHelper/FrogelineTrigger")]
internal class FrogelineTrigger : Trigger
{
	private readonly string eventName;

	public FrogelineTrigger(EntityData data, Vector2 offset) : base(data, offset)
	{
		eventName = data.Attr("event");
	}

	public override void OnEnter(Player player)
	{
		base.OnEnter(player);
		Level level = base.Scene as Level;
		if (eventName == "tgu_intro" && !level.Session.GetFlag("thegreatunknown_intro"))
		{
			level.Session.SetFlag("thegreatunknown_intro");
			base.Scene.Add(new TGU_CS0_Intro(player));
			RemoveSelf();
		}
		if (eventName == "cu_intro" && !level.Session.GetFlag("cu_intro"))
		{
			level.Session.SetFlag("cu_intro");
			base.Scene.Add(new S2A12_Intro(player));
			RemoveSelf();
		}
		if (eventName == "fs_hubintro" && !level.Session.GetFlag("foolspace_hubintro"))
		{
			level.Session.SetFlag("foolspace_hubintro");
			base.Scene.Add(new FS_CS0_HubIntro(base.Scene, player));
			RemoveSelf();
		}
		if (eventName == "funy_meme" && !level.Session.GetFlag("funy_meme"))
		{
			level.Session.SetFlag("funy_meme");
			base.Scene.Add(new CS_FunnyAsHell(player));
			RemoveSelf();
		}
		if (eventName == "fs_hub_out")
		{
			Add(new Coroutine(FS_BackgroundHRT(player)));
		}
		if (eventName == "ridgs" && !level.Session.GetFlag("ridgs"))
		{
			level.Session.SetFlag("ridgs");
			base.Scene.Add(new _100ridgs_walkie(player));
			RemoveSelf();
		}
		if (eventName == "sebscutscene" && !level.Session.GetFlag("sebscutscene"))
		{
			level.Session.SetFlag("sebscutscene");
			base.Scene.Add(new sebsCutscene(player));
			RemoveSelf();
		}
			
	}
	private IEnumerator FS_BackgroundHRT(Player player)
	{
		Level level = Scene as Level;
		float start = Left;
		float end = Right;
		while (true)
		{
			if (!level.Session.GetFlag("blackhole"))
			{
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
}
