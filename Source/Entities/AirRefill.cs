using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/AirRefill")]
public class AirRefill : CustomRefill
{
    private static ParticleType _pShatter;
    private static ParticleType _pRegen;
    private static ParticleType _pGlow;

    private static ParticleType _pBubbly;

    public class AirDash() : Component(true, false)
    {
        public int Count;

        public override void Update()
        {
            base.Update();
            if (Entity is Player player)
            {
                if (Scene.OnInterval(0.1f) && Calc.Random.Chance(0.75f))
                {
                    (Scene as Level).ParticlesBG.Emit(_pBubbly, 2, player.Center, new Vector2(4, 4));
                }
            }
        }
    }

    public AirRefill(EntityData data, Vector2 offset) : base(data.Position + offset, false, data.Bool("oneUse", false))
    {
        RefillDash = RefillStamina = AlwaysUse = true;

        this.SetTexture("objects/FemtoHelper/airRefill/")
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetCollectLogic((player) => player.Get<AirDash>() is not { } s)
            .SetOnCollect(OnCollect);
    }

    public void OnCollect(Player player)
    {
        player.UseRefill(false);

        if (player.Get<AirDash>() is AirDash airDash)
        {
            airDash.Count++;
        }
        else
        {
            player.Add(new AirDash());
        }
    }

    [OnLoad]
    public static void LoadHooks()
    {
        IL.Celeste.Player.DashUpdate += Player_DashUpdate;
        IL.Celeste.Player.RedDashUpdate += Player_DashUpdate;
        IL.Celeste.Player.NormalUpdate += Player_NormalUpdate;
    }

    private static void Player_NormalUpdate(ILContext il)
    {
        ILCursor cursor = new(il);

        // wallless wallbounce
        while (cursor.TryGotoNextBestFit(
            MoveType.After,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdcI4(-1),
            instr => instr.MatchCallOrCallvirt<Player>("WallJumpCheck")
            ))
        {
            // just recieved the result of player.WallJumpCheck(-1)
            // we dupe the result so we can consume it and keep one for the code
            // if it's false, then we run the "else if (wallless wallbounce)... etc" part
            cursor.EmitDup();
            cursor.EmitLdarg0();
            cursor.EmitDelegate(CheckForWalllessWallbounce);
            // regardless of if we wallbounce or not, no need to change states since it's already StNormal, and we can pop the result
            cursor.EmitPop();
        }
    }

    private static void Player_DashUpdate(ILContext il)
    {
        ILCursor cursor = new(il);
        ILLabel skipRet = cursor.DefineLabel();

        // midair tech
        if (cursor.TryGotoNextBestFit(MoveType.After, instr => instr.MatchLdarg0(), instr => instr.MatchLdfld<Player>("jumpGraceTimer")))
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(CheckForMidairTech);
        }

        cursor.Index = 0;

        //*/ wallless wallbounce
        if (cursor.TryGotoNextBestFit(
            MoveType.After,
            instr => instr.MatchCallOrCallvirt<Player>("SuperWallJump"),
            instr => instr.MatchLdcI4(0),
            instr => instr.MatchRet(),
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdcI4(-1),
            instr => instr.MatchCallOrCallvirt<Player>("WallJumpCheck")
            ))
        {
            // just recieved the result of player.WallJumpCheck(-1)
            // we dupe the result so we can consume it and keep one for the code
            // if it's false, then we run the "else if (wallless wallbounce)... etc" part
            cursor.EmitDup();
            cursor.EmitLdarg0();
            cursor.EmitDelegate(CheckForWalllessWallbounce);
            // if we do the wallbounce here, we need to return StNormal
            cursor.EmitBrfalse(skipRet);
            // pop the original result, since CIL doesn't like it when you can jump to a point with different stack states
            cursor.EmitPop();
            cursor.EmitLdcI4(Player.StNormal);
            cursor.EmitRet();
            cursor.MarkLabel(skipRet);
        }
        //*/

        Console.WriteLine(il);
    }
    private static bool CheckForWalllessWallbounce(bool walljumpcheck, Player player)
    {
        if (!walljumpcheck && player.Get<AirDash>() is { } airDash && player.DashAttacking && player.SuperWallJumpAngleCheck)
        {
            player.SuperWallJump((int)player.Facing);
            if (--airDash.Count < 1)
            {
                airDash.RemoveSelf();
            }
            return true;
        }

        return false;
    }

    private static float CheckForMidairTech(float orig, Player player)
    {
        if (player.Get<AirDash>() is { } airDash)
        {
            // only consume if grounded
            if (orig <= 0 && --airDash.Count < 1)
            {
                airDash.RemoveSelf();
            }
            return 1f;
        }
        return orig;
    }

    [OnUnload]

    public static void UnloadHooks()
    {
        IL.Celeste.Player.DashUpdate -= Player_DashUpdate;
        IL.Celeste.Player.RedDashUpdate -= Player_DashUpdate;
    }

    [OnLoadContent]
    public static void OnLoadFloatRefillContent(bool firstLoad)
    {
        _pShatter = new(P_Shatter)
        {
            Color = Calc.HexToColor("e5d3ff"),
            Color2 = Calc.HexToColor("9885fc")
        };

        _pRegen = new(P_Regen)
        {
            Color = Calc.HexToColor("a6a5ff"),
            Color2 = Calc.HexToColor("6d71e0")
        };

        _pGlow = new(P_Glow)
        {
            Color = Calc.HexToColor("a6a5ff"),
            Color2 = Calc.HexToColor("6d71e0")
        };

        _pBubbly = new()
        {
            Acceleration = new(0, -20),
            Color = Calc.HexToColor("7a77ff"),
            Color2 = Calc.HexToColor("b68aff"),
            ColorMode = ParticleType.ColorModes.Fade,
            FadeMode = ParticleType.FadeModes.InAndOut,
            LifeMin = 0.5f,
            LifeMax = 1f,
            Friction = 25f,
            RotationMode = ParticleType.RotationModes.SameAsDirection,
            ScaleOut = true,
            Size = 1f,
            SizeRange = 0.5f,
            SpeedMin = 10f,
            SpeedMax = 20f,
            SpinMin = 4f,
            SpinMax = 8f,
            SpinFlippedChance = true,
            Source = GFX.Game["particles/circle"],
            Direction = 270,
            DirectionRange = 45,
        };
    }
}
