local FemtoHelperCinematicText = {}

FemtoHelperCinematicText.name = "FemtoHelper/CinematicText"
FemtoHelperCinematicText.depth = -3000000
FemtoHelperCinematicText.texture = "loenn/FemtoHelper/cinematictext"
FemtoHelperCinematicText.justification = {0.5, 0.5}

FemtoHelperCinematicText.fieldInformation = {
    mainColor = {
        fieldType = "color"
    },
    outlineColor = {
        fieldType = "color"
    }
}

FemtoHelperCinematicText.placements = {
    name = "default",
    data = {
        activationTag = "tag1",
        charList = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'\\\".,",
        charOriginX = 0,
        charOriginY = -2,
        delay = 0,
        depth = -10000,
        dialogID = "FemtoHelper_PlutoniumText_Example",
        disappearTime = 2,
        effects = true,
        fontHeight = 12,
        fontPath = "objects/FemtoHelper/PlutoniumText/example",
        fontWidth = 8,
        hud = true,
        ignoreAudioRegex = false,
	    instantReload = false,
	    instantLoad = false,
        mainColor = "FFFFFFFF",
        nextTextTag = "tag2",
        obfuscated = false,
        onlyOnce = false,
        outlineColor = "000000FF",
        parallax = 1,
        phaseIncrement = 30,
        scale = 1,
        shadow = false,
        shake = false,
        shakeAmount = 1,
        spacing = 8,
        speed = 16,
        textSound = "event:/FemtoHelper/example_text_sound",
        twitch = false,
        twitchChance = 0,
        wave = true,
        wavePhaseOffset = 0,
        waveSpeed = 2,
        waveX = 0,
        waveY = 1,
    }
}

return FemtoHelperCinematicText