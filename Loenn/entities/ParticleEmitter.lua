local drawableSprite = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")

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

function FemtoHelperParticleEmitter.drawSelected(room, layer, entity, color)
  local x, y = entity.x, entity.y
  local spawnspread = entity.particleSpawnSpread
  drawing.callKeepOriginalColor(function()
    love.graphics.setColor(1.0, 0.3, 0.0, 0.4)

    love.graphics.rectangle("fill", x - (spawnspread), y - (spawnspread), spawnspread * 2, spawnspread * 2)
  end)
end

function FemtoHelperParticleEmitter.sprite(room, entity)
  local emitter_texture = "loenn/FemtoHelper/ParticleEmitter"
  local particle_texture = split(entity.particleTexture, ",")[1];
  local color1 = entity.particleColor
  local color2 = entity.particleColor2

  local MainSprite = drawableSprite.fromTexture(emitter_texture, entity)
  local Particle1 = drawableSprite.fromTexture(particle_texture, entity, color1)
  local Particle2 = drawableSprite.fromTexture(particle_texture, entity, color2)

  Particle1:addPosition(-8, -8)
  Particle2:addPosition(8, -8)

  Particle1:setColor(color1)
  Particle2:setColor(color2)

  Particle1:setScale(entity.particleSize - entity.particleSizeRange)
  Particle2:setScale(entity.particleSize + entity.particleSizeRange)

  local sprites = {
    MainSprite,
    Particle1,
    Particle2
  }
  return sprites
end

function FemtoHelperParticleEmitter.selection(room, entity)
    local x, y = entity.x, entity.y

    local mainRectangle = utils.rectangle(x-6, y-6, 12, 12)
    return mainRectangle
end

function split(inputstr, sep)
  if sep == nil then
    sep = "%s"
  end
  local t = {}
  for str in string.gmatch(inputstr, "([^"..sep.."]+)") do
    table.insert(t, str)
  end
  return t
end

return FemtoHelperParticleEmitter