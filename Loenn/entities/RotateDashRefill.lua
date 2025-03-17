local FemtoHelperRotateDashRefill = {}

FemtoHelperRotateDashRefill.name = "FemtoHelper/RotateDashRefill"
FemtoHelperRotateDashRefill.depth = -100
FemtoHelperRotateDashRefill.placements = {
    {
        name = "rotate_refill",
        data = {
            oneUse = false,
            scalar = 1.5,
            texture = "objects/FemtoHelper/rotateRefillCCW/",
            effectColors = "7958ad,cbace6,634691",
            particleColors = "ae99db,6f66d1,daa7e6,856fe3",
            angle = 90,
        }
    },
}

function FemtoHelperRotateDashRefill.texture(room, entity)
    return entity.texture.."idle00"
end

return FemtoHelperRotateDashRefill