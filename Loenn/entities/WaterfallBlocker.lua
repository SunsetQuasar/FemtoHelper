local drawableSprite = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")

local FemtoHelperWaterfallBlocker = {}

FemtoHelperWaterfallBlocker.name = "FemtoHelper/WaterfallBlocker"
FemtoHelperWaterfallBlocker.depth = 0
FemtoHelperWaterfallBlocker.minimumSize = {8, 8}
FemtoHelperWaterfallBlocker.placements = {
    name = "default",
    width = 8,
    height = 8
}

function FemtoHelperWaterfallBlocker.sprite(room, entity)
    local spr = {}
    local sprite = drawableSprite.fromTexture("loenn/FemtoHelper/WaterfallBlocker", entity)
    sprite:addPosition(entity.width / 2, entity.height / 2)
    spr = {
        sprite,
        drawableRectangle.fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height, {0.4, 0.4, 0.4, 0.2}, {0.8, 0.9, 1, 0.8})
    }
    return spr
end

return FemtoHelperWaterfallBlocker