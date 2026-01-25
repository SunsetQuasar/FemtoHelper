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
public class CinematicTextTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    public readonly string ActivationTag = data.Attr("activationTag", "tag1");

    public readonly List<CinematicText> List = [];

    public readonly bool StopText = data.Bool("stopText", false);

    public readonly bool StopImmediately = data.Bool("stopImmediately", false);

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach(CinematicText t in Scene.Tracker.GetEntities<CinematicText>())
        {
            List.Add(t);
        }
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);
        foreach (var t in List.Where(t => t.ActivationTag == ActivationTag))
        {
            if (StopText)
            {
                if (StopImmediately)
                {
                    if (!t.Retriggerable)
                    {
                        t.RemoveSelf();
                        return;
                    }
                    t.StopText = false;
                    t.Activated = t.Entered = false;
                    t.DisappearPercent = 1f;
                    t.FinalStringLen = 0;
                    t.Finished = false;
                    if (t.ActualSequence is { } q) t.Remove(q);
                    t.ActualSequence = null;
                    return;
                }
                if (t.Activated) t.StopText = true;
            } else
            {
                t.Enter();
            }
        }
    }
}
