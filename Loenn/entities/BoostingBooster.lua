local FemtoHelperBoostingBooster = {}

FemtoHelperBoostingBooster.name = "FemtoHelper/BoostingBooster"
FemtoHelperBoostingBooster.depth = -8500
FemtoHelperBoostingBooster.placements = {
    {
        name = "green",
        data = {
            red = false,
        }
    },
    {
        name = "red",
        data = {
            red = true,
        }
    }
}

function FemtoHelperBoostingBooster.texture(room, entity)
    local red = entity.red

    if red then
        return "loenn/FemtoHelper/boostingBoosterRed"

    else
        return "loenn/FemtoHelper/boostingBooster"
    end
end

return FemtoHelperBoostingBooster