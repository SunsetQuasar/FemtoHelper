  
module FemtoHelperCustomCliffsideFlag

using ..Ahorn, Maple

@mapdef Entity "FemtoHelper/CustomCliffsideWindFlag" CustomCliffsideFlag(x::Integer, y::Integer, index::Integer=0, spritesPath::String="scenery/cliffside/flag", sineFrequency::Number=1.0, sineAmplitude::Number=1.0, naturalDraft::Number=0.0)

const placements = Ahorn.PlacementDict(
    "Custom Wind Flag (Femto Helper)" => Ahorn.EntityPlacement(
        CustomCliffsideFlag,
    )
)

Ahorn.editingOptions(entity::CustomCliffsideFlag) = Dict{String, Any}(
)

function flagSprite(entity::CustomCliffsideFlag)
    index = Int(get(entity.data, "index", 0))
    path = get(entity.data, "spritesPath", "scenery/cliffside/flag")
    lookup = lpad(string(index), 2, "0")

    return "$(path)$(lookup)"
end

function Ahorn.selection(entity::CustomCliffsideFlag)
    x, y = Ahorn.position(entity)
    sprite = flagSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y, jx=0.0, jy=0.0)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomCliffsideFlag, room::Maple.Room)
    sprite = flagSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0, jx=0.0, jy=0.0)
end

end