local femto_stars = {}

femto_stars.name = "FemtoHelper/FemtoStars"
femto_stars.fieldInformation = {
    backgroundColor = {
        fieldType = "color",
    },
    starCount = {
        fieldType = "integer"
    },
    trailCount = {
        fieldType = "integer"
    }
}

femto_stars.defaultData = {
    trailCount = 8,
    colors = "008080FF",
    minXSpeed = 0.0,
    maxXSpeed = 0.0,
    minYSpeed = 12.0,
    maxYSpeed = 24.0,
    extraLoopBorderX = 0.0,
    extraLoopBorderY = 0.0,
    starCount = 100,
    backgroundColor = "000000",
    backgroundAlpha = 1.0,
    sprite = "bgs/02/stars",
    scrollX = 0.0,
    scrollY = 0.0,
    trailSeparation = 1.0,
    animationRate = 2.0,
    alphas = "0.7"
}

return femto_stars