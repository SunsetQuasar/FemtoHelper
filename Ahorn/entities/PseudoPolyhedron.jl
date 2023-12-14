module FemtoHelperPseudoPolyhedron

using ..Ahorn, Maple

@mapdef Entity "FemtoHelper/PseudoPolyhedron" PseudoPolyhedron(x::Integer, y::Integer, rotationOffset::Number=0.0, rotationSpeed::Number=30.0, baseWidth::Number=10.0, baseHeight::Number=3.0, tipBaseOffsetX::Number=0.0, tipBaseOffsetY::Number=-14.0, color::String="ffffff", baseSideCount::Integer=4, isPrism::Bool=false, alpha::Number=1.0, depth::Integer=6000, samples::Integer=128)

const placements = Ahorn.PlacementDict(
   "Rotating Pyramid/Prism (Femto Helper)" => Ahorn.EntityPlacement(
      PseudoPolyhedron
   )
)

function Ahorn.selection(entity::PseudoPolyhedron)
   x, y = Ahorn.position(entity)

   return Ahorn.getSpriteRectangle("loenn/FemtoHelper/PseudoPolyhedron", x, y)
end

function getColor(color)
   if haskey(Ahorn.XNAColors.colors, color)
       return Ahorn.XNAColors.colors[color]
 
   else
       try
           return ((Ahorn.argb32ToRGBATuple(parse(Int, replace(color, "#" => ""), base=16))[1:3] ./ 255)..., 1.0)
 
       catch
 
       end
   end
 
   return (1.0, 1.0, 1.0, 1.0)
 end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PseudoPolyhedron, room::Maple.Room)
sprite = "ahorn/PseudoPolyhedron"
rawColor = get(entity.data, "color", (1.0,1.0,1.0,1.0))
color = getColor(rawColor)
alpha = get(entity.data, "alpha", 1.0)
Ahorn.drawSprite(ctx, sprite, 0, 0, tint=color .* (1.0,1.0,1.0,alpha))
end

end