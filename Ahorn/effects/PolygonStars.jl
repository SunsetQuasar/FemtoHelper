module FemtoHelperPolygonStars

using ..Ahorn, Maple

@mapdef Effect "FemtoHelper/PolygonStars" PolygonStars(only::String="*", exclude::String="", sideCount::Integer=10, pointinessMultiplier::Number=2.0, minRotationSpeed::Number=1.0, maxRotationSpeed::Number=2.0, minSize::Number=2.0, maxSize::Number=8.0, loopBorder::Number=64.0, colors::String="008080", angle::Number=270.0, alpha::Number=1.0, minSpeed::Number=24.0, maxSpeed::Number=48.0, amount::Integer=50, scroll::Number=0.5)

placements = PolygonStars

function Ahorn.canFgBg(PolygonStars)
    return true, true
end

end