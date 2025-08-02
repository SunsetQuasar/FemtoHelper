local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local FemtoHelperTheContraption = {}

FemtoHelperTheContraption.name = "FemtoHelper/TheContraption"
FemtoHelperTheContraption.depth = 0
FemtoHelperTheContraption.minimumSize = {8, 8}
FemtoHelperTheContraption.placements = {
    {
        name = "TheContraption",
        data = {
            width = 24, 
            height = 24,
            spritePath = "objects/Femtohelper/TheContraption/",
            particles = true,
            reducedDebris = false,
        }
    }
}

function FemtoHelperTheContraption.sprite(room, entity)

    local sprites = {}

    table.insert(sprites, drawableRectangle.fromRectangle("fill", entity.x - 1, entity.y, entity.width + 2, entity.height, {0, 0, 0, 1}):getDrawableSprite())
    table.insert(sprites, drawableRectangle.fromRectangle("fill", entity.x, entity.y - 1, entity.width, entity.height + 2, {0, 0, 0, 1}):getDrawableSprite())

    for k = 1, entity.width / 8 do
        for l = 1, entity.height / 8 do

            local k2 = k-1
            local l2 = l-1

            local num4 = (entity.width / 8) == 1 and 3 or (k2 ~= 0 and ((k2 ~= (entity.width / 8) - 1) and 1 or 2) or 0)
            local num5 = (entity.height / 8) == 1 and 3 or (l2 ~= 0 and ((l2 ~= (entity.height / 8) - 1) and 1 or 2) or 0)

            local spr = drawableSprite.fromTexture(entity.spritePath.."nineSlice", entity);
            spr:useRelativeQuad(num4 * 8, num5 * 8, 8, 8)
            spr:addPosition(k2 * 8, l2 * 8)
            table.insert(sprites, spr)
        end
    end

    local cog = drawableSprite.fromTexture(entity.spritePath.."cog", entity);
    local cog1 = drawableSprite.fromTexture(entity.spritePath.."cog", entity);
    local cog2 = drawableSprite.fromTexture(entity.spritePath.."cog", entity);
    local cog3 = drawableSprite.fromTexture(entity.spritePath.."cog", entity);
    local cog4 = drawableSprite.fromTexture(entity.spritePath.."cog", entity);

    cog:setJustification(0.5, 0.5)
    cog:addPosition(entity.width / 2, entity.height / 2)

    cog1:setColor({0.25, 0.25, 0.25, 1})
    cog1:setJustification(0.5, 0.5)
    cog1:addPosition(entity.width / 2, entity.height / 2)
    cog1:addPosition(0, 1)

    cog2:setColor({0.25, 0.25, 0.25, 1})
    cog2:setJustification(0.5, 0.5)
    cog2:addPosition(entity.width / 2, entity.height / 2)
    cog2:addPosition(1, 0)

    cog3:setColor({0.25, 0.25, 0.25, 1})
    cog3:setJustification(0.5, 0.5)
    cog3:addPosition(entity.width / 2, entity.height / 2)
    cog3:addPosition(0, -1)

    cog4:setColor({0.25, 0.25, 0.25, 1})
    cog4:setJustification(0.5, 0.5)
    cog4:addPosition(entity.width / 2, entity.height / 2)
    cog4:addPosition(-1, 0)

    table.insert(sprites, cog1)
    table.insert(sprites, cog2)
    table.insert(sprites, cog3)
    table.insert(sprites, cog4)
    table.insert(sprites, cog)

    return sprites
end


return FemtoHelperTheContraption