module FemtoHelperCustomSpeedRotateSpinner

using ..Ahorn, Maple

@pardef CustomSpeedRotateSpinner(x1::Integer, y1::Integer, x2::Integer=x1 + 16, y2::Integer=y1, clockwise::Bool=false, rotateTime::Number=1.8, isBlade::Bool=false, isDust::Bool=false, noParticles::Bool=false;) =
    Entity("FemtoHelper/CustomSpeedRotateSpinner", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], clockwise=clockwise, rotateTime=rotateTime, isBlade=isBlade, isDust=isDust, noParticles=noParticles)

function rotatingSpinnerFinalizer(entity::CustomSpeedRotateSpinner)
    x, y = Int(entity.data["x"]), Int(entity.data["y"])
    entity.data["x"], entity.data["y"] = x + 32, y
    entity.data["nodes"] = [(x, y)]
end

const placements = Ahorn.PlacementDict(
    "Star (Rotating, Clockwise, Custom Speed) (Femto Helper)" => Ahorn.EntityPlacement(
        CustomSpeedRotateSpinner,
        "point",
        Dict{String, Any}(
            "isBlade" => false,
            "isDust" => false,
            "clockwise" => true
        ),
        rotatingSpinnerFinalizer
    ),
    "Star (Rotating, Custom Speed) (Femto Helper)" => Ahorn.EntityPlacement(
        CustomSpeedRotateSpinner,
        "point",
        Dict{String, Any}(
            "isBlade" => false,
            "isDust" => false,
            "clockwise" => false
        ),
        rotatingSpinnerFinalizer
    ),
    "Blade (Rotating, Clockwise, Custom Speed) (Femto Helper)" => Ahorn.EntityPlacement(
        CustomSpeedRotateSpinner,
        "point",
        Dict{String, Any}(
            "isBlade" => true,
            "isDust" => false,
            "clockwise" => true
        ),
        rotatingSpinnerFinalizer
    ),
    "Blade (Rotating, Custom Speed) (Femto Helper)" => Ahorn.EntityPlacement(
        CustomSpeedRotateSpinner,
        "point",
        Dict{String, Any}(
            "isBlade" => true,
            "isDust" => false,
            "clockwise" => false
        ),
        rotatingSpinnerFinalizer
    ),
    "Dust Sprite (Rotating, Clockwise, Custom Speed) (Femto Helper)" => Ahorn.EntityPlacement(
        CustomSpeedRotateSpinner,
        "point",
        Dict{String, Any}(
            "isBlade" => false,
            "isDust" => true,
            "clockwise" => true
        ),
        rotatingSpinnerFinalizer
    ),
    "Dust Sprite (Rotating, Custom Speed) (Femto Helper)" => Ahorn.EntityPlacement(
        CustomSpeedRotateSpinner,
        "point",
        Dict{String, Any}(
            "isBlade" => false,
            "isDust" => true,
            "clockwise" => false
        ),
        rotatingSpinnerFinalizer
    ),
)

Ahorn.nodeLimits(entity::CustomSpeedRotateSpinner) = 1, 1

function Ahorn.selection(entity::CustomSpeedRotateSpinner)
    nx, ny = Int.(entity.data["nodes"][1])
    x, y = Ahorn.position(entity)

    return [Ahorn.Rectangle(x - 8, y - 8, 16, 16), Ahorn.Rectangle(nx - 8, ny - 8, 16, 16)]
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::CustomSpeedRotateSpinner, room::Maple.Room)
    clockwise = get(entity.data, "clockwise", false)
    dir = clockwise ? 1 : -1

    centerX, centerY = Int.(entity.data["nodes"][1])
    x, y = Ahorn.position(entity)

    radius = sqrt((centerX - x)^2 + (centerY - y)^2)

    Ahorn.drawCircle(ctx, centerX, centerY, radius, Ahorn.colors.selection_selected_fc)
    Ahorn.drawArrow(ctx, centerX + radius, centerY, centerX + radius, centerY + 0.001 * dir, Ahorn.colors.selection_selected_fc, headLength=6)
    Ahorn.drawArrow(ctx, centerX - radius, centerY, centerX - radius, centerY + 0.001 * -dir, Ahorn.colors.selection_selected_fc, headLength=6)

    Ahorn.drawSprite(ctx, "danger/starfish00", x, y)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CustomSpeedRotateSpinner, room::Maple.Room)
    centerX, centerY = Int.(entity.data["nodes"][1])

    Ahorn.drawSprite(ctx, "danger/starfish00", centerX, centerY)
end

end