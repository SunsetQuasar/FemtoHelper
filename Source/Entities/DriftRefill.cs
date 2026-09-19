using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.FemtoHelper.Entities.SparkRefill;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/DriftRefill")]
public class DriftRefill : CustomRefill
{
    public class DriftDash : Component
    {
        public bool CurrentlyDashing = false;
        private int count;
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                if (value > count)
                {
                    Dying = false;
                    applyGooberTimer = 0f;
                }
                count = value;
            }
        }
        public bool Dying = false;
        private float applyGooberTimer = 0;
        public bool ApplyGoober = false;

        public DriftDash() : base(true, false)
        {
            Count = 1;
        }

        public override void Update()
        {
            base.Update();
            applyGooberTimer = Calc.Approach(applyGooberTimer, 0.1f, Engine.DeltaTime);
            if (applyGooberTimer >= 0.1f)
            {
                if (Dying)
                {
                    RemoveSelf();
                }
                else
                {
                    ApplyGoober = true;
                }
            }
        }

        public void Die()
        {
            Dying = true;
            applyGooberTimer = 0f;
        }
    }

    private static ParticleType _pShatter;

    private static ParticleType _pRegen;

    private static ParticleType _pGlow;

    public DriftRefill(EntityData data, Vector2 offset)
       : base(data.Position + offset, false, data.Bool("oneUse", false))
    {
        RespawnTime = data.Float("respawnTime", 2.5f);

        this.SetTexture("objects/FemtoHelper/holdRefill/")
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetCollectLogic((player) => player.Get<DriftDash>() is not { } driftDash || (driftDash.CurrentlyDashing && driftDash.Count <= 1))
            .SetOnCollect(OnCollect);

        VisualOffset = data.Vector2("visualOffsetX", "visualOffsetY", Vector2.Zero);
    }
    private void OnCollect(Player player)
    {
        player.UseRefill(false);
        if (player.Get<DriftDash>() is DriftDash driftDash)
        {
            driftDash.Count++;
        }
        else
        {
            player.Add(new DriftDash());
        }
    }

    [OnLoadContent]
    public static void LoadContent(bool firstLoad)
    {
        _pShatter = new(P_Shatter)
        {
            Color = Calc.HexToColor("fff8d3"),
            Color2 = Calc.HexToColor("fcd385")
        };

        _pRegen = new(P_Regen)
        {
            Color = Calc.HexToColor("ffd7a5"),
            Color2 = Calc.HexToColor("e09a6d")
        };

        _pGlow = new(P_Glow)
        {
            Color = Calc.HexToColor("ffd7a5"),
            Color2 = Calc.HexToColor("e09a6d")
        };
    }

    private static ILHook _dashCoroutineHook;

    [OnLoad]
    public static void LoadHooks()
    {
        _dashCoroutineHook = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(), ModSpeedReset);
        On.Celeste.Player.DashCoroutine += Player_DashCoroutine;
        On.Celeste.Player.DashUpdate += Player_DashUpdate;

        On.Celeste.Player.DashBegin += Player_DashBegin;
        On.Celeste.Player.DashEnd += Player_DashEnd;

        On.Celeste.Level.Update += Level_Update;
    }

    private static int Player_DashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self)
    {
        if (self.Get<DriftDash>() is { CurrentlyDashing: true })
        {
            self.dashTrailCounter = 2;
        }
        return orig(self);
    }

    private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        if (self.Tracker.GetEntity<Player>() is Player player && player.Get<DriftDash>() is not null)
        {
            bool prev = SaveData.Instance.Assists.SuperDashing;
            try
            {
                SaveData.Instance.Assists.SuperDashing = true;
                orig(self);
            } 
            finally
            {
                SaveData.Instance.Assists.SuperDashing = prev;
            }
        } 
        else
        {
            orig(self);
        }
    }

    private static void ModSpeedReset(ILContext il)
    {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdloc1(),
            instr => instr.MatchLdflda<Player>("DashDir") || instr.MatchLdflda<Player>("Speed"), //kill two birds with one stone
            instr => instr.MatchLdfld<Vector2>("Y")
            ))
        {
            cursor.EmitLdloc1();
            cursor.EmitDelegate(GaslightTheIfStatement);
        }
    }

    private static float GaslightTheIfStatement(float orig, Player player)
    {
        if (player.Get<DriftDash>() is { ApplyGoober: true })
        {
            //trick the code into thinking we have insane downwards speed so it can never think we shouldn't keep speed after a dash ends
            return int.MaxValue;
        }
        return orig;
    }

    // modified from ExtendedVariantMode
    private static IEnumerator Player_DashCoroutine(On.Celeste.Player.orig_DashCoroutine orig, Player self)
    {
        IEnumerator routine = orig(self).SafeEnumerate();
        while (routine.MoveNext())
        {
            object o = routine.Current;
            if (o != null && o.GetType() == typeof(float))
            {
                yield return o;
                while (self.Get<DriftDash>() is not null && (Input.Dash.Check || Input.CrouchDash.Check))
                {
                    self.dashAttackTimer = 0.15f; // hold the dash attack timer to continue breaking dash blocks and such.
                    self.gliderBoostTimer = 0.30f; // hold the glider boost timer to still get boosted by jellies.

                    yield return null;
                }
            }
            else
            {
                yield return o;
            }
        }

        yield break;
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);
        if (self.Get<DriftDash>() is { } driftDash) driftDash.CurrentlyDashing = true;
    }

    private static void Player_DashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        orig(self);
        if (self.Get<DriftDash>() is { } driftDash && driftDash.CurrentlyDashing && !driftDash.Dying)
        {
            driftDash.CurrentlyDashing = false;
            if (--driftDash.Count < 1)
            {
                driftDash.Die();
            }
        }
    }

    [OnUnload]
    public static void UnloadHooks()
    {
        _dashCoroutineHook?.Dispose();
        _dashCoroutineHook = null;
        On.Celeste.Player.DashCoroutine -= Player_DashCoroutine;
        On.Celeste.Player.DashUpdate -= Player_DashUpdate;

        On.Celeste.Player.DashBegin -= Player_DashBegin;
        On.Celeste.Player.DashEnd -= Player_DashEnd;

        On.Celeste.Level.Update -= Level_Update;
    }
}
