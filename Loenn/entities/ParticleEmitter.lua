local drawableSprite = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local drawableRectangle = require("structs.drawable_rectangle")

local drawableText = require("structs.drawable_text")

local function split(inputstr, sep)
  if sep == nil then
    sep = "%s"
  end
  local t = {}
  for str in string.gmatch(inputstr, "([^"..sep.."]+)") do
    table.insert(t, str)
  end
  return t
end

local FemtoHelperParticleEmitter = {}

FemtoHelperParticleEmitter.name = "FemtoHelper/ParticleEmitter"
FemtoHelperParticleEmitter.depth = -999999
-- FemtoHelperParticleEmitter.texture = "ahorn/ParticleEmitter"
FemtoHelperParticleEmitter.justification = {0.5, 0.5}

FemtoHelperParticleEmitter.fieldInformation = {
    particleColorMode = {
        options = {
            {"Color 1", 0},
            {"Choose Color 1/2", 1},
            {"Blink Between Color 1/2", 2},
            {"Fade Color 1 -> 2", 3}
        },
        editable = false
    },
    particleFadeMode = {
        options = {
            {"No Fade", 0},
            {"Linear Fade", 1},
            {"Late Fade", 2},
            {"In And Out", 3}
        },
        editable = false
    },
    particleRotationMode = {
        options = {
            {"Static", 0},
            {"Random", 1},
            {"Follow Direction", 2}
        },
        editable = false
    },
    particleColor = {
      fieldType = "color",
      useAlpha = true
    },
    particleColor2 = {
      fieldType = "color",
      useAlpha = true
    }
}
FemtoHelperParticleEmitter.placements = {
    name = "default",
    data = {
      particleColor="FF7777FF",
      particleColor2="9C2E36FF",
      particleTexture="particles/feather",
      particleFadeMode=2,
      particleColorMode=3,
      particleRotationMode=1,
      particleLifespanMin=1.25,
      particleLifespanMax=1.75,
      particleSize=1.5,
      particleSizeRange=0.5,
      particleScaleOut=true,
      particleSpeedMin=10.0,
      particleSpeedMax=20.0,
      particleFriction=50.0,
      particleAccelX=30.0,
      particleAccelY=-60.0,
      particleFlipChance=true,
      particleSpinSpeedMin=4.0,
      particleSpinSpeedMax=8.0,
      particleCount=1,
      particleSpawnSpread=4,
      spawnInterval=0.1,
      spawnChance=90.0,
      particleAngle=180.0,
      particleAngleRange=360.0,
      bloomAlpha=0.0,
      bloomRadius=6.0,
      particleAlpha=1.0,
      foreground=false,
      tag="",
      flag="",
      noTexture = false,
      attachToPlayer = false,
      attachToPlayerOffsetX = 0,
      attachToPlayerOffsetY = 0,
    }
}

local function getColor(color)
    local success, r, g, b = utils.parseHexColor(color)
    return success and {r, g, b} or {1, 1, 1}
end

function FemtoHelperParticleEmitter.sprite(room, entity)
  local emitter_texture = "loenn/FemtoHelper/ParticleEmitter"
  local emitter_textureOutline = "loenn/FemtoHelper/ParticleEmitterOutline"
  local particle_texture = entity.particleTexture
  
  local color1 = getColor(entity.particleColor)
  local color2 = getColor(entity.particleColor2)

  local MainSprite = drawableSprite.fromTexture(emitter_texture, entity)
  local MainSpriteOutline = drawableSprite.fromTexture(emitter_textureOutline, entity)

  MainSprite:setColor(color1)
  MainSpriteOutline:setColor(color2)

  local Particle1 = drawableSprite.fromTexture(particle_texture, entity)
  local Particle2 = drawableSprite.fromTexture(particle_texture, entity)

  if Particle1 and Particle2 then
    Particle1:addPosition(-8, -8)
    Particle2:addPosition(8, -8)

    Particle1:setColor(color1)
    Particle2:setColor(color2)

    Particle1:setScale(entity.particleSize - entity.particleSizeRange)
    Particle2:setScale(entity.particleSize + entity.particleSizeRange)
  end

  return {
    drawableRectangle.fromRectangle("bordered", entity.x - entity.particleSpawnSpread, entity.y - entity.particleSpawnSpread, entity.particleSpawnSpread * 2, entity.particleSpawnSpread * 2, {color2[1], color2[2], color2[3], 0.25}, {color1[1], color1[2], color1[3], 0.25}),
    MainSprite,
    MainSpriteOutline,
    Particle1,
    Particle2,
  }
end

function FemtoHelperParticleEmitter.selection(room, entity)
    local x, y = entity.x, entity.y

    local mainRectangle = utils.rectangle(x-6, y-6, 12, 12)
    return mainRectangle
end

return FemtoHelperParticleEmitter