local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local FemtoHelperWaterZipMover = {}

local blockNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local centerColor = {0, 0, 0}
local ropeColor = {44 / 255, 85 / 255, 96 / 255, 93 / 255}


FemtoHelperWaterZipMover.name = "FemtoHelper/WaterZipMover"
FemtoHelperWaterZipMover.depth = -9999
FemtoHelperWaterZipMover.nodeVisibility = "never"
FemtoHelperWaterZipMover.nodeLimits = {1, 1}
FemtoHelperWaterZipMover.warnBelowSize = {16, 16}
FemtoHelperWaterZipMover.fieldInformation = {
    behavior = {
        options = {
            "Default", "NoReturn", "Permanent"
        },
        editable = false
    }
}
FemtoHelperWaterZipMover.placements = {
    {
        name = 'waterZipMover',
        data = {
            width = 16,
            height = 16,
            behavior = 'Default',
            canCarry = true,
            spritePath = 'objects/FemtoHelper/waterZipMover/'
        }
    },
    {
        name = 'waterZipMoverNoReturn',
        data = {
            width = 16,
            height = 16,
            behavior = 'NoReturn',
            canCarry = true,
            spritePath = 'objects/FemtoHelper/waterZipMover/'
        }
    }
}

local function addNodeSprites(sprites, entity, cogTexture, centerX, centerY, centerNodeX, centerNodeY)
    local nodeCogSprite = drawableSprite.fromTexture(cogTexture, entity)

    nodeCogSprite:setPosition(centerNodeX, centerNodeY)
    nodeCogSprite:setJustification(0.5, 0.5)

    local nodeCogSprite2 = drawableSprite.fromTexture(cogTexture, entity)

    nodeCogSprite2:setPosition(centerX, centerY)
    nodeCogSprite2:setJustification(0.5, 0.5)

    local noReturnIcon = drawableSprite.fromTexture(entity.spritePath..(entity.behavior == 'NoReturn' and 'noReturn' or 'permanent'), entity)

    noReturnIcon:setPosition(centerX, centerY)
    noReturnIcon:setJustification(0.5, 0.5)

    local points = {centerX, centerY, centerNodeX, centerNodeY}
    local leftLine = drawableLine.fromPoints(points, ropeColor, 1)
    local rightLine = drawableLine.fromPoints(points, ropeColor, 1)

    leftLine:setOffset(0, 4.5)
    rightLine:setOffset(0, -4.5)

    leftLine.depth = 5000
    rightLine.depth = 5000

    for _, sprite in ipairs(leftLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    for _, sprite in ipairs(rightLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    table.insert(sprites, nodeCogSprite)
    table.insert(sprites, nodeCogSprite2)
    if entity.behavior == 'NoReturn' or entity.behavior == 'Permanent' then table.insert(sprites, noReturnIcon) end
end

local function addBlockSprites(sprites, entity, blockTexture, lightsTextureL, lightsTextureR, x, y, width, height, nx, ny)
    --local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, centerColor)

    local frameNinePatch = drawableNinePatch.fromTexture(blockTexture, blockNinePatchOptions, x, y, width, height)
    local frameNinePatch2 = drawableNinePatch.fromTexture(blockTexture, blockNinePatchOptions, nx - width/2, ny - height/2, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()
    local frameSprites2 = frameNinePatch2:getDrawableSprite()

    local lightsSpriteL = drawableSprite.fromTexture(lightsTextureL, entity)

    lightsSpriteL:setJustification(0.0, 0.0)

    local lightsSpriteR = drawableSprite.fromTexture(lightsTextureR, entity)

    lightsSpriteR:addPosition(width, 0)
    lightsSpriteR:setJustification(1, 0.0)

    --table.insert(sprites, rectangle:getDrawableSprite())

    for _, sprite in ipairs(frameSprites) do
        table.insert(sprites, sprite)
    end

    for _, sprite in ipairs(frameSprites2) do
        sprite:setColor({1, 1, 1, 0.2})
        table.insert(sprites, sprite)
    end

    table.insert(sprites, lightsSpriteL)
    table.insert(sprites, lightsSpriteR)
end

function FemtoHelperWaterZipMover.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y

    local centerX, centerY = x + halfWidth, y + halfHeight
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    addNodeSprites(sprites, entity, entity.spritePath.."cog", centerX, centerY, centerNodeX, centerNodeY)
    addBlockSprites(sprites, entity, entity.spritePath.."nineSlice", entity.spritePath.."cornerL01", entity.spritePath.."cornerR01", x, y, width, height, centerNodeX, centerNodeY)

    return sprites
end

function FemtoHelperWaterZipMover.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    local cogSprite = drawableSprite.fromTexture(entity.spritePath.."cog", entity)
    local cogWidth, cogHeight = cogSprite.meta.width, cogSprite.meta.height

    local mainRectangle = utils.rectangle(x, y, width, height)
    local nodeRectangle = utils.rectangle(centerNodeX - math.floor(cogWidth / 2), centerNodeY - math.floor(cogHeight / 2), cogWidth, cogHeight)

    return mainRectangle, {nodeRectangle}
end

return FemtoHelperWaterZipMover