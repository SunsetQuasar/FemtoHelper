module FemtoHelperFrogelineTalker

using ..Ahorn, Maple

@mapdef Trigger "FemtoHelper/FrogelineTalker" FrogelineTalker(x::Integer, y::Integer, width::Integer=16, height::Integer=16, event::String="")

const placements = Ahorn.PlacementDict(
    "sussy trigger (Femboy Helper) (DO NOT RELEASE)" => Ahorn.EntityPlacement(
        FrogelineTalker,
        "rectangle",
    ),
)

end