module FemtoHelperWindPetals

using ..Ahorn, Maple

@mapdef Effect "FemtoHelper/WindPetals" WindPetals(only::String="*", exclude::String="", colors::String="66cc22", fallingSpeedMin::Number=6.0, fallingSpeedMax::Number=16.0, blurCount::Integer=15, blurDensity::Number=3.0, texture::String="particles/petal", particleCount::Integer=40, parallax::Number=1.0, spinAmountMultiplier::Number=1.0, spinSpeedMultiplier::Number=1.0, alpha::Number=1.0, scale::Number=1.0, minXDriftSpeed::Number=0.0, maxXDriftSpeed::Number=0.0)

placements = WindPetals

function Ahorn.canFgBg(WindPetals)
    return true, true
end

end