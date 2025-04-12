local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local FemtoHelperWaterBlock = {}

local blockNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local centerColor = {0, 0, 0}
local ropeColor = {44 / 255, 85 / 255, 96 / 255, 93 / 255}


FemtoHelperWaterBlock.name = "FemtoHelper/WaterBlock"
FemtoHelperWaterBlock.depth = -9999
FemtoHelperWaterBlock.warnBelowSize = {16, 16}
FemtoHelperWaterBlock.placements = {
    {
        name = 'waterBlock',
        data = {
            width = 16,
            height = 16,
            spritePath = 'objects/FemtoHelper/moveWater/nineslice',
        }
    }
}

local function addBlockSprites(sprites, entity, blockTexture, x, y, width, height)
    local frameNinePatch = drawableNinePatch.fromTexture(blockTexture, blockNinePatchOptions, x, y, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()

    for _, sprite in ipairs(frameSprites) do
        table.insert(sprites, sprite)
    end
end

function FemtoHelperWaterBlock.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    addBlockSprites(sprites, entity, entity.spritePath, x, y, width, height)

    return sprites
end

return FemtoHelperWaterBlock