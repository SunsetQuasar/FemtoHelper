local femto_stars = {}

femto_stars.name = "FemtoHelper/DigitalCascade"
femto_stars.fieldInformation = {
    symbolAmount = {
        fieldType = "integer"
    },
    afterimagesCap = {
        fieldType = "integer"
    },
    fadeScaleMode = {
        options = {
            {"No Scaling", 0},
            {"X and Y Scaling", 1},
            {"X Scaling Only", 2},
            {"Y Scaling Only", 3}
        },
        editable = false
    }
}

femto_stars.defaultData = {
    symbolAmount = 50,
    colors = "00cc00",
    alpha = 1.0,
    minSpeed = 20,
    maxSpeed = 40,
    angle = 0,
    extraLoopBorderX = 20,
    extraLoopBorderY = 20,
    spritePath = "bgs/FemtoHelper/digitalCascade/star",
    minLifetime = 1,
    maxLifetime = 2,
    fadeTime = 0.1,
    randomSymbol = false,
    afterimageFadeTime = 1.0,
    timeBetweenAfterimages = 0.25,
    scroll = 1.0,
    fadeScaleMode = 1,
    noFlipping = false,
    afterimageAlpha = 0.6
}

return femto_stars