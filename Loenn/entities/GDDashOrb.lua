local drawableSprite = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")

local FemtoHelperGDDashOrb = {}

FemtoHelperGDDashOrb.name = "FemtoHelper/GDDashOrb"
FemtoHelperGDDashOrb.depth = 0
FemtoHelperGDDashOrb.justification = {0.5, 0.5}
FemtoHelperGDDashOrb.depth = 2000
FemtoHelperGDDashOrb.placements = {
    {
        name = "default",
        data = {
            _gravityHelper = true,
            angle = 0,
            speed = 240,
            additive = false,
            pink = false
        }
    },
    {
        name = "pink",
        data = {
            _gravityHelper = true,
            angle = 0,
            speed = 240,
            additive = false,
            pink = true
        }
    }
}
FemtoHelperGDDashOrb.ignoredFields = {
    "_name", "_id","_gravityHelper"
}

function FemtoHelperGDDashOrb.sprite(room, entity)

    local ring = drawableSprite.fromTexture("objects/FemtoHelper/gddashorb/dashring", entity);

    local orb = drawableSprite.fromTexture("objects/FemtoHelper/gddashorb/dashorb", entity);

    orb.rotation = (entity.angle / 180) * 3.14159265359;
    if entity.pink then
        orb:setColor({255 / 255, 48 / 255, 240 / 255, 1});
    else
        orb:setColor({32 / 255, 238 / 255, 48 / 255, 1});
    end

    local spr = {
        ring,
        orb
    }
    return spr
end

return FemtoHelperGDDashOrb