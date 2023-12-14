local FemtoHelperLaCreatura = {}

local nodeStruct = require("structs.node")

FemtoHelperLaCreatura.name = "FemtoHelper/LaCreatura"
FemtoHelperLaCreatura.depth = -1000000
FemtoHelperLaCreatura.texture = "objects/FemtoHelper/butterfly/00"
FemtoHelperLaCreatura.fieldInformation = {
    number = {
        fieldType = "integer",
    }
}
FemtoHelperLaCreatura.placements = {
    {
        name = "creatura",
        data = {
            acceleration = 90,
            maxSpeed = 40
        }
    }
}

return FemtoHelperLaCreatura