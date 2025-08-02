local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")
local FemtoHelperNodePuffer = {}

FemtoHelperNodePuffer.name = "FemtoHelper/NodePuffer"
FemtoHelperNodePuffer.depth = 0
FemtoHelperNodePuffer.nodeVisibility = "never"
FemtoHelperNodePuffer.nodeLimits = {2, -1}
FemtoHelperNodePuffer.fieldInformation = {
    lineColor = {
      fieldType = "color"
    },
    activeLineColor = {
      fieldType = "color"
    },
}
FemtoHelperNodePuffer.placements = {
    {
        name = "nodePufferLeft",
        data = {
            speed = 2,
            right = false,
            lineColor = "FF69B4",
            activeLineColor = "FF1493",
        }
    },
    {
        name = "nodePufferRight",
        data = {
            speed = 2,
            right = true,
            lineColor = "FF69B4",
            activeLineColor = "FF1493",
        }
    }
}

function FemtoHelperNodePuffer.sprite(room, entity)

    local sprites = {}
    local nodePoints = {}

    table.insert(nodePoints, entity.x)
    table.insert(nodePoints, entity.y)

    local sprite = drawableSprite.fromTexture("objects/puffer/idle00", entity)

        if entity.right then
            sprite:setScale(1, 1)
        else
            sprite:setScale(-1, 1)
        end

    table.insert(sprites, sprite)

    for i = 1, #entity.nodes do
        local nodeSprite = drawableSprite.fromTexture("objects/puffer/idle00", entity)
        nodeSprite:setColor({1, 1, 1, 0.5})
        nodeSprite:setPosition(entity.nodes[i].x, entity.nodes[i].y)

        if entity.right then
            nodeSprite:setScale(1, 1)
        else
            nodeSprite:setScale(-1, 1)
        end

        table.insert(nodePoints, entity.nodes[i].x)
        table.insert(nodePoints, entity.nodes[i].y)

        table.insert(sprites, nodeSprite)
    end

    local Line = drawableLine.fromPoints(nodePoints, {1, 1, 1, 0.25}, 1)
    for _, sprite in ipairs(Line:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    return sprites
end

function FemtoHelperNodePuffer.selection(room, entity, nodes)

    local mainRectangle = {}
    local nodeRectangle = {}

    local sprite = drawableSprite.fromTexture("objects/puffer/idle00", entity)

    mainRectangle = utils.rectangle(x, y, sprite.meta.width * 2, sprite.meta.height * 2)

    for _, v in ipairs(nodes) do
        table.insert(nodeRectangle, utils.rectangle(v.x, v.y, sprite.meta.width * 2, sprite.meta.height * 2))
    end

    return mainRectangle, nodeRectangle
end

function FemtoHelperNodePuffer.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.right = not entity.right
    end

    return horizontal
end

return FemtoHelperNodePuffer