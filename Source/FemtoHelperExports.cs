
using MonoMod.ModInterop;
using Celeste.Mod.FemtoHelper.Entities;
using System;
using Celeste.Mod.FemtoHelper.PlutoniumText;

namespace Celeste.Mod.FemtoHelper;

[ModExportName("FemtoHelper")]
public static class FemtoHelperExports
{

    public static void GetHitMethod(Entity block, Player player, int dir) => (block as GenericSmwBlock)?.Hit(player, (GenericSmwBlock.Direction)dir);

    public static bool IsActive(Entity block)
    {
        return (block as GenericSmwBlock)?.Activated ?? false;
    }
    public static bool CanHitTop(Entity block)
    {
        return (block as GenericSmwBlock)?.CanHitTop ?? false;
    }
    public static bool CanHitBottom(Entity block)
    {
        return (block as GenericSmwBlock)?.CanHitBottom ?? false;
    }
    public static bool CanHitLeft(Entity block)
    {
        return (block as GenericSmwBlock)?.CanHitLeft ?? false;
    }
    public static bool CanHitRight(Entity block)
    {
        return (block as GenericSmwBlock)?.CanHitRight ?? false;
    }

    public static Entity TransformIntoEvilTheo(Actor from, string spriteOverride)
    {
        Holdable hFrom = from.Get<Holdable>();
        if (hFrom == null) return null;
        Entity evil = new EvilTheoCrystal(from.Position, spriteOverride);
        (evil as EvilTheoCrystal).Hold.SetSpeed(hFrom.GetSpeed());
        return evil;
    }

    public static Component GetEvilTheoCollider(Action<Entity> callback)
    {
        Component collider = new EntityCollider<EvilTheoCrystal>(callback);
        return collider;
    }

}


