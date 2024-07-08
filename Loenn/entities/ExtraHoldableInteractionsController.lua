local FemtoHelperExtraHoldableInteractionsController = {}

FemtoHelperExtraHoldableInteractionsController.name = "FemtoHelper/EHIController"
FemtoHelperExtraHoldableInteractionsController.depth = 0
FemtoHelperExtraHoldableInteractionsController.texture = "loenn/FemtoHelper/EHIController"
FemtoHelperExtraHoldableInteractionsController.justification = {0.5, 0.5}
FemtoHelperExtraHoldableInteractionsController.placements = {
    name = "default",
    data = {
        breakDashBlocks = true,
        activateKevins = true,
        flipCoreSwitches = true,
        hitLightningBreakerBoxes = true,
        activateMoveBlocks = true,
        activateSwapBlocks = true,
        dashBlockXSpeedReq = 120,
        crushBlockXSpeedReq = 120,
        breakerBoxXSpeedReq = 120,
        moveBlockXSpeedReq = 120,
        swapBlockXSpeedReq = 120,
        dashBlockYSpeedReq = 120,
        crushBlockYSpeedReq = 120,
        breakerBoxBlockYSpeedReq = 120,
        moveBlockYSpeedReq = 120,
        swapBlockYSpeedReq = 120,
        activateFallingBlocksHorizontal = false,
        activateFallingBlocksVertical = false,
        fallingBlockXSpeedReq = 120,
        fallingBlockYSpeedReq = 0
    }
}

return FemtoHelperExtraHoldableInteractionsController