local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local FemtoHelperMoverRefill = {}

FemtoHelperMoverRefill.name = "FemtoHelper/MoverRefill"
FemtoHelperMoverRefill.depth = -100
FemtoHelperMoverRefill.nodeLimits = {0, 1}
FemtoHelperMoverRefill.nodeVisibility = "never"
FemtoHelperMoverRefill.placements = {
    {
        name = "air",
        data = {
            oneUse = false,
            respawnTime = 2.5,
            visualOffsetX = 0,
            visualOffsetY = 0,
        }
    },
}

function FemtoHelperMoverRefill.sprite(room, entity)
    local sprPath = "objects/FemtoHelper/moverRefill/"

    local sprites = {}

    local vx = entity.visualOffsetX or 0;
    local vy = entity.visualOffsetY or 0;
    
    if #entity.nodes > 0 then
        local node = entity.nodes[1] or {x = entity.x, y = entity.y}

        for i=-1, 1 do
            for j=-1, 1 do
                if i ~= 0 and j ~= 0 then
                    local main_node_outline = drawableSprite.fromTexture("objects/switchGate/icon00", entity)
                    main_node_outline:setColor({0, 0, 0, 1})
                    main_node_outline:setPosition(node.x + i, node.y + j)
                    table.insert(sprites, main_node_outline)
                end
            end
        end

        local main_node = drawableSprite.fromTexture("objects/switchGate/icon00", entity)
        main_node:setColor({0.294, 0.86, 0.898, 1})
        main_node:setPosition(node.x, node.y)
        table.insert(sprites, main_node)

        table.insert(sprites, drawableLine.fromPoints({entity.x, entity.y, node.x, node.y}, {1, 1, 1, 0.25}, 1))
    end

    if vx ~= 0 or vy ~= 0 then
        local main = drawableSprite.fromTexture(sprPath.."outline00", entity)
        table.insert(sprites, main)

        local offset_spr = drawableSprite.fromTexture(sprPath.."idle00", entity)
        offset_spr:setColor({1, 1, 1, 0.5})
        offset_spr:addPosition(vx, vy)
        table.insert(sprites, offset_spr)
    else
        local main = drawableSprite.fromTexture(sprPath.."idle00", entity)
        table.insert(sprites, main)
    end

    return sprites
end

function FemtoHelperMoverRefill.selection(room, entity) 
    local nodeRecs = {}

    for k, node in pairs(entity.nodes) do
        table.insert(nodeRecs,  utils.rectangle(node.x - 8, node.y - 8, 16, 16))
    end

    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16), nodeRecs
end

return FemtoHelperMoverRefill