local FemtoHelperSparkRefill = {}

FemtoHelperSparkRefill.name = "FemtoHelper/SparkRefill"
FemtoHelperSparkRefill.depth = -100
FemtoHelperSparkRefill.placements = {
    {
        name = "spark",
        data = {
            oneUse = false,
            respawnTime = 2.5,
        }
    },
}

function FemtoHelperSparkRefill.texture(room, entity)
    return "objects/FemtoHelper/sparkRefill/idle00"
end

return FemtoHelperSparkRefill