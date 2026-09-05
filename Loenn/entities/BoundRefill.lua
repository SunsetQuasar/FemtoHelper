local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperBoundRefill = {}

FemtoHelperBoundRefill.name = "FemtoHelper/BoundRefill"
FemtoHelperBoundRefill.depth = -100
FemtoHelperBoundRefill.placements = {
    {
        name = "bound",
        data = {
            oneUse = false,
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
}

function FemtoHelperBoundRefill.sprite(room, entity)
    local sprPath = "objects/FemtoHelper/boundRefill/"

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

return FemtoHelperBoundRefill