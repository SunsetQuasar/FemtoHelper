using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/SparkRefill")]
public class SparkRefill : CustomRefill
{
    public class SparkDash() : Component(false, false)
    {
        public bool CurrentlyDashing = false;

        public int Count = 1;
    }
    private static ParticleType _pShatter;

    private static ParticleType _pRegen;

    private static ParticleType _pGlow;

    public SparkRefill(Vector2 position, EntityData data)
        : base(position, false, data.Bool("oneUse", false))
    {
        RespawnTime = data.Float("respawnTime", 2.5f);
        this.SetTexture("objects/FemtoHelper/sparkRefill/")
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetCollectLogic((player) => !(player.Get<SparkDash>() is { } s && (!s.CurrentlyDashing || s.Count > 1)))
            .SetOnCollect(OnCollect);
    }
    public SparkRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {
    }
    private void OnCollect(Player player)
    {
        player.UseRefill(false);
        if (player.Get<SparkDash>() is SparkDash s)
        {
            s.Count++;
        }
        else
        {
            player.Add(new SparkDash());
        }
    }

    private static ILHook _dashCoroutineHook;
    private static ILHook _redDashCoroutineHook;

    private static void ModDashSpeed(ILContext il)
    {
        ILCursor cursor = new(il);

        // find 240f in the method (dash speed) and multiply it with our modifier.
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(240f)))
        {
            cursor.EmitLdloc1();
            cursor.EmitDelegate(GetMultiplier);
            cursor.EmitMul();
        }
    }

    private static float GetMultiplier(Player player)
    {
        if (player.Get<SparkDash>() is { } s) return s.CurrentlyDashing ? 2f : 1f;
        return 1f;
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);
        if (self.Get<SparkDash>() is { } s) s.CurrentlyDashing = true;
    }

    private static void Player_DashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        orig(self);
        if (self.Get<SparkDash>() is { } s && s.CurrentlyDashing)
        {
            if (--s.Count < 1)
            {
                s.RemoveSelf();
            }
        }
    }

    [OnLoadContent]
    public static void LoadContent(bool firstLoad)
    {
        _pShatter = new(P_Shatter)
        {
            Color = Calc.HexToColor("fff8bc"),
            Color2 = Calc.HexToColor("fff8bc")
        };

        _pRegen = new(P_Regen)
        {
            Color = Calc.HexToColor("bdb040"),
            Color2 = Calc.HexToColor("bdb040")
        };

        _pGlow = new(P_Glow)
        {
            Color = Calc.HexToColor("bdb040"),
            Color2 = Calc.HexToColor("bdb040")
        };
    }

    [OnLoad]
    public static void LoadHooks()
    {
        _dashCoroutineHook = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(), ModDashSpeed);
        _redDashCoroutineHook = new ILHook(typeof(Player).GetMethod("RedDashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(), ModDashSpeed);
        On.Celeste.Player.DashEnd += Player_DashEnd;
        On.Celeste.Player.DashBegin += Player_DashBegin;
    }

    [OnUnload]
    public static void UnloadHooks()
    {
        _dashCoroutineHook?.Dispose();
        _dashCoroutineHook = null;

        _redDashCoroutineHook?.Dispose();
        _redDashCoroutineHook = null;

        On.Celeste.Player.DashEnd -= Player_DashEnd;
        On.Celeste.Player.DashBegin -= Player_DashBegin;
    }
}
