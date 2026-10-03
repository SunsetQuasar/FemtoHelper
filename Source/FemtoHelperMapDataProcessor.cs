using Celeste.Mod.FemtoHelper.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper;

internal class FemtoHelperMapDataProcessor : EverestMapDataProcessor
{
    internal static Dictionary<string, HashSet<string>> hookRelevantSIDs = []; 

    public override Dictionary<string, Action<BinaryPacker.Element>> Init() {
        return new()
        {
            {"entity:FemtoHelper/MovementModifier", (element) => {
                AddElement("FemtoHelper/MovementModifier");
            }}
        };
    }

    public void AddElement(string element)
    {
        Verbose($"Area '{Context.AreaData.SID}' has '{element}'.");
        if (hookRelevantSIDs.TryGetValue(Context.AreaData.SID, out var elements))
        {
            elements.Add(element);
        }
        else
        {
            hookRelevantSIDs.Add(Context.AreaData.SID, [element]);
        }
    }

    public override void Reset() 
    {
        //Verbose("MapDataProcessor Reset");
        hookRelevantSIDs.GetValueOrDefault(Context.AreaData.SID)?.Clear();
    }

    public override void End()
    {
        ///Verbose("MapDataProcessor End");
    }

    [OnLoad]
    public static void Load()
    {
        Everest.Events.Level.OnEnter += Level_OnEnter;
    }

    private static void Level_OnEnter(Session session, bool fromSaveData)
    {
        string SID = session.Area.SID;
        if (hookRelevantSIDs.TryGetValue(SID, out var elements))
        {
            if (elements.Contains("FemtoHelper/MovementModifier"))
            {
                MovementModifier.Load();
            } 
            else
            {
                MovementModifier.Unload();
            }
        }
    }

    [OnUnload]
    public static void Unload()
    {
        Everest.Events.Level.OnEnter -= Level_OnEnter;
    }
}
