module FemtoHelperParticleEmitter

using ..Ahorn, Maple

@pardef ParticleEmitter(
x1::Integer,
y1::Integer,
x2::Integer=x1 + 16,
y2::Integer=y1,
particleColor::String="ff7777",
particleColor2::String="9c2e36",
particleTexture::String="particles/feather",
particleFadeMode::Integer=2,
particleColorMode::Integer=3,
particleRotationMode::Integer=1,
particleLifespanMin::Number=1.25,
particleLifespanMax::Number=1.75,
particleSize::Number=1.5,
particleSizeRange::Number=0.5,
particleScaleOut::Bool=true,
particleSpeedMin::Number=10.0,
particleSpeedMax::Number=20.0,
particleFriction::Number=50.0,
particleAccelX::Number=30.0,
particleAccelY::Number=-60.0,
particleFlipChance::Bool=true,
particleSpinSpeedMin::Number=4.0,
particleSpinSpeedMax::Number=8.0,
particleCount::Integer=1,
particleSpawnSpread::Number=4,
spawnInterval::Number=0.1,
spawnChance::Number=90.0,
particleAngle::Number=180.0,
particleAngleRange::Number=360.0,
bloomAlpha::Number=0.0,
bloomRadius::Number=6.0,
particleAlpha::Number=1.0,
tag::String="",
flag::String="",
noTexture::Bool=false,
foreground::Bool=false) =
    Entity("FemtoHelper/ParticleEmitter",
    x=x1,
    y=y1,
    particleColor=particleColor,
    particleColor2=particleColor2,
    particleTexture=particleTexture,
    particleFadeMode=particleFadeMode,
    particleRotationMode=particleRotationMode,
    particleColorMode=particleColorMode,
    particleLifespanMin=particleLifespanMin,
    particleLifespanMax=particleLifespanMax,
    particleSize=particleSize,
    particleSizeRange=particleSizeRange,
    particleScaleOut=particleScaleOut,
    particleSpeedMin=particleSpeedMin,
    particleSpeedMax=particleSpeedMax,
    particleFriction=particleFriction,
    particleAccelX=particleAccelX,
    particleAccelY=particleAccelY,
    particleFlipChance=particleFlipChance,
    particleSpinSpeedMin=particleSpinSpeedMin,
    particleSpinSpeedMax=particleSpinSpeedMax,
    particleCount=particleCount,
    particleSpawnSpread=particleSpawnSpread,
    spawnInterval=spawnInterval,
    spawnChance=spawnChance,
    particleAngle=particleAngle,
    particleAngleRange=particleAngleRange,
    bloomAlpha=bloomAlpha,
    bloomRadius=bloomRadius,
    particleAlpha=particleAlpha,
    tag=tag,
    flag=flag,
    noTexture=noTexture,
    foreground=foreground)
    Ahorn.editingOptions(entity::ParticleEmitter) = Dict{String, Any}(
  "particleColorMode" => Dict{String, Int}(
    "Static" => 0,
    "Choose" => 1,
    "Blink" => 2,
    "Fade" => 3
  ),
  "particleFadeMode" => Dict{String, Int}(
    "None" => 0,
    "Linear" => 1,
    "Late" => 2,
    "InAndOut" => 3
  ),
  "particleRotationMode" => Dict{String, Int}(
    "None" => 0,
    "Random" => 1,
    "SameAsDirection" => 2
  )
)


const placements = Ahorn.PlacementDict(
   "Particle Emitter (Femto Helper)" => Ahorn.EntityPlacement(
      ParticleEmitter
   )
)

defaultTexture = "particles/stars/00.png"

function Ahorn.selection(entity::ParticleEmitter)
    x, y = Ahorn.position(entity)

    texture = get(entity.data, "particleTexture", defaultTexture)

    return Ahorn.getSpriteRectangle(texture, x, y)
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

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ParticleEmitter, room::Maple.Room)
  texture = get(entity.data, "particleTexture", defaultTexture)
  rawColor = get(entity.data, "particleColor", (1.0,1.0,1.0,1.0))
  color = getColor(rawColor)
  rawColor2 = get(entity.data, "particleColor2", (1.0,1.0,1.0,1.0))
  color2 = getColor(rawColor2)
  alpha = get(entity.data, "particlealpha", 1.0)
  boxsize = get(entity.data, "particleSpawnSpread", 8)
  Ahorn.drawSprite(ctx, texture, 0, 0, tint=color .* (1.0,1.0,1.0,alpha))
  Ahorn.drawRectangle(ctx, -boxsize ./ 2, -boxsize ./ 2, boxsize, boxsize, color2 .* (1.0, 1.0, 1.0, 0.4), color2)
end 

end