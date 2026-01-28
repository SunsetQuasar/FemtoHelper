using Celeste.Mod.FemtoHelper.Triggers;
using System;
using System.Collections;
using System.Linq;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/VitalDrainController")]
public class VitalDrainController : Entity
{
    private float oxygen;
    private float lastslider;
    private readonly string oxygenSlider;
    private bool usingSlider => string.IsNullOrWhiteSpace(oxygenSlider);

    public float Oxygen
    {
        get => oxygen;
        set
        {
            oxygen = value;
            if (!string.IsNullOrWhiteSpace(oxygenSlider))
            {
                SceneAs<Level>().Session.SetSlider(oxygenSlider, value / 500f);
            }
        }
    }

    private readonly bool fastDeath;
    private readonly float drainRate;
    private readonly float recoverRate;
    private readonly string drainingFlag;
    private readonly string styleTagIn;
    private readonly string styleTagOut;
    private readonly string colorgradeA;
    private readonly string colorgradeB;
    private readonly string musicParamName;
    private readonly float musicParamMin;
    private readonly float musicParamMax;
    private readonly float cameraZoomTarget;

    private float anxiety;

    private readonly bool debugView;

    private readonly string requireFlag;
    private readonly bool usingFlag;
    private readonly bool invertFlag;
    private readonly string pauseFlag;
    
    private readonly float flashThreshold;

    public VitalDrainController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Tag = Tags.Persistent | TagsExt.SubHUD;

        Depth = Depths.Top - 1000;

        debugView = data.Bool("debugView", false);

        fastDeath = data.Bool("fastDeath", true);
        drainRate = data.Float("drainRate", 150f);
        recoverRate = data.Float("recoverRate", 1000f);
        drainingFlag = data.Attr("flag", "o2_flag");
        styleTagIn = data.Attr("fadeInTag", "o2_in_tag");
        styleTagOut = data.Attr("fadeOutTag", "o2_out_tag");
        colorgradeA = data.Attr("colorgradeA", "none");
        colorgradeB = data.Attr("colorgradeB", "none");
        musicParamName = data.Attr("musicParamName", "param_here");
        musicParamMin = data.Float("musicParamMin", 0f);
        musicParamMax = data.Float("musicParamMax", 1f);
        anxiety = data.Float("anxiety", 0.3f);
        cameraZoomTarget = data.Float("cameraZoomTarget", 1f);

        requireFlag = data.Attr("useFlag", "");
        usingFlag = !string.IsNullOrWhiteSpace(requireFlag);

        invertFlag = data.Bool("invertFlag", false);

        pauseFlag = data.String("pauseFlag", "");

        flashThreshold = data.Float("flashThreshold", 0f);

        oxygenSlider = data.String("sliderName", "");
        
        oxygen = 500f;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        foreach (var gas in scene.Tracker.GetEntities<VitalDrainController>().Cast<VitalDrainController>()
                     .Where(gas => gas != this))
        {
            Oxygen = gas.Oxygen;
            gas.RemoveSelf();
        }

        lastslider = SceneAs<Level>().Session.GetSlider(oxygenSlider);
        Add(new Coroutine(StylegroundRoutine()));
    }

    public static void Load()
    {
        On.Celeste.Level.Update += ColorgradeHook;
        On.Celeste.Player.Render += PlayerOnRender;
    }

    private static void PlayerOnRender(On.Celeste.Player.orig_Render orig, Player self)
    {
        if (self.Scene.Tracker.GetEntity<VitalDrainController>() is { } v)
        {
            float temp = self.Stamina;
            if (v.Oxygen < v.flashThreshold)
            {
                self.Stamina = 0;
            }

            orig(self);
            self.Stamina = temp;
            return;
        }

        orig(self);
    }

    public static void Unload()
    {
        On.Celeste.Level.Update -= ColorgradeHook;
        On.Celeste.Player.Render -= PlayerOnRender;
    }

    private static void ColorgradeHook(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        VitalDrainController gas = self.Tracker.GetEntity<VitalDrainController>();
        if (gas == null) return;

        float lerp = Calc.Clamp(gas.oxygen, 0f, 500f) / 500f;
        if (lerp > 0.5f)
        {
            self.lastColorGrade = gas.colorgradeB;
            self.Session.ColorGrade = gas.colorgradeA;
            self.colorGradeEase = lerp;
        }
        else
        {
            self.lastColorGrade = gas.colorgradeA;
            self.Session.ColorGrade = gas.colorgradeB;
            self.colorGradeEase = 1f - lerp;
        }

        self.colorGradeEaseSpeed = 1f;
    }


    public override void Update()
    {
        base.Update();
        Player player = Scene.Tracker.GetEntity<Player>();
        if (player == null) return;
        Level level = SceneAs<Level>();
        float currSlider = level.Session.GetSlider(oxygenSlider) * 500;
        if (currSlider != lastslider)
        {
            oxygen = currSlider;
        }
        lastslider = level.Session.GetSlider(oxygenSlider) * 500;
        bool flg = EvaluateExpressionAsBoolOrFancyFlag(requireFlag, level.Session);
        if (invertFlag) flg = !flg;

        if (!EvaluateExpressionAsBoolOrFancyFlag(pauseFlag, level.Session) || string.IsNullOrEmpty(pauseFlag))
        {
            bool overrideDrain = false;
            foreach (Entity e in player.CollideAll<VitalSafetyTrigger>())
            {
                if (e is not VitalSafetyTrigger { Override: true } v ||
                    !EvaluateExpressionAsBoolOrFancyFlag(v.FlagToggle, level.Session)) continue;
                overrideDrain = true;
                level.Session.SetFlag(drainingFlag, Math.Sign(v.OverrideValue) == -1);
                Oxygen = Calc.Clamp(Oxygen + v.OverrideValue * Engine.DeltaTime, 0f, 500f);
            }

            if (!overrideDrain)
            {
                if (usingFlag ? !flg : !player.CollideCheck<VitalSafetyTrigger>())
                {
                    level.Session.SetFlag(drainingFlag, Math.Sign(drainRate) == 1);
                    Oxygen = Calc.Clamp(Oxygen - drainRate * Engine.DeltaTime, 0f, 500f);
                    //oxygen = Math.Max(oxygen - drainRate * Engine.DeltaTime, 0f);
                }
                else
                {
                    level.Session.SetFlag(drainingFlag, Math.Sign(recoverRate) == -1);
                    Oxygen = Calc.Clamp(Oxygen + recoverRate * Engine.DeltaTime, 0f, 500f);
                    //oxygen = Calc.Clamp(oxygen + recoverRate * Engine.DeltaTime, 0f, 500f);
                }
            }
        }

        float lerp = Calc.Clamp(Oxygen, 0f, 500f) / 500f;

        Distort.AnxietyOrigin = new Vector2((player.Center.X - level.Camera.X) / 320f,
            (player.Center.Y - level.Camera.Y) / 180f);
        Distort.anxiety = 1 - lerp;

        Audio.SetMusicParam(musicParamName, Calc.ClampedMap(lerp, 1, 0, musicParamMin, musicParamMax));

        level.ZoomSnap(new Vector2(160f, 90f), Calc.ClampedMap(Ease.SineInOut(lerp), 1, 0, 1, cameraZoomTarget));


        if (oxygen <= 0f)
        {
            player.Die(fastDeath ? Vector2.Zero : Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1),
                false, true);
        }

        level.Session.SetFlag("o2_flag_hcd", !(oxygen >= 500));
    }

    public override void Render()
    {
        base.Render();

        if (!debugView) return;

        Draw.Rect(Vector2.One * 12, 256, 24, Color.Red);
        Draw.Rect(Vector2.One * 12, 256 * (Calc.Clamp(oxygen, 0f, 500f) / 500f), 24, Color.Green);
    }

    public IEnumerator StylegroundRoutine()
    {
        while (true)
        {
            float fade = Calc.Clamp(oxygen, 0f, 500f) / 500f;
            float fadeInv = 1 - fade;
            foreach (Backdrop item in SceneAs<Level>().Background.GetEach<Backdrop>(styleTagIn))
            {
                item.ForceVisible = true;
                item.FadeAlphaMultiplier = fadeInv;
            }

            foreach (Backdrop item in SceneAs<Level>().Background.GetEach<Backdrop>(styleTagOut))
            {
                item.ForceVisible = true;
                item.FadeAlphaMultiplier = fade;
            }

            foreach (Backdrop item in SceneAs<Level>().Foreground.GetEach<Backdrop>(styleTagIn))
            {
                item.ForceVisible = true;
                item.FadeAlphaMultiplier = fadeInv;
            }

            foreach (Backdrop item in SceneAs<Level>().Foreground.GetEach<Backdrop>(styleTagOut))
            {
                item.ForceVisible = true;
                item.FadeAlphaMultiplier = fade;
            }

            yield return null;
        }
    }
}