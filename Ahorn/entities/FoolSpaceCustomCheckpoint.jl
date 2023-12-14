module FemtoHelperCustomCheckpoint

using ..Ahorn, Maple

@mapdef Entity "FemtoHelper/FoolSpaceCustomCheckpoint" CustomCheckpoint(x::Integer, y::Integer, digitA::String="0", digitB::String="0", spriteRoot::String="scenery/foolspace_checkpoint/", colors::String="FF0000,00FF00,0000FF")

const placements = Ahorn.PlacementDict(
    "Fully Custom Summit Checkpoint (Femto Helper)" => Ahorn.EntityPlacement(
        CustomCheckpoint
    )
)
function Ahorn.selection(entity::CustomCheckpoint)
    x, y = Ahorn.position(entity)
    root = get(entity.data, "spriteRoot", "0")
    baseSprite = "$(root)base02.png"

    return Ahorn.getSpriteRectangle(baseSprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomCheckpoint, room::Maple.Room)
    digit1 = get(entity.data, "digitA", "0")
    digit2 = get(entity.data, "digitB", "0")
    root = get(entity.data, "spriteRoot", "0")
    baseSprite = "$(root)base02.png"

    Ahorn.drawSprite(ctx, baseSprite, 0, 0)
    Ahorn.drawSprite(ctx, "$(root)numberbg0$digit1.png", -2, 4)
    Ahorn.drawSprite(ctx, "$(root)number0$digit1.png", -2, 4)
    Ahorn.drawSprite(ctx, "$(root)numberbg0$digit2.png", 2, 4)
    Ahorn.drawSprite(ctx, "$(root)number0$digit2.png", 2, 4)
end

end