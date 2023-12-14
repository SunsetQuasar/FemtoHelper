module FemtoHelperCustomFakeHeart

using ..Ahorn, Maple

@pardef CustomFakeHeart(
x1::Integer,
y1::Integer,
x2::Integer=x1 + 16,
y2::Integer=y1,
respawnTime::Number=3.0,
heartBehavior::Integer=0,
setFlag::Bool=true,
flag::String="custom_flag",
useFakeSounds::Bool=false,
pulse::Bool=true,
particleColor::String="f0fdff",
particleColor2::String="abd5ff",
freezeTime::Number=0.1,
audioEvent::String="event:/game/06_reflection/boss_spikes_burst",
particleTexture::String="particles/shard",
particleFadeMode::Integer=2,
particleColorMode::Integer=3,
particleRotationMode::Integer=1,
particleLifespanMin::Number=1.25,
particleLifespanMax::Number=1.75,
particleSize::Number=1.5,
particleSizeRange::Number=0.5,
flashOpacity::Number=1.0,
flashColor::String="ffffff",
particleScaleOut::Bool=false,
particleSpeedMin::Number=40.0,
particleSpeedMax::Number=70.0,
particleFriction::Number=35.0,
particleAccelX::Number=0.0,
particleAccelY::Number=-60.0,
particleFlipChance::Bool=true,
particleSpinSpeedMin::Number=2.0,
particleSpinSpeedMax::Number=4.0,
particleCount::Integer=12,
particleSpawnSpread::Number=8.0,
burstDuration::Number=1.0,
burstRadiusFrom::Number=4.0,
burstRadiusTo::Number=40.0,
burstAlpha::Number=0.5,
miniHeartMode::Bool=false,
removeBloomOnShatter::Bool=false,
shineParticleColor::String="5caefa",
spriteDirectory::String="collectables/heartGem/0",
noBloom::Bool=false,
noWiggle::Bool=false,
breakOnContact::Bool=false,
silentPulse::Bool=false) =
    Entity("FemtoHelper/CustomFakeHeart",
    x=x1,
    y=y1,
    respawnTime=respawnTime,
    heartBehavior=heartBehavior,
    setFlag=setFlag,
    flag=flag,
    useFakeSounds=useFakeSounds,
    pulse=pulse,
    particleColor=particleColor,
    particleColor2 = particleColor2,
    particleTexture=particleTexture,
    freezeTime=freezeTime,
    audioEvent=audioEvent,
    particleFadeMode=particleFadeMode,
    particleRotationMode=particleRotationMode,
    particleColorMode=particleColorMode,
    particleLifespanMin=particleLifespanMin,
    particleLifespanMax=particleLifespanMax,
    particleSize=particleSize,
    particleSizeRange=particleSizeRange,
    flashOpacity=flashOpacity,
    flashColor=flashColor,
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
    burstDuration=burstDuration,
    burstRadiusFrom=burstRadiusFrom,
    burstRadiusTo=burstRadiusTo,
    burstAlpha=burstAlpha,
    miniHeartMode=miniHeartMode,
    removeBloomOnShatter=removeBloomOnShatter,
    shineParticleColor=shineParticleColor,
    spriteDirectory=spriteDirectory,
    noBloom=noBloom,
    noWiggle=noWiggle,
    breakOnContact=breakOnContact,
    silentPulse=silentPulse)
    Ahorn.editingOptions(entity::CustomFakeHeart) = Dict{String, Any}(
  "heartBehavior" => Dict{String, Int}(
    "Shatter" => 0,
    "KillShatter" => 1,
    "Bounce" => 2,
    "PhaseThrough" => 3
  ),
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
   "Custom Fake Crystal Heart (Femto Helper)" => Ahorn.EntityPlacement(
      CustomFakeHeart
   )
)

function getHeartSprite(entity::CustomFakeHeart)
    color = Int(get(entity.data, "heartColor", -1))
    if 0 <= color <= 2
        return "collectables/heartGem/$(color)/00"
    else
        return get(entity.data, "spriteDirectory", "collectables/heartGem/0") * "/00"
    end
end

function Ahorn.selection(entity::CustomFakeHeart)
    x, y = Ahorn.position(entity)
    sprite = getHeartSprite(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomFakeHeart, room::Maple.Room)
    sprite = getHeartSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end