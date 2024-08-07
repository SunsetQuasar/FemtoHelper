﻿using Celeste.Mod.Entities;
using Celeste.Mod.FemtoHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Code.Entities;

[Tracked]
[CustomEntity("FemtoHelper/EHIController")]
public class ExtraHoldableInteractionsController : Entity
{
    public bool InteractWithDashBlocks;
    public bool InteractWithCrushBlocks;
    public bool InteractWithBreakerBoxes;
    public bool InteractWithCoreSwitch;
    public bool InteractWithMoveBlocks;
    public bool InteractWithSwapBlocks;
    public bool InteractWithFallingBlocksH;
    public bool InteractWithFallingBlocksV;

    public Vector2 DashBlockSpeedReq;
    public Vector2 CrushBlockSpeedReq;
    public Vector2 BreakerBoxSpeedReq;
    public Vector2 MoveBlockSpeedReq;
    public Vector2 SwapBlockSpeedReq;
    public Vector2 FallingBlockSpeedReq;

    public ExtraHoldableInteractionsController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Tag |= Tags.Persistent;

        InteractWithDashBlocks = data.Bool("breakDashBlock", true);
        InteractWithCrushBlocks = data.Bool("activateKevins", true);
        InteractWithCoreSwitch = data.Bool("flipCoreSwitches", true);
        InteractWithBreakerBoxes = data.Bool("hitLightningBreakerBoxes", true);
        InteractWithMoveBlocks = data.Bool("activateMoveBlocks", false);
        InteractWithSwapBlocks = data.Bool("activateSwapBlocks", false);
        InteractWithFallingBlocksH = data.Bool("activateFallingBlocksHorizontal", false);
        InteractWithFallingBlocksV = data.Bool("activateFallingBlocksVertical", false);

        DashBlockSpeedReq = new Vector2(data.Float("dashBlockXSpeedReq", 120f), data.Float("dashBlockYSpeedReq", 120f));
        CrushBlockSpeedReq = new Vector2(data.Float("crushBlockXSpeedReq", 120f), data.Float("crushBlockYSpeedReq", 120f));
        BreakerBoxSpeedReq = new Vector2(data.Float("breakerBoxXSpeedReq", 120f), data.Float("breakerBoxYSpeedReq", 120f));
        MoveBlockSpeedReq = new Vector2(data.Float("moveBlockXSpeedReq", 120f), data.Float("moveBlockYSpeedReq", 120f));
        SwapBlockSpeedReq = new Vector2(data.Float("swapBlockXSpeedReq", 120f), data.Float("swapBlockYSpeedReq", 120f));
        FallingBlockSpeedReq = new Vector2(data.Float("fallingBlockXSpeedReq", 120f), data.Float("fallingBlockYSpeedReq", 120f));
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (ExtraHoldableInteractionsController controller in scene.Tracker.GetEntities<ExtraHoldableInteractionsController>())
        {
            if (controller != this)
            {
                controller.RemoveSelf();
            }
        }
    }
}
