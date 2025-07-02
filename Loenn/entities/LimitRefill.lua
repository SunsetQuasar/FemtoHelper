local FemtoHelperLimitRefill = {}

FemtoHelperLimitRefill.name = "FemtoHelper/LimitRefill"
FemtoHelperLimitRefill.depth = -100
FemtoHelperLimitRefill.fieldInformation = {
    direction = {
        options = {
            "Up",
            "UpRight",
            "Right",
            "DownRight",
            "Down",
            "DownLeft",
            "Left",
            "UpLeft"
        },
        editable = false
    },
}
FemtoHelperLimitRefill.placements = {
}

-- i can't deal with this shit

for k, v in ipairs(FemtoHelperLimitRefill.fieldInformation.direction.options) do
    local placement = {
        name = "limit"..v,
        data = {
            oneUse = false,
            direction = v,
            respawnTime = 2.5
        }
    }
    table.insert(FemtoHelperLimitRefill.placements, placement)
end

function FemtoHelperLimitRefill.texture(room, entity)
    return "objects/FemtoHelper/limitRefill/"..(string.lower(entity.direction)).."/idle00"
end

-- i also can't deal with this shit

local cwtable = {
    Up = "UpRight",
    UpRight = "Right",
    Right = "DownRight",
    DownRight = "Down",
    Down = "DownLeft",
    DownLeft = "Left",
    Left = "UpLeft",
    UpLeft = "Up"
}

local ccwtable = {
    Up = "UpLeft",
    UpRight = "Up",
    Right = "UpRight",
    DownRight = "Right",
    Down = "DownRight",
    DownLeft = "Down",
    Left = "DownLeft",
    UpLeft = "Left"
}

local fliphtable = {
    Up = "Up",
    UpRight = "UpLeft",
    Right = "Left",
    DownRight = "DownLeft",
    Down = "Down",
    DownLeft = "DownRight",
    Left = "Right",
    UpLeft = "UpRight"
}

local flipvtable = {
    Up = "Down",
    UpRight = "DownRight",
    Right = "Right",
    DownRight = "UpRight",
    Down = "Up",
    DownLeft = "UpLeft",
    Left = "Left",
    UpLeft = "DownLeft"
}

function FemtoHelperLimitRefill.rotate(room, entity, direction)
    entity.direction = direction > 0 and cwtable[entity.direction] or ccwtable[entity.direction]
    return true
end

function FemtoHelperLimitRefill.flip(room, entity, horizontal, vertical)
    local way = horizontal and fliphtable or flipvtable

    if entity.direction == way[entity.direction] then return false end

    entity.direction = way[entity.direction]

    return true
end

return FemtoHelperLimitRefill