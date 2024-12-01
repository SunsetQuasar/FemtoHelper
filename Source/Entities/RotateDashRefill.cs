using System;
using System.Linq;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
public class ExtraTrailManager() : Component(true, true)
{
    public Player Player;
    public float DashTrailTimer;
    public int DashTrailCounter;
    public int DashParticleCount;

    public override void Added(Entity entity)
    {
        base.Added(entity);
        Player = entity as Player;
    }

    public override void Update()
    {
        base.Update();
        if (Player != null)
        {
            if (Player.StateMachine.state == Player.StDash)
            {
                DashTrailTimer = 0f;
                DashTrailCounter = 0;
            }

            if (DashTrailTimer > 0f)
            {
                DashTrailTimer -= Engine.DeltaTime;
                if (DashTrailTimer <= 0f)
                {
                    Player.CreateTrail();
                    DashTrailCounter--;
                    if (DashTrailCounter > 0)
                    {
                        DashTrailTimer = 0.07f;
                    }
                }
            }
            if (Player.Speed != Vector2.Zero && Scene.OnInterval(0.02f) && DashParticleCount > 0)
            {
                ParticleType type = ((!Player.wasDashB) ? Player.P_DashA : ((Player.Sprite.Mode != PlayerSpriteMode.MadelineAsBadeline) ? Player.P_DashB : Player.P_DashBadB));
                Player.level.ParticlesFG.Emit(type, Player.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), Player.DashDir.Angle());
                DashParticleCount--;
            }
        } 
        else
        {
            DashTrailTimer = 0f;
            DashTrailCounter = 0;
            DashParticleCount = 0;
        }
    }
}

[Tracked]
[CustomEntity("FemtoHelper/RotateDashRefill")]
public class RotateDashRefill : Entity
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

    public readonly float Angle;

    public readonly float Scalar;

    public readonly Color[] EffectColors;

    public RotateDashRefill(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        twoDashes = false;
        oneUse = data.Bool("oneUse", false);
        Angle = data.Float("angle", 90);
        Scalar = data.Float("scalar", 1.5f);
        EffectColors = data.Attr("effectColors", "7958ad,cbace6,634691").Split(',').Select(Calc.HexToColor).ToArray();
        if (EffectColors.Length != 3) EffectColors = [Calc.HexToColor("7958ad"), Calc.HexToColor("cbace6"), Calc.HexToColor("634691")];
        string text = data.Attr("texture", "objects/refill/");
        string[] colors = data.Attr("colors", "dba0d0,ca6dd1,e6aec1,e376df").Split(',');
        if (colors.Length != 4) colors = "dba0d0,ca6dd1,e6aec1,e376df".Split(',');
        pShatter = new ParticleType(Refill.P_Shatter);
        pRegen = new ParticleType(Refill.P_Regen);
        pGlow = new ParticleType(Refill.P_Glow);
        pShatter.Color = Calc.HexToColor(colors[0]);
        pShatter.Color2 = Calc.HexToColor(colors[1]);
        pRegen.Color = pGlow.Color = Calc.HexToColor(colors[2]);
        pRegen.Color2 = pGlow.Color2 = Calc.HexToColor(colors[3]);
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
            sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
        }));
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.8f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        UpdateY();
        Depth = -100;
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
        float num2 = (bloom.Y = sine.Value * 2f);
        float num5 = (flash.Y = (sprite.Y = num2));
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
        if (FemtoModule.Session.HasRotateDash && FemtoModule.Session.RotateDashAngle == Angle.ToRad() &&
            FemtoModule.Session.RotateDashScalar == Scalar) return;
        player.UseRefill(twoDashes);
        Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        FemtoModule.Session.HasRotateDash = true;
        FemtoModule.Session.RotateDashAngle = Angle.ToRad();
        FemtoModule.Session.RotateDashScalar = Scalar;
        FemtoModule.Session.RotateDashColors = EffectColors;
        Collidable = false;
        Add(new Coroutine(RefillRoutine(player)));
        respawnTimer = 2.5f;
    }

    private IEnumerator RefillRoutine(Player player)
    {
        Celeste.Freeze(0.05f);
        yield return null;
        level.Shake();
        sprite.Visible = (flash.Visible = false);
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

    public static void Load()
    {
        On.Celeste.LevelLoader.LoadingThread += RotateDashInitialize;
        On.Celeste.PlayerHair.GetHairColor += RotateDashCustomColor;
        On.Celeste.Player.Die += RotateDashDeathHook;
        On.Celeste.Player.DashBegin += RotateDashBeginHook;
        On.Celeste.Player.DashCoroutine += RotateDashCoroutineHook;
        On.Celeste.Player.Added += RotateDashAddComponent;
        On.Celeste.Player.Update += RotateDashBugCheck;
    }

    public static void Unload()
    {
        On.Celeste.LevelLoader.LoadingThread -= RotateDashInitialize;
        On.Celeste.PlayerHair.GetHairColor -= RotateDashCustomColor;
        On.Celeste.Player.Die -= RotateDashDeathHook;
        On.Celeste.Player.DashBegin -= RotateDashBeginHook;
        On.Celeste.Player.DashCoroutine -= RotateDashCoroutineHook;
        On.Celeste.Player.Added -= RotateDashAddComponent;
        On.Celeste.Player.Update -= RotateDashBugCheck;
    }



    private static void RotateDashInitialize(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
    {
        orig(self);
        RotateDashInitialize();
    }

    private static PlayerDeadBody RotateDashDeathHook(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        RotateDashInitialize();
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }

    private static Color RotateDashCustomColor(On.Celeste.PlayerHair.orig_GetHairColor orig, PlayerHair self, int index)
    {
        return FemtoModule.Session.HasRotateDash ? Color.Lerp(FemtoModule.Session.RotateDashColors[0], FemtoModule.Session.RotateDashColors[1], (float)(Math.Sin(self.Scene.TimeActive * 4) * 0.5f) + 0.5f) : orig(self, index);
    }
    private static IEnumerator RotateDashCoroutineHook(On.Celeste.Player.orig_DashCoroutine orig, Player self)
    {
        if (FemtoModule.Session.HasRotateDash)
        {
            Engine.TimeRate = 0.05f;
            self.Add(new Coroutine(CodecumberPortStuff.RotateDashAlignAnim(self)));
            yield return 0.005f;
            Engine.TimeRate = 1;

            self.Speed = Vector2.Transform(self.Speed, Matrix.CreateRotationZ(-FemtoModule.Session.RotateDashAngle));
            (self.Scene as Level).DirectionalShake(self.Speed.SafeNormalize());
            self.Speed *= FemtoModule.Session.RotateDashScalar;
            self.StateMachine.State = 0;
            FemtoModule.Session.HasRotateDash = false;
            FemtoModule.Session.HasStartedRotateDashing = false;
            ExtraTrailManager t = self.Get<ExtraTrailManager>();
            if(t != null)
            {
                t.DashTrailTimer = 0.06f;
                t.DashTrailCounter = 3;
                t.DashParticleCount = 10;
            }
            self.level.Displacement.AddBurst(self.Center, 0.4f, 8f, 64f, 0.5f, Ease.QuadOut, Ease.QuadOut);
            yield return null;

        }
        else
        {
            yield return new SwapImmediately(orig(self));
        }
    }
        

    private static void RotateDashBeginHook(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        Vector2 tempSpeed = Vector2.Zero;
        tempSpeed = self.Speed;
        orig(self);
        if (!FemtoModule.Session.HasRotateDash) return;
        self.Speed = tempSpeed;
        FemtoModule.Session.HasStartedRotateDashing = true;
    }

    private static void RotateDashBugCheck(On.Celeste.Player.orig_Update orig, Player self)
    {
        if (FemtoModule.Session.HasStartedRotateDashing && FemtoModule.Session.HasRotateDash && self.StateMachine.State != 2)
        {
            FemtoModule.Session.HasRotateDash = false;
            FemtoModule.Session.HasStartedRotateDashing = false;
            Engine.TimeRate = 1;
        }
        orig(self);
    }

    private static void RotateDashAddComponent(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
    {
        orig(self, scene);
        self.Add(new RotateDashIndicator());
        self.Add(new ExtraTrailManager());
    }

    public static void RotateDashInitialize()
    {
        FemtoModule.Session.HasRotateDash = false;
        FemtoModule.Session.RotateDashAngle = 0;
        FemtoModule.Session.RotateDashScalar = 1;
        FemtoModule.Session.HasStartedRotateDashing = false;
    }
}