local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local FemtoHelperCircleMover = {}

local blockNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local centerColor = {0, 0, 0}
local ropeColor = {44 / 255, 85 / 255, 96 / 255, 93 / 255}


FemtoHelperCircleMover.name = "FemtoHelper/CircleMover"
FemtoHelperCircleMover.depth = -9999
FemtoHelperCircleMover.nodeVisibility = "never"
FemtoHelperCircleMover.nodeLimits = {1, 1}
FemtoHelperCircleMover.warnBelowSize = {16, 16}
FemtoHelperCircleMover.fieldInformation = {
    behavior = {
        options = {
            "Default", "NoReturn", "Permanent"
        },
        editable = false
    }
}
FemtoHelperCircleMover.placements = {
    {
        name = 'circleMover',
        data = {
            width = 16,
            height = 16,
            behavior = 'Default',
            chainZipperFlag = "",
            counterClockwise = true,
            activateFallingBlocks = false,
            angle = 180,
            spritePath = 'objects/FemtoHelper/circleMover/'
        }
    },
    {
        name = 'circleMoverNoReturn',
        data = {
            width = 16,
            height = 16,
            behavior = 'NoReturn',
            chainZipperFlag = "",
            counterClockwise = true,
            activateFallingBlocks = false,
            angle = 180,
            spritePath = 'objects/FemtoHelper/circleMover/'
        }
    }
}

local function addNodeSprites(sprites, entity, cogTexture, centerX, centerY, centerNodeX, centerNodeY)
    local nodeCogSprite = drawableSprite.fromTexture(entity.spritePath.."lonnAnchor", entity)

    nodeCogSprite:setPosition(centerNodeX, centerNodeY)
    nodeCogSprite:setJustification(0.5, 0.5)

    local nodeCogSprite2 = drawableSprite.fromTexture(cogTexture, entity)

    local dx, dy = (entity.x + entity.width/2) - centerNodeX, (entity.y + entity.height/2) - centerNodeY
    local radius = math.sqrt(dx ^ 2 + dy ^ 2)

    local angleOffset = math.atan(dy / dx) + (dx >= 0 and 0 or math.pi)

    local clockwise = not entity.counterClockwise
    local sign = (clockwise and 1 or -1)
    local angle = sign * (entity.angle or 180.0) * math.pi / 180.0

    local th = angleOffset + angle
    local endpointX = centerNodeX - entity.width/2 + math.cos(th) * radius
    local endpointY = centerNodeY - entity.height/2 + math.sin(th) * radius

    nodeCogSprite2:setPosition(endpointX + entity.width/2, endpointY + entity.height/2)
    nodeCogSprite2:setJustification(0.5, 0.5)
    nodeCogSprite2:setColor({1, 1, 1, 0.5})

    local nodeCogSprite3 = drawableSprite.fromTexture(cogTexture, entity)

    nodeCogSprite3:setPosition(centerX, centerY)
    nodeCogSprite3:setJustification(0.5, 0.5)

    local points = {centerX, centerY, centerNodeX, centerNodeY}
    local leftLine = drawableLine.fromPoints(points, ropeColor, 1)

    leftLine.depth = 5000

    for _, sprite in ipairs(leftLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    table.insert(sprites, nodeCogSprite)
    table.insert(sprites, nodeCogSprite2)
    table.insert(sprites, nodeCogSprite3)
end

local precision = 64

local function addArc(sprites, x, y, width, height, r, offsetAngle, arcAngle)
    local points = {}

    for i = 0, precision do
        local th = offsetAngle + arcAngle * i / precision
        table.insert(points, (x + math.cos(th) * r))
        table.insert(points, (y + math.sin(th) * r))
    end

    table.insert(sprites, drawableLine.fromPoints(points, {1.0, 1.0, 1.0, 0.3}))
end

local function addBlockSprites(sprites, entity, blockTexture, lightsTextureL, lightsTextureR, x, y, width, height, nx, ny)
    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, centerColor)

    local dx, dy = (x + width/2) - nx, (y + height/2) - ny
    local radius = math.sqrt(dx ^ 2 + dy ^ 2)

    local angleOffset = math.atan(dy / dx) + (dx >= 0 and 0 or math.pi)

    local clockwise = not entity.counterClockwise
    local sign = (clockwise and 1 or -1)
    local angle = sign * (entity.angle or 180.0) * math.pi / 180.0

    addArc(sprites, nx, ny, width, height, radius, angleOffset, angle)

    local frameNinePatch = drawableNinePatch.fromTexture(blockTexture, blockNinePatchOptions, x, y, width, height)
    local th = angleOffset + angle
    local endpointX = nx + math.cos(th) * (radius)
    local endpointY = ny + math.sin(th) * (radius)
    local frameNinePatch2 = drawableNinePatch.fromTexture(blockTexture, blockNinePatchOptions, endpointX - width/2, endpointY - height/2, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()
    local frameSprites2 = frameNinePatch2:getDrawableSprite()

    local lightsSpriteR = drawableSprite.fromTexture(lightsTextureR, entity)

    lightsSpriteR:addPosition(width / 2, height / 2)
    lightsSpriteR:setJustification(0.5, 0.5)

    local lightsSpriteL = drawableSprite.fromTexture(lightsTextureL, entity)

    lightsSpriteL:setJustification(0.5, 0.5)
    lightsSpriteL:addPosition(width / 2, height / 2)

    table.insert(sprites, rectangle:getDrawableSprite())

    for _, sprite in ipairs(frameSprites) do
        table.insert(sprites, sprite)
    end

    for _, sprite in ipairs(frameSprites2) do
        sprite:setColor({1, 1, 1, 0.5})
        table.insert(sprites, sprite)
    end

    table.insert(sprites, lightsSpriteR)
    table.insert(sprites, lightsSpriteL)
end

function FemtoHelperCircleMover.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y

    local centerX, centerY = x + halfWidth, y + halfHeight
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    
    local suffix = entity.behavior == "NoReturn" and "NoReturn" or ""

    addNodeSprites(sprites, entity, entity.spritePath.."cog"..suffix, centerX, centerY, centerNodeX, centerNodeY)
    addBlockSprites(sprites, entity, entity.spritePath.."block"..suffix, entity.spritePath.."centerRing"..suffix, entity.spritePath.."centerGem"..suffix, x, y, width, height, centerNodeX, centerNodeY)

    return sprites
end

function FemtoHelperCircleMover.selection(room, entity)
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

return FemtoHelperCircleMover