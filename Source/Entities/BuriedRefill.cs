using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/BuriedRefill")]
public class BuriedRefill : CustomRefill
{
    public bool canAppear = false; // can show up potentially
    public bool hasAppeared = false; // player got the fuck out of the way

    public BuriedRefill(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Bool("twoDash", false), data.Bool("oneUse", false))
    {

        RespawnTime = data.Float("respawnTime", 2.5f);

        this.SetCollectLogic((player) => hasAppeared && player.UseRefill(twoDashes));

        Visible = false;
    }

    public override void Update()
    {
        if (hasAppeared)
        {
            base.Update();
        }
        else
        {
            canAppear = true;
            //foreach (Refill refill in Scene.Entities.FindAll<Refill>()) // i can't believe refills aren't fuckin tracked :sob:
            foreach (Refill refill in Scene.Tracker.GetEntitiesTrackIfNeeded<Refill>()) // i can't believe refills can be tracked now! fuck yes!
            {
                if (CollideCheck(refill))
                {
                    canAppear = false;
                    break;
                }
            }
            if (canAppear && !CollideCheck<Player>())
            {
                hasAppeared = Visible = true;
            }
        }
    }
}
