local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperAvertRefill = {}

FemtoHelperAvertRefill.name = "FemtoHelper/AvertRefill"
FemtoHelperAvertRefill.depth = -100
FemtoHelperAvertRefill.fieldInformation = {
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
FemtoHelperAvertRefill.placements = {
    {
        name = "avertUp",
        data = {
            oneUse = false,
            direction = "Up",
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
        {
        name = "avertDown",
        data = {
            oneUse = false,
            direction = "Down",
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
        {
        name = "avertLeft",
        data = {
            oneUse = false,
            direction = "Left",
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
        {
        name = "avertRight",
        data = {
            oneUse = false,
            direction = "Right",
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0
        }
    },
}

local spintable = {
    up = 0,
    upright = 45,
    right = 90,
    downright = 135,
    down = 180,
    downleft = 225,
    left = 270,
    upleft = 315,
}

function FemtoHelperAvertRefill.flip(room, entity, horizontal, vertical)
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

function FemtoHelperAvertRefill.rotate(room, entity, direction)
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

function FemtoHelperAvertRefill.sprite(room, entity)
    local sprPath = "objects/FemtoHelper/bubbleRedirect/"

    local angle = spintable[string.lower(entity.direction)] * 0.0174533

    local sprites = {}

    local vx = entity.visualOffsetX or 0;
    local vy = entity.visualOffsetY or 0;
    
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

return FemtoHelperAvertRefill