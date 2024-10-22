local FemtoHelperClutterShadowController = {}

FemtoHelperClutterShadowController.name = "FemtoHelper/ClutterShadowController"
FemtoHelperClutterShadowController.depth = 0
FemtoHelperClutterShadowController.texture = "loenn/FemtoHelper/clutternoshadow"
FemtoHelperClutterShadowController.justification = {0.5, 0.5}
FemtoHelperClutterShadowController.placements = {
    name = "default",
    data = {
        enabledAlpha = 0.7,
        disabledAlpha = 0.3,
        enabledColor = "000000",
        disabledColor = "000000",
    }
}

return FemtoHelperClutterShadowController