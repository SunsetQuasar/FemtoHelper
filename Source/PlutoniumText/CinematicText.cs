using Celeste.Mod.FemtoHelper.PlutoniumText;
using Celeste.Mod.FemtoHelper.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/CinematicText")]
[Tracked]
public partial class CinematicText : Entity
{
    public string Str;
    public Color Color1;
    public Color Color2;
    public readonly bool Shadow;
    public readonly int Spacing;
    public readonly PlutoniumTextComponent Text;
    public readonly float Parallax;
    public int FinalStringLen = 0;
    public char MovingChar = ' ';
    public float MovingCharPercent;
    public bool Activated = false;
    public Vector2 MovingCharOffset;
    public readonly float Delay;
    public readonly float SpeedMultiplier;
    public readonly string Audio;
    public bool Entered;
    public float Timer;

    public float DisappearDelay;
    public float DisappearPercent = 1f;

    public int Cur = 0;

    public readonly SoundSource SoundSource;

    public readonly string ActivationTag;
    public readonly string NextTextTag;

    public readonly Regex NoSound = NoSoundRegex();

    public readonly float Scale;
    public bool Hud => Text?.Layer == TextLayer.HUD || Text?.Layer == TextLayer.AdditiveHUD;

    public readonly bool TruncateSliders;
    public readonly int Decimals;
    public readonly bool IgnoreRegex;

    public readonly bool OnlyOnce;
    public EntityID Id;

    public readonly bool InstantReload;
    public bool HasInstantReloaded;

    public readonly bool InstantLoad;
    public readonly bool Retriggerable;

    public readonly List<PlutoniumTextNodes.Node> Nodes;

    public bool Finished;

    public bool StopText;

    public readonly string VisibilityFlag;

    public readonly Vector2 RenderOffset;

    public Coroutine ActualSequence = null;

    private readonly bool legacy;

    public readonly Vector2 Justify;
    public readonly Vector2 Anchor;

    public readonly bool HudCanZoom;
    public float ZoomFactor => !Hud ? 1 : (HudCanZoom ? 1 : SceneAs<Level>().Zoom);
    public CinematicText(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {

        TruncateSliders = data.Bool("truncateSliderValues", false);
        Decimals = data.Int("decimals", -1);

        if (data.NodesOffset(offset).Length > 0) Position = data.NodesOffset(offset)[0];
        if (data.NodesOffset(offset).Length > 1)
        {
            RenderOffset = data.NodesOffset(offset)[1] - Position;
        }

        Anchor = Position;

        Nodes = [];

        Str = Dialog.Clean(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example"));

        if (!Dialog.Has(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example")))
        {
            Nodes.Add(new PlutoniumTextNodes.Text(Str));
        }
        else
        {
            Nodes = PlutoniumTextNodes.Parse(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example"), TruncateSliders, Decimals);
        }

        Color1 = Calc.HexToColorWithAlpha(data.Attr("mainColor", "ffffffff"));
        Color2 = Calc.HexToColorWithAlpha(data.Attr("outlineColor", "000000ff"));

        Depth = data.Int("depth", -100);

        Shadow = data.Bool("shadow", false);

        Justify = data.Vector2("justifyX", "justifyY", Vector2.Zero);

        string list = data.Attr("charList", "");
        legacy = !string.IsNullOrWhiteSpace(list);

        string fontFilePath = data.Attr("fontDataPath", data.Attr("fontPath", "objects/FemtoHelper/PlutoniumText/example"));

        Vector2 size = new(data.Int("fontWidth", 7), data.Int("fontHeight", 7));

        Spacing = legacy ? data.Int("spacing", 7) - (int)size.X : 0;
        Spacing += data.Int("extraSpacing", 0);

        //Hud = data.Bool("hud", false);

        TextEffectData effectData = new();

        if (data.Bool("effects", true))
        {
            effectData = new TextEffectData(
                data.Bool("wave", false),
                new Vector2(data.Float("waveX", 0), data.Float("waveY", 2)),
                data.Float("wavePhaseOffset", 90) * Calc.DegToRad,
                data.Bool("shake", false),
                data.Float("shakeAmount", 2),
                data.Bool("obfuscated", false),
                data.Bool("twitch", false),
                data.Float("twitchChance", 5f) / 100f,
                data.Float("phaseIncrement", 25f) * Calc.DegToRad,
                data.Float("waveSpeed", 5f),
                data.Bool("rainbow", false),
                Position
            );
        }

        TextLayer layer = data.Enum("layer", data.Bool("hud", false) ? TextLayer.HUD : TextLayer.Gameplay);
        //Add(Text = new PlutoniumTextComponent(path, list, size, Hud ? PlutoniumText.TextLayer.HUD : PlutoniumText.TextLayer.Gameplay, BeforeRenderCallback, RenderCallback));
        Add(Text = new PlutoniumTextComponent(fontFilePath, layer, BeforeRenderCallback, RenderCallback, effectData, list, size));

        Parallax = data.Float("parallax", 1);

        MovingCharOffset = new Vector2(data.Float("charOriginX", 0), data.Float("charOriginY", -8));

        Delay = data.Float("delay", 0f);

        SpeedMultiplier = data.Float("speed", 5f);

        Audio = data.Attr("textSound", "event:/FemtoHelper/example_text_sound");

        DisappearDelay = data.Float("disappearTime", 3f);

        SoundSource = new SoundSource()
        {
            Position = this.Position
        };

        ActivationTag = data.Attr("activationTag", "tag1");
        NextTextTag = data.Attr("nextTextTag", "");

        HudCanZoom = data.Bool("hudZoomSupport", false);

        Scale = data.Float("scale", 1);

        IgnoreRegex = data.Bool("ignoreAudioRegex", false);
        OnlyOnce = data.Bool("onlyOnce", false);
        InstantReload = data.Bool("instantReload", false);
        InstantLoad = data.Bool("instantLoad", false);

        Retriggerable = data.Bool("retriggerable", false);

        VisibilityFlag = data.Attr("visibilityFlag", "");

        Add(new BeforeRenderHook(BeforeRender));

        Id = id;
    }

    public override void Awake(Scene scene)
    {
        Str = PlutoniumTextNodes.ConstructString(Nodes, scene as Level);

        base.Awake(scene);

        if (string.IsNullOrEmpty(ActivationTag)) Enter();

        if (InstantLoad || InstantReload && ((scene as Level)?.Session.GetFlag("PlutoniumInstaReload_" + Id) ?? false))
        {
            DoInstantReload(0f);
        }
    }

    public void DoInstantReload(float extra)
    {
        if (HasInstantReloaded) return;
        Activated = Entered = HasInstantReloaded = true;
        Str = PlutoniumTextNodes.ConstructString(Nodes, Scene as Level);
        FinalStringLen = Str.Length;
        Add(new Coroutine(InstaSequence()));
        foreach (CinematicText t in Scene.Tracker.GetEntities<CinematicText>())
        {
            if (t.ActivationTag != NextTextTag) continue;

            float extraTime = extra + 1 / (t.SpeedMultiplier == 0 ? t.SpeedMultiplier : float.Epsilon) * (t.Str.Length - t.Str.Count(f => f == ' '));
            t.DisappearDelay += extraTime;
            t.DoInstantReload(extraTime);
            // simulate the next text string being formed later by literally calculating how much time it takes to form and pretending it takes that much longer to disappear. fucking lol.
        }
    }

    public void Enter()
    {
        if (Entered) return;
        Timer = Delay;
        Entered = true;
    }

    public override void Update()
    {
        base.Update();
        if (!Entered || Activated) return;
        if (Timer > 0)
        {
            Timer = Math.Max(Timer - Engine.DeltaTime, 0);
            return;
        }
        Activated = true;
        if (ActualSequence == null) Add(ActualSequence = new Coroutine(Sequence()));
        if (InstantReload) (Scene as Level)?.Session.SetFlag("PlutoniumInstaReload_" + Id);
    }

    public IEnumerator Sequence()
    {
        for (Cur = 0; Cur < Str.Length; Cur++)
        {
            MovingChar = Str[Cur];
            MovingCharPercent = 0f;
            while (MovingCharPercent < 1f && MovingChar != ' ')
            {
                MovingCharPercent = Math.Min(MovingCharPercent + Engine.DeltaTime * SpeedMultiplier, 1);
                yield return null;
            }
            if ((!NoSound.IsMatch(MovingChar.ToString()) || IgnoreRegex && MovingChar != ' ') && !string.IsNullOrEmpty(Audio)) SoundSource.Play(Audio);
            FinalStringLen++;
        }
        MovingChar = ' ';
        if (!string.IsNullOrEmpty(NextTextTag))
        {
            foreach (CinematicText t in Scene.Tracker.GetEntities<CinematicText>())
            {
                if (t.ActivationTag == NextTextTag)
                {
                    t.Enter();
                }
            }
        }
        Finished = true;
        float count = 0f;
        while (count < DisappearDelay || (int)DisappearDelay == -1)
        {
            if (StopText) break;
            count += Engine.DeltaTime;
            yield return null;
        }
        for (DisappearPercent = 1f; DisappearPercent >= 0; DisappearPercent -= Engine.DeltaTime)
        {
            yield return null;
        }
        if (OnlyOnce) (Scene as Level)?.Session.DoNotLoad.Add(Id);
        if (Retriggerable)
        {
            StopText = false;
            Activated = Entered = false;
            DisappearPercent = 1f;
            FinalStringLen = 0;
            Finished = false;
        }
        else RemoveSelf();
        ActualSequence = null;
    }

    public IEnumerator InstaSequence()
    {
        Finished = true;
        float count = 0f;
        while (count < DisappearDelay || (int)DisappearDelay == -1)
        {
            if (StopText) break;
            count += Engine.DeltaTime;
            yield return null;
        }
        for (DisappearPercent = 1f; DisappearPercent >= 0; DisappearPercent -= Engine.DeltaTime)
        {
            yield return null;
        }
        if (Retriggerable)
        {
            StopText = false;
            Activated = Entered = false;
            DisappearPercent = 1f;
            FinalStringLen = 0;
            Finished = false;
        }
        else RemoveSelf();
    }

    public void BeforeRender()
    {
        if (Text.Layer == TextLayer.Gameplay) BeforeRenderCallback(SceneAs<Level>());
    }


    public override void Render()
    {
        if (Text.Layer == TextLayer.Gameplay) RenderCallback(SceneAs<Level>());
    }

    public void BeforeRenderCallback(Level level)
    {

    }

    public void RenderCallback(Level level)
    {
        string finalString2 = Str[0..FinalStringLen];
        if (Finished) finalString2 = PlutoniumTextNodes.ConstructString(Nodes, SceneAs<Level>());

        if (!Activated || !level.FancyCheckFlag(VisibilityFlag)) return;

        if (!level.FancyCheckFlag(VisibilityFlag)) return;

        bool flip = (SaveData.Instance?.Assists.MirrorMode ?? false);

        bool hudFlip = flip && Hud;

        float ww = 160f / level.Zoom;
        float hh = 90f / level.Zoom;

        Vector2 camPos = level.Camera.Position;
        Vector2 camCenter = camPos + new Vector2(ww, hh);
        Vector2 position2 = (((Position - camCenter) * Parallax / ZoomFactor) * new Vector2(hudFlip ? -1 : 1, 1)) + camCenter;

        Rectangle vis = Text.GetVisibilityRectangle(position2 + RenderOffset / ZoomFactor, Finished ? finalString2 : Str, Spacing, Scale, Justify, ZoomFactor);

        if (!vis.IsVisible()) return;

        float scale2 = Scale;

        if (Hud)
        {
            position2 *= 6f;
            scale2 *= 6f / ZoomFactor;
        }

        float alpha = Ease.SineInOut(DisappearPercent);

        float offset = Text.Font.StringSize(finalString2, Spacing).X + (string.IsNullOrEmpty(finalString2) ? 0f : (Text.Font.GetCharacter(MovingChar)?.GetKerning(finalString2.Last()).X ?? 0f));

        Vector2 stringSize = Text.Font.StringSize(Finished ? finalString2 : Str, Spacing) * scale2;

        position2.X -= stringSize.X * Justify.X;

        if(flip && (Text.Layer != TextLayer.HUD))
        {
            string rest = Str[FinalStringLen..Str.Length];
            position2.X += (Text.Font.StringSize(rest, Spacing).X + (string.IsNullOrEmpty(rest) ? 0f : (Text.Font.GetCharacter(MovingChar)?.GetKerning(rest.Last()).X ?? 0f))) * scale2;
            offset = Text.Font.StringSize(rest, Spacing).X - (string.IsNullOrEmpty(rest) ? 0f : (Text.Font.GetCharacter(MovingChar)?.GetKerning(rest.Last()).X ?? 0f));
            offset -= Text.Font.StringSize(rest, Spacing).X;
            offset -= Text.Font.StringSize(MovingChar.ToString(), Spacing).X;
        }

        Text.Print(position2 + (RenderOffset * ((Hud ? 6f : 1f) / ZoomFactor)), finalString2, Shadow, Spacing, Color.Transparent, Color2 * alpha, Justify * Vector2.UnitY, scale2, 0, flip && (Text.Layer != TextLayer.HUD));

        if (!Finished)
        {
            Text.EffectData.RainbowAnchor = Anchor + (offset * Vector2.UnitX);
            Text.Print(position2 + (RenderOffset * ((Hud ? 6f : 1f) / ZoomFactor)) + (((MovingCharOffset * Ease.SineInOut(1 - MovingCharPercent)) + offset * Vector2.UnitX) * scale2), MovingChar.ToString(), Shadow, Spacing, Color.Transparent, Color2 * alpha * MovingCharPercent, Justify * Vector2.UnitY, scale2, Cur, flip && (Text.Layer != TextLayer.HUD));
            Text.EffectData.RainbowAnchor = Anchor;
        }

        Text.Print(position2 + (RenderOffset * ((Hud ? 6f : 1f) / ZoomFactor)), finalString2, Shadow, Spacing, Color1 * alpha, Color.Transparent, Justify * Vector2.UnitY, scale2, 0, flip && (Text.Layer != TextLayer.HUD));

        if (!Finished)
        {
            Text.EffectData.RainbowAnchor = Anchor + (offset * Vector2.UnitX);
            Text.Print(position2 + (RenderOffset * ((Hud ? 6f : 1f) / ZoomFactor)) + (((MovingCharOffset * Ease.SineInOut(1 - MovingCharPercent)) + offset * Vector2.UnitX) * scale2), MovingChar.ToString(), Shadow, Spacing, Color1 * alpha * MovingCharPercent, Color.Transparent, Justify * Vector2.UnitY, scale2, Cur, flip && (Text.Layer != TextLayer.HUD));
            Text.EffectData.RainbowAnchor = Anchor;
        }
    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        bool flip = (SaveData.Instance?.Assists.MirrorMode ?? false);

        bool hudFlip = flip && Hud;

        Level level = SceneAs<Level>();

        float ww = 160f / level.Zoom;
        float hh = 90f / level.Zoom;

        Vector2 camPos = camera.Position;
        Vector2 camCenter = camPos + new Vector2(ww, hh);
        Vector2 position2 = (((Position - camCenter) * Parallax / ZoomFactor) * new Vector2(hudFlip ? -1 : 1, 1)) + camCenter;

        Vector2 p2f = position2.Floor();

        Draw.HollowRect(position2.X - 2f, position2.Y - 2f, 4f, 4f, Color.BlueViolet);

        Draw.HollowRect(Text.GetVisibilityRectangle(p2f + RenderOffset / ZoomFactor, PlutoniumTextNodes.ConstructString(Nodes, SceneAs<Level>()), Spacing, Scale, Justify, ZoomFactor), Color.MediumOrchid * 0.5f);
    }

    [GeneratedRegex("(\\s|\\{|\\})")]
    private static partial Regex ParseRegex();

    [GeneratedRegex(@"\.|!|,| |\?|\/|'|\*")]
    private static partial Regex NoSoundRegex();
}
