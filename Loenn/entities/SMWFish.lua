local drawableSprite = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")

local FemtoHelperSMWFish = {}

FemtoHelperSMWFish.name = "FemtoHelper/SMWFish"
FemtoHelperSMWFish.depth = -120000
FemtoHelperSMWFish.justification = {0, 0}
FemtoHelperSMWFish.placements = {
    name = "smwfishdefault",
    data = {
        path = "objects/FemtoHelper/SMWFish/normal/",
        audioPath = "event:/FemtoHelper/",
        blurp = false,
        big = false,
        initialSpeedX = 0, 
        initialSpeedY = 0,
        gravity = 260,
        activationFlag = "fish_flag",
        depth = -120000
    }
}

function FemtoHelperSMWFish.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(entity.path.."cheep", entity)
    if entity.blurp then
        sprite = drawableSprite.fromTexture(entity.path.."blurp", entity)
    end

    sprite:useRelativeQuad(0, 0, sprite.meta.height, sprite.meta.height)

    if entity.big then 
        sprite:addPosition(-sprite.meta.height, -sprite.meta.height)
    else 
        sprite:addPosition(-sprite.meta.height / 2, -sprite.meta.height / 2)
    end

    sprite:setJustification(0.5, 0.5)

    if entity.big then
        sprite:setScale(2, 2)
    else
        sprite:setScale(1, 1)
    end

    return sprite
end

function FemtoHelperSMWFish.selection(room, entity)
    local hh = sprite.meta.height
    local hh2 = sprite.meta.height
    if entity.big then
        hh = hh * 2
    else
        hh2 = hh2 / 2
    end

    return utils.rectangle(entity.x - hh2, entity.y - hh2, hh, hh)
end


return FemtoHelperSMWFish