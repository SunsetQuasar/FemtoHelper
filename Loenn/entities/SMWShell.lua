local drawableSprite = require("structs.drawable_sprite")

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
            shellSpeed = 200,
            discoSpeed = 120,
            discoAcceleration = 700,
            gravity = 800,
            airFriction = 250,
            groundFriction = 800,
            idleActivateTouchSwitches = true,
            discoSleep = false,
            maxFallSpeed = 200,
            bounceCount = 1,
            bounceCountDisplay = "SpriteText",
            discoSpriteRate = 50,
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
            shellSpeed = 200,
            discoSpeed = 120,
            discoAcceleration = 700,
            gravity = 800,
            airFriction = 250,
            groundFriction = 800,
            idleActivateTouchSwitches = true,
            discoSleep = false,
            maxFallSpeed = 200,
            bounceCount = 1,
            bounceCountDisplay = "SpriteText",
            discoSpriteRate = 50,
        }
    }
}

function FemtoHelperSMWShell.sprite(room, entity)

    local texture = ((entity.texturesPrefix..entity.mainSprite) or "objects/FemtoHelper/SMWShell/green").."_idle00"

    local shellSprite = drawableSprite.fromTexture(texture, entity)

    shellSprite:setJustification(0.5, 0.5)
    shellSprite:addPosition(0, 1)

    return shellSprite
end

function FemtoHelperSMWShell.selection(room, entity)
    local hh = 16
    local hh2 = 16

    return utils.rectangle(entity.x - hh / 2, entity.y - hh2 / 2, hh, hh2)
end


return FemtoHelperSMWShell