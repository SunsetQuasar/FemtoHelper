local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperOcularBarrier = {}

FemtoHelperOcularBarrier.name = "FemtoHelper/OcularBarrier"
FemtoHelperOcularBarrier.fieldInformation = {
    activeColor = {
        fieldType = "color"
    },
    inactiveColor = {
        fieldType = "color"
    },
    invertedActiveColor = {
        fieldType = "color"
    },
    invertedInactiveColor = {
        fieldType = "color"
    },
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

FemtoHelperOcularBarrier.minimumSize = {16, 16}

FemtoHelperOcularBarrier.placements = {
    name = "ocularBarrier",
    data = {
        flag = "lookout_interacting",
        invert = false,
        width = 16,
        height = 16,
        activeColor = "99FF66",
        inactiveColor = "005500",
        invertedActiveColor = "6699FF",
        invertedInactiveColor = "000055",
        texturePath = "objects/FemtoHelper/OcularBarrier/"
    }
}


local function getBlockTexture(entity)

    local block = entity.invert and entity.texturePath.."nineSliceOn" or entity.texturePath.."nineSliceOff"

    local center = entity.invert and entity.texturePath.."centerOn" or entity.texturePath.."centerOff"

    return block, center
end


function FemtoHelperOcularBarrier.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16

    local blockTexture, centerTexture = getBlockTexture(entity)

    local ninePatch = drawableNinePatch.fromTexture(blockTexture, ninePatchOptions, x, y, width, height)
    local centerSprite = drawableSprite.fromTexture(centerTexture, entity)
    local sprites = ninePatch:getDrawableSprite()

    for _, v in pairs(sprites) do
        v:setColor(entity.invert and entity.invertedActiveColor or entity.inactiveColor)
    end

    centerSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    centerSprite:setColor(entity.invert and entity.invertedActiveColor or entity.inactiveColor)
    table.insert(sprites, centerSprite)

    return sprites
end

FemtoHelperOcularBarrier.depth = 0

return FemtoHelperOcularBarrier