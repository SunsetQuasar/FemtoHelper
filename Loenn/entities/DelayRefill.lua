local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local FemtoHelperDelayRefill = {}

FemtoHelperDelayRefill.name = "FemtoHelper/DelayRefill"
FemtoHelperDelayRefill.depth = -100
FemtoHelperDelayRefill.nodeLimits = {0, 1}
FemtoHelperDelayRefill.nodeVisibility = "never"
FemtoHelperDelayRefill.fieldInformation = {
    direction = {
        options = {
            "Up",
            "Down",
            "Left",
            "Right"
        },
        editable = true
    },
}
FemtoHelperDelayRefill.placements = {
    {
        name = "delayUp",
        data = {
            oneUse = false,
            direction = "Up",
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0,
            delay = 1.2,
        }
    },
        {
        name = "delayDown",
        data = {
            oneUse = false,
            direction = "Down",
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0,
            delay = 1.2,
        }
    },
        {
        name = "delayLeft",
        data = {
            oneUse = false,
            direction = "Left",
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0,
            delay = 1.2,
        }
    },
        {
        name = "delayRight",
        data = {
            oneUse = false,
            direction = "Right",
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0,
            delay = 1.2,
        }
    },
}

local spintable = {
    up = 0,
    -- upright = 45,
    right = 90,
    -- downright = 135,
    down = 180,
    -- downleft = 225,
    left = 270,
    -- upleft = 315,
}

function FemtoHelperDelayRefill.flip(room, entity, horizontal, vertical)
    if horizontal then
        if string.lower(entity.direction) == "left" then entity.direction = "right"
            return true 
        end
        if string.lower(entity.direction) == "right" then entity.direction = "left"
            return true 
        end
    end

    if vertical then
        if string.lower(entity.direction) == "up" then entity.direction = "down"
            return true 
        end
        if string.lower(entity.direction) == "down" then entity.direction = "up"
            return true 
        end
    end

    return false
end

function FemtoHelperDelayRefill.rotate(room, entity, direction)
    if direction < 0 then
        if string.lower(entity.direction) == "up" then 
            entity.direction = "left"
        elseif string.lower(entity.direction) == "left" then
            entity.direction = "down"
        elseif string.lower(entity.direction) == "down" then
            entity.direction = "right"
        elseif string.lower(entity.direction) == "right" then
            entity.direction = "up" 
        end
    else
        if string.lower(entity.direction) == "up" then
            entity.direction = "right"
        elseif string.lower(entity.direction) == "right" then
            entity.direction = "down"
        elseif string.lower(entity.direction) == "down" then
            entity.direction = "left"
        elseif string.lower(entity.direction) == "left" then
            entity.direction = "up" 
        end
    end

    return true
end

function FemtoHelperDelayRefill.sprite(room, entity)
    local sprPath = "objects/FemtoHelper/bumpRefill/"

    local angle = spintable[string.lower(entity.direction)] * 0.0174533

    local sprites = {}

    local vx = entity.visualOffsetX or 0;
    local vy = entity.visualOffsetY or 0;

    local clock = drawableSprite.fromTexture(sprPath.."timer05", entity)
    if #entity.nodes > 0 then 
        local node = entity.nodes[1] or {x = entity.x, y = entity.y}
        clock:setPosition(entity.nodes[1].x, entity.nodes[1].y)
        table.insert(sprites, drawableLine.fromPoints({entity.x, entity.y, node.x, node.y}, {1, 1, 1, 0.25}, 1))
    end
    table.insert(sprites, clock)
    
    if vx ~= 0 or vy ~= 0 then
        local main = drawableSprite.fromTexture(sprPath.."outline", entity)
        main.rotation = angle
        table.insert(sprites, main)

        local offset_spr = drawableSprite.fromTexture(sprPath.."idle00", entity)
        offset_spr.rotation = angle
        offset_spr:setColor({1, 1, 1, 0.5})
        offset_spr:addPosition(vx, vy)
        table.insert(sprites, offset_spr)
    else
        local main = drawableSprite.fromTexture(sprPath.."idle00", entity)
        main.rotation = angle
        table.insert(sprites, main)
    end

    return sprites
end

function FemtoHelperDelayRefill.selection(room, entity) 
    local nodeRecs = {}

    for k, node in pairs(entity.nodes) do
        table.insert(nodeRecs,  utils.rectangle(node.x - 8, node.y - 8, 16, 16))
    end

    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16), nodeRecs
end

return FemtoHelperDelayRefill