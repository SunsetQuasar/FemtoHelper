using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/BoundRefill")]
public class BoundRefill : CustomRefill
{
    public class Bounder() : DashEffect()
    {
        public override void OnDashBegin(Player player)
        {
            base.OnDashBegin(player);
            player.Add(new Coroutine(BoundRoutine(player)));
        }
    }

    [OnLoad]
    public static void LoadHooks()
    {
    }

    [OnUnload]
    public static void UnloadHooks()
    {
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

    public readonly bool StrictDashCheck;

    public BoundRefill(Vector2 position, EntityData data)
        : base(position, false, data.Bool("oneUse", false), data.String("pulseFlag", ""))
    {
        this.SetTexture("objects/FemtoHelper/boundRefill/")
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetCollectLogic((player) => DashEffect.ShouldUseRefill<Bounder>(player, 1))
            .SetOnCollect(OnCollect);

        RespawnTime = data.Float("respawnTime", 2.5f);

        VisualOffset = data.Vector2("visualOffsetX", "visualOffsetY", Vector2.Zero);
        StrictDashCheck = data.Bool("strictDashCheck", false);
    }


    public BoundRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {
    }

    private void OnCollect(Player player)
    {
        player.UseRefill(false);
        DashEffect.GiveToPlayer<Bounder>(player, 1, StrictDashCheck);
    }
}
