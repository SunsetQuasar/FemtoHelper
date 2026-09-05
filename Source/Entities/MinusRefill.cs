using System;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/MinusRefill")]
public class MinusRefill : CustomRefill
{
    private static readonly ParticleType _pShatter = new(P_Shatter)
    {
        Color = Calc.HexToColor("d3e8ff"),
        Color2 = Calc.HexToColor("85b0fc")
    };

    private static readonly ParticleType _pRegen = new(P_Regen)
    {
        Color = Calc.HexToColor("a5d1ff"),
        Color2 = Calc.HexToColor("6da0e0")
    };

    private static readonly ParticleType _pGlow = new(P_Glow)
    {
        Color = Calc.HexToColor("a5d1ff"),
        Color2 = Calc.HexToColor("6da0e0")
    };

    public MinusRefill(Vector2 position, EntityData data)
        : base(position, false, data.Bool("oneUse", false))
    {
        this.SetTexture("objects/FemtoHelper/minusRefill/")
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetOnCollect(OnCollect);

        AlwaysUse = true;
        RefillDash = RefillStamina = false;

        RespawnTime = data.Float("respawnTime", 2.5f);

        VisualOffset = data.Vector2("visualOffsetX", "visualOffsetY", Vector2.Zero);
    }

    public MinusRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {
    }

    private void OnCollect(Player player)
    {
        if (player.Dashes-- <= 0)
        {
            player.Die(-(Center - player.Center).SafeNormalize());
        }
    }
}
