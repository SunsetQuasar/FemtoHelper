module FemtoHelperVectorSpace

using ..Ahorn, Maple

@mapdef Effect "FemtoHelper/VectorSpace" VectorSpace(
    only::String="*", 
    exclude::String="", 
    spacingX::Number=30.0, 
    spacingY::Number=30.0, 
    velocityX::Number=-10.0, 
    velocityY::Number=10.0, 
    scroll::Number=1.0, 
    sineXOffsetMin::Number=0.0, 
    sineXOffsetMax::Number=180.0, 
    sineYOffsetMin::Number=0.0, 
    sineYOffsetMax::Number=180.0, 
    sineXFreqMin::Number=-90.0, 
    sineXFreqMax::Number=90.0, 
    sineYFreqMin::Number=-90.0, 
    sineYFreqMax::Number=90.0, 
    amplitude::Number=12.0, 
    renderTip::Bool=true,
    scaleTip::Bool=false,
    alpha::Number=1.0,
    color::String="888800,008888,880088",
    yFreqRelativeToX::Bool=false,
    yOffsetRelativeToX::Bool=false
    )

placements = VectorSpace

function Ahorn.canFgBg(VectorSpace)
    return true, true
end

end