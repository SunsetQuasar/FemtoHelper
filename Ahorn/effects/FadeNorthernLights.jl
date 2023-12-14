module FemtoHelperFadeNorthernLights

using ..Ahorn, Maple

@mapdef Effect "FemtoHelper/FadingNorthernLights" FadeNorthernLights(only::String="*", exclude::String="")

placements = FadeNorthernLights

function Ahorn.canFgBg(FadeNorthernLights)
    return true, true
end

end