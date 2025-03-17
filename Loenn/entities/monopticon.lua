local FemtoHelperMonopticon = {}

FemtoHelperMonopticon.name = "FemtoHelper/Monopticon"
FemtoHelperMonopticon.depth = -8500
FemtoHelperMonopticon.justification = {0.5, 1.0}
FemtoHelperMonopticon.nodeLineRenderType = "line"
FemtoHelperMonopticon.texture = "objects/lookout/lookout06"
FemtoHelperMonopticon.nodeLimits = {0, 0}
FemtoHelperMonopticon.fieldInformation = {
    dashCancelDelay = {
        fieldType = "integer"
    },
    openFrames = {
        fieldType = "integer"
    },
    preOpenFrames = {
        fieldType = "integer"
    },
    closeFrames = {
        fieldType = "integer"
    },
    cooldownFrames = {
        fieldType = "integer"
    },
}
FemtoHelperMonopticon.placements = {
    name = "monopticon",

    alternativeName = {"lookout", "binoculars"},
    data = {
        onlyY = false,
        interactFlag = "monopticon_interacting",
        blockDash = false,
        blockJump = false,
        dashCancelDelay = 9,
        dashInputCancel = false,
        openFrames = 20,
        preOpenFrames = 12,
        closeFrames = 20,
        cooldownFrames = 6,
        strictStateReset = false,
        binoAcceleration = 800,
        binoMaxSpeed = 240,
    }
}

return FemtoHelperMonopticon