local effect = {}

effect.name = "FemtoHelper/DistortedParallax"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    color = {
        fieldType = "color"
    },
    blendState = {
        options = {
            {"Alpha Blend", "alphablend"},
            {"Additive", "additive"}
        },
        editable = false
    },
    sliceMode = {
        options = {
            {"Horizontal", 0},
            {"Vertical", 1}
        },
        editable = false
    }
}

effect.defaultData = {
    texture = "bgs/04/bg0",
    posX = 0,
    posY = 0,
    speedX = 0,
    speedY = 0,
    scrollX = 1,
    scrollY = 1,
    loopX = true,
    loopY = true,
    color = "ffffff",
    alpha = 1,
    blendState = "alphablend",
    amplitudeX = 10,
    amplitudeY = 0,
    periodX = 40,
    periodY = 10,
    waveSpeedX = 3,
    waveSpeedY = 3,
    offsetX = 0,
    offsetY = 0,
    flipX = false,
    flipY = false,
    sliceMode = 0,
}

return nil --"FemtoHelper/DistortedParallax" is obsolete
--return effect