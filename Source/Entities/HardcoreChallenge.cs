using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Celeste.Mod.FemtoHelper.Entities;

public class HardcoreChallenge
{
    public string levelSet { get; set; }
    public bool ignoreGoldens { get; set; }

    public Dictionary<string, HashSet<string>> collectables { get; set; } = [];
    //whether you died in any level in the level set
    public Dictionary<string, int> deaths { get; set; } = [];
    public Dictionary<string, bool> completedArea { get; set; } = [];
    [YamlIgnore]
    public bool Failed => deaths.Any(kvp => kvp.Value > 0);
    [YamlIgnore]
    public int Total => collectables.Sum(kvp => kvp.Value.Count);
    [YamlIgnore]
    public int CompletedCount => completedArea.Count;
    
    public bool CompletedAllOf(string sids)
    {
        return !sids.Split(',').Any(i => !(completedArea.TryGetValue(i, out var area) && area));
    }

    public int TotalOf(string sids)
    {
        return sids.Split(',').Sum(i => collectables.TryGetValue(i, out var set) ? set.Count : 0);
    }

    public static void AddCollectable<T>(T collectable) where T : Entity
    {
        if (FemtoModule.SaveData is null || FemtoModule.SaveData.HardcoreChallenges is null) return;
        string levelSet = (collectable.Scene as Level).Session.Area.LevelSet;

        if (FemtoModule.SaveData.HardcoreChallenges.TryGetValue(levelSet, out var challenge))
        {
            if (collectable is Strawberry s && (s.Golden) && challenge.ignoreGoldens) return;
            string key = collectable.SourceId.ToString();
            if (key == default(EntityID).Key)
            {
                Warn($"HardcoreChallenge ({levelSet}): tried to collect an entity with a default entity ID! Allowing it, but it means any other ones with the same ID won't count!");
            }

            string sid = (collectable.Scene as Level).Session.Area.SID;

            if (challenge.collectables.TryGetValue(sid, out var set))
            {
                set.Add(key);
            }
            else
            {
                challenge.collectables.Add(sid, [key]);
            }

            if(collectable.Scene.Tracker.GetEntity<QueryHardcoreChallenge>() is { } q)
            {
                q.Query();
            }
        }
    }

    public static void CompleteArea()
    {
        if (Engine.Scene is Level level && FemtoModule.SaveData.GetCurrentChallenge() is HardcoreChallenge challenge)
        {
            string sid = level.Session.Area.SID;
            if (challenge.completedArea.TryGetValue(sid, out var _))
            {
                challenge.completedArea[sid] = true;
            }
        }
    }

    public HardcoreChallenge()
    {

    }

    public static void FailCurrent()
    {
        if (Engine.Scene is Level level && FemtoModule.SaveData.GetCurrentChallenge() is HardcoreChallenge challenge)
        {
            string sid = level.Session.Area.SID;
            if (challenge.deaths.TryGetValue(sid, out var deaths))
            {
                challenge.deaths[sid] = ++deaths;
            }
        }
    }

    public HardcoreChallenge(EntityData data) : this()
    {
        levelSet = data.String("levelSet", null);
        ignoreGoldens = data.Bool("ignoreGoldens", true);

        if (FemtoModule.SaveData.HardcoreChallenges is null)
        {
            Warn("FemtoModule.SaveData.HardcoreChallenges was null during a HardcoreChallenge ctor!");
            return;
        }

        if (FemtoModule.SaveData.HardcoreChallenges.TryGetValue(levelSet, out var _))
        {
            FemtoModule.SaveData.HardcoreChallenges[levelSet] = this;
        }
        else
        {
            FemtoModule.SaveData.HardcoreChallenges.Add(levelSet, this);
        }

        //populate dictionary
        foreach (AreaKey key in GetFromLevelSet(levelSet))
        {
            collectables.Add(key.SID, []);
            deaths.Add(key.SID, 0);
            completedArea.Add(key.SID, false);
        }
    }

    [OnLoad]
    public static void Load()
    {
        On.Celeste.Strawberry.OnCollect += Strawberry_OnCollect;
        On.Celeste.HeartGem.RegisterAsCollected += HeartGem_RegisterAsCollected;
        On.Celeste.Cassette.OnPlayer += Cassette_OnPlayer;
        On.Celeste.Player.Die += Player_Die;
        On.Celeste.Level.CompleteArea_bool_bool_bool += Level_CompleteArea_bool_bool_bool;
    }

    private static ScreenWipe Level_CompleteArea_bool_bool_bool(On.Celeste.Level.orig_CompleteArea_bool_bool_bool orig, Level self, bool spotlightWipe, bool skipScreenWipe, bool skipCompleteScreen)
    {
        CompleteArea();
        return orig(self, spotlightWipe, skipScreenWipe, skipCompleteScreen);
    }

    private static PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        var @return = orig(self, direction, evenIfInvincible, registerDeathInStats);
        if(@return != null) //i.e. the player *actually* died
        {
            self.Scene.OnEndOfFrame += () =>
            {
                if(self == null || self.Dead)
                {
                    FailCurrent();
                }
            };
        }
        return @return;
    }

    private static void Cassette_OnPlayer(On.Celeste.Cassette.orig_OnPlayer orig, Cassette self, Player player)
    {
        if (!self.collected) AddCollectable(self);
        orig(self, player);
    }

    private static void HeartGem_RegisterAsCollected(On.Celeste.HeartGem.orig_RegisterAsCollected orig, HeartGem self, Level level, string poemID)
    {
        orig(self, level, poemID);
        AddCollectable(self);
    }

    private static void Strawberry_OnCollect(On.Celeste.Strawberry.orig_OnCollect orig, Strawberry self)
    {
        orig(self);
        AddCollectable(self);
    }

    [OnUnload]
    public static void Unload()
    {
        On.Celeste.Strawberry.OnCollect -= Strawberry_OnCollect;
        On.Celeste.HeartGem.RegisterAsCollected -= HeartGem_RegisterAsCollected;
        On.Celeste.Cassette.OnPlayer -= Cassette_OnPlayer;
        On.Celeste.Player.Die -= Player_Die;
        On.Celeste.Level.CompleteArea_bool_bool_bool -= Level_CompleteArea_bool_bool_bool;
    }
}
[Tracked]
[CustomEntity("FemtoHelper/SetupHardcoreChallenge")]
public class SetupHardcoreChallenge(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{
    readonly EntityData data = data;

    public override void Awake(Scene scene)
    {
        string levelSet = data.String("levelSet", null);
        ArgumentNullException.ThrowIfNull(levelSet);

        //challenge already exists
        if (FemtoModule.SaveData.HardcoreChallenges.TryGetValue(levelSet, out var ch))
        {
            if (ch.Failed || ch.levelSet != levelSet)
            {
                Debug($"restarted current HardcoreChallenge: {levelSet}");
                _ = new HardcoreChallenge(data);
            }
        }
        else
        {
            Debug($"started a new HardcoreChallenge: {levelSet}");
            _ = new HardcoreChallenge(data);
        }
        base.Awake(scene);
    }

    public override void Render()
    {
        base.Render();
#if DEBUG
        Draw.Rect(Position, 8, 8, Color.LightSalmon);
#endif
    }
}
[Tracked]
[CustomEntity("FemtoHelper/QueryHardcoreChallenge")]
public class QueryHardcoreChallenge(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{
    readonly string sids = data.String("sids", null);
    readonly string levelCompleteSids = data.String("levelCompleteSids", null);
    readonly int requiredAmount = data.Int("requires", 0);
    readonly string flag = data.String("setFlag", "hardcoreChallenge_complete");

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        Query();
    }

    public void Query()
    {
        if (FemtoModule.SaveData.GetCurrentChallenge() is HardcoreChallenge challenge)
        {
            if (!challenge.Failed && challenge.TotalOf(sids) >= requiredAmount && challenge.CompletedAllOf(levelCompleteSids))
            {
                (Scene as Level).Session.SetFlag(flag);
            }
        }
    }

    public override void Render()
    {
        base.Render();
#if DEBUG
        Draw.Rect(Position, 8, 8, Color.Aquamarine);
#endif
    }
}