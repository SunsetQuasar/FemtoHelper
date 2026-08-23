using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/BoundRefill")]
public class BoundRefill : CustomRefill
{
    public class Bounder() : Component(false, false)
    {
    }

    [OnLoad]
    public static void LoadHooks()
    {
        On.Celeste.Player.DashBegin += Player_DashBegin;
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);
        if (self.Get<Bounder>() is not { } b) return;
        b.RemoveSelf();
        self.Add(new Coroutine(BoundRoutine(self)));
    }

    [OnUnload]
    public static void UnloadHooks()
    {
        On.Celeste.Player.DashBegin -= Player_DashBegin;
    }

    public static IEnumerator BoundRoutine(Player player)
    {
        yield return null;
        player.Speed.Y = -300;
        player.StateMachine.ForceState(Player.StNormal);
        Audio.Play("event:/game/general/spring", player.Center);
    }

    private static readonly ParticleType _pShatter = new(Refill.P_Shatter)
    {
        Color = Calc.HexToColor("ffc3fe"),
        Color2 = Calc.HexToColor("ffc3fe")
    };

    private static readonly ParticleType _pRegen = new(Refill.P_Regen)
    {
        Color = Calc.HexToColor("b95fb7"),
        Color2 = Calc.HexToColor("b95fb7")
    };

    private static readonly ParticleType _pGlow = new(Refill.P_Glow)
    {
        Color = Calc.HexToColor("b95fb7"),
        Color2 = Calc.HexToColor("b95fb7")
    };

    public BoundRefill(Vector2 position, EntityData data)
        : base(position, false, data.Bool("oneUse", false))
    {
        this.SetTexture("objects/FemtoHelper/boundRefill/")
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetCollectLogic((player) => player.Get<Bounder>() is null)
            .SetOnCollect(OnCollect);

        RespawnTime = data.Float("respawnTime", 2.5f);
    }


    public BoundRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {
    }

    private void OnCollect(Player player)
    {
        player.UseRefill(false);
        player.Add(new Bounder());
    }
}
