using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Celeste.Mod.FemtoHelper.Entities
{
    [CustomEntity("FemtoHelper/CinematicText")]
    public class CinematicText : Entity
    {
        public string str;
        public Color color1;
        public Color color2;
        public bool shadow;
        public int spacing;
        public PlutoniumText text;
        public float parallax;
        public string finalString = "";
        public char movingChar = ' ';
        public float movingCharPercent;
        public bool active = false;
        public Vector2 movingCharOffset;
        public Vector2 textOrigin;
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

        public Regex noSound = new Regex(@"\.|!|,| |\?|\/|'|\*");
        public CinematicText(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            base.Collider = new Hitbox(data.Width, data.Height);
            Add(new PlayerCollider(onPlayer));

            str = Dialog.Clean(data.Attr("dialogID", "FemtoHelper_PlutoniumText_Example"));

            color1 = Calc.HexToColorWithAlpha(data.Attr("mainColor", "ffffffff"));
            color2 = Calc.HexToColorWithAlpha(data.Attr("outlineColor", "000000ff"));

            Depth = data.Int("depth", -100);

            shadow = data.Bool("shadow", false);
            spacing = data.Int("spacing", 7);

            string path = data.Attr("fontPath", "objects/FemtoHelper/PlutoniumText/example");
            string list = data.Attr("charList", " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?'\".,ç");

            Vector2 size = new Vector2(data.Int("fontWidth", 7), data.Int("fontHeight", 7));

            Add(text = new PlutoniumText(path, list, size));

            parallax = data.Float("parallax", 1);

            movingCharOffset = new Vector2(data.Float("charOriginX", 0), data.Float("charOriginY", -8));

            delay = data.Float("delay", 0f);

            speedMultiplier = data.Float("speed", 5f);

            audio = data.Attr("textSound", "event:/FemtoHelper/example_text_sound");

            textOrigin = data.Nodes[0] + offset;

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
            soundSource.Position = Position - textOrigin;
        }

        public void onPlayer(Player player)
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
                if (!noSound.IsMatch(movingChar.ToString()) && !string.IsNullOrEmpty(audio)) soundSource.Play(audio);
                finalString += movingChar;
            }
            movingChar = ' ';
            if (disappearDelay == -1) yield break;
            yield return disappearDelay;
            for (disappearPercent = 1f; disappearPercent >= 0; disappearPercent -= Engine.DeltaTime)
            {
                yield return null;
            }
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();

            if (!active) return;

            Vector2 position = (Scene as Level).Camera.Position;
            Vector2 vector = position + new Vector2(160f, 90f);
            Vector2 position2 = (textOrigin - position + (textOrigin - vector) * (parallax - 1)) + position;

            int offset = finalString.Length * spacing;
            float alpha = Ease.SineInOut(disappearPercent);

            //outlines

            text.Print(position2, finalString, shadow, spacing, Color.Transparent, color2 * alpha, effectData);
            text.Print(position2 + (movingCharOffset * Ease.SineInOut(1 - movingCharPercent)) + (Vector2.UnitX * offset), movingChar.ToString(), shadow, spacing, Color.Transparent, color2 * movingCharPercent * alpha, effectData, cur);

            //main text

            text.Print(position2, finalString, shadow, spacing, color1 * alpha, Color.Transparent, effectData);
            text.Print(position2 + (movingCharOffset * Ease.SineInOut(1 - movingCharPercent)) + (Vector2.UnitX * offset), movingChar.ToString(), shadow, spacing, color1 * movingCharPercent * alpha, Color.Transparent, effectData, cur);
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Vector2 position = (Scene as Level).Camera.Position;
            Vector2 vector = position + new Vector2(160f, 90f);
            Vector2 position2 = (textOrigin - position + (textOrigin - vector) * (parallax - 1)) + position;
            Draw.HollowRect(position2.X - 2f, position2.Y - 2f, 4f, 4f, Color.BlueViolet);
        }
    }
}
