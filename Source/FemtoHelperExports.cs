
using MonoMod.ModInterop;
using Celeste.Mod.FemtoHelper.Entities;
using System;

namespace Celeste.Mod.FemtoHelper;

[ModExportName("FemtoHelper")]
public static class FemtoHelperExports
{

    public static void GetHitMethod(Entity block, Player player, int dir) => (block as Generic_SMWBlock)?.Hit(player, dir);

    public static bool IsActive(Entity block)
    {
        return (block as Generic_SMWBlock)?.Active ?? false;
    }
    public static bool CanHitTop(Entity block)
    {
        return (block as Generic_SMWBlock)?.CanHitTop ?? false;
    }
    public static bool CanHitBottom(Entity block)
    {
        return (block as Generic_SMWBlock)?.CanHitBottom ?? false;
    }
    public static bool CanHitLeft(Entity block)
    {
        return (block as Generic_SMWBlock)?.CanHitLeft ?? false;
    }
    public static bool CanHitRight(Entity block)
    {
        return (block as Generic_SMWBlock)?.CanHitRight ?? false;
    }

    public static object CreateTextEffectData(bool wavey, Vector2 amp, float offset, bool shakey, float amount, bool obfuscated, bool twitchy, float twitchChance, float phaseIncrement, float waveSpeed)
    {
        return new TextEffectData(wavey, amp, offset, shakey, amount, obfuscated, twitchy, twitchChance, phaseIncrement, waveSpeed);
    }

    public static object EmptyTextEffectData() => new TextEffectData();

    public static Component CreatePlutoniumTextComponent(string fontPath, string charList, Vector2 fontSize) => new PlutoniumText(fontPath, charList, fontSize);

    public static void PrintPlutoniumText(Component text, Vector2 pos, string str, bool shadow, int spacing, Color mainColor, Color outlineColor, object TextEffectData /* create an empty one for no effects */, float scale /* = 1 */, int id /* = 0 */)
    {
        (text as PlutoniumText)?.Print(pos, str, shadow, spacing, mainColor, outlineColor, (TextEffectData as TextEffectData) ?? new TextEffectData(), 1, 0);
    }

    public static void PrintPlutoniumTextCentered(Component text, Vector2 pos, string str, bool shadow, int spacing, Color mainColor, Color outlineColor, object TextEffectData /* create an empty one for no effects */, float scale /* = 1 */, int id /* = 0 */)
    {
        (text as PlutoniumText)?.PrintCentered(pos, str, shadow, spacing, mainColor, outlineColor, (TextEffectData as TextEffectData) ?? new TextEffectData(), 1, 0);
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
        Console.WriteLine("yo");
        Component collider = new EntityCollider<EvilTheoCrystal>(callback);
        return collider;
    }

}


