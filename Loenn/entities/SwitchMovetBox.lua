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
        path = "objects/FemtoHelper/switchMovetBox/",
        floaty = true,
        oneUse = false,
    }
}

FemtoHelperSwitchMovetBox.sprite = function(room, entity) 
    local sprites = {}

    local sprite = drawableSprite.fromTexture(entity.path.."back", entity)

    local stone = drawableSprite.fromTexture(entity.path.."crystal", entity)

    sprite:setColor({1, 1, 1, 1})
    sprite:setJustification(0.25, 0.25)
    stone:setColor(entity.color)
    stone:setJustification(0.25, 0.25)

    table.insert(sprites, sprite)
    table.insert(sprites, stone)

    if entity.oneUse then
        local sprite_oneuse = drawableSprite.fromTexture(entity.path.."back_oneuse", entity)
        sprite_oneuse:setJustification(0.25, 0.25)
        sprite_oneuse:setColor(entity.color)
        table.insert(sprites, sprite_oneuse)
    end

    return sprites
end

return FemtoHelperSwitchMovetBox