using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.FemtoHelper.Entities;
[CustomEntity("FemtoHelper/UpendRefill")]
public class UpendRefill : Entity
{
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
        Color = Calc.HexToColor("ffbdb0"),
        Color2 = Calc.HexToColor("ffbdb0")
    };

    private static ParticleType p_regen = new(Refill.P_Regen)
    {
        Color = Calc.HexToColor("ff8770"),
        Color2 = Calc.HexToColor("ff8770")
    };

    private static ParticleType p_glow = new(Refill.P_Glow)
    {
        Color = Calc.HexToColor("ff8770"),
        Color2 = Calc.HexToColor("ff8770")
    };

    private float respawnTimer;

    private enum Types
    {
        Horizontal,
        Vertical
    }

    private readonly Types type;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public UpendRefill(Vector2 position, EntityData data)
        : base(position)
    {
        base.Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        this.oneUse = data.Bool("oneUse", false);
        type = data.Enum("type", Types.Horizontal);
        string text;
        text = "objects/FemtoHelper/upendRefill/" + (type == Types.Horizontal ? "h/" : "v/");
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
    public UpendRefill(EntityData data, Vector2 offset)
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
        player.StateMachine.ForceState(Player.StNormal);
        if(type == Types.Vertical)
        {
            player.Speed.Y *= -2;
        } 
        else
        {
            player.Speed.X *= -2;
        }
        Audio.Play("event:/game/general/diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Collidable = false;
        Add(new Coroutine(UpendRefillRoutine(player)));
        respawnTimer = 2.5f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator UpendRefillRoutine(Player player)
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
