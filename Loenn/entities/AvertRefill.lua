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
            respawnTime = 2.5
        }
    },
        {
        name = "avertDown",
        data = {
            oneUse = false,
            direction = "Down",
            respawnTime = 2.5
        }
    },
        {
        name = "avertLeft",
        data = {
            oneUse = false,
            direction = "Left",
            respawnTime = 2.5
        }
    },
        {
        name = "avertRight",
        data = {
            oneUse = false,
            direction = "Right",
            respawnTime = 2.5
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
    if direction > 0 then
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

function FemtoHelperAvertRefill.rotation(room, entity)
    return spintable[string.lower(entity.direction)] * 0.0174533
end

function FemtoHelperAvertRefill.texture(room, entity)
    return "objects/FemtoHelper/bubbleRedirect/idle00"
end

return FemtoHelperAvertRefill