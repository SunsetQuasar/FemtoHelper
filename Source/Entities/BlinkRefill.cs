using Celeste.Mod.Registry;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.FemtoHelper.Entities.BlinkRefill;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/BlinkRefill")]
public class BlinkRefill : CustomRefill
{
    public class BlinkDash() : DashEffect()
    {
        internal List<(Type, string)> inactiveTypesAndSIDs = [];
        internal List<(Type, string)> uncollidableTypesAndSIDs = [];
        internal bool? strict;

        public override void OnDashBegin(Player player)
        {
            base.OnDashBegin(player);

            (player.Scene as Level).Session.SetFlag("blink_refill", true);
            Entity flagHelper = [];
            player.Scene.Add(flagHelper);
            Alarm.Set(flagHelper, 0.5f, () =>
            {
                (flagHelper.Scene as Level).Session.SetFlag("blink_refill", false);
                flagHelper.RemoveSelf();
            });

            foreach (var (type, str) in inactiveTypesAndSIDs)
            {
                List<Entity> l = player.Scene.Tracker.GetEntitiesTrackIfNeeded(type);
                foreach (Entity e in l)
                {
                    if (!(strict ?? false) || str.Equals(e.SourceData?.Name ?? "", StringComparison.InvariantCultureIgnoreCase))
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

            foreach (var (type, str) in uncollidableTypesAndSIDs)
            {
                List<Entity> l = player.Scene.Tracker.GetEntitiesTrackIfNeeded(type);
                foreach (Entity e in l)
                {
                    if (!(strict ?? false) || str.Equals(e.SourceData?.Name ?? "", StringComparison.InvariantCultureIgnoreCase))
                    {
                        bool prev = e.Collidable;
                        e.Collidable = false;

                        Entity helper = [];
                        e.Scene.Add(helper);

                        Alarm.Set(helper, 0.5f, () => {
                            e?.Collidable |= prev;
                            helper.RemoveSelf();
                        });
                    }
                }
            }

            if ((ReadyCount - 1) == 0)
            {
                inactiveTypesAndSIDs.Clear();
                uncollidableTypesAndSIDs.Clear();
                strict = null;
            }

        }

        public override void OnDashEnd(Player player)
        {
            base.OnDashEnd(player);
        }
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

    public readonly bool StrictDashCheck;

    public BlinkRefill(EntityData data, Vector2 offset) : base(data.Position + offset, false, data.Bool("oneUse", false), data.String("pulseFlag", ""))
    {
        inactiveTypesAndSIDs = [.. data.String("inactiveTypes", "crumbleBlock")
            .Split(',')
            .SelectMany<string, (Type, string)>(
                (str) => EntityRegistry.GetKnownTypesFromSid(str).Select(t => (t, str))
            )];

        uncollidableTypesAndSIDs = [.. data.String("uncollidableTypes", "refill,FemtoHelper/MinusRefill")
            .Split(',')
            .SelectMany<string, (Type, string)>(
                (str) => EntityRegistry.GetKnownTypesFromSid(str).Select(t => (t, str))
            )];

        strict = data.Bool("strictWhitelist", true);

        RefillDash = RefillStamina = AlwaysUse = true;

        RespawnTime = data.Float("respawnTime", 2.5f);
        VisualOffset = data.Vector2("visualOffsetX", "visualOffsetY", Vector2.Zero);

        this.SetTexture("objects/FemtoHelper/blinkRefill/")
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetCollectLogic((player) => DashEffect.ShouldUseRefill<BlinkDash>(player, 1))
            .SetOnCollect(OnCollect);

        StrictDashCheck = data.Bool("strictDashCheck", false);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (var (type, _) in inactiveTypesAndSIDs.Union(uncollidableTypesAndSIDs))
        {
            Tracker.AddTypeToTracker(type);
        }
        Tracker.Refresh();

        (scene as Level).Session.SetFlag("blink_refill", false);
    }

    private void OnCollect(Player player)
    {
        player.UseRefill(false);

        BlinkDash blinkDash = DashEffect.GiveToPlayer<BlinkDash>(player, 1, StrictDashCheck);
        blinkDash.inactiveTypesAndSIDs = [.. blinkDash.inactiveTypesAndSIDs.Union(inactiveTypesAndSIDs)];
        blinkDash.uncollidableTypesAndSIDs = [.. blinkDash.inactiveTypesAndSIDs.Union(uncollidableTypesAndSIDs)];

        blinkDash.strict ??= strict;
    }

    [OnLoad]
    public static void LoadHooks()
    {

    }


    [OnUnload]
    public static void UnloadHooks()
    {

    }
}
