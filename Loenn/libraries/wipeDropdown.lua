local celesteEnums = require("consts.celeste_enums")

-- Inject FemtoHelper wipes into the dropdown in map metadata.
-- From JungleHelper, thanks uhh whoever coded it sorry i have no idea who (max probably? thanks max)

celesteEnums.wipe_names["Triangles [Femto Helper]"] = "CliffhangerWipe" -- no namespace, haha fuck me
celesteEnums.wipe_names["Square [Femto Helper]"] = "Celeste.Mod.FemtoHelper.Wipes.SquareWipe"
celesteEnums.wipe_names["Sinewave [Femto Helper]"] = "Celeste.Mod.FemtoHelper.Wipes.SineWipe"
celesteEnums.wipe_names["Bars [Femto Helper]"] = "Celeste.Mod.FemtoHelper.Wipes.CirclerWipe"

return {}