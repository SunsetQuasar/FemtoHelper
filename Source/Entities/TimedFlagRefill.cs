using System;
using System.Linq;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/TimedFlagRefill")]
public class BooleanGem : Entity
{
    public static ParticleType P_Shatter;

    public static ParticleType P_Regen;

    public static ParticleType P_Glow;

    public static ParticleType P_ShatterTwo;

    public static ParticleType P_RegenTwo;

    public static ParticleType P_GlowTwo;

    private Sprite sprite;

    private Sprite flash;

    private Image outline;

    private Wiggler wiggler;

    private BloomPoint bloom;

    private VertexLight light;

    private Level level;

    private SineWave sine;

    private bool twoDashes;

    private bool oneUse;

    private ParticleType p_shatter;

    private ParticleType p_regen;

    private ParticleType p_glow;

    private float respawnTimer;

    public string flag;

    public bool stopMomentum;

    public enum FlagModes
    {
        OnThenOff = 0,
        OffThenOn = 1,
        ToggleTwice = 2
    }

    public FlagModes flagmode;

    public bool alwaysUse;

    public int duration;

    public bool refillDash;

    public bool refillStamina;

    public BooleanGem(Vector2 position, bool twoDashes, bool oneUse, string path, string pcolors)
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
        string text;
        text = path;
        p_shatter = new ParticleType(Refill.P_Shatter);
        p_regen = new ParticleType(Refill.P_Regen);
        p_glow = new ParticleType(Refill.P_Glow);
        p_shatter.Color = cols[0];
        p_shatter.Color2 = cols[1];
        p_regen.Color = cols[2];
        p_regen.Color2 = cols[3];
        p_glow.Color = cols[2];
        p_glow.Color2 = cols[3];
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

    }

    public BooleanGem(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("twoDash"), data.Bool("oneUse"), data.Attr("path", "objects/refill/"), data.Attr("particleColors", "d3edff,94a5ef,a5c3ff,6c74dd"))
    {
        P_Shatter = new ParticleType(Refill.P_Shatter);
        P_Regen = new ParticleType(Refill.P_Regen);
        P_Glow = new ParticleType(Refill.P_Glow);
        P_ShatterTwo = new ParticleType(Refill.P_ShatterTwo);
        P_RegenTwo = new ParticleType(Refill.P_RegenTwo);
        P_GlowTwo = new ParticleType(Refill.P_GlowTwo);
        flag = data.Attr("flag", "refill_fLag");
        stopMomentum = data.Bool("stopMomentum", true);
        alwaysUse = data.Bool("alwaysUse", false);
        flagmode = (FlagModes)data.Int("flagMode", 0);
        duration = data.Int("duration", 1);
        refillDash = data.Bool("refillDash", true);
        refillStamina = data.Bool("refillStamina", true);
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
            level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
        }
        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        bloom.Alpha = light.Alpha * 0.8f;
        if (Scene.OnInterval(2f) && sprite.Visible)
        {
            flash.Play("flash", restart: true);
            flash.Visible = true;
        }
    }

    private void Respawn()
    {
        if (!Collidable)
        {
            Collidable = true;
            sprite.Visible = true;
            outline.Visible = false;
            Depth = -100;
            wiggler.Start();
            Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
            level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
        }
    }

    private void UpdateY()
    {
        Sprite obj = flash;
        Sprite obj2 = sprite;
        float num2 = bloom.Y = sine.Value * 2f;
        float num5 = obj.Y = obj2.Y = num2;
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
        if (UseRefill2(player, twoDashes))
        {
            if (stopMomentum) player.Speed = Vector2.Zero;
            Add(new Coroutine(FlagRoutine(flag)));
            Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = 2.5f;
        }
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
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - (float)Math.PI / 2f);
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + (float)Math.PI / 2f);
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
            if (refillDash) player.Dashes = num;
            if (refillStamina) player.RefillStamina();
            flag = true;
        }

        if (flag || alwaysUse)
        {
            return true;
        }
        return false;
    }
    public IEnumerator FlagRoutine(string flag)
    {
        Level level = (Scene as Level);
        float pause = duration / 60f;
        switch (flagmode)
        {
            case FlagModes.OnThenOff:
                level.Session.SetFlag(flag, true);
                if (duration <= -1) yield break;
                yield return pause;
                level.Session.SetFlag(flag, false);
                break;
            case FlagModes.OffThenOn:
                level.Session.SetFlag(flag, false);
                if (duration <= -1) yield break;
                yield return pause;
                level.Session.SetFlag(flag, true);
                break;
            case FlagModes.ToggleTwice:
                level.Session.SetFlag(flag, !level.Session.GetFlag(flag));
                if (duration <= -1) yield break;
                yield return pause;
                level.Session.SetFlag(flag, !level.Session.GetFlag(flag));
                break;
        }

    }
}
