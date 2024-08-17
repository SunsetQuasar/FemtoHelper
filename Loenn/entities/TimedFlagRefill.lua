local FemtoHelperTimedFlagRefill = {}

FemtoHelperTimedFlagRefill.name = "FemtoHelper/TimedFlagRefill"
FemtoHelperTimedFlagRefill.depth = -100
FemtoHelperTimedFlagRefill.fieldInformation = {
    flagMode = {
        options = {
            {"On Then Off", 0},
            {"Off Then On", 1},
            {"Toggle Twice", 2},
        },
        editable = false
    },
    duration = {
        fieldType = "integer"
    }
}

FemtoHelperTimedFlagRefill.placements = {
    {
        name = "boolean",
        data = {
            twoDash = false,
            oneUse = false,
            flag = "refill_flag",
            stopMomentum = false,
            path = "objects/refill/",
            particleColors = "d3ffd4,85fc87,a5fff7,6de081",
            alwaysUse = false,
            flagMode = 0,
            duration = 1,
            refillDash = true,
            refillStamina = true
        }
    },
    {
        name = "booleanTwo",
        data = {
            twoDash = true,
            oneUse = false,
            flag = "refill_flag",
            stopMomentum = false,
            path = "objects/refillTwo/",
            particleColors = "FFD3F9,EF94E3,FFA5AA,DD6CCA",
            alwaysUse = false,
            flagMode = 0,
            duration = 1,
            refillDash = true,
            refillStamina = true
        }
    }
}

function FemtoHelperTimedFlagRefill.texture(room, entity)
    return entity.path.."idle00"
end

return FemtoHelperTimedFlagRefill