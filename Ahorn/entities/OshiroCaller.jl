module FemtoHelperOshiroCaller

using ..Ahorn, Maple

@pardef OshiroCaller(
x1::Integer,
y1::Integer,
repell::Bool=false,
justMakeOshiroLeave::Bool=false) =
    Entity("FemtoHelper/OshiroCaller",
    x=x1,
    y=y1,
    repell=repell,
    justMakeOshiroLeave=justMakeOshiroLeave)
    Ahorn.editingOptions(entity::OshiroCaller) = Dict{String, Any}(
)


const placements = Ahorn.PlacementDict(
   "Oshiro Caller (Call, Femto Helper)" => Ahorn.EntityPlacement(
    OshiroCaller
   )
)

function getSprite(entity::OshiroCaller)
    if entity.data["repell"] == true
        sprite = "objects/oshiroCaller/repeller00.png"
    else
        sprite = "objects/oshiroCaller/caller00.png"
    end
end

function Ahorn.selection(entity::OshiroCaller)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::OshiroCaller, room::Maple.Room)
sprite = getSprite(entity)
Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end