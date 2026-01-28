using System;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;
[CustomEntity("FemtoHelper/AvertRefill")]
public class AvertRefill : Entity
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

    private static ParticleType _pShatter = new(Refill.P_Shatter)
    {
        Color = Calc.HexToColor("ffadb3"),
        Color2 = Calc.HexToColor("ffadb3")
    };

    private static ParticleType _pRegen = new(Refill.P_Regen)
    {
        Color = Calc.HexToColor("ff606b"),
        Color2 = Calc.HexToColor("ff606b")
    };

    private static ParticleType _pGlow = new(Refill.P_Glow)
    {
        Color = Calc.HexToColor("ff606b"),
        Color2 = Calc.HexToColor("ff606b")
    };

    private float respawnTimer;
    private readonly float respawnTime;

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

    private Directions direction;

    
    public AvertRefill(Vector2 position, EntityData data)
        : base(position)
    {
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        this.oneUse = data.Bool("oneUse", false);
        direction = data.Enum("direction", Directions.Up);
        string text;
        text = "objects/FemtoHelper/bubbleRedirect/";
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
        Add(wiggler = Wiggler.Create(1f, 4f,  (float v) =>
        {
            sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
        }));

        sprite.Rotation = flash.Rotation = outline.Rotation = (float)direction * Calc.DegToRad;
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.8f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        UpdateY();
        Depth = -100;
        respawnTime = data.Float("respawnTime", 2.5f);
    }

    
    public AvertRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {
    }

    
    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
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
            level.ParticlesFG.Emit(_pGlow, 1, Position, Vector2.One * 5f);
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
            Audio.Play("event:/game/general/diamond_return", Position);
            level.ParticlesFG.Emit(_pRegen, 16, Position, Vector2.One * 2f);
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
        player.Speed = (-Vector2.UnitY * 240).Rotate((float)direction * Calc.DegToRad);
        Audio.Play("event:/game/general/diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Collidable = false;
        Add(new Coroutine(AvertRefillRoutine(player)));
        respawnTimer = respawnTime;
    }

    
    private IEnumerator AvertRefillRoutine(Player player)
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
        level.ParticlesFG.Emit(_pShatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(_pShatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse)
        {
            RemoveSelf();
        }
    }
}
