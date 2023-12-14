module FemtoHelperCustomParallaxBigWaterfall

using ..Ahorn, Maple

@pardef CustomParallaxBigWaterfall(x1::Integer, y1::Integer, x2::Integer=x1 + 16, y2::Integer=y1, width::Integer=8, height::Integer=8, parallax::Number=1.0, fallSpeedMultiplier::Number=1.0, surfaceOpacity::Number=0.5, fillOpacity::Number=0.3, surfaceColor::String="89dbf0", fillColor::String="29a7ea") =
    Entity("FemtoHelper/CustomParallaxBigWaterfall", x=x1, y=y1, width=width, height=height, parallax=parallax, fallSpeedMultiplier=fallSpeedMultiplier, surfaceOpacity=surfaceOpacity, fillOpacity=fillOpacity, surfaceColor=surfaceColor, fillColor=fillColor)

const placements = Ahorn.PlacementDict(
    "Big Waterfall (Custom Parallax, FG) (Femto Helper)" => Ahorn.EntityPlacement(
        CustomParallaxBigWaterfall,
        "rectangle",
        Dict{String, Any}(
            "layer" => "FG"
        )
    ),
    "Big Waterfall (Custom Parallax, BG) (Femto Helper)" => Ahorn.EntityPlacement(
        CustomParallaxBigWaterfall,
        "rectangle",
        Dict{String, Any}(
            "layer" => "BG"
        )
    ),
)

const fillColor = Ahorn.XNAColors.LightBlue .* 0.3
const surfaceColor = Ahorn.XNAColors.LightBlue .* 0.8

const waterSegmentLeftMatrix = [
    1 1 1 0 1 0;
    1 1 1 0 1 0;
    1 1 1 0 1 0;
    1 1 1 1 0 1;
    1 1 1 1 0 1;
    1 1 1 1 0 1;
    1 1 1 1 0 1;
    1 1 1 1 0 1;
    1 1 1 1 0 1;
    1 1 1 1 0 1;
    1 1 1 1 0 1;
    1 1 1 0 1 0;
    1 1 1 0 1 0;
    1 1 0 1 0 0;
    1 1 0 1 0 0;
    1 1 0 1 0 0;
    1 1 0 1 0 0;
    1 1 0 1 0 0;
    1 1 0 1 0 0;
    1 1 0 1 0 0;
    1 1 0 1 0 0;
]

const waterSegmentLeft = Ahorn.matrixToSurface(
    waterSegmentLeftMatrix,
    [
        fillColor,
        surfaceColor
    ]
)

const waterSegmentRightMatrix = [
    0 1 0 1 1 1;
    0 1 0 1 1 1;
    0 1 0 1 1 1;
    0 0 1 0 1 1;
    0 0 1 0 1 1;
    0 0 1 0 1 1;
    0 0 1 0 1 1;
    0 0 1 0 1 1;
    0 0 1 0 1 1;
    0 0 1 0 1 1;
    0 0 1 0 1 1;
    0 1 0 1 1 1;
    0 1 0 1 1 1;
    1 0 1 1 1 1;
    1 0 1 1 1 1;
    1 0 1 1 1 1;
    1 0 1 1 1 1;
    1 0 1 1 1 1;
    1 0 1 1 1 1;
    1 0 1 1 1 1;
    1 0 1 1 1 1;
]

const waterSegmentRight = Ahorn.matrixToSurface(
    waterSegmentRightMatrix,
    [
        fillColor,
        surfaceColor
    ]
)

Ahorn.minimumSize(entity::CustomParallaxBigWaterfall) = 8, 8
Ahorn.resizable(entity::CustomParallaxBigWaterfall) = true, true

Ahorn.selection(entity::CustomParallaxBigWaterfall) = Ahorn.getEntityRectangle(entity)

Ahorn.editingOptions(entity::CustomParallaxBigWaterfall) = Dict{String, Any}(
    "layer" => String["FG", "BG"]
)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomParallaxBigWaterfall, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 16))
    height = Int(get(entity.data, "height", 64))

    segmentHeightLeft, segmentWidthLeft = size(waterSegmentLeftMatrix)
    segmentHeightRight, segmentWidthRight = size(waterSegmentRightMatrix)

    Ahorn.Cairo.save(ctx)

    Ahorn.rectangle(ctx, 0, 0, width, height)
    Ahorn.clip(ctx)

    for i in 0:segmentHeightLeft:ceil(Int, height / segmentHeightLeft) * segmentHeightLeft
        Ahorn.drawImage(ctx, waterSegmentLeft, 0, i)
        Ahorn.drawImage(ctx, waterSegmentRight, width - segmentWidthRight, i)
    end

    # Drawing a rectangle normally doesn't guarantee that its the same color as above
    if height >= 0 && width >= segmentWidthLeft + segmentWidthRight
        fillRectangle = Ahorn.matrixToSurface(fill(0, (height, width - segmentWidthLeft - segmentWidthRight)), [fillColor])
        Ahorn.drawImage(ctx, fillRectangle, segmentWidthLeft, 0)
    end
    
    Ahorn.restore(ctx)
end

end