using Celeste.Mod.Entities;
using Celeste.Mod.FemtoHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Triggers;

[CustomEntity("FemtoHelper/CinematicTextTrigger")]
public class CinematicTextTrigger : Trigger
{
    public string activationTag;

    public List<CinematicText> list;

    public bool stopText;

    public CinematicTextTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        activationTag = data.Attr("activationTag", "tag1");
        stopText = data.Bool("stopText", false);
        list = new List<CinematicText>();
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach(CinematicText t in Scene.Tracker.GetEntities<CinematicText>())
        {
            list.Add(t);
        }
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);
        foreach(CinematicText t in list)
        {
            if(t.activationTag == activationTag)
            {
                if (stopText)
                {
                    if (t.active) t.stopText = true;
                } else
                {
                    t.Enter();
                }
            }
        }
    }
}
