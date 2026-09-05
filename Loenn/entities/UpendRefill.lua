local drawableSprite = require("structs.drawable_sprite")

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
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
    {
        name = "upendVertical",
        data = {
            oneUse = false,
            type = "Vertical",
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
}

function FemtoHelperUpendRefill.sprite(room, entity)
    local sprPath = "objects/FemtoHelper/upendRefill/"..(string.lower(entity.type) == "horizontal" and "h/" or "v/")

    local sprites = {}

    local vx = entity.visualOffsetX or 0;
    local vy = entity.visualOffsetY or 0;
    
    if vx ~= 0 or vy ~= 0 then
        local main = drawableSprite.fromTexture(sprPath.."outline", entity)
        table.insert(sprites, main)

        local offset_spr = drawableSprite.fromTexture(sprPath.."idle00", entity)
        offset_spr:setColor({1, 1, 1, 0.5})
        offset_spr:addPosition(vx, vy)
        table.insert(sprites, offset_spr)
    else
        local main = drawableSprite.fromTexture(sprPath.."idle00", entity)
        table.insert(sprites, main)
    end

    return sprites
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