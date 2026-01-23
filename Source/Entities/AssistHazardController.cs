using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/AssistHazardController")]
public class AssistHazardController : Entity
{
    public AssistHazardController() : base(Vector2.Zero)
    {
        Tag = Tags.Global | Tags.Persistent;
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level level = SceneAs<Level>();
        foreach (var entity in level.Tracker.GetEntities<AssistHazardController>().Cast<AssistHazardController>().Where(entity => entity != this))
        {
            entity.RemoveSelf();
        }
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

    public static void Load()
    {
        On.Celeste.Solid.Update += NoAssistSquishHook;
        On.Celeste.Player.OnSquish += NoAssistSquishPlayer;
        On.Celeste.Player.DreamDashUpdate += DreamDashBounceHook;
    }

    public static void Unload()
    {
        On.Celeste.Solid.Update -= NoAssistSquishHook;
        On.Celeste.Player.OnSquish -= NoAssistSquishPlayer;
        On.Celeste.Player.DreamDashUpdate -= DreamDashBounceHook;
    }
}