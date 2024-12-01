using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.FemtoHelper.Entities;
[CustomEntity("FemtoHelper/NoLightClutterSwitch")]
public class NoLightClutterSwitch : Solid
{
    public const float LightingAlphaAdd = 0.05f;

    public static readonly ParticleType PPressed = new ParticleType(ClutterSwitch.P_Pressed);

    private const int PressedAdd = 10;

    private const int PressedSpriteAdd = 2;

    private const int UnpressedLightRadius = 32;

    private const int PressedLightRadius = 24;

    private const int BrightLightRadius = 64;

    private readonly ClutterBlock.Colors color;

    private float startY;

    private float atY;

    private float speedY;

    private bool pressed;

    private readonly Sprite sprite;

    private readonly Image icon;

    private float targetXScale = 1f;

    private readonly VertexLight vertexLight;

    private bool playerWasOnTop;

    private SoundSource cutsceneSfx;


    public NoLightClutterSwitch(Vector2 position, ClutterBlock.Colors color)
        : base(position, 32f, 16f, safe: true)
    {
        this.color = color;
        startY = (atY = Y);
        OnDashCollide = OnDashed;
        SurfaceSoundIndex = 21;
        Add(sprite = GFX.SpriteBank.Create("clutterSwitch"));
        sprite.Position = new Vector2(16f, 16f);
        sprite.Play("idle");
        Add(icon = new Image(GFX.Game["objects/resortclutter/icon_" + color]));
        icon.CenterOrigin();
        icon.Position = new Vector2(16f, 8f);
        Add(vertexLight = new VertexLight(new Vector2(CenterX - X, -1f), Color.Aqua, 1f, 32, 64));
    }

    public NoLightClutterSwitch(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Enum("type", ClutterBlock.Colors.Green))
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (color == ClutterBlock.Colors.Lightning && SceneAs<Level>().Session.GetFlag("disable_lightning"))
        {
            BePressed();
        }
        else if (SceneAs<Level>().Session.GetFlag("oshiro_clutter_cleared_" + (int)color))
        {
            BePressed();
        }
    }

    private void BePressed()
    {
        pressed = true;
        atY += 10f;
        Y += 10f;
        sprite.Y += 2f;
        sprite.Play("active");
        Remove(icon);
        vertexLight.StartRadius = 24f;
        vertexLight.EndRadius = 48f;
    }

    public override void Update()
    {
        base.Update();
        if (HasPlayerOnTop())
        {
            if (speedY < 0f)
            {
                speedY = 0f;
            }
            speedY = Calc.Approach(speedY, 70f, 200f * Engine.DeltaTime);
            MoveTowardsY(atY + (pressed ? 2 : 4), speedY * Engine.DeltaTime);
            targetXScale = 1.2f;
            if (!playerWasOnTop)
            {
                Audio.Play("event:/game/03_resort/clutterswitch_squish", Position);
            }
            playerWasOnTop = true;
        }
        else
        {
            if (speedY > 0f)
            {
                speedY = 0f;
            }
            speedY = Calc.Approach(speedY, -150f, 200f * Engine.DeltaTime);
            MoveTowardsY(atY, (0f - speedY) * Engine.DeltaTime);
            targetXScale = 1f;
            if (playerWasOnTop)
            {
                Audio.Play("event:/game/03_resort/clutterswitch_return", Position);
            }
            playerWasOnTop = false;
        }
        sprite.Scale.X = Calc.Approach(sprite.Scale.X, targetXScale, 0.8f * Engine.DeltaTime);
    }

    private DashCollisionResults OnDashed(Player player, Vector2 direction)
    {
        if (!pressed && direction == Vector2.UnitY)
        {
            Celeste.Freeze(0.2f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Level obj = Scene as Level;
            obj.Session.SetFlag("oshiro_clutter_cleared_" + (int)color);
            obj.Session.SetFlag("oshiro_clutter_door_open", setTo: false);
            vertexLight.StartRadius = 64f;
            vertexLight.EndRadius = 128f;
            obj.DirectionalShake(Vector2.UnitY, 0.6f);
            obj.Particles.Emit(PPressed, 20, TopCenter - Vector2.UnitY * 10f, new Vector2(16f, 8f));
            BePressed();
            sprite.Scale.X = 1.5f;
            Add(color == ClutterBlock.Colors.Lightning
                ? new Coroutine(LightningRoutine(player))
                : new Coroutine(AbsorbRoutine(player)));
        }
        return DashCollisionResults.NormalCollision;
    }

    private IEnumerator LightningRoutine(Player player)
    {
        Level level = SceneAs<Level>();
        level.Session.SetFlag("disable_lightning");
        level.Session.Audio.Music.Progress++;
        level.Session.Audio.Apply();
        yield return Lightning.RemoveRoutine(level);
    }

    private IEnumerator AbsorbRoutine(Player player)
    {
        Add(cutsceneSfx = new SoundSource());
        float duration = 0f;
        if (color == ClutterBlock.Colors.Green)
        {
            cutsceneSfx.Play("event:/game/03_resort/clutterswitch_books");
            duration = 6.366f;
        }
        else if (color == ClutterBlock.Colors.Red)
        {
            cutsceneSfx.Play("event:/game/03_resort/clutterswitch_linens");
            duration = 6.15f;
        }
        else if (color == ClutterBlock.Colors.Yellow)
        {
            cutsceneSfx.Play("event:/game/03_resort/clutterswitch_boxes");
            duration = 6.066f;
        }
        Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
        {
            Audio.Play("event:/game/03_resort/clutterswitch_finish", Position);
        }, duration, start: true));
        player.StateMachine.State = 11;
        Vector2 target = Position + new Vector2(Width / 2f, 0f);
        ClutterAbsorbEffect effect = new ClutterAbsorbEffect();
        Scene.Add(effect);
        sprite.Play("break");
        Level level = SceneAs<Level>();
        level.Session.Audio.Music.Progress++;
        level.Session.Audio.Apply();
        Alarm.Set(this, 3f, delegate
        {
            float start2 = vertexLight.StartRadius;
            float end = vertexLight.EndRadius;
            Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 2f, start: true);
            tween2.OnUpdate = delegate (Tween t)
            {
                vertexLight.StartRadius = (int)Math.Round(MathHelper.Lerp(start2, 24f, t.Eased));
                vertexLight.EndRadius = (int)Math.Round(MathHelper.Lerp(end, 48f, t.Eased));
            };
            Add(tween2);
        });
        Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
        foreach (ClutterBlock item in Scene.Entities.FindAll<ClutterBlock>())
        {
            if (item.BlockColor == color)
            {
                item.Absorb(effect);
            }
        }
        foreach (ClutterBlockBase item2 in Scene.Entities.FindAll<ClutterBlockBase>())
        {
            if (item2.BlockColor == color)
            {
                item2.Deactivate();
            }
        }
        yield return 1.5f;
        player.StateMachine.State = 0;
        List<MTexture> images = GFX.Game.GetAtlasSubtextures("objects/resortclutter/" + color.ToString() + "_");
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Vector2 position = target + Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 320f);
                effect.FlyClutter(position, Calc.Random.Choose(images), shake: false, 0f);
            }
            level.Shake();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            yield return 0.05f;
        }
        yield return 1.5f;
        effect.CloseCabinets();
        yield return 0.2f;
        Input.Rumble(RumbleStrength.Medium, RumbleLength.FullSecond);
        yield return 0.3f;
    }

    public override void Removed(Scene scene)
    {
        Level level = scene as Level;
        base.Removed(scene);
    }
}

[CustomEntity("FemtoHelper/ClutterShadowController")]
[Tracked]
public class ClutterShadowController : Entity
{
    public readonly float EnabledAlpha;
    public readonly float DisabledAlpha;
    public Color EnabledColor;
    public Color DisabledColor;

    public ClutterShadowController(EntityData data, Vector2 offset) : base()
    {
        EnabledAlpha = data.Float("enabledAlpha", 0.7f);
        DisabledAlpha = data.Float("disabledAlpha", 0.3f);
        EnabledColor = Calc.HexToColor(data.Attr("enabledColor", "000000"));
        DisabledColor = Calc.HexToColor(data.Attr("disabledColor", "000000"));
    }

    public static void Load()
    {
        On.Celeste.ClutterBlockBase.Render += NoShadowClutterHook;
    }

    public static void Unload()
    {
        On.Celeste.ClutterBlockBase.Render -= NoShadowClutterHook;
    }

    public static void NoShadowClutterHook(On.Celeste.ClutterBlockBase.orig_Render orig, ClutterBlockBase self)
    {
        Color cl = self.color;
        ClutterShadowController c = self.Scene.Tracker.GetEntity<ClutterShadowController>();
        if (c != null)
        {
            self.color = (self.enabled ? c.EnabledColor : c.DisabledColor) * (self.enabled ? c.EnabledAlpha : c.DisabledAlpha);
        }
        orig(self);
        self.color = cl;
    }
}