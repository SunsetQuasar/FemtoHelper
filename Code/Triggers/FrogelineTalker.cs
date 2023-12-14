// Celeste.InteractTrigger
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity("FemtoHelper/FrogelineTalker")]
public class FrogelineTalker : Entity
{
	public const string FlagPrefix = "it_";

	public TalkComponent Talker;

	public List<string> Events;

	private int eventIndex;

	private float timeout;

	private bool used;

	private int totalIndexes;
	private float TextOffsetOnPaper;
	private float TextScale;
	private string memoSprite;
	private string memoTitleSprite;
	private string memoDialogID;

	public FrogelineTalker(EntityData data, Vector2 offset)
		: base(data.Position + offset)
	{
		Events = new List<string>();
		Events.Add(data.Attr("event"));

		totalIndexes = data.Int("totalIndexes", 3);
		TextOffsetOnPaper = data.Float("textOffsetOnPaper", 0f);
		TextScale = data.Float("textScale", 0.75f);
		memoSprite = data.Attr("spritePath", "FemtoHelper/exampleMemo/memo");
		memoTitleSprite = data.Attr("titleSpritePath", "FemtoHelper/exampleMemo/title_");
		memoDialogID = data.Attr("memoTextId", "FemtoHelper_Memo_exampleText");


		base.Collider = new Hitbox(data.Width, data.Height);
		for (int i = 2; i < 100 && data.Has("event_" + i) && !string.IsNullOrEmpty(data.Attr("event_" + i)); i++)
		{
			Events.Add(data.Attr("event_" + i));
		}
		Vector2 drawAt = new Vector2(data.Width / 2, 0f);
		if (data.Nodes.Length != 0)
		{
			drawAt = data.Nodes[0] - data.Position;
		}
		Add(Talker = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), drawAt, OnTalk));
		Talker.PlayerMustBeFacing = false;
	}

	public override void Added(Scene scene)
	{
		base.Added(scene);
		Session session = (scene as Level).Session;
		for (int i = 0; i < Events.Count; i++)
		{
			if (session.GetFlag("it_" + Events[i]))
			{
				eventIndex++;
			}
		}
		if (eventIndex >= Events.Count)
		{
			RemoveSelf();
		}
		else if (Events[eventIndex] == "ch5_theo_phone")
		{
			scene.Add(new TheoPhone(Position + new Vector2(base.Width / 2f - 8f, base.Height - 1f)));
		}
	}

	public void OnTalk(Player player)
	{
		base.Scene.Add(new HotelBreakout_Memo(player, totalIndexes, TextOffsetOnPaper, TextScale));
	}

	public override void Update()
	{
		if (used)
		{
			timeout -= Engine.DeltaTime;
			if (timeout <= 0f)
			{
				RemoveSelf();
			}
		}
		else
		{
			while (eventIndex < Events.Count && (base.Scene as Level).Session.GetFlag("it_" + Events[eventIndex]))
			{
				eventIndex++;
			}
			if (eventIndex >= Events.Count)
			{
				RemoveSelf();
			}
		}
		base.Update();
	}
}
