local FemtoHelperLaCreatura = {}

local nodeStruct = require("structs.node")

FemtoHelperLaCreatura.name = "FemtoHelper/LaCreatura"
FemtoHelperLaCreatura.depth = -1000000
FemtoHelperLaCreatura.texture = "objects/FemtoHelper/butterfly/00"
FemtoHelperLaCreatura.fieldInformation = {
    lightStartFade = {
        fieldType = "integer",
    },
    lightEndFade = {
        fieldType = "integer",
    }
}
FemtoHelperLaCreatura.placements = {
    {
        name = "creatura",
        data = {
            acceleration = 90,
            maxSpeed = 40,
            bloomRadius = 16,
            bloomAlpha = 0.75,
            lightStartFade = 12,
            lightEndFade = 24,
            lightAlpha = 1,
            colors = "74db93,dbc874,74a1db,e0779f,9677e0",
            randomStartPos = true,
            targetRangeRadius = 32,
            minFollowTime = 6,
            maxFollowTime = 12
        }
    }
}

return FemtoHelperLaCreatura