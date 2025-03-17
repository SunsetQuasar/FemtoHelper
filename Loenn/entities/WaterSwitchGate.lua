local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local FemtoHelperWaterSwitchGate = {}

local blockNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local centerColor = {0, 0, 0}
local ropeColor = {44 / 255, 85 / 255, 96 / 255, 93 / 255}


FemtoHelperWaterSwitchGate.name = "FemtoHelper/WaterSwitchGate"
FemtoHelperWaterSwitchGate.depth = -9999
FemtoHelperWaterSwitchGate.nodeLimits = {1, 1}
FemtoHelperWaterSwitchGate.warnBelowSize = {16, 16}
FemtoHelperWaterSwitchGate.placements = {
    {
        name = 'waterSwitchGate',
        data = {
            width = 16,
            height = 16,
            canCarry = true,
            spritePath = 'objects/FemtoHelper/waterSwitchGate/',
            persistent = false
        }
    }
}

local function addBlockSprites(sprites, entity, blockTexture, arrowTexture, x, y, width, height)

    local frameNinePatch = drawableNinePatch.fromTexture(blockTexture, blockNinePatchOptions, x, y, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()

    local arrow = drawableSprite.fromTexture(arrowTexture, entity)

    arrow:addPosition(width/2, height/2)

    for _, sprite in ipairs(frameSprites) do
        table.insert(sprites, sprite)
    end

    table.insert(sprites, arrow)
end

function FemtoHelperWaterSwitchGate.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    addBlockSprites(sprites, entity, entity.spritePath.."nineSlice", entity.spritePath.."icon00", x, y, width, height)

    return sprites
end

return FemtoHelperWaterSwitchGate