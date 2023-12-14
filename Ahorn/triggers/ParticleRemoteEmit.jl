module FemtoHelperParticleRemoteEmit

using ..Ahorn, Maple

@mapdef Trigger "FemtoHelper/ParticleRemoteEmit" ParticleRemoteEmit(x::Integer, y::Integer, width::Integer=16, height::Integer=16, tag::String="tag")

const placements = Ahorn.PlacementDict(
    "Remote Particle Emitter Activator (Femto Helper)" => Ahorn.EntityPlacement(
        ParticleRemoteEmit,
        "rectangle",
    ),
)

end