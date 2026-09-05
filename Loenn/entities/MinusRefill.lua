local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperMinusRefill = {}

FemtoHelperMinusRefill.name = "FemtoHelper/MinusRefill"
FemtoHelperMinusRefill.depth = -100
FemtoHelperMinusRefill.placements = {
    {
        name = "minus",
        data = {
            oneUse = false,
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
}

function FemtoHelperMinusRefill.sprite(room, entity)
    local sprPath = "objects/FemtoHelper/minusRefill/"

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

return FemtoHelperMinusRefill