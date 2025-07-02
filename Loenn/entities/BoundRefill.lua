local FemtoHelperBoundRefill = {}

FemtoHelperBoundRefill.name = "FemtoHelper/BoundRefill"
FemtoHelperBoundRefill.depth = -100
FemtoHelperBoundRefill.placements = {
    {
        name = "bound",
        data = {
            oneUse = false,
            respawnTime = 2.5,
        }
    },
}

function FemtoHelperBoundRefill.texture(room, entity)
    return "objects/FemtoHelper/boundRefill/idle00"
end

return FemtoHelperBoundRefill