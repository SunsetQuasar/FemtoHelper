local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawing = require("utils.drawing")
local utils = require("utils")

local FemtoHelperDirectionalLineIndicator = {}

FemtoHelperDirectionalLineIndicator.name = "FemtoHelper/DirectionalLine"
FemtoHelperDirectionalLineIndicator.nodeLimits = {1, 1}

FemtoHelperDirectionalLineIndicator.fieldInformation = {
    positionEase = {
        options = {
            {"Linear", 0},
            {"SineIn", "SineIn"},
            {"SineOut", "SineOut"},
            {"SineInOut", "SineInOut"},
            {"QuadIn", "QuadIn"},
            {"QuadOut", "QuadOut"},
            {"QuadInOut", "QuadInOut"},
            {"CubeIn", "CubeIn"},
            {"CubeOut", "CubeOut"},
            {"CubeInOut", "CubeInOut"},
            {"QuintIn", "QuintIn"},
            {"QuintOut", "QuintOut"},
            {"QuintInOut", "QuintInOut"},
            {"ExpoIn", "ExpoIn"},
            {"ExpoOut", "ExpoOut"},
            {"ExpoInOut", "ExpoInOut"},
            {"BackIn", "BackIn"},
            {"BackOut", "BackOut"},
            {"BackInOut", "BackInOut"},
            {"BigBackIn", "BigBackIn"},
            {"BigBackOut", "BigBackOut"},
            {"BigBackInOut", "BigBackInOut"},
            {"ElasticIn", "ElasticIn"},
            {"ElasticOut", "ElasticOut"},
            {"ElasticInOut", "ElasticInOut"},
            {"BounceIn", "BounceIn"},
            {"BounceOut", "BounceOut"},
            {"BounceInOut", "BounceInOut"}
        },
        editable = false
    },
    alphaInEase = {
        options = {
            {"Linear", 0},
            {"SineIn", "SineIn"},
            {"SineOut", "SineOut"},
            {"SineInOut", "SineInOut"},
            {"QuadIn", "QuadIn"},
            {"QuadOut", "QuadOut"},
            {"QuadInOut", "QuadInOut"},
            {"CubeIn", "CubeIn"},
            {"CubeOut", "CubeOut"},
            {"CubeInOut", "CubeInOut"},
            {"QuintIn", "QuintIn"},
            {"QuintOut", "QuintOut"},
            {"QuintInOut", "QuintInOut"},
            {"ExpoIn", "ExpoIn"},
            {"ExpoOut", "ExpoOut"},
            {"ExpoInOut", "ExpoInOut"},
            {"BackIn", "BackIn"},
            {"BackOut", "BackOut"},
            {"BackInOut", "BackInOut"},
            {"BigBackIn", "BigBackIn"},
            {"BigBackOut", "BigBackOut"},
            {"BigBackInOut", "BigBackInOut"},
            {"ElasticIn", "ElasticIn"},
            {"ElasticOut", "ElasticOut"},
            {"ElasticInOut", "ElasticInOut"},
            {"BounceIn", "BounceIn"},
            {"BounceOut", "BounceOut"},
            {"BounceInOut", "BounceInOut"}
        },
        editable = false
    },
    alphaOutEase = {
        options = {
            {"Linear", 0},
            {"SineIn", "SineIn"},
            {"SineOut", "SineOut"},
            {"SineInOut", "SineInOut"},
            {"QuadIn", "QuadIn"},
            {"QuadOut", "QuadOut"},
            {"QuadInOut", "QuadInOut"},
            {"CubeIn", "CubeIn"},
            {"CubeOut", "CubeOut"},
            {"CubeInOut", "CubeInOut"},
            {"QuintIn", "QuintIn"},
            {"QuintOut", "QuintOut"},
            {"QuintInOut", "QuintInOut"},
            {"ExpoIn", "ExpoIn"},
            {"ExpoOut", "ExpoOut"},
            {"ExpoInOut", "ExpoInOut"},
            {"BackIn", "BackIn"},
            {"BackOut", "BackOut"},
            {"BackInOut", "BackInOut"},
            {"BigBackIn", "BigBackIn"},
            {"BigBackOut", "BigBackOut"},
            {"BigBackInOut", "BigBackInOut"},
            {"ElasticIn", "ElasticIn"},
            {"ElasticOut", "ElasticOut"},
            {"ElasticInOut", "ElasticInOut"},
            {"BounceIn", "BounceIn"},
            {"BounceOut", "BounceOut"},
            {"BounceInOut", "BounceInOut"}
        },
        editable = false
    },
    spriteCount = {
      fieldType = "integer"
    },
    color = {
      fieldType = "color"
    }
}
FemtoHelperDirectionalLineIndicator.placements = {
    name = "default",
    data = {
        activationTime = 1,
        alphaOutPercent = 0.3,
        deactivationFlag = "",
        alphaInEase = "SineInOut",
        duration = 2,
        spriteCount = 4,
        activationFlag = "",
        texture = "objects/FemtoHelper/directionalArrow/arrow",
        depth = -250,
        deactivationTime = 1,
        alphaMultiplier = 1,
        alphaOutEase = "SineInOut",
        positionEase = "SineInOut",
        alphaInPercent = 0.3,
        orientSprite = true,
        color = "ffffff"
    }
}

function FemtoHelperDirectionalLineIndicator.depth(room, entity)
    return entity.depth
end

function FemtoHelperDirectionalLineIndicator.sprite(room, entity)
    local sprites = {}

    if entity.nodes then
        for _, node in ipairs(entity.nodes) do
            for i=0, entity.spriteCount, 1
            do
                if i == entity.spriteCount then
                    local rect = drawableRectangle.fromRectangle("bordered", node.x - 2, node.y - 2, 4, 4, {1, 1, 1, 0.4}, {1, 1, 1, 0.6})
                    rect:setColor(entity.color, entity.color)
                    local r, g, b = unpack(rect.color or {})
                    local newColor = {r or 1, g or 1, b or 1, 0.4}
                    local newColor2 = {r or 1, g or 1, b or 1, 1}
                    rect:setColor(newColor, newColor2)
                    table.insert(sprites, rect)
                else
                    local arrow = drawableSprite.fromTexture(entity.texture, entity)
                    local xx = node.x - entity.x;
                    local yy = node.y - entity.y;
                    arrow:setPosition(entity.x + ((i / entity.spriteCount) * xx), entity.y + ((i / entity.spriteCount) * yy))
                    arrow:setColor(entity.color)
                    arrow:setAlpha(entity.alphaMultiplier)
                    arrow.rotation = math.atan2(yy, xx)
                    table.insert(sprites, arrow)
                end
            end
        end
    end

    return sprites
end

function FemtoHelperDirectionalLineIndicator.nodeSprite()
end

function FemtoHelperDirectionalLineIndicator.nodeRectangle(room, entity, node, nodeIndex)
    return utils.rectangle(node.x-6, node.y-6, 12, 12)
end

function FemtoHelperDirectionalLineIndicator.selection(room, entity)
    local x, y = entity.x, entity.y

    local mainRectangle = utils.rectangle(x-6, y-6, 12, 12)
    return mainRectangle
end




return FemtoHelperDirectionalLineIndicator