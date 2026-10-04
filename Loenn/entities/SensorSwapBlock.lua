local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local enums = require("consts.celeste_enums")

local FemtoHelperSensorSwapBlock = {}

local nodeFrameColor = {1.0, 1.0, 1.0, 0.35}

local frameNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

local frameNodeNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    color = nodeFrameColor
}

local trailNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    useRealSize = true,
    color = nodeFrameColor
}


local themes = {
    "Normal", "Moon"
}

local trailDepth = 8999
local blockDepth = -9999

FemtoHelperSensorSwapBlock.name = "FemtoHelper/SensorSwapBlock"
FemtoHelperSensorSwapBlock.nodeLimits = {1, 1}
FemtoHelperSensorSwapBlock.placements = {}
FemtoHelperSensorSwapBlock.warnBelowSize = {16, 16}
FemtoHelperSensorSwapBlock.nodeVisibility = "always"
FemtoHelperSensorSwapBlock.placements = {
    {
        name = "sensor",
        data = {
            width = 16,
            height = 16,
        }
    }
}


local function addBlockSprites(sprites, entity, position, frameTexture, middleTexture, isNode)
    local x, y = position.x or 0, position.y or 0
    local width, height = entity.width or 8, entity.height or 8

    local ninePatchOptions = isNode and frameNodeNinePatchOptions or frameNinePatchOptions
    local frameNinePatch = drawableNinePatch.fromTexture(frameTexture, ninePatchOptions, x, y, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()
    local middleSprite = drawableSprite.fromTexture(middleTexture, position)

    middleSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    middleSprite.depth = blockDepth

    if isNode then
        middleSprite:setColor(nodeFrameColor)
    end

    for _, sprite in ipairs(frameSprites) do
        sprite.depth = blockDepth

        table.insert(sprites, sprite)
    end

    table.insert(sprites, middleSprite)
end

local function addTrailSprites(sprites, entity, trailTexture)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y
    local width, height = entity.width or 8, entity.height or 8
    local drawWidth, drawHeight = math.abs(x - nodeX) + width, math.abs(y - nodeY) + height

    x, y = math.min(x, nodeX), math.min(y, nodeY)

    local frameNinePatch = drawableNinePatch.fromTexture(trailTexture, trailNinePatchOptions, x, y, drawWidth, drawHeight)
    local frameSprites = frameNinePatch:getDrawableSprite()

    for _, sprite in ipairs(frameSprites) do
        sprite.depth = trailDepth

        table.insert(sprites, sprite)
    end
end

function FemtoHelperSensorSwapBlock.sprite(room, entity)
    local sprites = {}

    addTrailSprites(sprites, entity, "objects/FemtoHelper/sensorSwapBlock/target")
    addBlockSprites(sprites, entity, entity, "objects/FemtoHelper/sensorSwapBlock/blockRed", "objects/FemtoHelper/sensorSwapBlock/midBlockRed00")

    return sprites
end

function FemtoHelperSensorSwapBlock.nodeSprite(room, entity, node)
    local sprites = {}

    addBlockSprites(sprites, entity, node, "objects/FemtoHelper/sensorSwapBlock/blockRed", "objects/FemtoHelper/sensorSwapBlock/midBlockRed00", true)

    return sprites
end

function FemtoHelperSensorSwapBlock.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y
    local width, height = entity.width or 8, entity.height or 8

    return utils.rectangle(x, y, width, height), {utils.rectangle(nodeX, nodeY, width, height)}
end

return FemtoHelperSensorSwapBlock