local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local FemtoHelperWaterMoveBlock = {}

local blockNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local centerColor = {0, 0, 0}
local ropeColor = {44 / 255, 85 / 255, 96 / 255, 93 / 255}


FemtoHelperWaterMoveBlock.name = "FemtoHelper/MovingWaterBlock"
FemtoHelperWaterMoveBlock.depth = -9999
FemtoHelperWaterMoveBlock.warnBelowSize = {16, 16}
FemtoHelperWaterMoveBlock.placements = {
    {
        name = 'waterMoveBlock',
        data = {
            width = 16,
            height = 16,
            maxSpeed = 60,
            angle = 180,
            canCarry = true,
            spritePath = 'objects/FemtoHelper/moveWater/'
        }
    }
}

local function addBlockSprites(sprites, entity, blockTexture, arrowTexture, x, y, width, height)

    local frameNinePatch = drawableNinePatch.fromTexture(blockTexture, blockNinePatchOptions, x, y, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()

    local arrow = drawableSprite.fromTexture(arrowTexture, entity)

    arrow:addPosition(width/2, height/2)

    arrow.rotation = entity.angle * 0.0174533

    for _, sprite in ipairs(frameSprites) do
        table.insert(sprites, sprite)
    end

    table.insert(sprites, arrow)
end

function FemtoHelperWaterMoveBlock.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    addBlockSprites(sprites, entity, entity.spritePath.."nineSlice", entity.spritePath.."arrow", x, y, width, height)

    return sprites
end

return FemtoHelperWaterMoveBlock