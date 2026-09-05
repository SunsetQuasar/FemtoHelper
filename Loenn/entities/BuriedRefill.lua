local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local FemtoHelperBuriedRefill = {}

FemtoHelperBuriedRefill.name = "FemtoHelper/BuriedRefill"
FemtoHelperBuriedRefill.depth = -100
FemtoHelperBuriedRefill.placements = {
    {
        name = "buried",
        data = {
            oneUse = true,
            twoDash = true,
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
}

function FemtoHelperBuriedRefill.sprite(room, entity)
    local sprPath = entity.twoDash and "objects/refillTwo/" or "objects/refill/"

    local sprites = {}

    local vx = entity.visualOffsetX or 0;
    local vy = entity.visualOffsetY or 0;
    
    if vx ~= 0 or vy ~= 0 then
        local main = drawableSprite.fromTexture(sprPath.."outline", entity)
        table.insert(sprites, main)

        local offset_spr = drawableSprite.fromTexture(sprPath.."idle00", entity)
        offset_spr:setColor({1, 1, 1, 0.5})
        offset_spr:addPosition(vx, vy)
        offset_spr:useRelativeQuad(8, 0, 8, 16, true)
        offset_spr:addPosition(0, -8)
        table.insert(sprites, offset_spr)

        local offset_spr2 = drawableSprite.fromTexture(sprPath.."outline", entity)
        offset_spr2:setColor({1, 1, 1, 0.5})
        offset_spr2:addPosition(vx, vy)
        offset_spr2:useRelativeQuad(0, 0, 8, 16, true)
        offset_spr2:addPosition(-8, -8)
        table.insert(sprites, offset_spr2)
    else
        local main = drawableSprite.fromTexture(sprPath.."idle00", entity)
        main:useRelativeQuad(8, 0, 8, 16, true)
        main:addPosition(0, -8)
        table.insert(sprites, main)

        local main2 = drawableSprite.fromTexture(sprPath.."outline", entity)
        main2:useRelativeQuad(0, 0, 8, 16, true)
        main2:addPosition(-8, -8)
        table.insert(sprites, main2)
    end

    return sprites
end

function FemtoHelperBuriedRefill.selection(room, entity)
    return utils.rectangle((entity.x or 0) - 8, (entity.y or 0) - 8, 16, 16)
end

return FemtoHelperBuriedRefill