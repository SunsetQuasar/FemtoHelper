using Celeste.Mod.FemtoHelper.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.FemtoHelper.Metadata;

public static class FemtoHelperMetadata
{
    public static List<SessionHeartDefinition> SessionHeartMeta = [];

    public static Dictionary<string, SessionHeartDefinition> SessionHeartMetaByGroupName = [];

    public static List<string> SessionHeartGroupNames = [];

    public static bool HasSessionHearts => SessionHeartMeta is { } && SessionHeartMeta.Count > 0;

    public static List<SessionHeartDefinition> GetSessionHearts(Session session)
    {
        string name = "Maps/" + session.MapData.Filename + ".meta";
        if (!Everest.Content.TryGet(name, out ModAsset metadata))
        {
            return null;
        }
        if (metadata?.PathVirtual?.StartsWith("Maps") != true)
        {
            return null;
        }
        if (metadata == null || !metadata.TryDeserialize<SessionHeartMetadata>(out var result))
        {
            Log($"Failed to deserialize SessionHearts in {name}.yaml! Please check for syntax issues.", LogLevel.Error);
            return null;
        }
        List<SessionHeartDefinition> list = result?.SessionHeartGroups;
        if (list == null)
        {
            return null;
        }
        return list;
    }

    [OnLoad]
    public static void Load()
    {
        On.Celeste.LevelLoader.LoadingThread += OnLoadLevel;
    }

    public static void OnLoadLevel(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
    {
        SessionHeartMeta = GetSessionHearts(self.session);

        if(SessionHeartMeta != null)
        {
            SessionHeartMetaByGroupName = new(SessionHeartMeta.Select(def => new KeyValuePair<string, SessionHeartDefinition>(def.Name, def)));

            SessionHeartGroupNames = [.. SessionHeartMeta.Select(def => def.Name)];
        }

        orig(self);

        if (SessionHeartMeta != null && SessionHeartMeta.Count != 0)
        {
            self.Level.Add(new SessionHeartDisplay());
        }
    }

    [OnUnload]
    public static void Unload()
    {
        On.Celeste.LevelLoader.LoadingThread -= OnLoadLevel;
    }
}
