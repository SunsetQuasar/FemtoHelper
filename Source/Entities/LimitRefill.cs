using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.FemtoHelper.Entities;
[CustomEntity("FemtoHelper/LimitRefill")]
public class LimitRefill : Entity
{
    public class DirectionConstraint : Component
    {
        public bool[,] Dirs = new bool[3, 3];
        public DirectionConstraint(Directions dir) : base(false, false)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Dirs[i, j] = false;
                }
            }
            switch (dir)
            {
                default:
                    Dirs[0, 1] = true;
                    break;
                case Directions.UpRight:
                    Dirs[0, 2] = true;
                    break;
                case Directions.Right:
                    Dirs[1, 2] = true;
                    break;
                case Directions.DownRight:
                    Dirs[2, 2] = true;
                    break;
                case Directions.Down:
                    Dirs[2, 1] = true;
                    break;
                case Directions.DownLeft:
                    Dirs[2, 0] = true;
                    break;
                case Directions.Left:
                    Dirs[1, 0] = true;
                    break;
                case Directions.UpLeft:
                    Dirs[0, 0] = true;
                    break;
            }
        }
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

    private static ParticleType _pShatter = new(Refill.P_Shatter)
    {
        Color = Calc.HexToColor("7affe8"),
        Color2 = Calc.HexToColor("7affe8")
    };

    private static ParticleType _pRegen = new(Refill.P_Regen)
    {
        Color = Calc.HexToColor("00cca9"),
        Color2 = Calc.HexToColor("00cca9")
    };

    private static ParticleType _pGlow = new(Refill.P_Glow)
    {
        Color = Calc.HexToColor("00cca9"),
        Color2 = Calc.HexToColor("00cca9")
    };

    private float respawnTimer;
    private readonly float respawnTime;

    public enum Directions
    {
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,
    }

    private Directions direction;

    
    public LimitRefill(Vector2 position, EntityData data)
        : base(position)
    {
        base.Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        this.oneUse = data.Bool("oneUse", false);
        direction = data.Enum("direction", Directions.Up);
        string text;
        text = "objects/FemtoHelper/limitRefill/" + direction switch
        {
            Directions.UpRight => "upright/",
            Directions.Right => "right/",
            Directions.DownRight => "downright/",
            Directions.Down => "down/",
            Directions.DownLeft => "downleft/",
            Directions.Left => "left/",
            Directions.UpLeft => "upleft/",
            _ => "up/",
        };
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
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.8f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        UpdateY();
        base.Depth = -100;
        respawnTime = data.Float("respawnTime", 2.5f);
    }

    
    public LimitRefill(EntityData data, Vector2 offset)
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
        else if (base.Scene.OnInterval(0.1f))
        {
            level.ParticlesFG.Emit(_pGlow, 1, Position, Vector2.One * 5f);
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
            Collidable = true;
            sprite.Visible = true;
            outline.Visible = false;
            base.Depth = -100;
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
        player.UseRefill(false);
        if (player.Get<DirectionConstraint>() is DirectionConstraint d) d.RemoveSelf();
        player.Add(new DirectionConstraint(direction));
        Audio.Play("event:/game/general/diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Collidable = false;
        Add(new Coroutine(LimitRefillRoutine(player)));
        respawnTimer = respawnTime;
    }

    
    private IEnumerator LimitRefillRoutine(Player player)
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

    public static void Load()
    {
        On.Celeste.Player.DashBegin += Player_DashBegin;
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        if (self.Get<DirectionConstraint>() is { } d) d.RemoveSelf();
        orig(self);
    }

    public static void Unload()
    {
        On.Celeste.Player.DashBegin -= Player_DashBegin;
    }
}
