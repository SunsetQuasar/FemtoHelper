using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[TrackedAs(typeof(SwapBlock))]
[CustomEntity("FemtoHelper/SensorSwapBlock")]
public class SensorSwapBlock : SwapBlock
{
    readonly Collider sensor;
    float sensingCharge;

    // instead of a switcheroo, store a permanent copy of the original particle type
    // if somehow the entity fails to restore the swap block particles, the next time it doesn't should fix it
    private static ParticleType pMoveBackupCopy = new(P_Move);

    private static ParticleType pMove = new(P_Move)
    {
        Color = Calc.HexToColor("36fbcc"),
        Color2 = Calc.HexToColor("30beb5"),
    };

    public SensorSwapBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, Themes.Moon)
    {
        sensor = new Hitbox(moveRect.Width, moveRect.Height, moveRect.X, moveRect.Y);

        foreach (DashListener dashListener in Components.GetAll<DashListener>())
        {
            Action<Vector2> orig = dashListener.OnDash;
            dashListener.OnDash = (dir) =>
            {
                if (CheckSensor())
                {
                    orig(dir);
                }
            };
        }

        MTexture mTexture = GFX.Game["objects/FemtoHelper/sensorSwapblock/block"];
        MTexture mTexture2 = GFX.Game["objects/FemtoHelper/sensorSwapblock/blockRed"];
        MTexture mTexture3 = GFX.Game["objects/FemtoHelper/sensorSwapblock/target"];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                nineSliceGreen[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                nineSliceRed[i, j] = mTexture2.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                nineSliceTarget[i, j] = mTexture3.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }

        middleGreen = new Sprite(GFX.Game, "objects/FemtoHelper/sensorSwapBlock/");
        middleGreen.Justify = Vector2.One * 0.5f;
        middleGreen.CenterOrigin();
        middleGreen.AddLoop("idle", "midBlock", 0.08f);
        middleGreen.Play("idle");

        middleRed = new Sprite(GFX.Game, "objects/FemtoHelper/sensorSwapBlock/");
        middleRed.Justify = Vector2.One * 0.5f;
        middleRed.CenterOrigin();
        middleRed.AddLoop("idle", "midBlockRed", 0.08f);
        middleRed.Play("idle");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
    }

    public override void Update()
    {
        sensingCharge = Calc.Approach(sensingCharge, CheckSensor() ? 1f : 0f, Engine.DeltaTime * 8f);
        path.timer += Engine.DeltaTime * Ease.SineInOut(sensingCharge) * 8f;
        // lol
        try
        {
            P_Move = pMove;
            base.Update();
        } 
        finally
        {
            P_Move = pMoveBackupCopy;
        }
    }

    public bool CheckSensor()
    {
        sensor.Position -= Position;
        Collider temp = Collider;
        Collider = sensor;

        bool ret = Scene.Tracker.GetEntity<Player>() switch
        {
            Player player => CollideCheck(player),
            _ => false
        };

        Collider = temp;
        sensor.Position += Position;

        return ret;
    }
}
