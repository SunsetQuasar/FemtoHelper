local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local FemtoHelperWaterFallingBlock = {}

local blockNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local centerColor = {0, 0, 0}
local ropeColor = {44 / 255, 85 / 255, 96 / 255, 93 / 255}


FemtoHelperWaterFallingBlock.name = "FemtoHelper/WaterFallingBlock"
FemtoHelperWaterFallingBlock.depth = -9999
FemtoHelperWaterFallingBlock.warnBelowSize = {16, 16}
FemtoHelperWaterFallingBlock.fieldInformation = {
    direction = {
        options = {
            {"Down", 0},
            {"Right", 1},
            {"Up", 2},
            {"Left", 3}
        },
        editable = false
    }
}
FemtoHelperWaterFallingBlock.placements = {
    {
        name = 'waterFallingBlockD',
        data = {
            width = 16,
            height = 16,
            canCarry = true,
            direction = 0,
            spritePath = "objects/FemtoHelper/waterFallingBlock/",
        }
    },
    {
        name = 'waterFallingBlockR',
        data = {
            width = 16,
            height = 16,
            canCarry = true,
            direction = 1,
            spritePath = "objects/FemtoHelper/waterFallingBlock/",
        }
    },
    {
        name = 'waterFallingBlockU',
        data = {
            width = 16,
            height = 16,
            canCarry = true,
            direction = 2,
            spritePath = "objects/FemtoHelper/waterFallingBlock/",
        }
    },
    {
        name = 'waterFallingBlockL',
        data = {
            width = 16,
            height = 16,
            canCarry = true,
            direction = 3,
            spritePath = "objects/FemtoHelper/waterFallingBlock/",
        }
    }
}

local angles = {
    90 * 0.0174533,
    0 * 0.0174533,
    270 * 0.0174533,
    180 * 0.0174533,
}

local function addBlockSprites(sprites, entity, blockTexture, arrowTexture, x, y, width, height)

    local frameNinePatch = drawableNinePatch.fromTexture(blockTexture, blockNinePatchOptions, x, y, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()

    
    for _, sprite in ipairs(frameSprites) do
        table.insert(sprites, sprite)
    end

    for i=1, width/8 do
        local arrow = drawableSprite.fromTexture(arrowTexture, entity)

        arrow:setPosition(x + (i * 8) - 4, y + height/2 + math.sin(i * 2) * 2)
    
        arrow.rotation = angles[entity.direction+1]

        table.insert(sprites, arrow)
    end
end

function FemtoHelperWaterFallingBlock.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    addBlockSprites(sprites, entity, entity.spritePath.."nineSlice", entity.spritePath.."arrow", x, y, width, height)

    return sprites
end

return FemtoHelperWaterFallingBlock