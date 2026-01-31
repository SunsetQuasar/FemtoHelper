using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

public class SMWMessage : Entity
{
    public string DialogID;
    public float Percent;
    public bool Opening;
    public bool ShouldRenderRect => Percent > 0;
    public bool ShouldRenderText => Percent == 1;
    public static Vector2 ScreenCenter => new Vector2(Engine.Width, Engine.Height) / 2f;
    public static Vector2 TextBoxSize => new Vector2(Engine.Width, Engine.Height) / 2f;
    public static bool ContinuePressed => Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed;
    public TimeRateModifier TheWorld;
    public FancyText.Text Text;
    public int textStart;
    public float TransitionPercent = 0f;
    public bool Transitioning => TransitionPercent > 0;
    private float timer;
    public bool Waiting;
    public SMWMessage(string dialogID) : base()
    {
        AddTag(TagsExt.SubHUD);
        DialogID = dialogID;
        Add(TheWorld = new TimeRateModifier(1f));
        Add(new Coroutine(Routine()) { UseRawDeltaTime = true });
        Text = FancyText.Parse(Dialog.Get(DialogID), (int)(TextBoxSize.X - 40), -1);
        textStart = 0;
    }

    public override void Update()
    {
        base.Update();
        timer += Engine.RawDeltaTime;

        Player player = Scene.Tracker.GetEntity<Player>();
        if(player is null || player.Dead)
        {
            TheWorld.Multiplier = 1f;
        }
    }

    public IEnumerator Routine()
    {
        Opening = true;
        Add(new Coroutine(StopTime()) { UseRawDeltaTime = true });
        yield return 0.1f;
        Percent = 0f;
        while (Percent < 1f)
        {
            yield return null;
            Percent = Calc.Approach(Percent, 1f, Engine.RawDeltaTime * 3f);
        }
        Percent = 1f;
        Opening = false;
        int page = 0;
        while (page < Text.Pages)
        {
            Waiting = true;
            while (!ContinuePressed) yield return null;
            Waiting = false;

            Input.MenuConfirm.ConsumePress();
            Input.MenuCancel.ConsumePress();

            TransitionPercent = 0f;
            while (TransitionPercent < 1f)
            {
                yield return null;
                TransitionPercent = Calc.Approach(TransitionPercent, 1f, Engine.RawDeltaTime * 6f);
            }
            TransitionPercent = 1f;

            yield return 0.1f;

            textStart = Text.GetNextPageStart(textStart);
            page++;

            while (TransitionPercent > 0f)
            {
                yield return null;
                TransitionPercent = Calc.Approach(TransitionPercent, 0f, Engine.RawDeltaTime * 6f);
            }
            TransitionPercent = 0f;
        }

        Waiting = true;
        while (!ContinuePressed) yield return null;
        Waiting = false;
        yield return 0.1f;
        while (Percent > 0f)
        {
            yield return null;
            Percent = Calc.Approach(Percent, 0f, Engine.RawDeltaTime * 3f);
        }
        Percent = 0f;
        Add(new Coroutine(StartTime()) { UseRawDeltaTime = true });
    }

    public IEnumerator StopTime()
    {
        TheWorld.Multiplier = 1f;
        while (TheWorld.Multiplier > 0f)
        {
            yield return null;
            TheWorld.Multiplier = Calc.Approach(TheWorld.Multiplier, 0f, Engine.RawDeltaTime * 12f);
        }
    }

    public IEnumerator StartTime()
    {
        TheWorld.Multiplier = 0f;
        while (TheWorld.Multiplier < 1f)
        {
            yield return null;
            TheWorld.Multiplier = Calc.Approach(TheWorld.Multiplier, 1f, Engine.RawDeltaTime * 12f);
        }
    }

    public override void Render()
    {
        base.Render();

        if (ShouldRenderRect)
        {
            Vector2 size = TextBoxSize * Percent;
            Draw.Rect(ScreenCenter - (size / 2f), size.X, size.Y, Color.Black);
        }

        if (ShouldRenderText)
        {
            Text?.DrawJustifyPerLine(ScreenCenter, new Vector2(0.5f, 0.5f), Vector2.One, 1f, textStart);
        }

        if (Waiting)
        {
            Vector2 position = (ScreenCenter + (TextBoxSize / 2f)) - (Vector2.One * 24f) + (Vector2.UnitY * (timer % 1 < 0.25f ? 3 : -3));
            GFX.Gui["textboxbutton"].DrawCentered(position);
        }

        if (Transitioning)
        {
            Vector2 size = TextBoxSize * TransitionPercent;

            Draw.Rect(ScreenCenter - (TextBoxSize / 2f), TextBoxSize.X, size.Y / 2f, Color.Black);
            Draw.Rect(ScreenCenter - (TextBoxSize / 2f), size.X / 2f, TextBoxSize.Y, Color.Black);

            Draw.Rect(ScreenCenter - (TextBoxSize / 2f) + ((1 - TransitionPercent) * TextBoxSize * Vector2.UnitY / 2f) + (TextBoxSize * Vector2.UnitY / 2f), TextBoxSize.X, size.Y / 2f, Color.Black);
            Draw.Rect(ScreenCenter - (TextBoxSize / 2f) + ((1 - TransitionPercent) * TextBoxSize * Vector2.UnitX / 2f) + (TextBoxSize * Vector2.UnitX / 2f), size.X / 2f, TextBoxSize.Y, Color.Black);
        }
    }
}
