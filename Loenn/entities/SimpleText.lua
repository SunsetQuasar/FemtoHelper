local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local drawableLine = require("structs.drawable_line")

local FemtoHelperSimpleText = {}

FemtoHelperSimpleText.name = "FemtoHelper/SimpleText"
FemtoHelperSimpleText.depth = -3000000
FemtoHelperSimpleText.texture = "loenn/FemtoHelper/simpletext"
FemtoHelperSimpleText.justification = {0.5, 0.5}
FemtoHelperSimpleText.nodeLimits = {0, 1}
FemtoHelperSimpleText.nodeVisibility = "always"
FemtoHelperSimpleText.nodeRenderType = "fan"

FemtoHelperSimpleText.fieldInformation = {
    mainColor = {
        fieldType = "color",
        useAlpha = true
    },
    outlineColor = {
        fieldType = "color",
        useAlpha = true
    }
}

FemtoHelperSimpleText.placements = {
    name = "default",
    data = {
        charList = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'\\\".,ç",
        depth = -10000,
        dialogID = "FemtoHelper_PlutoniumText_Example",
        fontHeight = 12,
        fontPath = "objects/FemtoHelper/PlutoniumText/example",
        fontWidth = 8,
        hud = true,
        truncateSliderValues = false,
        mainColor = "FFFFFFFF",
        outlineColor = "000000FF",
        parallax = 1,
        scale = 1,
        shadow = false,
        spacing = 8,
        visibilityFlag = "",
        decimals = -1,
    }
}

function FemtoHelperSimpleText.nodeSprite(room, entity, node) 
    local spr = {}
    local sprite = drawableSprite.fromTexture("loenn/FemtoHelper/simpletextnode", entity)
    sprite:addPosition(node.x - entity.x, node.y - entity.y);
    
    table.insert(spr, sprite)
    table.insert(spr, drawableLine.fromPoints({entity.x, entity.y, node.x, node.y}, {1, 0, 0.5, 0.3}, 1))

    return spr
end

function FemtoHelperSimpleText.selection(room, entity)
    local nodelist = {}
    for _, node in ipairs(entity.nodes) do

        table.insert(nodelist, utils.rectangle(node.x - 4, node.y - 4, 8, 8))

    end
    return utils.rectangle(entity.x - 16, entity.y - 4, 32, 8), nodelist
end

return FemtoHelperSimpleText