module FemtoHelperFrogelineTrigger

using ..Ahorn, Maple

@mapdef Trigger "FemtoHelper/FrogelineTrigger" FrogelineTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, event::String="")

const placements = Ahorn.PlacementDict(
    "The Great Unknown Event Trigger (Femto Helper)" => Ahorn.EntityPlacement(
        FrogelineTrigger,
        "rectangle",
    ),
)

end