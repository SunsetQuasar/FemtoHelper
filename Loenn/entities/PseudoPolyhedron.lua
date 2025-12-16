local drawableSprite = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local drawableLine = require("structs.drawable_line")

local FemtoHelperPseudoPolyhedron = {}

FemtoHelperPseudoPolyhedron.name = "FemtoHelper/PseudoPolyhedron"
FemtoHelperPseudoPolyhedron.justification = {0.5, 0.5}
FemtoHelperPseudoPolyhedron.fieldInformation = {
   color = {
      fieldType = "color"
   }
}
FemtoHelperPseudoPolyhedron.placements = {
    name = "default",
    data = {
      rotationOffset=0.0,
      rotationSpeed=30.0,
      
      baseWidth=10.0,
      baseHeight=3.0,
      tipBaseOffsetX=0.0,
      tipBaseOffsetY=-14.0,
      color="ffffff",
      baseSideCount=4,
      isPrism=false,
      alpha=1.0,
      depth=6000,
      samples=128,
      topBaseWidth = 10,
      topBaseHeight = 3,
    }
}

local defaultColor = {89 / 255, 88 / 255, 102 / 255}

function FemtoHelperPseudoPolyhedron.sprite(room, entity)

   local color = entity.color

   local sides = entity.baseSideCount
   
   local pi = 3.1415926

   local trigContent = (entity.rotationOffset / entity.samples / sides) * pi * 2

   local sprites = {}

   --base
   local base = {}
   for i=0, sides, 1
   do
      table.insert(base, entity.x + math.sin((trigContent) + (pi / (sides / 2) * i)) * entity.baseWidth)
      table.insert(base, entity.y + math.cos((trigContent) + (pi / (sides / 2) * i)) * entity.baseHeight)
   end
   table.insert(sprites, drawableLine.fromPoints(base, color, 1))

   if not entity.isPrism then
      --sides to tip
      for i=0, sides, 1
      do
         local points = {}
         table.insert(points, entity.x + math.sin((trigContent) + (pi / (sides / 2) * i)) * entity.baseWidth)
         table.insert(points, entity.y + math.cos((trigContent) + (pi / (sides / 2) * i)) * entity.baseHeight)
         table.insert(points, entity.x + entity.tipBaseOffsetX)
         table.insert(points, entity.y + entity.tipBaseOffsetY)
         table.insert(sprites, drawableLine.fromPoints(points, color, 1))
      end
   else
      --top base
      local points = {}
      for i=0, sides, 1
      do
         table.insert(points, entity.x + entity.tipBaseOffsetX + math.sin((trigContent) + (pi / (sides / 2) * i)) * entity.topBaseWidth)
         table.insert(points, entity.y + entity.tipBaseOffsetY + math.cos((trigContent) + (pi / (sides / 2) * i)) * entity.topBaseHeight)
      end
      table.insert(sprites, drawableLine.fromPoints(points, color, 1))

      --side edges
      for i=0, sides, 1
      do
         local points = {}
         table.insert(points, entity.x + math.sin((trigContent) + (pi / (sides / 2) * i)) * entity.baseWidth)
         table.insert(points, entity.y + math.cos((trigContent) + (pi / (sides / 2) * i)) * entity.baseHeight)
         table.insert(points, entity.x + entity.tipBaseOffsetX + math.sin((trigContent) + (pi / (sides / 2) * i)) * entity.topBaseWidth)
         table.insert(points, entity.y + entity.tipBaseOffsetY + math.cos((trigContent) + (pi / (sides / 2) * i)) * entity.topBaseHeight)
         table.insert(sprites, drawableLine.fromPoints(points, color, 1))
      end
   end
      
   return sprites

   
   --return psprite
end
function FemtoHelperPseudoPolyhedron.depth(room, entity)
   return entity.depth
end

function FemtoHelperPseudoPolyhedron.selection(room, entity)
   local x, y = entity.x, entity.y

   local mainRectangle = utils.rectangle(x - entity.baseWidth, y - entity.baseHeight, entity.baseWidth * 2, entity.baseHeight * 2)
   return mainRectangle
 end

return FemtoHelperPseudoPolyhedron