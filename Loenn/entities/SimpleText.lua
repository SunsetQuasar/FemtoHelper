local FemtoHelperSimpleText = {}

FemtoHelperSimpleText.name = "FemtoHelper/SimpleText"
FemtoHelperSimpleText.depth = -3000000
FemtoHelperSimpleText.texture = "loenn/FemtoHelper/simpletext"
FemtoHelperSimpleText.justification = {0.5, 0.5}

FemtoHelperSimpleText.fieldInformation = {
    mainColor = {
        fieldType = "color"
    },
    outlineColor = {
        fieldType = "color"
    }
}

FemtoHelperSimpleText.placements = {
    name = "default",
    data = {
        charList = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'\\\".,ç",
        depth = -10000,
        dialogID = "FemtoHelper_PlutoniumText_Example",
        fontHeight = 12,
        fontPath = "objects/FemtoHelper/PlutoniumText/example",
        fontWidth = 8,
        hud = true,
        mainColor = "FFFFFFFF",
        outlineColor = "000000FF",
        parallax = 1,
        scale = 1,
        shadow = false,
        spacing = 8,
    }
}

return FemtoHelperSimpleText