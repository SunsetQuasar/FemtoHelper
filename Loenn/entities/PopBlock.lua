local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local FemtoHelperPopBlock = {}

FemtoHelperPopBlock.name = "FemtoHelper/PopBlock"
FemtoHelperPopBlock.depth = 0
FemtoHelperPopBlock.nodeVisibility = "never"
FemtoHelperPopBlock.minimumSize = {16, 16}
FemtoHelperPopBlock.nodeLimits = {0, 0}
FemtoHelperPopBlock.fieldInformation = {
    color = {
      fieldType = "color"
    },
    disabledColor = {
      fieldType = "color"
    },
    disabledColor2 = {
      fieldType = "color"
    },
    staticMoverColor = {
      fieldType = "color"
    },
    staticMoverDisabledColor = {
      fieldType = "color"
    },
    pushMode = {
        options = {
            "OppositeFacing",
            "Facing",
            "Right",
            "Down",
            "Left",
            "Up",
        },
        editable = false
    },
}
FemtoHelperPopBlock.placements = {
    {
        name = "PopBlock",
        data = {
            staticMoverColor = "586778",
            staticMoverDisabledColor = "23324E",
            pushMode = "OppositeFacing",
            color = "586778",
            disabledColor = "23324E",
            disabledColor2 = "112031",
            delay = 1.5,
            width = 16,
            height = 16,
            refillDash = false,
            path = "objects/FemtoHelper/PopBlock/",
        }
    },
    {
        name = "PopBlockRefill",
        data = {
            staticMoverColor = "609856",
            staticMoverDisabledColor = "264B38",
            pushMode = "OppositeFacing",
            color = "609856",
            disabledColor = "264B38",
            disabledColor2 = "132E23",
            delay = 1.5,
            width = 16,
            height = 16,
            refillDash = true,
            path = "objects/FemtoHelper/PopBlock/",
        }
    }
}

local centerSpriteLoc = {
    OppositeFacing = { x = 32, y = 0 },
    Facing = { x = 32, y = 8 },
    Right = { x = 8, y = 8 },
    Down = { x = 16, y = 24 },
    Left = { x = 8, y = 24 },
    Up = { x = 0, y = 24 }
}

function FemtoHelperPopBlock.sprite(room, entity)

    local sprites = {
        drawableRectangle.fromRectangle("fill", entity.x, entity.y + entity.height, entity.width, 2, entity.disabledColor2),
        drawableRectangle.fromRectangle("fill", entity.x + 1, entity.y + entity.height, entity.width - 2, 2, entity.disabledColor),
    }

    local path = entity.path or "objects/FemtoHelper/PopBlock/"

    local centerLoc = centerSpriteLoc[entity.pushMode or "OppositeFacing"]

    for x = 0, entity.width - 8, 8 do
        for y = 0, entity.height - 8, 8 do
            local center = drawableSprite.fromTexture(path.."solid", entity)

            if center then
                center:addPosition(x, y)
                center:useRelativeQuad(centerLoc.x, centerLoc.y, 8, 8)
                table.insert(sprites, center)
            end

            local edge = drawableSprite.fromTexture(path.."solid", entity)

            if x == 0 then
                if y == 0 then
                    edge:useRelativeQuad(0, 0, 8, 8)
                elseif y == entity.height - 8 then
                    edge:useRelativeQuad(0, 16, 8, 8)
                else 
                    edge:useRelativeQuad(0, 8, 8, 8)
                end
            elseif x == entity.width - 8 then
                if y == 0 then
                    edge:useRelativeQuad(16, 0, 8, 8)
                elseif y == entity.height - 8 then
                    edge:useRelativeQuad(16, 16, 8, 8)
                else 
                    edge:useRelativeQuad(16, 8, 8, 8)
                end
            else
                if y == 0 then
                    edge:useRelativeQuad(8, 0, 8, 8)
                elseif y == entity.height - 8 then
                    edge:useRelativeQuad(8, 16, 8, 8)
                else 
                    edge = nil
                end
            end

            if edge then
                edge:addPosition(x, y)
                table.insert(sprites, edge)
            end
        end
    end

    return sprites
end

return FemtoHelperPopBlock