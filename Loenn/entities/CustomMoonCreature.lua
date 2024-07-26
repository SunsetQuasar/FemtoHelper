local FemtoHelperCustomMoonCreature = {}

FemtoHelperCustomMoonCreature.name = "FemtoHelper/CustomMoonCreature"
FemtoHelperCustomMoonCreature.depth = 0
FemtoHelperCustomMoonCreature.texture = "scenery/moon_creatures/tiny01"
FemtoHelperCustomMoonCreature.fieldInformation = {
    trailSubcolor1 = {
        fieldType = "color"
    },
    trailSubcolor2 = {
        fieldType = "color"
    },
    lightColor = {
        fieldType = "color"
    }
}
FemtoHelperCustomMoonCreature.placements = {
    name = "default",
    data = {
        acceleration=90.0,
        colors="c34fc7,4f95c7,53c74f",
        sprite="moonCreatureTiny",
        tintSprite=false,
        trailCount=10,
        trailSubcolor1="bde4ee",
        trailSubcolor2="2f2941",
        trailSubcolor1LerpAmount=0.5,
        trailSubcolor2LerpAmount=0.5,
        trailBaseSize=3.0,
        followAcceleration=120.0,
        maxSpeed=40.0,
        maxFollowSpeed=70.0,
        maxFollowDistance=200.0,
        minFollowTime=6.0,
        maxFollowTime=12.0,
        trailCatchupSpeed=128.0,
        trailSpacing=2.0,
        targetRangeRadius=32.0,
        minTargetTime=0.8,
        maxTargetTime=4.0,
        trailBaseAlpha=1.0,
        trailTipAlpha=1.0,
        trailGravity=0.05,
        bloomAlpha=0.0,
        bloomRadius=0.0,
        lightColorIsMainColor=false,
        lightColor="ffffff",
        lightAlpha=0.0,
        lightStartFade=12,
        lightEndFade=24
    }
}

return FemtoHelperCustomMoonCreature