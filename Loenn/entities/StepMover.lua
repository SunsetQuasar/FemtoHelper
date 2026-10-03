local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local FemtoHelperStepMover = {}

FemtoHelperStepMover.name = "FemtoHelper/StepMover"
FemtoHelperStepMover.depth = 7000
FemtoHelperStepMover.nodeLimits = {2, 3}
FemtoHelperStepMover.nodeVisibility = "never"
FemtoHelperStepMover.warnBelowSize = {16, 16}
FemtoHelperStepMover.placements = {
    {
        name = "step_mover",
        data = {
            moveTime = 0.5,
            height = 16,
            targetBoxWidth = 16,
            icon = "objects/FemtoHelper/StepMover/crumbletarget",
            iconCountX = 1,
            iconCountY = 1,
            iconSpacingX = 8,
            iconSpacingY = 8,
            width = 16,
            targetBoxHeight = 16,
            linkActivationTo = "refill,crumbleBlock,booster",
            types = "refill,crumbleBlock,booster",
            strictWhitelist = false,
        }
    },
}

function FemtoHelperStepMover.sprite(room, entity)
    local sprPath = "loenn/FemtoHelper/stepMover"

    local sprites = {}

    local targetBoxWidth = tonumber(entity.targetBoxWidth) or entity.width or 16
    local targetBoxHeight = tonumber(entity.targetBoxHeight) or entity.height or 16

    table.insert(sprites, drawableRectangle.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, {0.1, 0.9, 1.0, 0.1}))
    if entity.nodes then
        for i, node in ipairs(entity.nodes) do
            if i ~= 3 then
                table.insert(sprites, drawableRectangle.fromRectangle("bordered", node.x, node.y, targetBoxWidth, targetBoxHeight, {1, 0.9, 0.1, 0.01}, {1, 0.9, 0.1, 0.05}))
            end
        end
    end

    local halfWidth = (entity.width or 16) / 2
    local halfHeight = (entity.height or 16) / 2

    local halfTargetWidth = targetBoxWidth / 2
    local halfTargetHeight = targetBoxHeight / 2

    if entity.nodes[1] then 
        local steps = math.floor(
            math.sqrt(
                (((entity.x + halfWidth) - (entity.nodes[1].x + halfTargetWidth)) * ((entity.x + halfWidth) - (entity.nodes[1].x + halfTargetWidth))) + 
                (((entity.y + halfHeight) - (entity.nodes[1].y + halfTargetHeight)) * ((entity.y + halfHeight) - (entity.nodes[1].y + halfTargetHeight)))
            ) / 4
        )
        -- limit steps so it doesn't draw like. one million sprites
        local steps = math.min(steps, 100)
        local delta = {
            x = (entity.nodes[1].x + halfTargetWidth) - (entity.x + halfWidth), 
            y = (entity.nodes[1].y + halfTargetHeight) - (entity.y + halfHeight) 
        }
        for i=0, steps-1 do
            if i % 2 ~= 1 then
                table.insert(sprites, drawableLine.fromPoints({
                    entity.x + halfWidth + (delta.x * (i / steps)),
                    entity.y + halfHeight + (delta.y * (i / steps)),
                    entity.x + halfWidth + (delta.x * ((i+1) / steps)),
                    entity.y + halfHeight + (delta.y * ((i+1) / steps)),
                }, {1, 1, 1, 0.1}, 1))
            end
        end

        local target = drawableSprite.fromTexture(sprPath.."Target", entity)
        target:setPosition(entity.nodes[1].x + halfTargetWidth, entity.nodes[1].y + halfTargetHeight)
        target:setColor({1, 0.2, 0.2, 0.2})
        table.insert(sprites, target)
        
        if entity.nodes[2] then 
            table.insert(sprites, drawableLine.fromPoints({entity.nodes[1].x + halfTargetWidth, entity.nodes[1].y + halfTargetHeight, entity.nodes[2].x + halfTargetWidth, entity.nodes[2].y + halfTargetHeight}, {1, 0.9, 0.1, 0.25}, 1))

            delta = {
                x = entity.nodes[2].x - entity.nodes[1].x, 
                y = entity.nodes[2].y - entity.nodes[1].y
            }

            local angle = math.atan2(delta.y, delta.x)

            local arrow_a = {
                x = halfTargetWidth + math.cos(angle + (7 * math.pi / 8)) * 8,
                y = halfTargetHeight + math.sin(angle + (7 * math.pi / 8)) * 8
            }

            table.insert(sprites, drawableLine.fromPoints({entity.nodes[2].x + arrow_a.x, entity.nodes[2].y + arrow_a.y, entity.nodes[2].x + halfTargetWidth, entity.nodes[2].y + halfTargetHeight}, {1, 0.9, 0.1, 0.25}, 1))

            local arrow_a = {
                x = halfTargetWidth + math.cos(angle - (7 * math.pi / 8)) * 8,
                y = halfTargetHeight + math.sin(angle - (7 * math.pi / 8)) * 8
            }

            table.insert(sprites, drawableLine.fromPoints({entity.nodes[2].x + arrow_a.x, entity.nodes[2].y + arrow_a.y, entity.nodes[2].x + halfTargetWidth, entity.nodes[2].y + halfTargetHeight}, {1, 0.9, 0.1, 0.25}, 1))

            if entity.nodes[3] then
                local steps = math.floor(
                    math.sqrt(
                        (((entity.nodes[2].x + halfTargetWidth) - (entity.nodes[3].x)) * ((entity.nodes[2].x + halfTargetWidth) - (entity.nodes[3].x))) + 
                        (((entity.nodes[2].y + halfTargetHeight) - (entity.nodes[3].y)) * ((entity.nodes[2].y + halfTargetHeight) - (entity.nodes[3].y)))
                    ) / 4
                )
                -- limit steps so it doesn't draw like. one million sprites
                local steps = math.min(steps, 100)
                local delta = {
                    x = (entity.nodes[3].x) - (entity.nodes[2].x + halfTargetWidth), 
                    y = (entity.nodes[3].y) - (entity.nodes[2].y + halfTargetHeight) 
                }
                for i=0, steps-1 do
                    if i % 2 ~= 1 then
                        table.insert(sprites, drawableLine.fromPoints({
                            entity.nodes[2].x + halfTargetWidth + (delta.x * (i / steps)),
                            entity.nodes[2].y + halfTargetHeight + (delta.y * (i / steps)),
                            entity.nodes[2].x + halfTargetWidth + (delta.x * ((i+1) / steps)),
                            entity.nodes[2].y + halfTargetHeight + (delta.y * ((i+1) / steps)),
                        }, {1, 1, 1, 0.1}, 1))
                    end
                end
            end

            local iconPath = entity.icon or "objects/FemtoHelper/StepMover/refilltarget"

            for i=0, (entity.iconCountX or 1) - 1 do
                for j=0, (entity.iconCountY or 1) - 1 do
                    local icon = drawableSprite.fromTexture(iconPath)
                    if icon then
                        local rect = icon:getRectangle();
                        local iconPos = entity.nodes[3] or ({x = entity.nodes[2].x + halfTargetWidth, y = entity.nodes[2].y + halfTargetHeight})
                        icon:setPosition(iconPos.x + ((entity.iconSpacingX or rect.width) * i), iconPos.y + ((entity.iconSpacingY or rect.height) * j))
                        table.insert(sprites, icon)
                    end
                end
            end
        end
    end

    local top_left = drawableSprite.fromTexture(sprPath.."Trigger", entity)
    top_left:setJustification(0, 0)
    top_left:setColor({0.1, 0.9, 1, 1})

    local bottom_right = drawableSprite.fromTexture(sprPath.."Trigger", entity)
    bottom_right.rotation = math.pi
    bottom_right:setJustification(0, 0)
    bottom_right:addPosition(entity.width, entity.height)
    bottom_right:setColor({0.1, 0.9, 1, 1})

    table.insert(sprites, top_left)
    table.insert(sprites, bottom_right)

    return sprites
end

function FemtoHelperStepMover.selection(room, entity) 
    local nodeRecs = {}

    local targetBoxWidth = tonumber(entity.targetBoxWidth) or entity.width or 16
    local targetBoxHeight = tonumber(entity.targetBoxHeight) or entity.height or 16

    if entity.nodes then
        for i, node in ipairs(entity.nodes or {}) do
            if i == 3 then
                table.insert(nodeRecs, utils.rectangle(node.x - 6, node.y - 6, 12, 12))
            else
                table.insert(nodeRecs, utils.rectangle(node.x, node.y, targetBoxWidth, targetBoxHeight))
            end
        end
    end

    return utils.rectangle(entity.x, entity.y, entity.width or 16, entity.height or 16), nodeRecs
end

return FemtoHelperStepMover