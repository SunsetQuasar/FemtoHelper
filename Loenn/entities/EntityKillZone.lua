local drawing = require("utils.drawing")
local utils = require("utils")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperEntityKillZone = {}

FemtoHelperEntityKillZone.name = "FemtoHelper/EntityKillZone"
FemtoHelperEntityKillZone.depth = -100
FemtoHelperEntityKillZone.fieldInformation = {
    fillColor = {
        fieldType = "color",
        useAlpha = true
    },
    outlineColor = {
        fieldType = "color",
        useAlpha = true
    }
}
FemtoHelperEntityKillZone.placements = {
    {
        name = "default",
        data = {
            outlineColor = "FFFFFF",
            fillColor = "FFFFFF",
            outlineAlpha = 0.2666,
            fillAlpha = 0.0666,
            width = 16,
            height = 16,
            whitelist="FemtoHelper/SMWShell",
            blacklist="",
            attachToSolid = false,
        }
    },
}

local function getColor(color)
    local success, r, g, b, a = utils.parseHexColor(color)
    return success and {r, g, b, a or 1} or {1, 1, 1, 1}
end

function FemtoHelperEntityKillZone.sprite(room, entity)

    local color1 = getColor(entity.outlineColor)
    local color2 = getColor(entity.fillColor)

    local MainSprite = drawableSprite.fromTexture("loenn/FemtoHelper/EntityKillZone", entity)

    MainSprite:setJustification(0.5, 0.5)
    MainSprite:addPosition(entity.width / 2, entity.height / 2)

    local factor = math.min(math.min(entity.width, entity.height) / 16, 1)

    MainSprite:setScale(factor, factor)

    return {
        drawableRectangle.fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height, {color2[1], color2[2], color2[3], entity.fillAlpha * color2[4]}, {color1[1], color1[2], color1[3], entity.outlineAlpha * color1[4]}),
        MainSprite
    }
end

return FemtoHelperEntityKillZone