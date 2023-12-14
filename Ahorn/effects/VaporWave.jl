module FemtoHelperVaporWave

using ..Ahorn, Maple

@mapdef Effect "FemtoHelper/VaporWave" FemtoStars(only::String="*", exclude::String="", lineCount::Number=24, horizon::Number=64)

placements = FemtoStars

function Ahorn.canFgBg(VaporWave)
    return true, true
end

end