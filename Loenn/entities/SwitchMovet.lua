local drawableSprite = require("structs.drawable_sprite")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local FemtoHelperSwitchMovet = {}

FemtoHelperSwitchMovet.name = "FemtoHelper/SwitchMovet"
FemtoHelperSwitchMovet.depth = 0
FemtoHelperSwitchMovet.nodeVisibility = "never"
FemtoHelperSwitchMovet.minimumSize = {16, 16}
FemtoHelperSwitchMovet.nodeLimits = {1, 1}
FemtoHelperSwitchMovet.fieldInformation = {
    color = {
      fieldType = "color"
    },
}
FemtoHelperSwitchMovet.placements = {
    {
        name = "SwitchMovet",
        data = {
            width = 16, 
            height = 16,
            color = "FF0000",
            path = "objects/FemtoHelper/switchMovet/"
        }
    }
}

function FemtoHelperSwitchMovet.sprite(room, entity)

    local sprites = {}

    table.insert(sprites, drawableRectangle.fromRectangle("fill", entity.x - 1, entity.y - 1, entity.width + 2, entity.height + 2, {0, 0, 0, 1}):getDrawableSprite())

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y

    local points = {entity.x + (entity.width / 2), entity.y + (entity.height / 2), nodeX + (entity.width / 2), nodeY + (entity.height / 2)}
    local Line = drawableLine.fromPoints(points, ropeColor, 1)

    for _, sprite in ipairs(Line:getDrawableSprite()) do
        sprite:setColor(entity.color)
        table.insert(sprites, sprite)
    end

    local nodeCog2 = drawableSprite.fromTexture(entity.path.."gearsmall", entity)
    nodeCog2:addPosition(entity.width / 2, entity.height / 2)
    nodeCog2:addPosition(nodeX - entity.x, nodeY - entity.y + 1)
    nodeCog2:setColor({0, 0, 0, 1})
    table.insert(sprites, nodeCog2)
    local nodeCog = drawableSprite.fromTexture(entity.path.."gearsmall", entity)
    nodeCog:addPosition(entity.width / 2, entity.height / 2)
    nodeCog:addPosition(nodeX - entity.x, nodeY - entity.y)
    table.insert(sprites, nodeCog)

    local frameNinePatch = drawableNinePatch.fromTexture(entity.path.."block", {mode = "fill", borderMode = "repeat"}, entity.x, entity.y, entity.width, entity.height)
    local frameSprites = frameNinePatch:getDrawableSprite()

    for _, sprite in ipairs(frameSprites) do
        table.insert(sprites, sprite)
    end

    local cog = drawableSprite.fromTexture(entity.path.."gear", entity)
    cog:addPosition(entity.width / 2, entity.height / 2)

    local cog1 = drawableSprite.fromTexture(entity.path.."gear", entity)
    cog1:addPosition(entity.width / 2, entity.height / 2)
    cog1:setColor({0.25, 0.25, 0.25, 1})
    cog1:addPosition(0, 1)

    local cog2 = drawableSprite.fromTexture(entity.path.."gear", entity)
    cog2:addPosition(entity.width / 2, entity.height / 2)
    cog2:setColor({0.25, 0.25, 0.25, 1})
    cog2:addPosition(0, -1)

    local cog3 = drawableSprite.fromTexture(entity.path.."gear", entity)
    cog3:addPosition(entity.width / 2, entity.height / 2)
    cog3:setColor({0.25, 0.25, 0.25, 1})
    cog3:addPosition(1, 0)

    local cog4 = drawableSprite.fromTexture(entity.path.."gear", entity)
    cog4:addPosition(entity.width / 2, entity.height / 2)
    cog4:setColor({0.25, 0.25, 0.25, 1})
    cog4:addPosition(-1, 0)

    table.insert(sprites, cog1)
    table.insert(sprites, cog2)
    table.insert(sprites, cog3)
    table.insert(sprites, cog4)
    table.insert(sprites, cog)

    return sprites
end

function FemtoHelperSwitchMovet.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    local cogSprite = drawableSprite.fromTexture(entity.path.."gearsmall", entity)
    local cogWidth, cogHeight = cogSprite.meta.width, cogSprite.meta.height

    local mainRectangle = utils.rectangle(x, y, width, height)
    local nodeRectangle = utils.rectangle(centerNodeX - math.floor(cogWidth / 2), centerNodeY - math.floor(cogHeight / 2), cogWidth, cogHeight)

    return mainRectangle, {nodeRectangle}
end

return FemtoHelperSwitchMovet