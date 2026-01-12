local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperSwitchMovetBox = {}

FemtoHelperSwitchMovetBox.name = "FemtoHelper/SwitchMovetBox"
FemtoHelperSwitchMovetBox.depth = -10550
FemtoHelperSwitchMovetBox.fieldInformation = {
    color = {
        fieldType = "color",
    }
}
FemtoHelperSwitchMovetBox.placements = {
    name = "SwitchMovetBox",
    data = {
        color = "FF0000",
        path = "objects/FemtoHelper/switchMovetBox/"
    }
}

FemtoHelperSwitchMovetBox.sprite = function(room, entity) 
    local sprite = drawableSprite.fromTexture(entity.path.."back", entity)

    local stone = drawableSprite.fromTexture(entity.path.."crystal", entity)

    sprite:setColor({1, 1, 1, 1})
    sprite:setJustification(0.25, 0.25)
    stone:setColor(entity.color)
    stone:setJustification(0.25, 0.25)
    return {sprite, stone}
end

return FemtoHelperSwitchMovetBox