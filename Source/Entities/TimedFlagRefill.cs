using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/TimedFlagRefill")]
public class BooleanGem : CustomRefill
{
    public enum FlagModes
    {
        OnThenOff = 0,
        OffThenOn = 1,
        ToggleTwice = 2
    }
    public readonly string Flag;
    public readonly bool StopMomentum;
    public readonly FlagModes Flagmode;
    public readonly int Duration;
    private readonly ParticleType pShatter;
    private readonly ParticleType pRegen;
    private readonly ParticleType pGlow;

    public BooleanGem(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Bool("twoDash"), data.Bool("oneUse"))
    {
        Color[] cols = [.. data.Attr("particleColors", "d3edff,94a5ef,a5c3ff,6c74dd")
                      .Split(',')
                      .Select(str => Calc.HexToColor(str.Trim()))];

        pShatter = new ParticleType(p_shatter);
        pRegen = new ParticleType(p_regen);
        pGlow = new ParticleType(p_glow);
        pShatter.Color = cols[0];
        pShatter.Color2 = cols[1];
        pRegen.Color = cols[2];
        pRegen.Color2 = cols[3];
        pGlow.Color = cols[2];
        pGlow.Color2 = cols[3];

        AudioPath = TwoDashAudioPath = data.Attr("audioPath", "event:/game/general/");

        // i'm a dumbass, fix dumbass 
        if (AudioPath == "event:/game/general/" && twoDashes)
        {
            TwoDashAudioPath = "event:/new_content/game/10_farewell/";
        }

        Flag = data.Attr("flag", "refill_flag");
        StopMomentum = data.Bool("stopMomentum", true);
        AlwaysUse = data.Bool("alwaysUse", false);
        Flagmode = (FlagModes)data.Int("flagMode", 0);
        Duration = data.Int("duration", 1);
        RefillDash = data.Bool("refillDash", true);
        RefillStamina = data.Bool("refillStamina", true);

        this.SetTexture(data.Attr("path", "objects/refill/"))
            .SetParticles(pShatter, pRegen, pGlow)
            .SetOnCollect(OnCollect);

        RespawnTime = data.Float("respawnTime", 2.5f);

        VisualOffset = data.Vector2("visualOffsetX", "visualOffsetY", Vector2.Zero);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        (scene as Level).Session.SetFlag("flag", false);
    }

    private void OnCollect(Player player)
    {
        if (StopMomentum) player.Speed = Vector2.Zero;
        Add(new Coroutine(FlagRoutine(Flag)));
    }

    public IEnumerator FlagRoutine(string flag)
    {
        Level level = Scene as Level;
        float pause = Duration / 60f;
        switch (Flagmode)
        {
            default:
            case FlagModes.OnThenOff:
                level.Session.SetFlag(flag, true);
                if (Duration <= -1) yield break;
                yield return pause;
                level.Session.SetFlag(flag, false);
                break;
            case FlagModes.OffThenOn:
                level.Session.SetFlag(flag, false);
                if (Duration <= -1) yield break;
                yield return pause;
                level.Session.SetFlag(flag, true);
                break;
            case FlagModes.ToggleTwice:
                level.Session.SetFlag(flag, !level.Session.GetFlag(flag));
                if (Duration <= -1) yield break;
                yield return pause;
                level.Session.SetFlag(flag, !level.Session.GetFlag(flag));
                break;
        }

    }
}
