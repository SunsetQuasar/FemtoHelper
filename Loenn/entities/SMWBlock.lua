local drawableSprite = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")

local FemtoHelperSMWBlock = {}

FemtoHelperSMWBlock.name = "FemtoHelper/SMWBlock"
FemtoHelperSMWBlock.nodeLimits = {1, 1}
FemtoHelperSMWBlock.depth = -6000
FemtoHelperSMWBlock.minimumSize = {16, 16}
FemtoHelperSMWBlock.justification = {0, 0}
FemtoHelperSMWBlock.fieldInformation = {
    ejectDirection = {
        options = {
            {"Up", 0},
            {"Right", 1},
            {"Left", 2},
            {"Down", 3},
            {"Opposite From Impact", 4}
        },
        editable = false
    },
    hitFlagBehavior = {
        options = {
            {"Always Activate", 0},
            {"Always Deactivate", 1},
            {"Toggle", 2}
        },
        editable = false
    }
}
FemtoHelperSMWBlock.placements = {
    {
    name = "smwblockdefault",
    data = {
        width = 16,
        height = 16,
        animationRate = 8,
        ejectDistance = 24,
        ejectOffsetX = 0,
        ejectOffsetY = 0,
        ejectDestinationOffsetX = 0,
        ejectDestinationOffsetY = 0,
        indicate = true,
	    ejectDuration = 0.5,
        path = "objects/FemtoHelper/SMWBlock/solid/",
        solidBeforeHit = true,
        ejectDirection = 4,
        rewardContainerWidth = 16,
        rewardContainerHeight = 16,
        canHitTop = true,
        canHitBottom = true,
        canHitLeft = true,
        canHitRight = true,
        hitFlag = "",
        switchAppearanceFlag = "",
        hitFlagBehavior = 2,
        switchMode = false,
        giveCoyoteFramesOnHit = false,
        audioPath = "event:/FemtoHelper/",
        neededFlag = "",
        ejectFromPoint = true,
        ejectToPoint = false,
        spriteOffsetX = 0,
        spriteOffsetY = 0,
        depth = -15000,
        specialEntityHandling = true,
        affectVisible = true,
        affectActive = true,
        affectCollidable = false,
        dashableKaizo = false,
    },
},
{
    name = "smwblockhidden",
    data = {
        width = 16,
        height = 16,
        animationRate = 8,
        ejectDistance = 24,
        ejectOffsetX = 0,
        ejectOffsetY = 0,
        ejectDestinationOffsetX = 0,
        ejectDestinationOffsetY = 0,
        indicate = false,
	    ejectDuration = 0.5,
        path = "objects/FemtoHelper/SMWBlock/hidden/",
        solidBeforeHit = false,
        ejectDirection = 4,
        rewardContainerWidth = 16,
        rewardContainerHeight = 16,
        canHitTop = true,
        canHitBottom = true,
        canHitLeft = true,
        canHitRight = true,
        hitFlag = "",
        switchAppearanceFlag = "",
        hitFlagBehavior = 2,
        switchMode = false,
        giveCoyoteFramesOnHit = false,
        audioPath = "event:/FemtoHelper/",
        neededFlag = "",
        ejectFromPoint = true,
        ejectToPoint = false,
        spriteOffsetX = 0,
        spriteOffsetY = 0,
        depth = -15000,
        specialEntityHandling = true,
        affectVisible = true,
        affectActive = true,
        affectCollidable = false,
        dashableKaizo = false,
    }
},
{
    name = "smwblockswitch",
    data = {
        width = 16,
        height = 16,
        animationRate = 8,
        ejectDistance = 24,
        ejectOffsetX = 0,
        ejectOffsetY = 0,
        ejectDestinationOffsetX = 0,
        ejectDestinationOffsetY = 0,
        indicate = true,
	    ejectDuration = 0.5,
        path = "objects/FemtoHelper/SMWBlock/switch/",
        solidBeforeHit = true,
        ejectDirection = 4,
        rewardContainerWidth = 16,
        rewardContainerHeight = 16,
        canHitTop = true,
        canHitBottom = true,
        canHitLeft = true,
        canHitRight = true,
        hitFlag = "smwblock_flag",
        switchAppearanceFlag = "",
        hitFlagBehavior = 2,
        switchMode = true,
        giveCoyoteFramesOnHit = false,
        audioPath = "event:/FemtoHelper/",
        neededFlag = "",
        ejectFromPoint = true,
        ejectToPoint = false,
        spriteOffsetX = 0,
        spriteOffsetY = 0,
        depth = -15000,
        specialEntityHandling = true,
        affectVisible = true,
        affectActive = true,
        affectCollidable = false,
        dashableKaizo = false,
    }
}
}

    

function FemtoHelperSMWBlock.sprite(room, entity)

    local spr = {}

    local rWidth = entity.rewardContainerWidth or 16
    local rHeight = entity.rewardContainerHeight or 16

    if entity.nodes then
        for _, node in ipairs(entity.nodes) do

            sprite = drawableSprite.fromTexture(entity.path.."indicator", entity)

            sprite:useRelativeQuad(0, 0, sprite.meta.height, sprite.meta.height)

            if entity.indicate == false then
                sprite:setColor({1, 1, 1, 0.5})
            end
            sprite:setJustification(0, 0)

            local color1 = entity.switchMode and {0.3, 0.3, 0.3, 0.1} or {1, 0, 0.5, 0.4}
            local color2 = entity.switchMode and {0.4, 0.4, 0.4, 0.2} or {1, 0.5, 0.7, 0.6}
            local color3 = entity.switchMode and {0.3, 0.3, 0.3, 0.07} or {1, 0, 0.5, 0.3}

            spr = {
                sprite,
                drawableLine.fromPoints({entity.x + (entity.width/2), entity.y + (entity.height/2), node.x + (rWidth/2), node.y + (rHeight/2)}, color3, 1),
                drawableRectangle.fromRectangle("bordered", node.x, node.y, rWidth, rHeight, color1, color2)
            }
        end
    end
    return spr
end

function FemtoHelperSMWBlock.nodeSprite()
end

function FemtoHelperSMWBlock.nodeRectangle(room, entity, node, nodeIndex)

    local rWidth = entity.rewardContainerWidth or 16
    local rHeight = entity.rewardContainerHeight or 16

    return utils.rectangle(node.x, node.y, rWidth, rHeight)
end


return FemtoHelperSMWBlock