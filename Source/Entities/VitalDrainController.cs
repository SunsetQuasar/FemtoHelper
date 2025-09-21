using Celeste.Mod.FemtoHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Linq;
using Celeste.Mod.FemtoHelper.Utils;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/VitalDrainController")]
public class VitalDrainController : Entity
{
    private float oxygen;

    public float Oxygen
    {
        get { return oxygen; }
        set
        {
            oxygen = value;
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

        oxygen = 500f;
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        foreach (var gas in scene.Tracker.GetEntities<VitalDrainController>().Cast<VitalDrainController>().Where(gas => gas != this))
        {
            oxygen = gas.oxygen;
            gas.RemoveSelf();
        }
        Add(new Coroutine(StylegroundRoutine()));
    }

    public static void Load()
    {
        On.Celeste.Level.Update += ColorgradeHook;
    }

    public static void Unload()
    {
        On.Celeste.Level.Update -= ColorgradeHook;
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
        bool flg = Util.EvaluateExpressionAsBoolOrFancyFlag(requireFlag, level.Session);
        if (invertFlag) flg = !flg;

        if (!Util.EvaluateExpressionAsBoolOrFancyFlag(pauseFlag, level.Session))
        {
            bool overrideDrain = false;
            foreach (Entity e in player.CollideAll<VitalSafetyTrigger>())
            {
                if (e is VitalSafetyTrigger {Override: true} v && Util.EvaluateExpressionAsBoolOrFancyFlag(v.FlagToggle, level.Session))
                {
                    overrideDrain = true;
                    oxygen = Calc.Clamp(oxygen + v.OverrideValue * Engine.DeltaTime, 0f, 500f);
                }
            }
            
            if (!overrideDrain) {
                if (usingFlag ? !flg : !player.CollideCheck<VitalSafetyTrigger>())
                {
                    level.Session.SetFlag(drainingFlag, true);
                    oxygen = Calc.Approach(oxygen, 0f, drainRate * Engine.DeltaTime);
                    //oxygen = Math.Max(oxygen - drainRate * Engine.DeltaTime, 0f);
                }
                else
                {
                    level.Session.SetFlag(drainingFlag, false);
                    oxygen = Calc.Approach(oxygen, 500f, recoverRate * Engine.DeltaTime);
                    //oxygen = Calc.Clamp(oxygen + recoverRate * Engine.DeltaTime, 0f, 500f);
                }
            }
        } 
        
        float lerp = Calc.Clamp(oxygen, 0f, 500f) / 500f;

        Distort.AnxietyOrigin = new Vector2((player.Center.X - level.Camera.X) / 320f, (player.Center.Y - level.Camera.Y) / 180f);
        Distort.anxiety = 1 - lerp;

        Audio.SetMusicParam(musicParamName, Calc.ClampedMap(lerp, 1, 0, musicParamMin, musicParamMax));

        level.ZoomSnap(new Vector2(160f, 90f), Calc.ClampedMap(Ease.SineInOut(lerp), 1, 0, 1, cameraZoomTarget));


        if (oxygen <= 0f)
        {
            player.Die(fastDeath ? Vector2.Zero : Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1), false, true);
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