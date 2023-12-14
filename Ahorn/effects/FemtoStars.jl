module FemtoHelperFemtoStars

using ..Ahorn, Maple

@mapdef Effect "FemtoHelper/FemtoStars" FemtoStars(only::String="*", exclude::String="", trailCount::Integer=8, colors::String="008080", minXSpeed::Number=0.0, maxXSpeed::Number=0.0, minYSpeed::Number=12.0, maxYSpeed::Number=24.0, extraLoopBorderX::Number=0.0, extraLoopBorderY::Number=0.0, starCount::Integer=100, backgroundColor::String="000000", backgroundAlpha::Number=1.0, sprite::String="bgs/02/stars", scrollX::Number=0.0, scrollY::Number=0.0, trailSeparation::Number=1.0, animationRate::Number=2.0, alphas::String="0.7")

placements = FemtoStars

function Ahorn.canFgBg(FemtoStars)
    return true, true
end

end