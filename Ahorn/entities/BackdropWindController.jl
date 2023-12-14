module FemtoHelperBackdropWindController

using ..Ahorn, Maple

@mapdef Entity "FemtoHelper/BackdropWindController" BackdropWindController(x::Integer, y::Integer, tag::String="windbackdrop", strengthMultiplier::Number=1.0)

const placements = Ahorn.PlacementDict(
   "Backdrop Wind Controller (Femto Helper)" => Ahorn.EntityPlacement(
      BackdropWindController
   )
)

function Ahorn.selection(entity::BackdropWindController)
   x, y = Ahorn.position(entity)

   return Ahorn.getSpriteRectangle("loenn/FemtoHelper/BackdropWindController", x, y)
end

sprite = "ahorn/BackdropWindController"
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BackdropWindController, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0,0)

end