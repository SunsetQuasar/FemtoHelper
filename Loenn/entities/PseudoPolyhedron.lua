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

   local points = {}
   
   local pi = 3.1415926

   local trigContent = (entity.rotationOffset / entity.samples / sides) * pi * 2

   if entity.isPrism == false then
      for i=0, sides, 1
      do
         --base
         table.insert(points, entity.x + math.sin((trigContent) + (pi / (sides / 2) * i)) * entity.baseWidth)
         table.insert(points, entity.y + math.cos((trigContent) + (pi / (sides / 2) * i)) * entity.baseHeight)
      end
      for i=0, sides, 1
      do
         --tip
         table.insert(points, entity.x + entity.tipBaseOffsetX)
         table.insert(points, entity.y + entity.tipBaseOffsetY)
         --base again ahah
         table.insert(points, entity.x + math.sin((trigContent) + (pi / (sides / 2) * i)) * entity.baseWidth)
         table.insert(points, entity.y + math.cos((trigContent) + (pi / (sides / 2) * i)) * entity.baseHeight)
      end
   else
      for i=0, sides, 1
      do
         --base
         table.insert(points, entity.x + math.sin((trigContent) + (pi / (sides / 2) * i)) * entity.baseWidth)
         table.insert(points, entity.y + math.cos((trigContent) + (pi / (sides / 2) * i)) * entity.baseHeight)
      end
      for i=0, sides, 1
      do
         table.insert(points, entity.x + entity.tipBaseOffsetX + math.sin((trigContent) + (pi / (sides / 2) * i)) * entity.topBaseWidth)
         table.insert(points, entity.y + entity.tipBaseOffsetY + math.cos((trigContent) + (pi / (sides / 2) * i)) * entity.topBaseHeight)

         table.insert(points, entity.x + entity.tipBaseOffsetX + math.sin((trigContent) + (pi / (sides / 2) * (i + 1))) * entity.topBaseWidth)
         table.insert(points, entity.y + entity.tipBaseOffsetY + math.cos((trigContent) + (pi / (sides / 2) * (i + 1))) * entity.topBaseHeight)

         table.insert(points, entity.x + math.sin((trigContent) + (pi / (sides / 2) * (i+1))) * entity.baseWidth)
         table.insert(points, entity.y + math.cos((trigContent) + (pi / (sides / 2) * (i+1))) * entity.baseHeight)
      end
   end
      
      return drawableLine.fromPoints(points, color, 1)

   
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