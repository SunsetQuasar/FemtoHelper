local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local drawableLine = require("structs.drawable_line")

local FemtoHelperCinematicText = {}

FemtoHelperCinematicText.name = "FemtoHelper/CinematicText"
FemtoHelperCinematicText.depth = -3000000
FemtoHelperCinematicText.texture = "loenn/FemtoHelper/cinematictext"
FemtoHelperCinematicText.justification = {0.5, 0.5}
FemtoHelperCinematicText.nodeVisibility = "always"
FemtoHelperCinematicText.nodeRenderType = "line"

FemtoHelperCinematicText.nodeLimits = {0, 2}

FemtoHelperCinematicText.fieldInformation = {
    mainColor = {
        fieldType = "color",
        useAlpha = true
    },
    outlineColor = {
        fieldType = "color",
        useAlpha = true
    }
}

FemtoHelperCinematicText.placements = {
    name = "default",
    data = {
        activationTag = "tag",
        charList = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'\\\".,",
        charOriginX = 0,
        charOriginY = -2,
        delay = 0,
        depth = -10000,
        dialogID = "FemtoHelper_PlutoniumText_Example",
        disappearTime = 2,
        effects = true,
        fontHeight = 12,
        fontPath = "objects/FemtoHelper/PlutoniumText/example",
        fontWidth = 8,
        hud = true,
        ignoreAudioRegex = false,
	    instantReload = false,
	    instantLoad = false,
        truncateSliderValues = false,
        mainColor = "FFFFFFFF",
        nextTextTag = "tag2",
        obfuscated = false,
        onlyOnce = false,
        outlineColor = "000000FF",
        parallax = 1,
        phaseIncrement = 30,
        scale = 1,
        shadow = false,
        shake = false,
        shakeAmount = 1,
        spacing = 8,
        speed = 16,
        textSound = "event:/FemtoHelper/example_text_sound",
        twitch = false,
        twitchChance = 0,
        wave = true,
        wavePhaseOffset = 0,
        waveSpeed = 2,
        waveX = 0,
        waveY = 1,
        retriggerable = false,
        visibilityFlag = "",
        decimals = -1,
    }
}

function FemtoHelperCinematicText.nodeSprite(room, entity, node, nodeIndex) 
    local spr = {}
    local sprite = (nodeIndex == 1) and drawableSprite.fromTexture("loenn/FemtoHelper/cinematictext", entity) or drawableSprite.fromTexture("loenn/FemtoHelper/cinematictextnode", entity)
    sprite:addPosition(node.x - entity.x, node.y - entity.y);
    if (nodeIndex == 1) then
        sprite:setColor({1, 1, 1, 0.5})
    end

    table.insert(spr, sprite)
    table.insert(spr, drawableLine.fromPoints({entity.x, entity.y, node.x, node.y}, {1, 0, 0.5, 0.3}, 1))

    return spr
end

function FemtoHelperCinematicText.selection(room, entity)
    local nodelist = {}
    for i, node in ipairs(entity.nodes) do

        local rect = utils.rectangle(node.x - 4, node.y - 4, 8, 8)

        if (i == 1) then
            rect = utils.rectangle(node.x - 16, node.y - 4, 32, 8)
        end

        table.insert(nodelist, rect)

    end
    return utils.rectangle(entity.x - 16, entity.y - 4, 32, 8), nodelist
end

return FemtoHelperCinematicText