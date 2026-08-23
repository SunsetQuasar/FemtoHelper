using System;
using System.Collections;
using static Celeste.TempleGate;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/AvertRefill")]
public class AvertRefill : CustomRefill
{
    private static readonly ParticleType _pShatter = new(P_Shatter)
    {
        Color = Calc.HexToColor("ffadb3"),
        Color2 = Calc.HexToColor("ffadb3")
    };

    private static readonly ParticleType _pRegen = new(P_Regen)
    {
        Color = Calc.HexToColor("ff606b"),
        Color2 = Calc.HexToColor("ff606b")
    };

    private static readonly ParticleType _pGlow = new(P_Glow)
    {
        Color = Calc.HexToColor("ff606b"),
        Color2 = Calc.HexToColor("ff606b")
    };

    private enum Directions
    {
        Up = 0,
        UpRight = 45,
        Right = 90,
        DownRight = 135,
        Down = 180,
        DownLeft = 225,
        Left = 270,
        UpLeft = 315,
    }

    private readonly Directions direction;


    public AvertRefill(Vector2 position, EntityData data)
        : base(position, false, data.Bool("oneUse", false))
    {
        direction = data.Enum("direction", Directions.Up);

        this.SetTexture("objects/FemtoHelper/bubbleRedirect/")
            .SetOnCollect(OnCollect)
            .SetParticles(_pShatter, _pRegen, _pGlow);

        RefillDash = RefillStamina = false;
        AlwaysUse = true;
        RespawnTime = data.Float("respawnTime", 2.5f);

        sprite.Rotation = flash.Rotation = outline.Rotation = (float)direction * Calc.DegToRad;
    }


    public AvertRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {
    }

    private void OnCollect(Player player)
    {
        player.Speed = (-Vector2.UnitY * 240).Rotate((float)direction * Calc.DegToRad);
    }
}
