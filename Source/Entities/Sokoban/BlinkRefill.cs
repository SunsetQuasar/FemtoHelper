using Celeste.Mod.Registry;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.FemtoHelper.Entities.SlashRefill;

namespace Celeste.Mod.FemtoHelper.Entities.Sokoban;

[CustomEntity("FemtoHelper/BlinkRefill")]
public class BlinkRefill : CustomRefill
{
    public class BlinkDash(List<(Type, string)> inactive, List<(Type, string)> uncollidable, bool strict) : Component(false, false)
    {
        internal List<(Type, string)> inactiveTypesAndSIDs = inactive;
        internal List<(Type, string)> uncollidableTypesAndSIDs = uncollidable;
        internal readonly bool strict = strict;
        public bool CurrentlyDashing;
        public int Count;
    }

    private static readonly ParticleType _pShatter = new(P_Shatter)
    {
        Color = Calc.HexToColor("d3ffef"),
        Color2 = Calc.HexToColor("85fce5")
    };

    private static readonly ParticleType _pRegen = new(P_Regen)
    {
        Color = Calc.HexToColor("a5eeff"),
        Color2 = Calc.HexToColor("6dbfe0")
    };

    private static readonly ParticleType _pGlow = new(P_Glow)
    {
        Color = Calc.HexToColor("a5eeff"),
        Color2 = Calc.HexToColor("6dbfe0")
    };
    private readonly List<(Type, string)> inactiveTypesAndSIDs;
    private readonly List<(Type, string)> uncollidableTypesAndSIDs;
    private readonly bool strict;

    public BlinkRefill(EntityData data, Vector2 offset) : base(data.Position + offset, false, data.Bool("oneUse", false))
    {
        inactiveTypesAndSIDs = [.. data.String("inactiveTypes", "MaxHelpingHand/CustomizableCrumblePlatform").
            Split(',').
            SelectMany<string, (Type, string)>(
                (str) => EntityRegistry.GetKnownTypesFromSid(str).Select(t => (t, str))
            )];

        uncollidableTypesAndSIDs = [.. data.String("uncollidableTypes", "refill,FemtoHelper/MinusRefill").
            Split(',').
            SelectMany<string, (Type, string)>(
                (str) => EntityRegistry.GetKnownTypesFromSid(str).Select(t => (t, str))
            )];

        strict = data.Bool("strictWhitelist", true);

        RefillDash = RefillStamina = AlwaysUse = true;

        this.SetTexture("objects/FemtoHelper/blinkRefill/")
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetCollectLogic((player) => player.Get<BlinkDash>() is not { } s || (s.CurrentlyDashing && s.Count <= 1))
            .SetOnCollect(OnCollect);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (var (type, _) in inactiveTypesAndSIDs.Union(uncollidableTypesAndSIDs))
        {
            Tracker.AddTypeToTracker(type);
            Tracker.Refresh();
        }

        (scene as Level).Session.SetFlag("blink_refill", false);
    }

    private void OnCollect(Player player)
    {
        player.UseRefill(false);

        if (player.Get<BlinkDash>() is BlinkDash blinkDash)
        {
            blinkDash.Count++;
            blinkDash.inactiveTypesAndSIDs = [.. blinkDash.inactiveTypesAndSIDs.Union(inactiveTypesAndSIDs)];
            blinkDash.uncollidableTypesAndSIDs = [.. blinkDash.inactiveTypesAndSIDs.Union(uncollidableTypesAndSIDs)];
        }
        else
        {
            player.Add(new BlinkDash(inactiveTypesAndSIDs, uncollidableTypesAndSIDs, strict));
        }
    }

    [OnLoad]
    public static void LoadHooks()
    {

        On.Celeste.Player.DashEnd += Player_DashEnd;
        On.Celeste.Player.DashBegin += Player_DashBegin;
    }

    private static void Player_DashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        orig(self);

        if (self.Get<BlinkDash>() is { } blinkDash && blinkDash.CurrentlyDashing)
        {
            blinkDash.CurrentlyDashing = false;
            if (--blinkDash.Count < 1)
            {
                blinkDash.RemoveSelf();
            }
        }
    }
    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);

        if (self.Get<BlinkDash>() is { } blinkDash)
        {
            blinkDash.CurrentlyDashing = true;
            (self.Scene as Level).Session.SetFlag("blink_refill", true);
            Entity flagHelper = [];
            self.Scene.Add(flagHelper);
            Alarm.Set(flagHelper, 0.5f, () =>
            {
                (flagHelper.Scene as Level).Session.SetFlag("blink_refill", false);
                flagHelper.RemoveSelf();
            });

            foreach (var (type, str) in blinkDash.inactiveTypesAndSIDs)
            {
                List<Entity> l = self.Scene.Tracker.GetEntitiesTrackIfNeeded(type);
                foreach (Entity e in l)
                {
                    if (!blinkDash.strict || str.Equals(e.SourceData?.Name ?? "", StringComparison.InvariantCultureIgnoreCase))
                    {
                        bool prev = e.Active;
                        e.Active = false;
                        Entity helper = [];
                        e.Scene.Add(helper);

                        Alarm.Set(helper, 0.5f, () =>
                        {
                            e?.Active |= prev;
                            helper.RemoveSelf();
                        });
                    }
                }
            }

            foreach (var (type, str) in blinkDash.uncollidableTypesAndSIDs)
            {
                List<Entity> l = self.Scene.Tracker.GetEntitiesTrackIfNeeded(type);
                foreach (Entity e in l)
                {
                    if (!blinkDash.strict || str.Equals(e.SourceData?.Name ?? "", StringComparison.InvariantCultureIgnoreCase))
                    {
                        bool prev = e.Collidable;
                        e.Collidable = false;
                        Alarm.Set(e, 0.5f, () => e.Collidable |= prev);
                    }
                }
            }
        }
    }

    [OnUnload]
    public static void UnloadHooks()
    {

        On.Celeste.Player.DashEnd -= Player_DashEnd;
        On.Celeste.Player.DashBegin -= Player_DashBegin;
    }
}
