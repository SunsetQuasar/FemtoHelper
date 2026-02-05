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
    layer = {
        options = {
            {"BelowBG", "BelowBG"},
            {"AboveBG", "AboveBG"},
            {"Gameplay", "Gameplay"},
            {"BelowFG", "BelowFG"},
            {"AboveFG", "AboveFG"},
            {"HUD", "HUD"},
            {"AdditiveHUD", "AdditiveHUD"}
        },
        editable = false
    },
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
        depth = -10000,
        fontDataPath = "Graphics/FemtoHelper/PlutoniumText/Example.xml",
        justifyX = 0.5,
        justifyY = 0.5,
        dialogID = "FemtoHelper_PlutoniumText_Example",
        --hud = true,
        truncateSliderValues = false,
        mainColor = "FFFFFFFF",
        outlineColor = "000000FF",
        parallax = 1,
        scale = 1,
        shadow = false,
        extraSpacing = 0,
        visibilityFlag = "",
        decimals = -1,
        layer = "AboveFG",
        hudZoomSupport = true,
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

function FemtoHelperSimpleText.sprite(room, entity, node, nodeIndex)
    local main = drawableSprite.fromTexture("loenn/FemtoHelper/simpletext", entity)
    local outline = drawableSprite.fromTexture("loenn/FemtoHelper/simpletextOutline", entity)

    main:setColor(entity.mainColor)
    outline:setColor(entity.outlineColor)

    return {
        main,
        outline
    }
end

function FemtoHelperSimpleText.selection(room, entity)
    local nodelist = {}
    for _, node in ipairs(entity.nodes) do

        table.insert(nodelist, utils.rectangle(node.x - 4, node.y - 4, 8, 8))

    end
    return utils.rectangle(entity.x - 8, entity.y - 4, 16, 8), nodelist
end

return FemtoHelperSimpleText