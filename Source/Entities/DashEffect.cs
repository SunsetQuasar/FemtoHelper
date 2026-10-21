using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.FemtoHelper.Entities.SlashRefill;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked(true)]
public abstract class DashEffect : Component
{
    public int ReadyCount;
    public int UsingCount;
    public bool CurrentlyDashing => UsingCount > 0;
    internal static bool CanDashStorage;
    public bool StrictDashCheck;
    public virtual void OnDashBegin(Player player)
    {

    }
    public virtual void OnDashEnd(Player player)
    {

    }
    public DashEffect() : base(true, false)
    {
        ReadyCount = 0;
        UsingCount = 0;
    }

    public DashEffect(int amount) : this()
    {
        ReadyCount = amount;
    }

    [OnLoad]
    public static void Load()
    {
        Everest.Events.Player.OnBeforeUpdate += Player_OnBeforeUpdate;
        On.Celeste.Player.DashBegin += Player_DashBegin;
        On.Celeste.Player.DashEnd += Player_DashEnd;
    }

    public static bool ShouldUseRefill<T>(Player player, int amount) where T : DashEffect
    {
        return player.Get<T>() is not { } t || (t.ReadyCount < amount);
    }

    public virtual void GivenToPlayer(Player player, int amount)
    {

    }

    public static void GiveToPlayer<T>(Player player, int amount, bool strict = false) where T : DashEffect, new()
    {
        if (player.Get<T>() is T t)
        {
            t.ReadyCount = Math.Max(t.ReadyCount, amount);
            t.StrictDashCheck = strict;
            t.GivenToPlayer(player, amount);
        } 
        else
        {
            T newT = new()
            {
                ReadyCount = amount,
                StrictDashCheck = strict
            };
            player.Add(newT);
            newT.GivenToPlayer(player, amount);
        }
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);

        foreach(DashEffect dashEffect in self.Components.GetAll<DashEffect>())
        {
            if ((dashEffect.StrictDashCheck && (!CanDashStorage || self.CurrentBooster != null)) || dashEffect.ReadyCount <= 0) continue;

            dashEffect.OnDashBegin(self);
            dashEffect.ReadyCount--;
            dashEffect.UsingCount++;
        }
    }
    private static void Player_DashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        orig(self);
        foreach (DashEffect dashEffect in self.Components.GetAll<DashEffect>())
        {
            if (dashEffect.UsingCount <= 0) continue;

            dashEffect.OnDashEnd(self);
            dashEffect.UsingCount = 0;
        }
    }

    private static void Player_OnBeforeUpdate(Player self)
    {
        CanDashStorage = self.CanDash;
    }

    [OnUnload]
    public static void Unload()
    {
        Everest.Events.Player.OnBeforeUpdate -= Player_OnBeforeUpdate;
    }
}
