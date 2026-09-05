local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperRotateDashRefill = {}

FemtoHelperRotateDashRefill.name = "FemtoHelper/RotateDashRefill"
FemtoHelperRotateDashRefill.depth = -100
FemtoHelperRotateDashRefill.placements = {
    {
        name = "rotate_refill",
        data = {
            oneUse = false,
            scalar = 1.5,
            texture = "objects/FemtoHelper/rotateRefillCCW/",
            effectColors = "7958ad,cbace6,634691",
            particleColors = "ae99db,6f66d1,daa7e6,856fe3",
            angle = 90,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
}

function FemtoHelperRotateDashRefill.sprite(room, entity)
    local sprites = {}

    local vx = entity.visualOffsetX or 0;
    local vy = entity.visualOffsetY or 0;
    
    if vx ~= 0 or vy ~= 0 then
        local main = drawableSprite.fromTexture(entity.texture.."outline", entity)
        table.insert(sprites, main)

        local offset_spr = drawableSprite.fromTexture(entity.texture.."idle00", entity)
        offset_spr:setColor({1, 1, 1, 0.5})
        offset_spr:addPosition(vx, vy)
        table.insert(sprites, offset_spr)
    else
        local main = drawableSprite.fromTexture(entity.texture.."idle00", entity)
        table.insert(sprites, main)
    end

    return sprites
end

return FemtoHelperRotateDashRefill