local drawableSprite = require("structs.drawable_sprite")
--local drawing = require("utils.drawing")
local utils = require("utils")
--local drawableLine = require("structs.drawable_line")
--local drawableRectangle = require("structs.drawable_rectangle")
local enums = require("consts.celeste_enums")

local FemtoHelperTouchPlatform = {}

FemtoHelperTouchPlatform.name = "FemtoHelper/TouchPlatform"
FemtoHelperTouchPlatform.depth = -100
FemtoHelperTouchPlatform.minimumSize = {16, 8}
FemtoHelperTouchPlatform.fieldInformation = {
    sprite = {
        options = {
            {"brown", "brown"},
            {"checkeredA", "checkeredA"},
            {"checkeredB", "checkeredB"},
            {"gray", "gray"}
        },
        editable = true
    },
    surfaceIndex = {
        options = enums.tileset_sound_ids,
        fieldType = "integer"
    }
}

FemtoHelperTouchPlatform.placements = {
    {
        name = "default",
        data = {
            gravity = 200,
            speedOnTriggerX = 0,
	        speedOnTriggerY = 0,
            sprite = "gray",
            surfaceIndex = -1,
        }
    },
    {
        name = "rise",
        data = {
            gravity = -200,
            speedOnTriggerX = 0,
	        speedOnTriggerY = 120,
            sprite = "brown",
            surfaceIndex = -1,
        }
    },
}

function FemtoHelperTouchPlatform.sprite(room, entity)
    local sprites = {}

    for i = 0, entity.width - 8, 8 do
        local slice = drawableSprite.fromTexture("objects/FemtoHelper/TouchPlatform/"..entity.sprite, entity)

        if i == 0 then
            slice:useRelativeQuad(0, 0, 8, 16, true)
        elseif i == entity.width - 8 then
            slice:useRelativeQuad(16, 0, 8, 16, true)
        else
            slice:useRelativeQuad(8, 0, 8, 16, true)
        end

        slice:addPosition(i, 0)

        table.insert(sprites, slice)
    end

    return sprites
end

return FemtoHelperTouchPlatform