using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

public class SMWMessage : Entity
{
    public string DialogID;
    public float Percent;
    public bool Opening;
    public bool ShouldRender => Percent > 0;
    public static Vector2 ScreenCenter => new Vector2(Engine.Width, Engine.Height) / 2f;
    public static Vector2 TextBoxSize => new Vector2(Engine.Width, Engine.Height) / 2f;
    public bool ContinuePressed => Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed;
    public TimeRateModifier TheWorld;
    public SMWMessage(string dialogID) : base()
    {
        AddTag(TagsExt.SubHUD);
        DialogID = dialogID;
        Add(TheWorld = new TimeRateModifier(1f));
        Add(new Coroutine(Routine()) { UseRawDeltaTime = true });
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
        while (!ContinuePressed) yield return null;
        yield return 0.1f;
        while (Percent > 0f)
        {
            yield return null;
            Percent = Calc.Approach(Percent, 0f, Engine.RawDeltaTime * 3f);
        }
        Percent = 0f;
        Add(new Coroutine(StartTime()) { UseRawDeltaTime = true });

        //Engine.Scene.Add(new global::Celeste.Mod.FemtoHelper.Entities.SMWMessage("CH3_OSHIRO_START_CHASE"));
    }

    public IEnumerator StopTime()
    {
        TheWorld.Multiplier = 1f;
        while (TheWorld.Multiplier > 0f)
        {
            yield return null;
            TheWorld.Multiplier = Calc.Approach(TheWorld.Multiplier, 0f, Engine.RawDeltaTime * 5f);
        }
    }

    public IEnumerator StartTime()
    {
        TheWorld.Multiplier = 0f;
        while (TheWorld.Multiplier < 1f)
        {
            yield return null;
            TheWorld.Multiplier = Calc.Approach(TheWorld.Multiplier, 1f, Engine.RawDeltaTime * 5f);
        }
    }

    public override void Render()
    {
        base.Render();

        if (ShouldRender)
        {
            float eased = Opening switch
            {
                true => Percent,
                _ => Percent
            };
            Vector2 size = TextBoxSize * eased;
            Draw.Rect(ScreenCenter - (size / 2f), size.X, size.Y, Color.Black);
        }
    }
}
