using System;
using System.Reflection;
using Celeste.Mod.FemtoHelper.Utils;
using Celeste.Mod.Helpers;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using static Celeste.TrackSpinner;

namespace Celeste.Mod.FemtoHelper.Entities;

[TrackedAs(typeof(Holdable))]
public class SMWHoldable : Holdable
{
    public Action<Vector2> OnClipDeath;
    public Action<Vector2> OnClipWiggleSuccess;

    public float TurnPercent;

    public SMWHoldable() : base(0f)
    {
    }

    private static ILHook _hookPickupCoroutine;

    private static ILHook _hookOrigPickup;

    public static void Load()
    {
        On.Celeste.Player.UpdateCarry += hook_UpdateCarry;
        On.Celeste.Holdable.Release += hook_Release;
        _hookPickupCoroutine =
            new ILHook(
                typeof(Player).GetMethod("PickupCoroutine", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetStateMachineTarget(), PickupRoutineHook);
        IL.Celeste.Player.NormalUpdate += hook_NormalUpdate;

        _hookOrigPickup = new ILHook(typeof(Player).GetMethod("orig_Pickup", BindingFlags.Instance | BindingFlags.NonPublic), hook_OrigPickup);
        On.Celeste.Holdable.Pickup += Holdable_Pickup;
    }

    private static bool Holdable_Pickup(On.Celeste.Holdable.orig_Pickup orig, Holdable self, Player player)
    {
        if(self is SMWHoldable smwholdable)
        {
            //smwholdable.TurnPercent = smwholdable.TurnPercent = player.Facing == Facings.Left ? 0 : 1;
            smwholdable.TurnPercent = Calc.Map(player.CenterX - smwholdable.Entity.CenterX, self.Entity.Width / 2, -self.Entity.Width / 2);
        }
        return orig(self, player);
    }

    private static void hook_OrigPickup(ILContext il)
    {
        ILCursor cursor = new(il);
        while (cursor.TryGotoNext(MoveType.After, (Instruction instr) => instr.MatchLdcR4(0.35f)))
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(ModPickupSpeed);
        }
    }

    private static void hook_Release(On.Celeste.Holdable.orig_Release orig, Holdable self, Vector2 force)
    {
        if (self is SMWHoldable smwholdable)
        {
            if(self.Holder is {} p)
            {
                smwholdable.TurnPercent = p.Facing == Facings.Left ? 0 : 1;
            }
            
            if (smwholdable.Entity.CollideCheck<Solid>())
            {
                bool flag = !(self.Entity as Actor)?.TrySquishWiggleNoPusher(5, 5) ?? false;
                if (flag)
                {
                    smwholdable.OnClipDeath?.Invoke(force);
                    return;
                } 
                else 
                {
                    smwholdable.OnClipWiggleSuccess?.Invoke(force);
                }
            }
        }

        orig(self, force);
    }

    private static void hook_NormalUpdate(ILContext il)
    {
        ILCursor cursor = new(il);

        while (cursor.TryGotoNext(instr => instr.MatchCallOrCallvirt<Player>("Drop")))
        {
            ILLabel dontDrop = cursor.DefineLabel();
            cursor.EmitDelegate(DontDropCheck);
            cursor.EmitBrtrue(dontDrop);
            cursor.EmitLdarg0();
            cursor.Index++;
            cursor.MarkLabel(dontDrop);
        }

        cursor.Index = 0;

        while (cursor.TryGotoNextBestFit(MoveType.After, 4,
                   instr => instr.MatchBltUn(out ILLabel _),
                   instr => instr.MatchLdarg0(),
                   instr => instr.MatchCallOrCallvirt<Player>("get_CanUnDuck"))
              )
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(CanUnDuckHack);
        }
    }

    private static bool CanUnDuckHack(bool orig, Player p)
    {
        if (p.Holding is not SMWHoldable) return orig;
        if (Input.MoveY != 1)
        {
            if(orig) p.Sprite.Scale = new Vector2(0.8f, 1.2f);
            return orig;
        }


        return false;
    }

    private static bool DontDropCheck(Player p)
    {
        return p.Holding is SMWHoldable;
    }

    private static void hook_UpdateCarry(On.Celeste.Player.orig_UpdateCarry orig, Player self)
    {
        if (self.Holding is SMWHoldable smwholdable)
        {
            if(self.Facing == Facings.Left)
            {
                smwholdable.TurnPercent = Calc.Approach(smwholdable.TurnPercent, 0, 6 * Engine.DeltaTime);
            } 
            else
            {
                smwholdable.TurnPercent = Calc.Approach(smwholdable.TurnPercent, 1, 6 * Engine.DeltaTime);
            }

            float x = Calc.LerpClamp(-12, 12, Ease.CubeInOut(smwholdable.TurnPercent));
            float y = self.Ducking ? -8 : -12;
            self.carryOffset = new(x, y);
        }

        orig(self);
    }


    private static void PickupRoutineHook(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNextBestFit(MoveType.After, 8, instruction => instruction.MatchLdcR4(0.16f),
                instruction => instruction.MatchLdcI4(1))) return;
        cursor.Index--;
        cursor.EmitLdloc(1);
        cursor.EmitDelegate(ModPickupSpeed);
    }

    private static float ModPickupSpeed(float orig, Player player)
    {
        //Logger.Log(LogLevel.Warn, "FemtoHelper/SMWHoldable", player.Holding.GetType().FullName);
        return player.Holding is SMWHoldable ? 0f : orig;
    }

    public static void Unload()
    {
        On.Celeste.Player.UpdateCarry -= hook_UpdateCarry;
        _hookPickupCoroutine?.Dispose();
        _hookPickupCoroutine = null;
        IL.Celeste.Player.NormalUpdate -= hook_NormalUpdate;
        _hookOrigPickup?.Dispose();
        _hookOrigPickup = null;
        On.Celeste.Holdable.Pickup -= Holdable_Pickup;
    }
}