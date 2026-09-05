using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.FemtoHelper.Entities.SlashRefill;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/FloatRefill")]
public class FloatRefill : CustomRefill
{
    public class FloatDash() : Component(true, false)
    {
        public bool CurrentlyDashing = false;
        public int Count = 1;

        public override void Update()
        {
            base.Update();
            if (Entity is Player player && Scene is Level level && player.Top < level.Bounds.Top - 16)
            {
                player.Die(Vector2.Zero);
            }
        }
    }

    private static ParticleType _pShatter;
    private static ParticleType _pRegen;
    private static ParticleType _pGlow;

    public FloatRefill(EntityData data, Vector2 offset) : base(data.Position + offset, false, data.Bool("oneUse", false))
    {
        RefillDash = false;
        RefillStamina = true;

        this.SetTexture("objects/FemtoHelper/floatRefill/")
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetCollectLogic((player) => player.Get<FloatDash>() is not { } s || (s.CurrentlyDashing && s.Count <= 1))
            .SetOnCollect(OnCollect);
    }
    public void OnCollect(Player player)
    {
        if (player.Get<FloatDash>() is FloatDash s)
        {
            s.Count++;
        }
        else
        {
            player.Add(new FloatDash());
        }
    }

    [OnLoadContent]
    public static void OnLoadFloatRefillContent(bool firstLoad)
    {
        _pShatter = new(P_Shatter)
        {
            Color = Calc.HexToColor("d3ffff"),
            Color2 = Calc.HexToColor("a8fbed")
        };

        _pRegen = new(P_Regen)
        {
            Color = Calc.HexToColor("c8f6ff"),
            Color2 = Calc.HexToColor("9cdbe5")
        };

        _pGlow = new(P_Glow)
        {
            Color = Calc.HexToColor("c8f6ff"),
            Color2 = Calc.HexToColor("9cdbe5")
        };
    }

    [OnLoad]
    public static void LoadHooks()
    {
        using (new DetourConfigContext(new DetourConfig("FemtoHelper_AfterAll").WithPriority(int.MaxValue)).Use())
        {
            IL.Celeste.Player.NormalUpdate += ModNormalUpdate;
        }

        On.Celeste.Player.DashEnd += Player_DashEnd;
        On.Celeste.Player.DashBegin += Player_DashBegin;
    }

    private static void ModNormalUpdate(ILContext il)
    {
        ILCursor cursor = new(il);

        // find out where the constant 900 (downward acceleration) is loaded into the stack
        while (cursor.TryGotoNextBestFit(
                MoveType.Before,
                instr => instr.MatchLdloc(11),
                instr => instr.MatchLdcR4(900f)))
        {
            cursor.Index++;
            while (cursor.Next.MatchDup()) cursor.Index++;
            //we should be duping either the last dup or ldloc11 (player target y speed)
            cursor.EmitDup();

            //don't assume ldc.r4 900 is the next instruction
            cursor.GotoNext(MoveType.After, instr => instr.MatchLdcR4(900f));

            cursor.EmitLdarg0();
            cursor.EmitDelegate(ApplyGravityMultiplier);
        }

    }

    private static float ApplyGravityMultiplier(float target, float gravity, Player player)
    {
        if (!(player.Get<FloatDash>() is { } _))
        {
            return gravity;
        }

        float gravityMultiplier = -0.65f;

        if (player.Speed.Y > target)
        {
            // if going faster than the target speed, do not invert gravity to avoid speeding Maddy up,
            // since the gravity is supposed to be negative and all that.
            return gravity * Math.Abs(gravityMultiplier);
        }

        return gravity * gravityMultiplier;
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);

        if (self.Get<FloatDash>() is { } floatDash)
        {
            floatDash.CurrentlyDashing = true;
        }
    }

    private static void Player_DashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        orig(self);

        if (self.Get<FloatDash>() is { } floatDash && floatDash.CurrentlyDashing)
        {
            floatDash.CurrentlyDashing = false;
            if (--floatDash.Count < 1)
            {
                floatDash.RemoveSelf();
            }
        }
    }

    [OnUnload]
    public static void UnloadHooks()
    {
        IL.Celeste.Player.NormalUpdate -= ModNormalUpdate;
        On.Celeste.Player.DashEnd -= Player_DashEnd;
        On.Celeste.Player.DashBegin -= Player_DashBegin;
    }
}
