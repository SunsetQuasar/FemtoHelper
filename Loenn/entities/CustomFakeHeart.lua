local FemtoHelperCustomFakeHeart = {}

FemtoHelperCustomFakeHeart.name = "FemtoHelper/CustomFakeHeart"
FemtoHelperCustomFakeHeart.depth = -2000000
FemtoHelperCustomFakeHeart.justification = {0.5, 0.5}
FemtoHelperCustomFakeHeart.fieldInformation = {
    heartBehavior = {
        options = {
            {"Shatter", 0},
            {"Kill & Shatter", 1},
            {"Bounce", 2},
            {"Phase Through", 3}
        },
        editable = false
    },
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
    }
}
FemtoHelperCustomFakeHeart.placements = {
    name = "default",
    data = {
        respawnTime=3.0,
        heartBehavior=0,
        setFlag=true,
        flag="custom_flag",
        useFakeSounds=false,
        pulse=true,
        particleColor="f0fdff",
        particleColor2="abd5ff",
        freezeTime=0.1,
        audioEvent="event:/game/06_reflection/boss_spikes_burst",
        particleTexture="particles/shard",
        particleFadeMode=2,
        particleColorMode=3,
        particleRotationMode=1,
        particleLifespanMin=1.25,
        particleLifespanMax=1.75,
        particleSize=1.5,
        particleSizeRange=0.5,
        flashOpacity=1.0,
        flashColor="ffffff",
        particleScaleOut=false,
        particleSpeedMin=40.0,
        particleSpeedMax=70.0,
        particleFriction=35.0,
        particleAccelX=0.0,
        particleAccelY=-60.0,
        particleFlipChance=true,
        particleSpinSpeedMin=2.0,
        particleSpinSpeedMax=4.0,
        particleCount=12,
        particleSpawnSpread=8.0,
        burstDuration=1.0,
        burstRadiusFrom=4.0,
        burstRadiusTo=40.0,
        burstAlpha=0.5,
        miniHeartMode=false,
        removeBloomOnShatter=false,
        shineParticleColor="5caefa",
        spriteDirectory="collectables/heartGem/0",
        noBloom=false,
        noWiggle=false,
        breakOnContact=false,
        silentPulse=false,
        collectFlash=true,
        flashOverPlayer=false
    }
}

function FemtoHelperCustomFakeHeart.texture(room, entity)
    return entity.spriteDirectory .. "/00"
end

return FemtoHelperCustomFakeHeart