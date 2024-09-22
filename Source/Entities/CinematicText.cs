using Celeste.Mod.Entities;
using Celeste.Mod.FemtoHelper.Utils;
using Celeste.Mod.UI;
using Microsoft.Build.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/CinematicText")]
[Tracked]
public class CinematicText : Entity
{
    public string str;
    public Color color1;
    public Color color2;
    public bool shadow;
    public int spacing;
    public PlutoniumText text;
    public float parallax;
    public int finalStringLen = 0;
    public char movingChar = ' ';
    public float movingCharPercent;
    public bool active = false;
    public Vector2 movingCharOffset;
    public float delay;
    public float speedMultiplier;
    public string audio;
    public bool entered;
    public float timer;

    public float disappearDelay;
    public float disappearPercent = 1f;

    public int cur = 0;

    public TextEffectData effectData;

    public SoundSource soundSource;

    public string activationTag;
    public string nextTextTag;

    public Regex noSound = new Regex(@"\.|!|,| |\?|\/|'|\*");

    public float scale;
    public bool hud;
    public bool ignoreRegex;

    public bool onlyOnce;
    public EntityID id;

    public bool instantReload;
    public bool HasInstantReloaded;

    public bool instantLoad;
    public bool retriggerable;

    public List<PlutoniumTextNodes.Node> nodes;

    public VirtualRenderTarget buffer;

    public bool finished;

    public bool stopText;

    public string visibilityFlag;
    public CinematicText(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {

        if (data.NodesOffset(offset).Length > 0) Position = data.NodesOffset(offset)[0];

        nodes = new List<PlutoniumTextNodes.Node>();

        str = Dialog.Clean(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example"));

        string[] split_str = Regex.Split(Dialog.Get(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example")), "(\\s|\\{|\\})");
        string[] split_str2 = new string[split_str.Length];
        int num = 0;
        for (int i = 0; i < split_str.Length; i++)
        {
            if (!string.IsNullOrEmpty(split_str[i]))
            {
                split_str2[num++] = split_str[i];
            }
        }

        for (int i = 0; i < split_str2.Length; i++)
        {
            if (split_str2[i] == "{")
            {
                i++;

                for (; i < split_str2.Length && split_str2[i] != "}"; i++)
                {
                    if (!string.IsNullOrWhiteSpace(split_str2[i]))
                    {
                        string[] splitOnceAgain = split_str2[i].Split(';');
                        if (splitOnceAgain.Length == 3)
                        {
                            nodes.Add(new PlutoniumTextNodes.Flag(splitOnceAgain[0], splitOnceAgain[1], splitOnceAgain[2]));
                        }
                        else
                        {
                            nodes.Add(new PlutoniumTextNodes.Counter(split_str2[i]));
                        }
                    }
                }
            }
            else
            {
                nodes.Add(new PlutoniumTextNodes.Text(split_str2[i]));
            }
        }

        color1 = Calc.HexToColorWithAlpha(data.Attr("mainColor", "ffffffff"));
        color2 = Calc.HexToColorWithAlpha(data.Attr("outlineColor", "000000ff"));

        Depth = data.Int("depth", -100);

        shadow = data.Bool("shadow", false);
        spacing = data.Int("spacing", 7);

        string path = data.Attr("fontPath", "objects/FemtoHelper/PlutoniumText/example");
        string list = data.Attr("charList", " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'\".,ç");

        Vector2 size = new Vector2(data.Int("fontWidth", 7), data.Int("fontHeight", 7));

        Add(text = new PlutoniumText(path, list, size));

        Add(new BeforeRenderHook(BeforeRender));

        parallax = data.Float("parallax", 1);

        movingCharOffset = new Vector2(data.Float("charOriginX", 0), data.Float("charOriginY", -8));

        delay = data.Float("delay", 0f);

        speedMultiplier = data.Float("speed", 5f);

        audio = data.Attr("textSound", "event:/FemtoHelper/example_text_sound");

        disappearDelay = data.Float("disappearTime", 3f);

        if (data.Bool("effects", false)) effectData = new TextEffectData(
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
        else effectData = new TextEffectData();

        soundSource = new SoundSource();

        activationTag = data.Attr("activationTag", "tag1");
        nextTextTag = data.Attr("nextTextTag", "");

        scale = data.Float("scale", 1);
        hud = data.Bool("hud", false);
        if (hud)
        {
            Tag |= TagsExt.SubHUD;
            buffer = VirtualContent.CreateRenderTarget("plutoniumText_" + id.ToString(), 1920, 1080);
        }
        else
        {
            buffer = VirtualContent.CreateRenderTarget("plutoniumText_" + id.ToString(), 320, 180);
        }

        ignoreRegex = data.Bool("ignoreAudioRegex", false);
        onlyOnce = data.Bool("onlyOnce", false);
        instantReload = data.Bool("instantReload", false);
        instantLoad = data.Bool("instantLoad", false);

        retriggerable = data.Bool("retriggerable", false);

        visibilityFlag = data.Attr("visibilityFlag", "");

        this.id = id;
    }

    public override void Awake(Scene scene)
    {
        str = PlutoniumTextNodes.ConstructString(nodes, scene as Level);

        base.Awake(scene);

        if (string.IsNullOrEmpty(activationTag)) Enter();

        if (instantLoad || (instantReload && (scene as Level).Session.GetFlag("PlutoniumInstaReload_" + id.ToString())))
        {
            InstantReload(0f);
        }
    }

    public void InstantReload(float extra)
    {
        if (HasInstantReloaded) return;
        active = entered = HasInstantReloaded = true;
        finalStringLen = str.Length;
        Add(new Coroutine(InstaSequence()));
        foreach (CinematicText t in Scene.Tracker.GetEntities<CinematicText>())
        {
            if (t.activationTag == nextTextTag)
            {
                float extraTime = extra + ((1 / (t.speedMultiplier == 0 ? t.speedMultiplier : float.Epsilon)) * (t.str.Length - t.str.Count(f => f == ' ')));
                t.disappearDelay += extraTime;
                t.InstantReload(extraTime);
                // simulate the next text string being formed later by literally calculating how much time it takes to form and pretending it takes that much longer to disappear. fucking lol.
            }
        }
    }

    public void Enter()
    {
        if (entered) return;
        timer = delay;
        entered = true;
    }

    public override void Update()
    {
        base.Update();
        if (!entered || active) return;
        if (timer > 0)
        {
            timer = Math.Max(timer - Engine.DeltaTime, 0);
            return;
        }
        active = true;
        Add(new Coroutine(Sequence()));
        if(instantReload) (Scene as Level).Session.SetFlag("PlutoniumInstaReload_" + id.ToString());
    }

    public IEnumerator Sequence()
    {
        for (cur = 0; cur < str.Length; cur++)
        {
            movingChar = str[cur];
            movingCharPercent = 0f;
            while (movingCharPercent < 1f && movingChar != ' ')
            {
                movingCharPercent = Math.Min(movingCharPercent + Engine.DeltaTime * speedMultiplier, 1);
                yield return null;
            }
            if ((!noSound.IsMatch(movingChar.ToString()) || (ignoreRegex && movingChar != ' ')) && !string.IsNullOrEmpty(audio)) soundSource.Play(audio);
            finalStringLen++;
        }
        movingChar = ' ';
        if (!string.IsNullOrEmpty(nextTextTag))
        {
            foreach (CinematicText t in Scene.Tracker.GetEntities<CinematicText>())
            {
                if (t.activationTag == nextTextTag)
                {
                    t.Enter();
                }
            }
        }
        finished = true;
        float count = 0f;
        while(count < disappearDelay || disappearDelay == -1)
        {
            if (stopText) break;
            count += Engine.DeltaTime;
            yield return null;
        }
        for (disappearPercent = 1f; disappearPercent >= 0; disappearPercent -= Engine.DeltaTime)
        {
            yield return null;
        }
        if (onlyOnce) (Scene as Level).Session.DoNotLoad.Add(id);
        if (retriggerable)
        {
            stopText = false;
            active = entered = false;
            disappearPercent = 1f;
            finalStringLen = 0;
            finished = false;
        }
        else RemoveSelf();
    }

    public IEnumerator InstaSequence()
    {
        finished = true;
        float count = 0f;
        while (count < disappearDelay || disappearDelay == -1)
        {
            if (stopText) break;
            count += Engine.DeltaTime;
            yield return null;
        }
        for (disappearPercent = 1f; disappearPercent >= 0; disappearPercent -= Engine.DeltaTime)
        {
            yield return null;
        }
        if (retriggerable)
        {
            stopText = false;
            active = entered = false;
            disappearPercent = 1f;
            finalStringLen = 0;
            finished = false;
        }
        else RemoveSelf();
    }

    public void BeforeRender()
    {
        string finalString2 = str[0..finalStringLen];
        if (finished) finalString2 = PlutoniumTextNodes.ConstructString(nodes, SceneAs<Level>());

        if (!active) return;

        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, Matrix.Identity);

        Vector2 position = (Scene as Level).Camera.Position;
        Vector2 vector = position + new Vector2(160f, 90f);
        Vector2 position2 = (Position - position + (Position - vector) * (parallax - 1)) + position;

        int offset = finalStringLen * spacing;

        float scale2 = scale;

        position2 -= position;

        if (hud)
        {
            position2 *= 6;
            scale2 *= 6;
        }

        //outlines

        text.Print(position2, finalString2, shadow, spacing, Color.Transparent, color2, effectData, scale2);
        text.Print(position2 + (movingCharOffset * Ease.SineInOut(1 - movingCharPercent) * scale2) + (Vector2.UnitX * offset * scale2), movingChar.ToString(), shadow, spacing, Color.Transparent, color2 * movingCharPercent, effectData, scale2, cur);

        //main text

        text.Print(position2, finalString2, shadow, spacing, color1, Color.Transparent, effectData, scale2);
        text.Print(position2 + (movingCharOffset * Ease.SineInOut(1 - movingCharPercent) * scale2) + (Vector2.UnitX * offset * scale2), movingChar.ToString(), shadow, spacing, color1 * movingCharPercent, Color.Transparent, effectData, scale2, cur);

        Draw.SpriteBatch.End();
    }

    public override void Render()
    {
        base.Render();

        if (!active || !(Scene as Level).FancyCheckFlag(visibilityFlag)) return;

        float alpha = Ease.SineInOut(disappearPercent);

        if (hud)
        {
            SubHudRenderer.EndRender();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, (Engine.ScreenMatrix.M11 * 6) < 6 ? Matrix.Identity : Engine.ScreenMatrix);
        }

        Draw.SpriteBatch.Draw(buffer, hud ? Vector2.Zero : (Scene as Level).Camera.Position, null, Color.White * alpha, 0, Vector2.Zero, 1, (SaveData.Instance.Assists.MirrorMode && hud) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

        if (hud)
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
        Vector2 position2 = (Position - position + (Position - vector) * (parallax - 1)) + position;
        Draw.HollowRect(position2.X - 2f, position2.Y - 2f, 4f, 4f, Color.BlueViolet);
    }
}
