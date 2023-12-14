local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperCustomCheckpoint = {}

FemtoHelperCustomCheckpoint.name = "FemtoHelper/FoolSpaceCustomCheckpoint"
FemtoHelperCustomCheckpoint.depth = 8999
FemtoHelperCustomCheckpoint.fieldInformation = {
    number = {
        fieldType = "integer",
    }
}
FemtoHelperCustomCheckpoint.placements = {
    name = "default",
    data = {
        digitA = "0",
        digitB = "0",
        spriteRoot = "scenery/foolspace_checkpoint/",
        colors = "FF0000,00FF00,0000FF"
    }
}

function FemtoHelperCustomCheckpoint.sprite(room, entity)
    local sprite_root = entity.spriteRoot
    local sprite_digit_a = entity.digitA
    local sprite_digit_b = entity.digitB
    local number = entity.number or 0
    local digit1 = math.floor(number % 100 / 10)
    local digit2 = number % 10

    local backSprite = drawableSprite.fromTexture(sprite_root .. "base02", entity)
    local backDigit1 = drawableSprite.fromTexture(sprite_root .. "numberbg0" .. sprite_digit_a, entity)
    local frontDigit1 = drawableSprite.fromTexture(sprite_root .. "number0" .. sprite_digit_a, entity)
    local backDigit2 = drawableSprite.fromTexture(sprite_root .. "numberbg0" .. sprite_digit_b, entity)
    local frontDigit2 = drawableSprite.fromTexture(sprite_root .. "number0" .. sprite_digit_b, entity)

    backDigit1:addPosition(-2, 4)
    frontDigit1:addPosition(-2, 4)
    backDigit2:addPosition(2, 4)
    frontDigit2:addPosition(2, 4)

    local sprites = {
        backSprite,
        backDigit1,
        backDigit2,
        frontDigit1,
        frontDigit2
    }

    return sprites
end

return FemtoHelperCustomCheckpoint