using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Celeste;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.FemtoHelper.Entities;
[CustomEntity("FemtoHelper/SparkRefill")]
public class SparkRefill : Entity
{
    public class SparkDash() : Component(false, false)
    {

    }


    private Sprite sprite;

    private Sprite flash;

    private Image outline;

    private Wiggler wiggler;

    private BloomPoint bloom;

    private VertexLight light;

    private Level level;

    private SineWave sine;

    private bool oneUse;

    private static ParticleType p_shatter = new(Refill.P_Shatter)
    {
        Color = Calc.HexToColor("fff8bc"),
        Color2 = Calc.HexToColor("fff8bc")
    };

    private static ParticleType p_regen = new(Refill.P_Regen)
    {
        Color = Calc.HexToColor("bdb040"),
        Color2 = Calc.HexToColor("bdb040")
    };

    private static ParticleType p_glow = new(Refill.P_Glow)
    {
        Color = Calc.HexToColor("bdb040"),
        Color2 = Calc.HexToColor("bdb040")
    };

    private float respawnTimer;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public SparkRefill(Vector2 position, EntityData data)
        : base(position)
    {
        base.Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        this.oneUse = data.Bool("oneUse", false);
        string text;
        text = "objects/FemtoHelper/sparkRefill/";
        Add(outline = new Image(GFX.Game[text + "outline"]));
        outline.CenterOrigin();
        outline.Visible = false;
        Add(sprite = new Sprite(GFX.Game, text + "idle"));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        Add(flash = new Sprite(GFX.Game, text + "flash"));
        flash.Add("flash", "", 0.05f);
        flash.OnFinish = (_) =>
        {
            flash.Visible = false;
        };
        flash.CenterOrigin();
        Add(wiggler = Wiggler.Create(1f, 4f, [MethodImpl(MethodImplOptions.NoInlining)] (float v) =>
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
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public SparkRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
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
        else if (base.Scene.OnInterval(0.1f))
        {
            level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Respawn()
    {
        if (!Collidable)
        {
            Collidable = true;
            sprite.Visible = true;
            outline.Visible = false;
            base.Depth = -100;
            wiggler.Start();
            Audio.Play("event:/game/general/diamond_return", Position);
            level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void UpdateY()
    {
        Sprite obj = flash;
        Sprite obj2 = sprite;
        float num2 = (bloom.Y = sine.Value * 2f);
        float y = (obj2.Y = num2);
        obj.Y = y;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        if (sprite.Visible)
        {
            sprite.DrawOutline();
        }
        base.Render();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnPlayer(Player player)
    {
        player.UseRefill(false);
        if (player.Get<SparkDash>() is { } d) d.RemoveSelf();
        player.Add(new SparkDash());
        Audio.Play("event:/game/general/diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Collidable = false;
        Add(new Coroutine(SparkRefillRoutine(player)));
        respawnTimer = 2.5f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator SparkRefillRoutine(Player player)
    {
        Celeste.Freeze(0.05f);
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
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse)
        {
            RemoveSelf();
        }
    }
}
