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
    // gives back the using charge when the dash ends (AirRefill only consumes the effect when tech succeeds)
    public bool GiveBackUsed = false;
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
        // if it gives back, assume it will
        return player.Get<T>() is not { } t || ((t.GiveBackUsed ? (t.ReadyCount + t.UsingCount) : t.ReadyCount) < amount);
    }

    public virtual void GivenToPlayer(Player player, int amount)
    {

    }

    public static T GiveToPlayer<T>(Player player, int amount, bool strict = false) where T : DashEffect, new()
    {
        if (player.Get<T>() is T t)
        {
            t.ReadyCount = Math.Max(t.ReadyCount, amount);
            t.StrictDashCheck = strict;
            t.GivenToPlayer(player, amount);

            return t;
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

            return newT;
        }
    }

    public static T SetCharges<T>(Player player, int amount, bool strict = false) where T : DashEffect, new()
    {
        if (player.Get<T>() is T t)
        {
            t.ReadyCount = 0;
        }

        return GiveToPlayer<T>(player, amount, strict);
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);

        foreach (DashEffect dashEffect in self.Components.GetAll<DashEffect>())
        {
            //Debug($"CurrentBooster: {(self.CurrentBooster is null ? "null" : self.CurrentBooster.GetType().Name)}");
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
            if (dashEffect.GiveBackUsed)
            {
                dashEffect.ReadyCount += dashEffect.UsingCount;
            }
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
