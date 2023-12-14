module FemtoHelperCustomMoonCreature

using ..Ahorn, Maple

@mapdef Entity "FemtoHelper/CustomMoonCreature" CustomMoonCreature(
x::Integer,
y::Integer,
acceleration::Number=90.0,
colors::String="c34fc7,4f95c7,53c74f",
sprite::String="moonCreatureTiny",
tintSprite::Bool=false,
trailCount::Integer=10,
trailSubcolor1::String="bde4ee",
trailSubcolor2::String="2f2941",
trailSubcolor1LerpAmount::Number=0.5,
trailSubcolor2LerpAmount::Number=0.5,
trailBaseSize::Number=3.0,
followAcceleration::Number=120.0,
maxSpeed::Number=40.0,
maxFollowSpeed::Number=70.0,
maxFollowDistance::Number=200.0,
minFollowTime::Number=6.0,
maxFollowTime::Number=12.0,
trailCatchupSpeed::Number=128.0,
trailSpacing::Number=2.0,
targetRangeRadius::Number=32.0,
minTargetTime::Number=0.8,
maxTargetTime::Number=4.0,
trailBaseAlpha::Number=1.0,
trailTipAlpha::Number=1.0,
trailGravity::Number=0.05,
bloomAlpha::Number=0.0,
bloomRadius::Number=0.0,
lightColorIsMainColor::Bool=false,
lightColor::String="ffffff",
lightAlpha::Number=0.0,
lightStartFade::Integer=12,
lightEndFade::Integer=24)
   Ahorn.editingOptions(entity::CustomMoonCreature) = Dict{String, Any}(
)


const placements = Ahorn.PlacementDict(
   "Custom Moon Creature (FemtoHelper)" => Ahorn.EntityPlacement(
    CustomMoonCreature
   )
)

function getSprite(entity::CustomMoonCreature)
    sprite = "scenery/moon_creatures/tiny01.png"
end

function Ahorn.selection(entity::CustomMoonCreature)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomMoonCreature, room::Maple.Room)
sprite = getSprite(entity)
Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end