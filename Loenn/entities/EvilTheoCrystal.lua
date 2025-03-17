local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperEvilTheoCrystal = {}

FemtoHelperEvilTheoCrystal.name = "FemtoHelper/EvilTheoCrystal"
FemtoHelperEvilTheoCrystal.depth = 100
FemtoHelperEvilTheoCrystal.placements = {
    name = "evil_theo_crystal",
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "characters/FemtoHelper/evilTheoCrystal/idleEvil00"

function FemtoHelperEvilTheoCrystal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end
function FemtoHelperEvilTheoCrystal.selection(room, entity)
    local hh = 24
    local hh2 = 24

    return utils.rectangle(entity.x - hh / 2, entity.y - (hh2 / 2) + offsetY, hh, hh2)
end

return FemtoHelperEvilTheoCrystal