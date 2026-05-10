local drawableSprite = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")

local FemtoHelperGDDashOrb = {}

FemtoHelperGDDashOrb.name = "FemtoHelper/GDDashOrb"
FemtoHelperGDDashOrb.depth = 0
FemtoHelperGDDashOrb.justification = {0.5, 0.5}
FemtoHelperGDDashOrb.depth = 2000
FemtoHelperGDDashOrb.nodeLimits = {1, 1}
FemtoHelperGDDashOrb.nodeVisibility = "never"
FemtoHelperGDDashOrb.placements = {
    {
        name = "default",
        data = {
            _gravityHelper = true,
            --angle = 0,
            speed = 240,
            additive = false,
            pink = false
        }
    },
    {
        name = "pink",
        data = {
            _gravityHelper = true,
            --angle = 0,
            speed = 240,
            additive = false,
            pink = true
        }
    }
}
FemtoHelperGDDashOrb.ignoredFields = {
    "_name", "_id","_gravityHelper"
}

function FemtoHelperGDDashOrb.sprite(room, entity)

    local ring = drawableSprite.fromTexture("objects/FemtoHelper/gddashorb/dashring", entity);

    local orb = drawableSprite.fromTexture("objects/FemtoHelper/gddashorb/dashorb", entity);

    local nodes = entity.nodes

    orb.rotation = nodes and math.atan2(nodes[1].y - entity.y, nodes[1].x - entity.x) or entity.angle and ((entity.angle / 180) * 3.14159265359) or 0;
    if entity.pink then
        orb:setColor({255 / 255, 48 / 255, 240 / 255, 1});
    else
        orb:setColor({32 / 255, 238 / 255, 48 / 255, 1});
    end

    local spr = {
        ring,
        orb
    }

    if entity.nodes then
        local rectangle = drawableRectangle.fromRectangle("fill", nodes[1].x - 2, nodes[1].y - 2, 4, 4, {1, 1, 1, 1})
        local line = drawableLine.fromPoints({entity.x, entity.y, nodes[1].x, nodes[1].y}, {1, 1, 1, 0.5}, 1)
        
        table.insert(spr, rectangle)
        table.insert(spr, line)
    end

    return spr
end

function FemtoHelperGDDashOrb.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local size = 24
    local halfsize = size * 0.5

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y

    return utils.rectangle(x - halfsize, y - halfsize, size, size), {utils.rectangle(nodeX - 2, nodeY - 2, 4, 4)}
end

return FemtoHelperGDDashOrb