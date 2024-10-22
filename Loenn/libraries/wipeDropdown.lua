local celesteEnums = require("consts.celeste_enums")

-- Inject FemtoHelper wipes into the dropdown in map metadata.
-- From JungleHelper, thanks uhh whoever coded it sorry i have no idea who (maddie probably? thanks maddie)

celesteEnums.wipe_names["Triangles [Femto Helper]"] = "CliffhangerWipe" -- no namespace, haha fuck me
celesteEnums.wipe_names["Square [Femto Helper]"] = "Celeste.Mod.FemtoHelper.Wipes.SquareWipe"
celesteEnums.wipe_names["Sinewave [Femto Helper]"] = "Celeste.Mod.FemtoHelper.Wipes.SineWipe"
celesteEnums.wipe_names["Bars [Femto Helper]"] = "Celeste.Mod.FemtoHelper.Wipes.CirclerWipe"
celesteEnums.wipe_names["Diamond [Femto Helper]"] = "Celeste.Mod.FemtoHelper.Wipes.DiamondWipe"
celesteEnums.wipe_names["Diagrid [Femto Helper]"] = "Celeste.Mod.FemtoHelper.Wipes.DiagridWipe"
celesteEnums.wipe_names["Dissolve [Femto Helper]"] = "Celeste.Mod.FemtoHelper.Wipes.DissolveWipe"
celesteEnums.wipe_names["Helios [Femto Helper]"] = "Celeste.Mod.FemtoHelper.Wipes.SolarWipe"
-- celesteEnums.wipe_names["Unwise [Femto Helper]"] = "Celeste.Mod.FemtoHelper.Wipes.UnwiseWipe"

return {}