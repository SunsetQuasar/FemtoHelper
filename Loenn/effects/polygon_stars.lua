local polygon_stars = {}

polygon_stars.name = "FemtoHelper/PolygonStars"
polygon_stars.fieldInformation = {
    amount = {
        fieldType = "integer"
    },
    sideCount = {
        fieldType = "integer"
    }
}

polygon_stars.defaultData = {
    sideCount = 10,
    pointinessMultiplier = 2.0,
    minRotationSpeed = 1.0,
    maxRotationSpeed = 2.0,
    minSize = 2.0,
    maxSize = 8.0,
    loopBorder = 64.0,
    colors = "008080",
    angle = 270.0,
    alpha = 1.0,
    minSpeed = 24.0,
    maxSpeed = 48.0,
    amount = 50,
    scroll = 0.5
}

return polygon_stars