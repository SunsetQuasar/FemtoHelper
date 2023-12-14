local wind_petals = {}

wind_petals.name = "FemtoHelper/WindPetals"
wind_petals.fieldInformation = {
    blurCount = {
        fieldType = "integer"
    },
    particleCount = {
        fieldType = "integer"
    }
}

wind_petals.defaultData = {
    colors = "66cc22",
    fallingSpeedMin = 6.0,
    fallingSpeedMax = 16.0,
    blurCount = 15,
    blurDensity = 3.0,
    texture = "particles/petal",
    particleCount = 40,
    parallax = 1.0,
    spinAmountMultiplier = 1.0,
    spinSpeedMultiplier = 1.0,
    alpha = 1.0,
    scale = 1.0,
    minXDriftSpeed = 0.0,
    maxXDriftSpeed = 0.0
}

return wind_petals