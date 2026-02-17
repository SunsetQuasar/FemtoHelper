local drawableSpriteStruct = require("structs.drawable_sprite")

local FemtoHelperCustomSpeedRotateSpinner = {}

local speeds = {
    slow = "Slow",
    normal = "Normal",
    fast = "Fast",
}

local dustEdgeColor = {1.0, 0.0, 0.0}

-- Values are {dust, star}
local FemtoHelperCustomSpeedRotateSpinnerTypes = {
    blade = {false, true},
    dust = {true, false},
    starfish = {false, false},
}

FemtoHelperCustomSpeedRotateSpinner.name = "FemtoHelper/CustomSpeedRotateSpinner"
FemtoHelperCustomSpeedRotateSpinner.nodeLimits = {1, 1}
FemtoHelperCustomSpeedRotateSpinner.nodeLineRenderType = "circle"
FemtoHelperCustomSpeedRotateSpinner.depth = -50
FemtoHelperCustomSpeedRotateSpinner.placements = {}

for typeName, typeAttributes in pairs(FemtoHelperCustomSpeedRotateSpinnerTypes) do
    for i = 1, 2 do
        local clockwise = i == 1
        local placementName = string.format("%s_%s", typeName, clockwise and "clockwise" or "counter_clockwise")
        local dust, star = unpack(typeAttributes)

        table.insert(FemtoHelperCustomSpeedRotateSpinner.placements, {
            name = placementName,
            data = {
                clockwise = clockwise,
                isDust = dust,
                isBlade = star,
                rotateTime = 1.8,
                noParticles = false,
                scaleX = 1,
                scaleY = 1,
                attach = true,
            }
        })
    end
end

local function getSprite(room, entity, alpha)
    local sprites = {}

    local dust = entity.isDust
    local star = entity.isBlade

    if star then
        local bladeTexture = "danger/blade01"

        table.insert(sprites, drawableSpriteStruct.fromTexture(bladeTexture, entity))

    elseif dust then
        local dustBaseTexture = "danger/dustcreature/base01"
        local dustCenterTexture = "danger/dustcreature/center00"
        local dustBaseOutlineTexture = "dust_creature_outlines/base01"
        table.insert(sprites, drawableSpriteStruct.fromTexture(dustBaseTexture, entity))
        table.insert(sprites, drawableSpriteStruct.fromTexture(dustCenterTexture, entity))
        local dustBaseOutlineSprite = drawableSpriteStruct.fromInternalTexture(dustBaseOutlineTexture, entity)
        
        dustBaseOutlineSprite:setColor(dustEdgeColor)

        table.insert(sprites, dustBaseOutlineSprite)
    else
        local starfishTexture = "danger/starfish15"

        table.insert(sprites, drawableSpriteStruct.fromTexture(starfishTexture, entity))
    end

    if alpha then
        for _, sprite in ipairs(sprites) do
            sprite:setColor({1.0, 1.0, 1.0, alpha})
        end
    end

    return sprites
end

function FemtoHelperCustomSpeedRotateSpinner.sprite(room, entity)
    return getSprite(room, entity)
end

function FemtoHelperCustomSpeedRotateSpinner.nodeSprite(room, entity, node)
    local entityCopy = table.shallowcopy(entity)

    entityCopy.x = node.x
    entityCopy.y = node.y

    return getSprite(room, entityCopy, 0.3)
end

return FemtoHelperCustomSpeedRotateSpinner