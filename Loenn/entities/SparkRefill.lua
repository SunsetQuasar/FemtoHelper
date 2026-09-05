local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperSparkRefill = {}

FemtoHelperSparkRefill.name = "FemtoHelper/SparkRefill"
FemtoHelperSparkRefill.depth = -100
FemtoHelperSparkRefill.placements = {
    {
        name = "spark",
        data = {
            oneUse = false,
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
}

function FemtoHelperSparkRefill.sprite(room, entity)
    local sprPath = "objects/FemtoHelper/sparkRefill/"

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

return FemtoHelperSparkRefill