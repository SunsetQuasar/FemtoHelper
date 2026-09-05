local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperPopRefill = {}

FemtoHelperPopRefill.name = "FemtoHelper/PopRefill"
FemtoHelperPopRefill.depth = -100

FemtoHelperPopRefill.placements = {
    {
        name = "PopRefill",
        data = {
            oneUse = false,
            twoDash = false,
            spawnTime = 2.5,
            respawnTimer = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
    {
        name = "PopRefillTwo",
        data = {
            oneUse = false,
            twoDash = true,
            spawnTime = 2.5,
            respawnTimer = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    }
}

function FemtoHelperPopRefill.sprite(room, entity)
    local sprPath = entity.twoDash and "objects/refillTwo/" or "objects/refill/"

    local sprites = {}

    local vx = entity.visualOffsetX or 0;
    local vy = entity.visualOffsetY or 0;
    
    if vx ~= 0 or vy ~= 0 then
        local main = drawableSprite.fromTexture(sprPath.."outline", entity)
        table.insert(sprites, main)

        local offset_spr = drawableSprite.fromTexture(sprPath.."outline", entity)
        offset_spr:setColor({1, 1, 1, 0.5})
        offset_spr:addPosition(vx, vy)
        table.insert(sprites, offset_spr)
    else
        local main = drawableSprite.fromTexture(sprPath.."outline", entity)
        table.insert(sprites, main)
    end

    return sprites
end

return FemtoHelperPopRefill