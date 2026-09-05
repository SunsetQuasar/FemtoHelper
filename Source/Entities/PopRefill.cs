

// Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Celeste.Refill
using System;
using System.Collections;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/PopRefill")]
public class PopRefill : CustomRefill
{

    public bool DoRespawn = false;
    public float SpawnTime = 2.5f;
    
    public PopRefill(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Bool("twoDash"), data.Bool("oneUse"))
    {
        RespawnTime = data.Float("respawnTime", 2.5f);
        SpawnTime = data.Float("spawnTime", 2.5f);
        this.SetRespawnGate(() => DoRespawn)
            .SetOnRespawn(() => outline.Color = Color.White);

        VisualOffset = data.Vector2("visualOffsetX", "visualOffsetY", Vector2.Zero);
    }
    
    public override void Added(Scene scene)
    {
        base.Added(scene);
        Collidable = false;
        sprite.Visible = false;
        flash.Visible = false;
        outline.Visible = true;
        Depth = 8999;
        respawnTimer = SpawnTime;
        Add(new Coroutine(StartRoutine()));
    }

    public IEnumerator StartRoutine()
    {
        while (true)
        {
            if (CollideCheck<Player>())
            {
                Audio.Play("event:/game/03_resort/fluff_tendril_touch", Position);
                outline.Color *= 0.2f;
                break;
            }
            yield return null;
        }
        while (true)
        {
            if (!CollideCheck<Player>())
            {
                Audio.Play("event:/game/03_resort/fluff_tendril_emerge", Position);
                outline.Color *= 2.5f;
                break;
            }
            yield return null;
        }
        DoRespawn = true;
        respawnTimer = SpawnTime;
    }
}
