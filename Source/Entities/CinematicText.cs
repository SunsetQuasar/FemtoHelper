using Celeste.Mod.FemtoHelper.Utils;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework.Graphics;
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
    public readonly PlutoniumText Text;
    public readonly float Parallax;
    public int FinalStringLen = 0;
    public char MovingChar = ' ';
    public float MovingCharPercent;
    public bool Active = false;
    public Vector2 MovingCharOffset;
    public readonly float Delay;
    public readonly float SpeedMultiplier;
    public readonly string Audio;
    public bool Entered;
    public float Timer;

    public float DisappearDelay;
    public float DisappearPercent = 1f;

    public int Cur = 0;

    public readonly TextEffectData EffectData;

    public readonly SoundSource SoundSource;

    public readonly string ActivationTag;
    public readonly string NextTextTag;

    public readonly Regex NoSound = new Regex(@"\.|!|,| |\?|\/|'|\*");

    public readonly float Scale;
    public readonly bool Hud;

    public readonly bool TruncateSliders;
    public readonly bool IgnoreRegex;

    public readonly bool OnlyOnce;
    public EntityID Id;

    public readonly bool InstantReload;
    public bool HasInstantReloaded;

    public readonly bool InstantLoad;
    public readonly bool Retriggerable;

    public readonly List<PlutoniumTextNodes.Node> Nodes;

    public readonly VirtualRenderTarget Buffer;

    public bool Finished;

    public bool StopText;

    public readonly string VisibilityFlag;

    public readonly Vector2 RenderOffset;
    public CinematicText(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {

        TruncateSliders = data.Bool("truncateSliderValues", false);

        if (data.NodesOffset(offset).Length > 0) Position = data.NodesOffset(offset)[0];
        if (data.NodesOffset(offset).Length > 1)
        {
            RenderOffset = data.NodesOffset(offset)[1] - Position;
        }

        Nodes = [];

        Str = Dialog.Clean(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example"));

        string[] splitStr = MyRegex().Split(Dialog.Get(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example")));
        string[] splitStr2 = new string[splitStr.Length];
        int num = 0;
        foreach (var t in splitStr)
        {
            if (!string.IsNullOrEmpty(t))
            {
                splitStr2[num++] = t;
            }
        }

        for (int i = 0; i < splitStr2.Length; i++)
        {
            if (splitStr2[i] == "{")
            {
                i++;

                for (; i < splitStr2.Length && splitStr2[i] != "}"; i++)
                {
                    if (string.IsNullOrWhiteSpace(splitStr2[i])) continue;
                    
                    string[] splitOnceAgain = splitStr2[i].Split(';');
                    if (splitOnceAgain.Length == 3)
                    {
                        Nodes.Add(new PlutoniumTextNodes.Flag(splitOnceAgain[0], splitOnceAgain[1], splitOnceAgain[2]));
                    }
                    else
                    {
                        if (splitStr2[i][0] == '@') {
                            Nodes.Add(new PlutoniumTextNodes.Slider(splitStr2[i].Remove(0,1), TruncateSliders));
                        } else {
                            Nodes.Add(new PlutoniumTextNodes.Counter(splitStr2[i]));
                        }
                    }
                }
            }
            else
            {
                Nodes.Add(new PlutoniumTextNodes.Text(splitStr2[i]));
            }
        }

        Color1 = Calc.HexToColorWithAlpha(data.Attr("mainColor", "ffffffff"));
        Color2 = Calc.HexToColorWithAlpha(data.Attr("outlineColor", "000000ff"));

        Depth = data.Int("depth", -100);

        Shadow = data.Bool("shadow", false);
        Spacing = data.Int("spacing", 7);

        string path = data.Attr("fontPath", "objects/FemtoHelper/PlutoniumText/example");
        string list = data.Attr("charList", " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'\".,ç");

        Vector2 size = new Vector2(data.Int("fontWidth", 7), data.Int("fontHeight", 7));

        Add(Text = new PlutoniumText(path, list, size));

        Add(new BeforeRenderHook(BeforeRender));

        Parallax = data.Float("parallax", 1);

        MovingCharOffset = new Vector2(data.Float("charOriginX", 0), data.Float("charOriginY", -8));

        Delay = data.Float("delay", 0f);

        SpeedMultiplier = data.Float("speed", 5f);

        Audio = data.Attr("textSound", "event:/FemtoHelper/example_text_sound");

        DisappearDelay = data.Float("disappearTime", 3f);

        if (data.Bool("effects", false)) EffectData = new TextEffectData(
            data.Bool("wave", false),
            new Vector2(data.Float("waveX", 0), data.Float("waveY", 2)),
            data.Float("wavePhaseOffset", 90) * Calc.DegToRad,
            data.Bool("shake", false),
            data.Float("shakeAmount", 2),
            data.Bool("obfuscated", false),
            data.Bool("twitch", false),
            data.Float("twitchChance", 5f) / 100f,
            data.Float("phaseIncrement", 25f) * Calc.DegToRad,
            data.Float("waveSpeed", 5f)
            );
        else EffectData = new TextEffectData();

        SoundSource = new SoundSource()
        {
            Position = Position
        };

        ActivationTag = data.Attr("activationTag", "tag1");
        NextTextTag = data.Attr("nextTextTag", "");

        Scale = data.Float("scale", 1);
        Hud = data.Bool("hud", false);
        if (Hud)
        {
            Tag |= TagsExt.SubHUD;
            Buffer = VirtualContent.CreateRenderTarget("plutoniumText_" + id.ToString(), 1920, 1080);
        }
        else
        {
            Buffer = VirtualContent.CreateRenderTarget("plutoniumText_" + id.ToString(), 320, 180);
        }

        IgnoreRegex = data.Bool("ignoreAudioRegex", false);
        OnlyOnce = data.Bool("onlyOnce", false);
        InstantReload = data.Bool("instantReload", false);
        InstantLoad = data.Bool("instantLoad", false);

        Retriggerable = data.Bool("retriggerable", false);

        VisibilityFlag = data.Attr("visibilityFlag", "");

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
        Active = Entered = HasInstantReloaded = true;
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
        if (!Entered || Active) return;
        if (Timer > 0)
        {
            Timer = Math.Max(Timer - Engine.DeltaTime, 0);
            return;
        }
        Active = true;
        Add(new Coroutine(Sequence()));
        if(InstantReload) (Scene as Level)?.Session.SetFlag("PlutoniumInstaReload_" + Id);
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
        while(count < DisappearDelay || (int)DisappearDelay == -1)
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
            Active = Entered = false;
            DisappearPercent = 1f;
            FinalStringLen = 0;
            Finished = false;
        }
        else RemoveSelf();
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
            Active = Entered = false;
            DisappearPercent = 1f;
            FinalStringLen = 0;
            Finished = false;
        }
        else RemoveSelf();
    }

    public void BeforeRender()
    {

        string finalString2 = Str[0..FinalStringLen];
        if (Finished) finalString2 = PlutoniumTextNodes.ConstructString(Nodes, SceneAs<Level>());

        if (!Active) return;

        Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);

        Vector2 position = (Scene as Level).Camera.Position;
        Vector2 vector = position + new Vector2(160f, 90f);
        Vector2 position2 = Position - position + (Position - vector) * (Parallax - 1) + position;

        int offset = FinalStringLen * Spacing;

        float scale2 = Scale;

        position2 -= position;

        if (Hud)
        {
            position2 *= 6;
            scale2 *= 6;
        }

        //outlines

        Text.Print(position2 + RenderOffset * (Hud ? 6 : 1), finalString2, Shadow, Spacing, Color.Transparent, Color2, EffectData, scale2);
        Text.Print(position2 + RenderOffset * (Hud ? 6 : 1) + MovingCharOffset * Ease.SineInOut(1 - MovingCharPercent) * scale2 + Vector2.UnitX * offset * scale2, MovingChar.ToString(), Shadow, Spacing, Color.Transparent, Color2 * MovingCharPercent, EffectData, scale2, Cur);

        //main text

        Text.Print(position2 + RenderOffset * (Hud ? 6 : 1), finalString2, Shadow, Spacing, Color1, Color.Transparent, EffectData, scale2);
        Text.Print(position2 + RenderOffset * (Hud ? 6 : 1) + MovingCharOffset * Ease.SineInOut(1 - MovingCharPercent) * scale2 + Vector2.UnitX * offset * scale2, MovingChar.ToString(), Shadow, Spacing, Color1 * MovingCharPercent, Color.Transparent, EffectData, scale2, Cur);

        Draw.SpriteBatch.End();
    }

    public override void Render()
    {
        MTexture orDefault = GFX.ColorGrades.GetOrDefault((Scene as Level).lastColorGrade, GFX.ColorGrades["none"]);
        MTexture orDefault2 = GFX.ColorGrades.GetOrDefault((Scene as Level).Session.ColorGrade, GFX.ColorGrades["none"]);
        if ((Scene as Level).colorGradeEase > 0f && orDefault != orDefault2)
        {
            ColorGrade.Set(orDefault, orDefault2, (Scene as Level).colorGradeEase);
        }
        else
        {
            ColorGrade.Set(orDefault2);
        }

        base.Render();

        if (!Active || !(Scene as Level).FancyCheckFlag(VisibilityFlag)) return;

        float alpha = Ease.SineInOut(DisappearPercent);

        if (Hud)
        {
            SubHudRenderer.EndRender();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, Engine.ScreenMatrix.M11 * 6 < 6 ? Matrix.Identity : Engine.ScreenMatrix);
        }

        Draw.SpriteBatch.Draw(Buffer, Hud ? Vector2.Zero : (Scene as Level).Camera.Position, null, Color.White * alpha, 0, Vector2.Zero, 1, SaveData.Instance.Assists.MirrorMode && Hud ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

        if (Hud)
        {
            SubHudRenderer.EndRender();

            SubHudRenderer.BeginRender();
        }
    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        Vector2 position = (Scene as Level).Camera.Position;
        Vector2 vector = position + new Vector2(160f, 90f);
        Vector2 position2 = Position - position + (Position - vector) * (Parallax - 1) + position;
        Draw.HollowRect(position2.X - 2f, position2.Y - 2f, 4f, 4f, Color.BlueViolet);
    }

    [GeneratedRegex("(\\s|\\{|\\})")]
    private static partial Regex MyRegex();
}
