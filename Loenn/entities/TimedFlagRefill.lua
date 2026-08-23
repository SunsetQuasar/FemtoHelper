local drawableSprite = require("structs.drawable_sprite")

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
            refillStamina = true,
            respawnTime = 2.5,
            audioPath = "event:/game/general/",
            visualOffsetX = 0,
            visualOffsetY = 0
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
            refillStamina = true,
            respawnTime = 2.5,
            audioPath = "event:/game/general/",
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    }
}

function FemtoHelperTimedFlagRefill.sprite(room, entity)

    local sprites = {}

    local vx = entity.visualOffsetX or 0;
    local vy = entity.visualOffsetY or 0;
    
    if vx ~= 0 or vy ~= 0 then
        local main = drawableSprite.fromTexture(entity.path.."outline", entity)
        table.insert(sprites, main)

        local offset_spr = drawableSprite.fromTexture(entity.path.."idle00", entity)
        offset_spr:setColor({1, 1, 1, 0.5})
        offset_spr:addPosition(vx, vy)
        table.insert(sprites, offset_spr)
    else
        local main = drawableSprite.fromTexture(entity.path.."idle00", entity)
        table.insert(sprites, main)
    end

    return sprites
end

return FemtoHelperTimedFlagRefill