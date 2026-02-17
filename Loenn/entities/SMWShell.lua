local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local FemtoHelperSMWShell = {}

FemtoHelperSMWShell.name = "FemtoHelper/SMWShell"
FemtoHelperSMWShell.depth = -10
FemtoHelperSMWShell.fieldInformation = {
    bounceCountDisplay = {
        options = {
            "None", "HUD", "SpriteText"
        },
        editable = false
    },
    touchKickBehavior = {
        options = {
            "Normal", "Ignore", "Kill"
        },
        editable = false
    },
    outlineTextureType = {
        options = {
            "None", "Black", "White", "Tiny"
        },
        editable = false
    }
}
FemtoHelperSMWShell.placements = {
    {
        name = "smw_shell",
        data = {
            doSplashEffect = true,
            texturesPrefix = "objects/FemtoHelper/SMWShell/",
            ignoreOtherShells = false,
            doFreezeFrames = true,
            disco = false,
            discoSprites = "yellow,blue,red,green,teal,gray,gold,gray",
            audioPath = "event:/FemtoHelper/",
            mainSprite = "green",
            canBeBouncedOn = true,
            touchKickBehavior = "Normal",
            carriable = true,
            shellSpeed = 182,
            discoSpeed = 120,
            discoAcceleration = 700,
            gravity = 750,
            airFriction = 0,
            groundFriction = 800,
            idleActivateTouchSwitches = true,
            discoSleep = false,
            maxFallSpeed = 160,
            bounceCount = 1,
            bounceCountDisplay = "SpriteText",
            discoSpriteRate = 50,
            bubble = false,
            downwardsLeniencySpeed = 300,
            holdYOffset = -6,
            holdYCrouchOffset = -6,
            useFixedThrowSpeeds = false,
            fixedNeutralThrowSpeed = 182,
            fixedForwardThrowSpeed = 182,
            noInteractionDuration = 0.28,
            outlineTextureType = "Tiny",
            upwardsThrowSpeed = -400,
            dontRefill = false,
            oneUse = false,
            bounceSpeedMultiplier = 1,
            bounceLengthMultiplier = 1,
        }
    },
    {
        name = "smw_shell_disco",
        data = {
            doSplashEffect = true,
            texturesPrefix = "objects/FemtoHelper/SMWShell/",
            ignoreOtherShells = false,
            doFreezeFrames = true,
            disco = true,
            discoSprites = "yellow,blue,red,green,teal,gray,gold,gray",
            audioPath = "event:/FemtoHelper/",
            mainSprite = "green",
            canBeBouncedOn = true,
            touchKickBehavior = "Normal",
            carriable = true,
            shellSpeed = 182,
            discoSpeed = 120,
            discoAcceleration = 700,
            gravity = 750,
            airFriction = 0,
            groundFriction = 800,
            idleActivateTouchSwitches = true,
            discoSleep = false,
            maxFallSpeed = 160,
            bounceCount = 1,
            bounceCountDisplay = "SpriteText",
            discoSpriteRate = 50,
            bubble = false,
            downwardsLeniencySpeed = 300,
            holdYOffset = -6,
            holdYCrouchOffset = -6,
            useFixedThrowSpeeds = false,
            fixedNeutralThrowSpeed = 182,
            fixedForwardThrowSpeed = 182,
            noInteractionDuration =  0.28,
            outlineTextureType = "Tiny",
            upwardsThrowSpeed = -400,
            dontRefill = false,
            oneUse = false,
            bounceSpeedMultiplier = 1,
            bounceLengthMultiplier = 1,
        }
    }
}

function FemtoHelperSMWShell.sprite(room, entity)

    local texture = ((entity.texturesPrefix..entity.mainSprite) or "objects/FemtoHelper/SMWShell/green").."_idle00"

    local shellSprite = drawableSprite.fromTexture(texture, entity)

    shellSprite:setJustification(0.5, 0.5)
    shellSprite:addPosition(0, 1)

    if entity.bubble then
        local x, y = entity.x or 0, entity.y or 0
        local points = drawing.getSimpleCurve({x - 11, y + 6}, {x + 11, y + 6}, {x - 0, y + 1})
        local lineSprites = drawableLine.fromPoints(points):getDrawableSprite()
        local jellySprite = drawableSprite.fromTexture(texture, entity)

        table.insert(lineSprites, 1, shellSprite)

        return lineSprites

    end

    return shellSprite
end

function FemtoHelperSMWShell.selection(room, entity)
    local hh = 16
    local hh2 = 16

    return utils.rectangle(entity.x - hh / 2, entity.y - hh2 / 2, hh, hh2)
end


return FemtoHelperSMWShell