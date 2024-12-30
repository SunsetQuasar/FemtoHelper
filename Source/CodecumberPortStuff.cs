using System;
using System.Collections;
using Celeste.Mod.FemtoHelper.Entities;

namespace Celeste.Mod.FemtoHelper;

public class CodecumberPortStuff
{
    public static void Load()
    {
        RotateDashRefill.Load();
        BoostingBoosterSorryIStoleFromCommunalHelper.Load();
        EvilTheoCrystal.Load();
        On.Celeste.Solid.Update += NoAssistSquishHook;
        On.Celeste.Player.OnSquish += NoAssistSquishPlayer;
        On.Celeste.Player.DreamDashUpdate += DreamDashBounceHook;
    }

    public static void Unload()
    {
        RotateDashRefill.Unload();
        BoostingBoosterSorryIStoleFromCommunalHelper.Unload();
        EvilTheoCrystal.Unload();
        On.Celeste.Solid.Update -= NoAssistSquishHook;
        On.Celeste.Player.OnSquish -= NoAssistSquishPlayer;
        On.Celeste.Player.DreamDashUpdate -= DreamDashBounceHook;
    }

    public static void NoAssistSquishHook(On.Celeste.Solid.orig_Update orig, Solid self)
    {
        bool assist1 = SaveData.Instance.Assists.Invincible;
        if (self.Scene.Tracker.GetEntity<AssistHazardController>() != null)
        {
            SaveData.Instance.Assists.Invincible = true;
        }
        orig(self);
        SaveData.Instance.Assists.Invincible = assist1;
    }

    public static void NoAssistSquishPlayer(On.Celeste.Player.orig_OnSquish orig, Player self, CollisionData data)
    {
        bool assist1 = SaveData.Instance.Assists.Invincible;
        if (self.Scene.Tracker.GetEntity<AssistHazardController>() != null)
        {
            SaveData.Instance.Assists.Invincible = true;
        }
        orig(self, data);
        SaveData.Instance.Assists.Invincible = assist1;
    }

    public static int DreamDashBounceHook(On.Celeste.Player.orig_DreamDashUpdate orig, Player self)
    {
        bool assist1 = SaveData.Instance.Assists.Invincible;
        if (self.Scene.Tracker.GetEntity<AssistHazardController>() != null)
        {
            SaveData.Instance.Assists.Invincible = true;
        }
        int a = orig(self);
        SaveData.Instance.Assists.Invincible = assist1;
        return a;
    }
        
    public static float VectorToAngle(Vector2 vector)
    {
        return (float)Math.Atan2(vector.Y, vector.X);
    }
}