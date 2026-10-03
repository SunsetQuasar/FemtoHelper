using Celeste.Mod.FemtoHelper.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.FemtoHelper;

[ModImportName("SpeedrunTool.SaveLoad")]
public static class SRTInterop
{
    internal static object SaveLoadAction;

    public static Func<Type, string[], object> RegisterStaticTypes;

    public static Action<object> Unregister;

    [OnLoad]
    public static void Load()
    {
        typeof(SRTInterop).ModInterop(); //:3333333
        SaveLoadAction = RegisterStaticTypes?.Invoke(typeof(MovementModifier), ["_modifiers"]);
    }

    [OnUnload]
    public static void Unload()
    {
        Unregister?.Invoke(SaveLoadAction);
    }
}
