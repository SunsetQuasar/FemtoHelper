local FemtoHelperUpendRefill = {}

FemtoHelperUpendRefill.name = "FemtoHelper/UpendRefill"
FemtoHelperUpendRefill.depth = -100
FemtoHelperUpendRefill.fieldInformation = {
    type = {
        options = {
            "Horizontal",
            "Vertical",
        },
        editable = false
    },
}
FemtoHelperUpendRefill.placements = {
    {
        name = "upendHorizontal",
        data = {
            oneUse = false,
            type = "Horizontal",
            respawnTime = 2.5,
        }
    },
    {
        name = "upendVertical",
        data = {
            oneUse = false,
            type = "Vertical",
            respawnTime = 2.5,
        }
    },
}

function FemtoHelperUpendRefill.texture(room, entity)
    return "objects/FemtoHelper/upendRefill/"..(string.lower(entity.type) == "horizontal" and "h/" or "v/").."idle00"
end

function FemtoHelperUpendRefill.rotate(room, entity, direction)
        if string.lower(entity.type) == "horizontal" then entity.type = "vertical"
            return true 
        end
        if string.lower(entity.type) == "vertical" then entity.type = "horizontal"
            return true 
        end

    return true
end

return FemtoHelperUpendRefill