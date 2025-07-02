local FemtoHelperMinusRefill = {}

FemtoHelperMinusRefill.name = "FemtoHelper/MinusRefill"
FemtoHelperMinusRefill.depth = -100
FemtoHelperMinusRefill.placements = {
    {
        name = "minus",
        data = {
            oneUse = false,
            respawnTime = 2.5,
        }
    },
}

function FemtoHelperMinusRefill.texture(room, entity)
    return "objects/FemtoHelper/minusRefill/idle00"
end

return FemtoHelperMinusRefill