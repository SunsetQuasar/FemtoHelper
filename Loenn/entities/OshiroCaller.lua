local FemtoHelperOshiroCaller = {}

FemtoHelperOshiroCaller.name = "FemtoHelper/OshiroCaller"
FemtoHelperOshiroCaller.depth = -1000000
FemtoHelperOshiroCaller.justification = {0.5, 0.5}
FemtoHelperOshiroCaller.placements = {
    {
        name = "call",
        data = {
            repell = false,
            justMakeOshiroLeave = false
        }
    },
    {
        name = "repel",
        data = {
            repell = true,
            justMakeOshiroLeave = false
        }
    }
}

function FemtoHelperOshiroCaller.texture(room, entity)
    return entity.repell and "objects/FemtoHelper/oshiroCaller/repeller00" or "objects/FemtoHelper/oshiroCaller/caller00"
end

return FemtoHelperOshiroCaller