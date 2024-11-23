local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local FemtoHelperGloriousPassage = {}

FemtoHelperGloriousPassage.name = "FemtoHelper/GloriousPassage"
FemtoHelperGloriousPassage.minimumSize = {8, 8}

FemtoHelperGloriousPassage.fieldInformation = {
	spawnpointIndex = {
		fieldType = "integer"
	},
	depth = {
		fieldType = "integer"
	}
}

FemtoHelperGloriousPassage.placements = {
    {
        name = "passage",
        data = {
            width = 16,
            height = 24,
            roomName = "",
			audio = "event:/FemtoHelper/smw_door_opens",
			closedPath = "objects/FemtoHelper/SMWDoor/closed",
			openPath = "objects/FemtoHelper/SMWDoor/open",
            simpleTrigger = false,
            faceLeft = false,
			spawnpointIndex = 0,
			pressUpToOpen = false,
            keepDashes = true,
			depth = 120,
            sameRoom = true,
            carryHoldablesOver = true,
			flag = ""
        }
	},
	{
		name = "passageUp",
        data = {
            width = 16,
            height = 24,
            roomName = "",
			audio = "event:/FemtoHelper/smw_door_opens",
			closedPath = "objects/FemtoHelper/SMWDoor/closed",
			openPath = "objects/FemtoHelper/SMWDoor/open",
            simpleTrigger = false,
            faceLeft = false,
			spawnpointIndex = 0,
			pressUpToOpen = true,
            keepDashes = true,
			depth = 120,
            sameRoom = true,
            carryHoldablesOver = true,
			flag = ""
        }
	},
	{
		name = "passageSimple",
        data = {
            width = 16,
            height = 24,
            roomName = "",
			audio = "",
			closedPath = "objects/FemtoHelper/SMWDoor/closed",
			openPath = "objects/FemtoHelper/SMWDoor/open",
            simpleTrigger = true,
            faceLeft = false,
			spawnpointIndex = 0,
			pressUpToOpen = false,
            keepDashes = true,
			depth = 120,
            sameRoom = true,
            carryHoldablesOver = true,
			flag = ""
        }
    }
}

local ninePatchOptions = {
    mode = "border",
    borderMode = "repeat",
}

function FemtoHelperGloriousPassage.depth(room, entity, viewport)
	return entity.depth or 120
end

function FemtoHelperGloriousPassage.minimumSize(room, entity)
	return not entity.simpleTrigger and {16, 24} or {8, 8}
end

function FemtoHelperGloriousPassage.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local crystalTexture = entity.closedPath
    local rectTexture = "objects/FemtoHelper/SMWDoor/rect"

    local ninePatch = drawableNinePatch.fromTexture(rectTexture, ninePatchOptions, x, y, width, height)
    local crystalSprite = drawableSprite.fromTexture(crystalTexture, entity)
    local sprites = ninePatch:getDrawableSprite()

    crystalSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    if entity.simpleTrigger then
        crystalSprite:setColor({1, 1, 1, 0.3})
    end
    table.insert(sprites, crystalSprite)

    return sprites
end

return FemtoHelperGloriousPassage