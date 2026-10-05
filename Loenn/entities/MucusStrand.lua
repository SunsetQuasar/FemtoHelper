local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local FemtoHelperMucusStrand = {}

FemtoHelperMucusStrand.name = "FemtoHelper/MucusStrand"
FemtoHelperMucusStrand.depth = -100
FemtoHelperMucusStrand.nodeLimits = {1, 1}
FemtoHelperMucusStrand.nodeVisibility = "never"
FemtoHelperMucusStrand.fieldInformation = {

}
FemtoHelperMucusStrand.placements = {
    {
        name = "mucus",
        data = {

        }
    },
}

function FemtoHelperMucusStrand.sprite(room, entity)
    local sprPath = "objects/FemtoHelper/mucusStrand/ball"
    local sprites = {}

    local node = entity.nodes[1] or {x = entity.x or 0, y = entity.y or 0}

    local length = math.sqrt(
            ((entity.x - entity.nodes[1].x) * (entity.x - entity.nodes[1].x)) + 
            ((entity.y - entity.nodes[1].y) * (entity.y - entity.nodes[1].y))
    )

    -- clamp length so we don't draw too many sprites
    length = math.min(length, 512)

    local steps = length / 3.6

    local delta_x = entity.nodes[1].x - entity.x
    local delta_y = entity.nodes[1].y - entity.y

    utils.setSimpleCoordinateSeed(entity.x, entity.y)
    for i = 0, 1, (1 / steps) do
        local offset = math.floor(math.random() * 4)
        for x = -1, 1 do
            for y = -1, 1 do
                if x ~= 0 and y ~= 0 then
                    local ball = drawableSprite.fromTexture(sprPath, entity)
                    ball:addPosition((delta_x * i) + x, (delta_y * i) + y)
                    ball:useRelativeQuad(offset * 16, 0, 16, 16)
                    ball:addPosition(-8, -8)
                    ball:setColor({0, 0, 0, 0.8})

                    table.insert(sprites, ball)
                end
            end
        end
    end

    utils.setSimpleCoordinateSeed(entity.x, entity.y)
    for i = 0, 1, (1 / steps) do
        local ball = drawableSprite.fromTexture(sprPath, entity)
        ball:addPosition(delta_x * i, delta_y * i)
        ball:useRelativeQuad(math.floor(math.random() * 4) * 16, 0, 16, 16)
        ball:addPosition(-8, -8)
        ball:setColor({1, 1, 1, 0.5})
        table.insert(sprites, ball)
    end

    return sprites
end

function FemtoHelperMucusStrand.selection(room, entity) 
    local nodeRecs = {}
    if entity.nodes then
        for k, node in ipairs(entity.nodes or {}) do
            table.insert(nodeRecs,  utils.rectangle(node.x - 6, node.y - 6, 12, 12))
        end
    end

    return utils.rectangle(entity.x - 6, entity.y - 6, 12, 12), nodeRecs
end

return FemtoHelperMucusStrand