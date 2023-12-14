local drawableRectangle = require("structs.drawable_rectangle")
local xnaColors = require("consts.xna_colors")
local utils = require("utils")
local waterfallHelper = require("helpers.waterfalls")


local FemtoHelperCustomParallaxBigWaterfall = {}

FemtoHelperCustomParallaxBigWaterfall.name = "FemtoHelper/CustomParallaxBigWaterfall"
FemtoHelperCustomParallaxBigWaterfall.minimumSize = {16, 16}
FemtoHelperCustomParallaxBigWaterfall.fieldInformation = {
    layer = {
        options = {"FG", "BG"},
        editable = false
    }
}
FemtoHelperCustomParallaxBigWaterfall.placements = {
    {
        name = "fg",
        data = {
            width = 16,
            height = 16,
            parallax = 1.3,
            fallSpeedMultiplier = 1,
            surfaceOpacity = 0.5,
            fillOpacity = 0.3,
            surfaceColor = "89dbf0",
            fillColor = "29a7ea",
            layer = "FG"
        }
    },
    {
        name = "bg",
        data = {
            width = 16,
            height = 16,
            parallax = 0.7,
            fallSpeedMultiplier = 1,
            surfaceOpacity = 0.5,
            fillOpacity = 0.3,
            surfaceColor = "89dbf0",
            fillColor = "29a7ea",
            layer = "BG"
        }
    }
}

function FemtoHelperCustomParallaxBigWaterfall.depth(room, entity)
    local foreground = waterfallHelper.isForeground(entity)

    return foreground and -49900 or 10010
end

function FemtoHelperCustomParallaxBigWaterfall.sprite(room, entity)
    local color = utils.getColor(entity.fillColor)
    local color2 = utils.getColor(entity.surfaceColor)

    local fillColor = {color[1] * entity.fillOpacity, color[2] * entity.fillOpacity, color[3] * entity.fillOpacity, entity.fillOpacity}
    local borderColor = {color2[1] * entity.surfaceOpacity, color2[2] * entity.surfaceOpacity, color2[3] * entity.surfaceOpacity, entity.surfaceOpacity}
    return waterfallHelper.getBigWaterfallSprite(room, entity, fillColor, borderColor)
end

FemtoHelperCustomParallaxBigWaterfall.rectangle = waterfallHelper.getBigWaterfallRectangle

return FemtoHelperCustomParallaxBigWaterfall