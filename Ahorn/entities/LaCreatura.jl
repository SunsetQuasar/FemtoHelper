module FemtoHelperLaCreatura

using ..Ahorn, Maple

@pardef LaCreatura(
x1::Integer,
y1::Integer,
number::Integer=1,
acceleration::Number=90.0,
mainColors::String="c34fc7,4f95c7,53c74f",
butterflyMode::Bool=false,
sprite::String="moonCreatureTiny") =
    Entity("FemtoHelper/LaCreatura",
    x=x1,
    y=y1,
    number=number,
    acceleration=acceleration,
    mainColors=mainColors,
    butterflyMode=butterflyMode,
    sprite=sprite)
    Ahorn.editingOptions(entity::LaCreatura) = Dict{String, Any}(
)


const placements = Ahorn.PlacementDict(
   "La Creatura (Femboy Helper)" => Ahorn.EntityPlacement(
    LaCreatura
   )
)

function getSprite(entity::LaCreatura)
    sprite = "scenery/moon_creatures/tiny03.png"
end

function Ahorn.selection(entity::LaCreatura)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LaCreatura, room::Maple.Room)
sprite = getSprite(entity)
Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end