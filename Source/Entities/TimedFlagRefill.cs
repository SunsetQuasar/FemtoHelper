using System;
using System.Linq;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/TimedFlagRefill")]
public class BooleanGem : Entity
{
    public static ParticleType PShatter;

    public static ParticleType PRegen;

    public static ParticleType PGlow;

    public static ParticleType PShatterTwo;

    public static ParticleType PRegenTwo;

    public static ParticleType PGlowTwo;

    private readonly Sprite sprite;

    private readonly Sprite flash;

    private readonly Image outline;

    private readonly Wiggler wiggler;

    private readonly BloomPoint bloom;

    private readonly VertexLight light;

    private Level level;

    private readonly SineWave sine;

    private readonly bool twoDashes;

    private readonly bool oneUse;

    private readonly ParticleType pShatter;

    private readonly ParticleType pRegen;

    private readonly ParticleType pGlow;

    private float respawnTimer;

    public readonly string Flag;

    public readonly bool StopMomentum;

    public enum FlagModes
    {
        OnThenOff = 0,
        OffThenOn = 1,
        ToggleTwice = 2
    }

    public readonly FlagModes Flagmode;

    public readonly bool AlwaysUse;

    public readonly int Duration;

    public readonly bool RefillDash;

    public readonly bool RefillStamina;

    private readonly float respawnTime;

    public BooleanGem(Vector2 position, bool twoDashes, bool oneUse, string path, string pcolors, float respawnTime)
        : base(position)
    {
        //thanks communal helper
        Color[] cols = pcolors
                      .Split(',')
                      .Select(str => Calc.HexToColor(str.Trim()))
                      .ToArray();
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        this.twoDashes = twoDashes;
        this.oneUse = oneUse;
        var text = path;
        pShatter = new ParticleType(Refill.P_Shatter);
        pRegen = new ParticleType(Refill.P_Regen);
        pGlow = new ParticleType(Refill.P_Glow);
        pShatter.Color = cols[0];
        pShatter.Color2 = cols[1];
        pRegen.Color = cols[2];
        pRegen.Color2 = cols[3];
        pGlow.Color = cols[2];
        pGlow.Color2 = cols[3];
        Add(outline = new Image(GFX.Game[text + "outline"]));
        outline.CenterOrigin();
        outline.Visible = false;
        Add(sprite = new Sprite(GFX.Game, text + "idle"));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        Add(flash = new Sprite(GFX.Game, text + "flash"));
        flash.Add("flash", "", 0.05f);
        flash.OnFinish = delegate
        {
            flash.Visible = false;
        };
        flash.CenterOrigin();
        Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
        {
            sprite.Scale = flash.Scale = Vector2.One * (1f + v * 0.2f);
        }));
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.8f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f));
        sine.Randomize();
        UpdateY();
        Depth = -100;
        this.respawnTime = respawnTime;
    }

    public BooleanGem(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("twoDash"), data.Bool("oneUse"), data.Attr("path", "objects/refill/"), data.Attr("particleColors", "d3edff,94a5ef,a5c3ff,6c74dd"), data.Float("respawnTime", 2.5f))
    {
        PShatter = new ParticleType(Refill.P_Shatter);
        PRegen = new ParticleType(Refill.P_Regen);
        PGlow = new ParticleType(Refill.P_Glow);
        PShatterTwo = new ParticleType(Refill.P_ShatterTwo);
        PRegenTwo = new ParticleType(Refill.P_RegenTwo);
        PGlowTwo = new ParticleType(Refill.P_GlowTwo);
        Flag = data.Attr("flag", "refill_fLag");
        StopMomentum = data.Bool("stopMomentum", true);
        AlwaysUse = data.Bool("alwaysUse", false);
        Flagmode = (FlagModes)data.Int("flagMode", 0);
        Duration = data.Int("duration", 1);
        RefillDash = data.Bool("refillDash", true);
        RefillStamina = data.Bool("refillStamina", true);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        (scene as Level).Session.SetFlag("flag", false);
    }

    public override void Update()
    {
        base.Update();
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
        }
        else if (Scene.OnInterval(0.1f))
        {
            level.ParticlesFG.Emit(pGlow, 1, Position, Vector2.One * 5f);
        }
        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        bloom.Alpha = light.Alpha * 0.8f;
        if (!Scene.OnInterval(2f) || !sprite.Visible) return;
        
        flash.Play("flash", restart: true);
        flash.Visible = true;
    }

    private void Respawn()
    {
        if (Collidable) return;
        
        Collidable = true;
        sprite.Visible = true;
        outline.Visible = false;
        Depth = -100;
        wiggler.Start();
        Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
        level.ParticlesFG.Emit(pRegen, 16, Position, Vector2.One * 2f);
    }

    private void UpdateY()
    {
        float num2 = bloom.Y = sine.Value * 2f;
        float num5 = flash.Y = sprite.Y = num2;
    }

    public override void Render()
    {
        if (sprite.Visible)
        {
            sprite.DrawOutline();
        }
        base.Render();
    }

    private void OnPlayer(Player player)
    {
        if (!UseRefill2(player, twoDashes)) return;
        
        if (StopMomentum) player.Speed = Vector2.Zero;
        Add(new Coroutine(FlagRoutine(Flag)));
        Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Collidable = false;
        Add(new Coroutine(RefillRoutine(player)));
        respawnTimer = respawnTime;
    }

    private IEnumerator RefillRoutine(Player player)
    {
        Celeste.Freeze(0.05f);
        yield return null;
        level.Shake();
        sprite.Visible = flash.Visible = false;
        if (!oneUse)
        {
            outline.Visible = true;
        }
        Depth = 8999;
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(pShatter, 5, Position, Vector2.One * 4f, num - (float)Math.PI / 2f);
        level.ParticlesFG.Emit(pShatter, 5, Position, Vector2.One * 4f, num + (float)Math.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse)
        {
            RemoveSelf();
        }
    }
    public bool UseRefill2(Player player, bool twoDashes)
    {
        bool flag = false;
        int num = player.MaxDashes;
        if (twoDashes)
        {
            num = 2;
        }
        if (player.Dashes < num || player.Stamina < 20f)
        {
            if (RefillDash) player.Dashes = num;
            if (RefillStamina) player.RefillStamina();
            flag = true;
        }

        return flag || AlwaysUse;
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
