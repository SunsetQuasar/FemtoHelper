local FemtoHelperPopRefill = {}

FemtoHelperPopRefill.name = "FemtoHelper/PopRefill"
FemtoHelperPopRefill.depth = -100

FemtoHelperPopRefill.placements = {
    {
        name = "PopRefill",
        data = {
            oneUse = false,
            twoDash = false,
            spawnTime = 2.5,
        }
    },
    {
        name = "PopRefillTwo",
        data = {
            oneUse = false,
            twoDash = true,
            spawnTime = 2.5,
        }
    }
}

function FemtoHelperPopRefill.texture(room, entity)
    return "objects/refill"..(entity.twoDash and "Two" or "" ).."/outline"
end

return FemtoHelperPopRefill