using Celeste.Mod.Core;
using Celeste.Mod.FemtoHelper.Metadata;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
public class SessionHeartDisplay : Entity
{
    public class HeartsCounter : Component
    {
        public Color FlashColor => Calc.HexToColor(FemtoHelperMetadata.SessionHeartMeta[CurrentSelected].Color);

        public bool Golden;

        public Vector2 Position;

        public bool CenteredX;

        public bool CanWiggle = true;

        public float Scale = 1f;

        public float Stroke = 2f;

        public float Rotation;

        public Color Color = Color.White;

        public Color OutOfColor = Color.LightGray;

        public bool OverworldSfx;

        private readonly int[] amount;

        private int outOf = -1;

        internal Wiggler wiggler, iconWiggler;

        internal float flashTimer;

        private string sAmount;

        private string sOutOf;

        private readonly MTexture x, arrow;

        private bool showOutOf;

        private int currentSelected = 0;

        private Color HighlightColor = Color.White;

        public static Color HighlightColorA = Calc.HexToColor("84FF54");

        public static Color HighlightColorB = Calc.HexToColor("FCFF59");

        public float arrowPercent = 0f;

        public int arrowLastPressed = 0;

        public int AmountOf(int id)
        {
            return id < 0 || id >= amount.Length ? 0 : amount[id];
        }

        public int CurrentSelected
        {
            get
            {
                return currentSelected;
            }
            set
            {
                if (currentSelected == value) return;
                if (value > currentSelected)
                {
                    arrowLastPressed = 1;
                    Audio.Play("event:/ui/main/rollover_up");
                }
                else if (value < currentSelected)
                {
                    arrowLastPressed = -1;
                    Audio.Play("event:/ui/main/rollover_down");
                }
                currentSelected = Utils.Mod(value, amount.Length);
                iconWiggler.Start();
                UpdateStrings();
            }
        }

        public int Amount
        {
            get
            {
                return amount[CurrentSelected];
            }
            set
            {
                if (amount[CurrentSelected] == value)
                {
                    return;
                }
                amount[CurrentSelected] = value;
                UpdateStrings();
                if (CanWiggle)
                {
                    if (OverworldSfx)
                    {
                        Audio.Play(Golden ? "event:/ui/postgame/goldberry_count" : "event:/ui/postgame/strawberry_count");
                    }
                    else
                    {
                        Audio.Play("event:/ui/game/increment_strawberry");
                    }
                    wiggler.Start();
                    flashTimer = 0.5f;
                }
            }
        }

        public int OutOf
        {
            get
            {
                return outOf;
            }
            set
            {
                outOf = value;
                UpdateStrings();
            }
        }

        public bool ShowOutOf
        {
            get
            {
                return showOutOf;
            }

            set
            {
                if (showOutOf != value)
                {
                    showOutOf = value;
                    UpdateStrings();
                }
            }
        }

        public float FullHeight
        {

            get
            {
                return Math.Max(ActiveFont.LineHeight, MTN.Journal["heartgem0"].Height);
            }
        }

        public Vector2 RenderPosition
        {

            get
            {
                return (((Entity != null) ? Entity.Position : Vector2.Zero) + Position).Round();
            }
        }


        public HeartsCounter(bool centeredX, int[] amount, int outOf = 0, bool showOutOf = false)
            : base(active: true, visible: true)
        {
            CenteredX = centeredX;
            this.amount = amount;
            this.outOf = outOf;
            this.showOutOf = showOutOf;
            UpdateStrings();
            wiggler = Wiggler.Create(0.5f, 3f);
            wiggler.StartZero = true;
            wiggler.UseRawDeltaTime = true;

            iconWiggler = Wiggler.Create(0.35f, 6f);
            iconWiggler.StartZero = true;
            iconWiggler.UseRawDeltaTime = true;
            x = GFX.Gui["x"];
            arrow = GFX.Gui["tinyarrow"];
        }


        private void UpdateStrings()
        {
            sAmount = Amount.ToString();
            if (outOf > -1)
            {
                sOutOf = "/" + outOf;
            }
            else
            {
                sOutOf = "";
            }
        }


        public void Wiggle()
        {
            wiggler.Start();
            iconWiggler.Start();
            flashTimer = 0.5f;
        }


        public override void Update()
        {
            base.Update();
            if (wiggler.Active)
            {
                wiggler.Update();
            }
            if (iconWiggler.Active)
            {
                iconWiggler.Update();
            }
            if (flashTimer > 0f)
            {
                flashTimer -= Engine.RawDeltaTime;
            }

            if (!CoreModule.Settings.AllowTextHighlight)
            {
                HighlightColor = HighlightColorA;
            }
            else if (Engine.Scene.OnRawInterval(0.1f))
            {
                if (HighlightColor == HighlightColorA)
                {
                    HighlightColor = HighlightColorB;
                }
                else
                {
                    HighlightColor = HighlightColorA;
                }
            }

            arrowPercent = Calc.Approach(arrowPercent, SceneAs<Level>().Paused ? 1f : 0f, Engine.DeltaTime * 4f);
        }


        public override void Render()
        {
            Vector2 renderPosition = RenderPosition;
            Vector2 vector = Calc.AngleToVector(Rotation, 1f);
            Vector2 vector2 = new Vector2(0f - vector.Y, vector.X);
            string text = (showOutOf ? sOutOf : "");
            float num = ActiveFont.Measure(sAmount).X;
            float num2 = ActiveFont.Measure(text).X;
            float num3 = 62f + (float)x.Width + 2f + num + num2;
            Color color = Color;
            if (flashTimer > 0f && Scene != null && Scene.BetweenRawInterval(0.05f))
            {
                color = FlashColor;
            }
            if (CenteredX)
            {
                renderPosition -= vector * (num3 / 2f) * Scale;
            }
            MTexture texture = GFX.Gui[FemtoHelperMetadata.SessionHeartMeta[CurrentSelected].UITexture];
            texture.DrawCentered(renderPosition + Vector2.UnitX * (+30 + (FemtoHelperMetadata.SessionHeartMeta.Count is { } && FemtoHelperMetadata.SessionHeartMeta.Count <= 1 ? 0 : (-20 * Ease.SineInOut(arrowPercent)))), Color.White, Scale + 0.25f, iconWiggler.Value * -0.25f);

            arrow.DrawCentered(renderPosition + (Vector2.UnitX * (arrowLastPressed == 1 ? iconWiggler.Value * -4f : 0f)) + Vector2.UnitX * (30 - 20 + 48), (arrowLastPressed == 1 ? HighlightColor : Color.White) * Ease.SineInOut(arrowPercent), 0.5f);
            arrow.DrawCentered(renderPosition + (Vector2.UnitX * (arrowLastPressed == -1 ? iconWiggler.Value * 4f : 0f)) + Vector2.UnitX * (30 - 20 - 48), (arrowLastPressed == -1 ? HighlightColor : Color.White) * Ease.SineInOut(arrowPercent), -0.5f);

            x.DrawCentered(renderPosition + vector * (62f + (float)x.Width * 0.5f) * Scale + vector2 * 2f * Scale, color, Scale);
            ActiveFont.DrawOutline(sAmount, renderPosition + vector * (num3 - num2 - num * 0.5f) * Scale + vector2 * (wiggler.Value * 18f) * Scale, new Vector2(0.5f, 0.5f), Vector2.One * Scale, color, Stroke, Color.Black);
            if (text != "")
            {
                ActiveFont.DrawOutline(text, renderPosition + vector * (num3 - num2 / 2f) * Scale, new Vector2(0.5f, 0.5f), Vector2.One * Scale, OutOfColor, Stroke, Color.Black);
            }
        }
    }

    private const float NumberUpdateDelay = 0.4f;

    private const float ComboUpdateDelay = 0.3f;

    private const float AfterUpdateDelay = 2f;

    private const float LerpInSpeed = 1.2f;

    private const float LerpOutSpeed = 2f;

    public static Color FlashColor = Calc.HexToColor("FF5E76");

    private readonly MTexture bg;

    public float DrawLerp;

    private float strawberriesUpdateTimer;

    private float strawberriesWaitTimer;

    private readonly HeartsCounter hearts;

    public SessionHeartDisplay()
    {
        Y = 984f;
        Depth = -101;
        Tag = (int)Tags.HUD | (int)Tags.Global | (int)Tags.PauseUpdate | (int)Tags.TransitionUpdate;
        bg = GFX.Gui["strawberryCountBG"];

        int[] amount = new int[FemtoHelperMetadata.SessionHeartMeta.Count];

        int i = 0;
        foreach (SessionHeartDefinition def in FemtoHelperMetadata.SessionHeartMeta)
        {
            amount[i] = FemtoModule.Session.SessionHeartCount(def.Name);
            i++;
        }

        Add(hearts = new HeartsCounter(centeredX: false, amount));
    }


    public override void Update()
    {
        List<int> diffs = [];
        int totalDiff = 0;
        int i = 0;
        int hasDiffAt = -1;
        foreach(string name in FemtoHelperMetadata.SessionHeartGroupNames)
        {
            int value = FemtoModule.Session.SessionHeartCount(name) - hearts.AmountOf(i);
            totalDiff += value;
            if(hasDiffAt == -1 && value != 0)
            {
                hasDiffAt = i;
            }
            diffs.Add(value);
            i++;
        }

        if (hasDiffAt != -1 && hearts.CurrentSelected != hasDiffAt)
        {
            hearts.CurrentSelected = hasDiffAt;
        }

        base.Update();
        Level level = Scene as Level;
        if (totalDiff > 0 && strawberriesUpdateTimer <= 0f)
        {
            strawberriesUpdateTimer = 0.4f;
        }
        if (totalDiff > 0 || strawberriesUpdateTimer > 0f || strawberriesWaitTimer > 0f || (level.Paused && level.PauseMainMenuOpen))
        {
            DrawLerp = Calc.Approach(DrawLerp, 1f, 1.2f * Engine.RawDeltaTime);
        }
        else
        {
            DrawLerp = Calc.Approach(DrawLerp, 0f, 2f * Engine.RawDeltaTime);
        }
        if (strawberriesWaitTimer > 0f)
        {
            strawberriesWaitTimer -= Engine.RawDeltaTime;
        }
        if (strawberriesUpdateTimer > 0f && DrawLerp == 1f)
        {
            strawberriesUpdateTimer -= Engine.RawDeltaTime;
            if (strawberriesUpdateTimer <= 0f)
            {

                if (totalDiff > 0 && diffs.Count > 0)
                {
                    hearts.CurrentSelected = hasDiffAt;
                    hearts.Amount++;
                }
                strawberriesWaitTimer = 2f;
                if (totalDiff > 0)
                {
                    strawberriesUpdateTimer = 0.3f;
                }
            }
        }

        if (level.Paused && level.PauseMainMenuOpen && strawberriesUpdateTimer <= 0 && totalDiff <= 0)
        {
            if (RightPressed)
            {
                hearts.CurrentSelected++;
                hearts.wiggler.Start();
                hearts.flashTimer = 0.5f;
            }
            else if (LeftPressed)
            {
                hearts.CurrentSelected--;
                hearts.wiggler.Start();
                hearts.flashTimer = 0.5f;
            }
        }

        if (Visible)
        {
            float num = Engine.Height - 96f;
            Y = Calc.Approach(Y, num, Engine.DeltaTime * 800f);
        }
        Visible = DrawLerp > 0f;
    }


    public override void Render()
    {
        Vector2 vec = Vector2.Lerp(new Vector2(-bg.Width, Y), new Vector2(32f, Y), Ease.CubeOut(DrawLerp));
        vec.X = Engine.Width - vec.X;
        vec = vec.Round();
        bg.DrawJustified(vec + new Vector2(-96f, 12f), new Vector2(0.5f, 0.5f), Color.White, -1);
        hearts.Position = vec + new Vector2(-bg.Width / 2 - 32, 0f - Y);
        hearts.Render();
    }
}
