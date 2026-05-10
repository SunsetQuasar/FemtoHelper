using Celeste.Mod.FemtoHelper.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper;

public class FemtoHelperSaveData : EverestModuleSaveData
{
    public Dictionary<string, HardcoreChallenge> HardcoreChallenges { get; set; } = [];

    public HardcoreChallenge GetCurrentChallenge()
    {
        if (Engine.Scene is Level level)
        {
            string levelSet = level.Session.Area.LevelSet;
            if(HardcoreChallenges.TryGetValue(levelSet, out var challenge))
            {
                return challenge;
            }
        }
        return null;
    }
}
