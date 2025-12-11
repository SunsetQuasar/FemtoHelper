local FemtoHelperCustomCliffsideFlag = {}

FemtoHelperCustomCliffsideFlag.name = "FemtoHelper/CustomCliffsideWindFlag"
FemtoHelperCustomCliffsideFlag.depth = 8998
FemtoHelperCustomCliffsideFlag.justification = {0.5, 0.5}
FemtoHelperCustomCliffsideFlag.fieldInformation = {
    index = {
        fieldType = "integer",
        options = {
            0, 1, 2, 3,
            4, 5, 6, 7,
            8, 9, 10
        },
        editable = true
    }
}
FemtoHelperCustomCliffsideFlag.placements = {
    name = "default",
    data = {
        index = 0,
        spritesPath = "scenery/cliffside/flag",
        sineFrequency = 1,
        sineAmplitude = 1,
        naturalDraft = 0,
        depth = 8999,
    }
}

function FemtoHelperCustomCliffsideFlag.texture(room, entity)
    return entity.spritesPath .. "0" .. entity.index
end

return FemtoHelperCustomCliffsideFlag