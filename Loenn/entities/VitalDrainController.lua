local FemtoHelperVitalDrainController = {}

FemtoHelperVitalDrainController.name = "FemtoHelper/VitalDrainController"
FemtoHelperVitalDrainController.depth = 0
FemtoHelperVitalDrainController.texture = "loenn/Femtohelper/vitalcontroller"
FemtoHelperVitalDrainController.placements = {
    name = "gaseous",
    data = {

        debugView = false,
        colorgradeA = "feelingdown",
        colorgradeB = "golden",
        drainRate = 150,
        fadeInTag = "o2_in_tag",
        fadeOutTag = "o2_out_tag",
        fastDeath = true,
        flag = "o2_flag",
        recoverRate = 1000,
        anxiety = 0.3,
        musicParamName = "param_here",
        musicParamMin = 0,
        musicParamMax = 1,
        cameraZoomTarget = 1,
        useFlag = "",
        invertFlag = false,
    }
}

return FemtoHelperVitalDrainController