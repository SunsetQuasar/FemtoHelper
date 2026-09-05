using Celeste.Mod.Helpers;
using Celeste.Mod.Registry;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Iced.Intel;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Celeste.Mod.FemtoHelper.Entities.SparkRefill;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/SlashRefill")]
public class SlashRefill : CustomRefill
{
    [Tracked]
    public class SlashDash() : Component(false, false)
    {
        public bool CurrentlyDashing = false;

        public int Count = 1;
    }
    [Tracked]
    public class Brittle() : Component(false, false)
    {
    }

    private readonly List<(Type, string)> targetTypesAndSIDs;

    private static ParticleType _pShatter;

    private static ParticleType _pRegen;
    private static ParticleType _pGlow;

    private readonly bool strict;

    public SlashRefill(EntityData data, Vector2 offset) : base(data.Position + offset, false, data.Bool("oneUse", false))
    {

        targetTypesAndSIDs = [.. data.String("brittleTypes", "refill,FemtoHelper/MinusRefill,MaxHelpingHand/CustomizableCrumblePlatform").
            Split(',').
            SelectMany<string, (Type, string)>(
                (str) => EntityRegistry.GetKnownTypesFromSid(str).Select(t => (t, str))
            )];

        AlwaysUse = true;

        this.SetTexture("objects/FemtoHelper/crashRefill/")
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetCollectLogic((player) => player.Get<SlashDash>() is not { } s || (s.CurrentlyDashing && s.Count <= 1))
            .SetOnCollect(OnCollect);

        strict = data.Bool("strictWhitelist", true);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (var (type, _) in targetTypesAndSIDs)
        {
            Tracker.AddTypeToTracker(type);
            Tracker.Refresh();
        }
    }

    public static void AddBrittle(Entity e)
    {
        if (e?.Get<Brittle>() is null)
        {
            e.Add(new Brittle());
        }
    }

    public static void RemoveBrittle(Entity e)
    {
        e?.Get<Brittle>()?.RemoveSelf();
    }

    public void OnCollect(Player player)
    {
        player.UseRefill(false);

        if (player.Get<SlashDash>() is SlashDash s)
        {
            s.Count++;
        }
        else
        {
            player.Add(new SlashDash());
        }

        // adding these modifies the tracker, which is a no-no within this method, since it's currently being called by the player iterating through PlayerColliders in the tracker
        // so we delay it until the end of the frame. maybe there's a better alternative? 
        player.Scene.OnEndOfFrame += () =>
        {
            foreach (var (type, str) in targetTypesAndSIDs)
            {
                List<Entity> l = Scene.Tracker.GetEntitiesTrackIfNeeded(type);
                foreach (Entity e in l)
                {
                    if (!strict || str.Equals(e.SourceData?.Name ?? "", StringComparison.InvariantCultureIgnoreCase))
                    {
                        AddBrittle(e);
                    }
                }
            }
        };
    }

    [OnLoadContent]

    public static void OnLoadSlashRefillContent(bool firstLoad)
    {
        _pShatter = new(P_Shatter)
        {
            Color = Calc.HexToColor("ffd9d3"),
            Color2 = Calc.HexToColor("fc9685")
        };

        _pRegen = new(P_Regen)
        {
            Color = Calc.HexToColor("ffa5a5"),
            Color2 = Calc.HexToColor("e06d7b")
        };

        _pGlow = new(P_Glow)
        {
            Color = Calc.HexToColor("ffa5a5"),
            Color2 = Calc.HexToColor("e06d7b")
        };
    }

    //shamelessly stolen from extended variant mode
    private static ILHook dashCoroutineHook;

    [OnLoad]
    public static void LoadHooks()
    {
        //On.Celeste.Player.DashBegin += modDashBegin; //i already hook it

        MethodInfo dashCoroutine = typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
        dashCoroutineHook = new ILHook(dashCoroutine, ModDashLength);

        IL.Celeste.Actor.MoveHExact += Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact += Actor_MoveVExact;
        On.Celeste.Player.DashEnd += Player_DashEnd;
        On.Celeste.Player.DashBegin += Player_DashBegin;
    }
    private static void Actor_MoveHExact(ILContext il)
    {
        ILCursor cursor = new(il);
        ILLabel skip_ret = cursor.DefineLabel();

        if (cursor.TryGotoNextBestFit(MoveType.After, instr => instr.MatchCallOrCallvirt<Entity>("CollideFirst"), instr => instr.MatchStloc3()))
        {
            cursor.EmitLdarg0(); // Actor self
            cursor.EmitLdloc1(); // num
            cursor.EmitLdloc2(); // num2
            cursor.EmitDelegate(InjectBrittleCheckH); // return whether to immediately return true from the MoveHExact method
            cursor.EmitBrfalse(skip_ret);
            cursor.EmitLdcI4(1);
            cursor.EmitRet();
            cursor.MarkLabel(skip_ret);
        }
    }

    private static bool InjectBrittleCheckH(Actor self, int num, int num2)
    {
        if (self is Player { DashAttacking: true, Dead: false } player && player.Get<SlashDash>() is { } slashDash)
        {
            Component brittle = player.CollideFirstByComponent<Brittle>(player.Position + Vector2.UnitX * num);
            if (brittle is { Entity: { } entity })
            {
                OnPlayerSlash(Vector2.UnitX * num, player, entity);
                player.Remove(slashDash);

                List<Entity> list = [.. player.Scene.Tracker.GetComponents<Brittle>().Select(brittle => brittle.Entity)];
                foreach (Entity e in list)
                {
                    if (e is not null)
                    {
                        RemoveBrittle(e);
                    }
                }
                return true;
            }
        }
        return false;
    }

    private static void Actor_MoveVExact(ILContext il)
    {
        ILCursor cursor = new(il);
        ILLabel skip_ret = cursor.DefineLabel();

        if (cursor.TryGotoNextBestFit(MoveType.After, instr => instr.MatchCallOrCallvirt<Entity>("CollideFirst"), instr => instr.MatchStloc3()))
        {
            cursor.EmitLdarg0(); // Actor self
            cursor.EmitLdloc1(); // num
            cursor.EmitLdloc2(); // num2
            cursor.EmitDelegate(InjectBrittleCheckV); // return whether to immediately return true from the MoveHExact method
            cursor.EmitBrfalse(skip_ret);
            cursor.EmitLdcI4(1);
            cursor.EmitRet();
            cursor.MarkLabel(skip_ret);
        }
    }

    private static bool InjectBrittleCheckV(Actor self, int num, int num2)
    {
        if (self is Player { DashAttacking: true, Dead: false } player && player.Get<SlashDash>() is { } slashDash)
        {
            Component brittle = player.CollideFirstByComponent<Brittle>(player.Position + Vector2.UnitY * num);
            if (brittle is { Entity: { } entity })
            {
                OnPlayerSlash(Vector2.UnitY * num, player, entity);
                player.Remove(slashDash);

                List<Entity> list = [.. player.Scene.Tracker.GetComponents<Brittle>().Select(brittle => brittle.Entity)];
                foreach (Entity e in list)
                {
                    if (e is not null)
                    {
                        RemoveBrittle(e);
                    }
                }
                return true;
            }
        }
        return false;
    }

    public static void OnPlayerSlash(Vector2 dir, Player player, Entity entity)
    {
        player.Rebound(-Math.Sign(dir.X));
        Audio.Play("event:/game/general/wall_break_ice", player.Position);
        int rows = (int)entity.Height >> 3;
        int cols = (int)entity.Width >> 3;

        float chance = 1f / MathF.Max((rows * cols) >> 3, 1);

        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (!Calc.Random.Chance(chance)) continue;
                player.Scene.Add(Engine.Pooler.Create<Debris>().Init(entity.TopLeft + new Vector2(4 + x * 8, 4 + y * 8), '3', true).BlastFrom(player.Center));
            }
        }

        entity.RemoveSelf();
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);

        if (self.Get<SlashDash>() is { } s)
        {
            bool superDash = SaveData.Instance.Assists.SuperDashing;

            const float factor = 1.5f;

            self.dashAttackTimer = (superDash ? 0.3f : 0.15f) * factor + 0.15f;
            self.gliderBoostTimer = (superDash ? 0.3f : 0.15f) * factor + (superDash ? 0.25f : 0.4f);

            s.CurrentlyDashing = true;
        }
    }

    private static void Player_DashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        orig(self);
        if (self.Get<SlashDash>() is { } s && s.CurrentlyDashing)
        {
            s.CurrentlyDashing = false;
            if (--s.Count < 1)
            {
                s.RemoveSelf();
            }
        }
    }

    private static void ModDashLength(ILContext il)
    {
        ILCursor cursor = new(il);

        // jump where 0.3 or 0.15f are loaded (those are dash times)
        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.3f) || instr.MatchLdcR4(0.15f)))
        {
            Logger.Log("ExtendedVariantMode/DashLength", $"Applying dash length to constant at {cursor.Index} in CIL code for {cursor.Method.FullName}");

            cursor.EmitLdloc1();
            cursor.EmitDelegate(DetermineDashLengthFactor);
            cursor.Emit(OpCodes.Mul);
        }

        //don't hook twice, set index to zero (smart)
        cursor.Index = 0;

        // jump wherever dashTrailCounter is saved
        while (cursor.TryGotoNext(instr => instr.MatchStfld<Player>("dashTrailCounter")))
        {
            Logger.Log("ExtendedVariantMode/DashLength", $"Modding dash trail counter at {cursor.Index} in CIL code for {cursor.Method.FullName}");

            cursor.EmitLdloc1();
            cursor.EmitDelegate(ApplyDashTrailCounter);

            // don't forget to move forward in order to avoid infinite loops!
            cursor.Index++;
        }
    }

    private static float DetermineDashLengthFactor(Player player)
    {
        return player.Get<SlashDash>() != null ? 1.5f : 1f;
    }

    private static int ApplyDashTrailCounter(int dashTrailCounter, Player player)
    {
        if (player.Get<SlashDash>() != null)
        {
            float lastDashDuration = SaveData.Instance.Assists.SuperDashing ? 0.3f : 0.15f;
            return (int)Math.Round(lastDashDuration * 1.5f) - 1;
        }
        return dashTrailCounter;
    }

    [OnUnload]
    public static void UnloadHooks()
    {
        dashCoroutineHook?.Dispose();
        dashCoroutineHook = null;

        IL.Celeste.Actor.MoveHExact -= Actor_MoveHExact;
        IL.Celeste.Actor.MoveVExact -= Actor_MoveVExact;
        On.Celeste.Player.DashEnd -= Player_DashEnd;
        On.Celeste.Player.DashBegin -= Player_DashBegin;
    }
}
