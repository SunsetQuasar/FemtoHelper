

// Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Celeste.Refill
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/PopRefill")]
public class PopRefill : Entity
{
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

    private ParticleType pShatter;

    private ParticleType pRegen;

    private ParticleType pGlow;

    private float respawnTimer;

    public bool DoRespawn = false;

    public float SpawnTime = 2.5f;

    
    public PopRefill(Vector2 position, bool twoDashes, bool oneUse, float spawnTime)
        : base(position)
    {
        base.Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        this.twoDashes = twoDashes;
        this.oneUse = oneUse;
        string text;
        if (twoDashes)
        {
            text = "objects/refillTwo/";
            pShatter = Refill.P_ShatterTwo;
            pRegen = Refill.P_RegenTwo;
            pGlow = Refill.P_GlowTwo;
        }
        else
        {
            text = "objects/refill/";
            pShatter = Refill.P_Shatter;
            pRegen = Refill.P_Regen;
            pGlow = Refill.P_Glow;
        }
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
        Add(wiggler = Wiggler.Create(1f, 4f,  (float v) =>
        {
            sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
        }));
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.8f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        UpdateY();
        base.Depth = -100;
        this.SpawnTime = spawnTime;
    }

    
    public PopRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Bool("twoDash"), data.Bool("oneUse"), data.Float("spawnTime"))
    {
    }

    
    public override void Added(Scene scene)
    {
        base.Added(scene);
        Collidable = false;
        sprite.Visible = false;
        flash.Visible = false;
        outline.Visible = true;
        Depth = 8999;
        respawnTimer = 2.5f;
        level = SceneAs<Level>();
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

    
    public override void Update()
    {
        base.Update();
        if (respawnTimer > 0f && DoRespawn)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
        }
        else if (base.Scene.OnInterval(0.1f))
        {
            level.ParticlesFG.Emit(pGlow, 1, Position, Vector2.One * 5f);
        }
        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        bloom.Alpha = light.Alpha * 0.8f;
        if (base.Scene.OnInterval(2f) && sprite.Visible)
        {
            flash.Play("flash", restart: true);
            flash.Visible = true;
        }
    }

    
    private void Respawn()
    {
        if (!Collidable)
        {
            outline.Color = Color.White;
            Collidable = true;
            sprite.Visible = true;
            outline.Visible = false;
            base.Depth = -100;
            wiggler.Start();
            Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
            level.ParticlesFG.Emit(pRegen, 16, Position, Vector2.One * 2f);
        }
    }

    
    private void UpdateY()
    {
        Sprite obj = flash;
        Sprite obj2 = sprite;
        float num2 = (bloom.Y = sine.Value * 2f);
        float y = (obj2.Y = num2);
        obj.Y = y;
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
        if (player.UseRefill(twoDashes))
        {
            Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = 2.5f;
        }
    }

    
    private IEnumerator RefillRoutine(Player player)
    {
        global::Celeste.Celeste.Freeze(0.05f);
        yield return null;
        level.Shake();
        Sprite obj = sprite;
        Sprite obj2 = flash;
        bool visible = false;
        obj2.Visible = false;
        obj.Visible = visible;
        if (!oneUse)
        {
            outline.Visible = true;
        }
        Depth = 8999;
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(pShatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(pShatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse)
        {
            RemoveSelf();
        }
    }
}
