local effect = {}

effect.name = "FemtoHelper/NewDistortedParallax"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    color = {
        fieldType = "color"
    },
    blendState = {
        options = {
            {"Alpha Blend", "alphablend"},
            {"Additive", "additive"},
            {"Subract", "subtract"},
            {"Multiply", "multiply"}
        },
        editable = false
    },
    filterMode = {
        options = {
            {"Point", "point"},
            {"Linear", "linear"},
            {"Anisotropic", "anisotropic"}
        },
        editable = false
    }
}


effect.defaultData = {
    offsetX = 0,
    offsetY = 0,
    texture = "bgs/disperse_clouds",
    loopX = true,
    loopY = true,
    scrollX = 1.0,
    scrollY = 1.0,
    speedX = 0,
    speedY = 0,
    color = "FFFFFF",
    blendState = "alphablend",
    filterMode = "point",
    alpha = 1,

    flipX = false,
    flipY = false,
    fadeIn = true,

    xXAmplitude = 0,
    xXPeriod = 40,
    xXOffset = 0,
    xXWaveSpeed = 0,

    xYAmplitude = 10,
    xYPeriod = 40,
    xYOffset = 0,
    xYWaveSpeed = 180,

    yXAmplitude = 0,
    yXPeriod = 40,
    yXOffset = 0,
    yXWaveSpeed = 0,

    yYAmplitude = 0,
    yYPeriod = 40,
    yYOffset = 0,
    yYWaveSpeed = 0,

    scale = 1,
    scaleAmplitude = 0,
    scaleSpeed = 0,
    scaleOffset = 0,

    rotationSpeed = 0,
    rotationOffset = 0,

    shaderPath = "FemtoHelper/DistortedParallax",
    waveRotationFix = true
}

return effect