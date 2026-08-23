using System;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/UpendRefill")]
public class UpendRefill : CustomRefill
{
    private static readonly ParticleType _pShatter = new(P_Shatter)
    {
        Color = Calc.HexToColor("ffbdb0"),
        Color2 = Calc.HexToColor("ffbdb0")
    };

    private static readonly ParticleType _pRegen = new(P_Regen)
    {
        Color = Calc.HexToColor("ff8770"),
        Color2 = Calc.HexToColor("ff8770")
    };

    private static readonly ParticleType _pGlow = new(P_Glow)
    {
        Color = Calc.HexToColor("ff8770"),
        Color2 = Calc.HexToColor("ff8770")
    };

    private enum Types
    {
        Horizontal,
        Vertical
    }

    private readonly Types type;

    public UpendRefill(Vector2 position, EntityData data)
        : base(position, false, data.Bool("oneUse", false))
    {
        type = data.Enum("type", Types.Horizontal);
        
        this.SetTexture("objects/FemtoHelper/upendRefill/" + (type == Types.Horizontal ? "h/" : "v/"))
            .SetOnCollect(OnCollect)
            .SetParticles(_pShatter, _pRegen, _pGlow);

        RefillDash = RefillStamina = false;
        AlwaysUse = true;
        RespawnTime = data.Float("respawnTime", 2.5f);
    }

    public UpendRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {
    }

    private void OnCollect(Player player)
    {
        player.StateMachine.ForceState(Player.StNormal);
        if (type == Types.Vertical)
        {
            player.Speed.Y *= -2;
        }
        else
        {
            player.Speed.X *= -2;
        }
    }
}
