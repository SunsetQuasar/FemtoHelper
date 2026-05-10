local FemtoHelperSessionHeart = {}

FemtoHelperSessionHeart.name = "FemtoHelper/SessionHeart"
FemtoHelperSessionHeart.depth = 0
FemtoHelperSessionHeart.justification = {0.5, 0.5}
FemtoHelperSessionHeart.placements = {
    {
        name = "default",
        data = {
            removeCameraTriggers = false,
            poem = "FemtoHelper_SessionHeart_examplePoem",
            group = "",
            sprite = "",
            lightColor = "ADE2D8",
            particleColor = "5BC4B1",
            guiColor = "5BC4B1",
            fakeParticles = false,
            bloomAlpha = 0.75,
            lightAlpha = 1,
            pulseSfx = "event:/game/general/crystalheart_pulse",
            bounceSfx = "event:/game/general/crystalheart_bounce",
            collectSfx = "event:/FemtoHelper/sessionheart_get",
            flashSprite = "heartGemWhite",
            guiSprite = "",
            quickSmash = false,
            quickSmashSfx = "event:/FemtoHelper/sessionheart_shatter",
        }
    }
}

function FemtoHelperSessionHeart.texture(room, entity)
    return "collectables/FemtoHelper/defaultSessionHeart/lonn"
end

return FemtoHelperSessionHeart