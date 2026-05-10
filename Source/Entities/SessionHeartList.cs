using Celeste.Mod.FemtoHelper.Metadata;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using static Monocle.MInput;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/SessionHeartPedestal")]
[Tracked]
public class SessionHeartPedestal : Entity
{
    public readonly bool controller;
    private readonly TalkComponent talker;
    public SessionHeartPedestal(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        float width = data.Width;
        float height = data.Height;
        Position += new Vector2(width * 0.5f, height);
        Add(talker = new(new Rectangle(-(int)(width * 0.5f), -(int)height, (int)width, (int)height), Vector2.UnitY * data.Float("offsetY", -16), (player) =>
        {
            //if ((Scene as Level).Session.GetFlag("FemtoHelper_SessionHeartMenu_Open")) return;
            Scene.Add(new SessionHeartList("Session Hearts"));
        }));
    }

    public override void Render()
    {
        base.Render();
    }
}

[Tracked]
public class SessionHeartList : Overlay
{
    private Level level;
    private float openPercent;
    private Wiggler closeWiggler;
    private string title;
    private float scroll, scrollSpeed, scrollMax;
    private SessionHeartDisplay display;
    public SessionHeartList(string title)
    {
        this.title = title;
        AddTag(Tags.HUD);
        AddTag(Tags.PauseUpdate);
        Depth = -5000;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = scene as Level;
        Add(new Coroutine(Enter()));
        Add(closeWiggler = Wiggler.Create(0.4f, 4f));
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        float height = 0f;
        if (FemtoHelperMetadata.HasSessionHearts)
        {
            foreach (var kvp in FemtoModule.Session?.SessionHearts ?? [])
            {
                if (!FemtoHelperMetadata.SessionHeartMetaByGroupName.TryGetValue(kvp.Key, out var def)) continue;
                MTexture icon = GFX.Gui[def.UITexture];
                foreach (string poem in kvp.Value)
                {
                    string clean = Dialog.Clean(poem);
                    height += Math.Max(icon.Height, ActiveFont.HeightOf(clean)) + 8f;
                }
            }
        }

        scrollMax = Math.Max(height - (Engine.Height - 128f - 128f), 0f);
    }

    public IEnumerator Enter()
    {
        Player player = Scene.Tracker.GetEntity<Player>();
        player?.StateMachine.State = Player.StDummy;

        display = level.Tracker.GetEntity<SessionHeartDisplay>();

        //level.Session.SetFlag("FemtoHelper_SessionHeartMenu_Open");
        level.StartPauseEffects();
        Tween open = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 0.25f, true);
        open.OnUpdate = (t) =>
        {
            openPercent = t.Eased;
        };
        Add(open);
        yield return 0.25f;

        //main loop
        scroll = scrollSpeed = 0;
        while(true)
        {
            if (Input.MenuCancel.Pressed)
            {
                Input.MenuCancel.ConsumePress();
                closeWiggler.Start();
                break;
            }

            scroll -= scrollSpeed * Engine.DeltaTime;
            if(scroll >= scrollMax || scroll <= 0)
            {
                scrollSpeed = 0f;
            }
            scroll = Calc.Clamp(scroll, 0, scrollMax);
            scrollSpeed = Calc.Approach(scrollSpeed, 0, Engine.DeltaTime * 4800f);
            scrollSpeed += ((MInput.Mouse.WheelDelta * 480f) + (Input.MenuUp.Check ? 6000f : (Input.MenuDown.Check ? -6000f : 0f))) * Engine.DeltaTime;

            yield return null;
        }

        Audio.Play("event:/ui/game/unpause");
        level.EndPauseEffects();
        Tween close = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 0.25f, true);
        close.OnUpdate = (t) =>
        {
            openPercent = 1 - t.Eased;
        };
        Add(close);
        yield return 0.25f;

        player?.StateMachine.State = Player.StNormal;
        //level.Session.SetFlag("FemtoHelper_SessionHeartMenu_Open", false);
        RemoveSelf();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        //level.Session.SetFlag("FemtoHelper_SessionHeartMenu_Open", false);
        level.EndPauseEffects();
    }

    public override void Render()
    {
        base.Render();

        float height = 128f - scroll;

        Draw.Rect(new Vector2(-4f, -4f), Engine.Width + 4, Engine.Height + 4, Color.Black * 0.7f * openPercent);

        Color strokeColor = Color.Black * (openPercent * openPercent * openPercent);
        ActiveFont.DrawEdgeOutline(title, new Vector2(Engine.Width * 0.5f, height), new Vector2(0.5f, 0.5f), Vector2.One * 2f, Color.Gray * openPercent, 4f, Color.DarkSlateBlue * openPercent, 2f, strokeColor);

        height += ActiveFont.HeightOf(title) + 24f;

        if (FemtoHelperMetadata.HasSessionHearts)
        {
            foreach (var kvp in FemtoModule.Session?.SessionHearts ?? [])
            {
                string key = kvp.Key;
                if(!FemtoHelperMetadata.SessionHeartMetaByGroupName.TryGetValue(key, out var def)) continue;
                MTexture icon = GFX.Gui[def.UITexture];
                Color defColor = Calc.HexToColor(def.Color);
                foreach (string poem in kvp.Value)
                {
                    string clean = Dialog.Clean(poem);
                    icon.DrawCentered(new Vector2((Engine.Width * 0.5f) - (icon.Width * 0.5f) - (ActiveFont.Measure(clean).X * 0.5f), height), Color.White * openPercent);
                    ActiveFont.DrawOutline(clean, new Vector2((Engine.Width * 0.5f) + (icon.Width * 0.5f), height), Vector2.One * 0.5f, Vector2.One, Color.Lerp(Color.White, defColor, 0.5f) * openPercent, 3f, Color.Black * openPercent * openPercent * openPercent);
                    height += Math.Max(icon.Height, ActiveFont.HeightOf(clean)) + 8f;
                }
            }
        }

        string label = "Close";

        Vector2 position = new(Engine.Width - 25, Engine.Height - 25);
        
        ButtonUI.Render(position, label, Input.MenuCancel, 0.5f, 1f, closeWiggler.Value * 0.05f, openPercent);
    }

    public override void Update()
    {
        base.Update();
        display?.Update();
    }
    /* 

    [OnLoad]
    public static void Load()
    {
        On.Monocle.MInput.MouseData.UpdateNull += MouseData_UpdateNull;
        On.Monocle.MInput.Update += MInput_Update;
    }

    public static bool lastActive;

    private static void MInput_Update(On.Monocle.MInput.orig_Update orig)
    {
        if(!lastActive && Engine.Instance.IsActive)
        {
            //update twice to ensure last = current
            MInput.Mouse.Update();
            MInput.Mouse.Update();
        }
        orig();
        lastActive = Engine.Instance.IsActive;
    }

    private static void MouseData_UpdateNull(On.Monocle.MInput.MouseData.orig_UpdateNull orig, MInput.MouseData self)
    {
        MouseState curr = self.CurrentState;
        orig(self);
        self.CurrentState = curr;
    }

    [OnUnload]
    public static void Unload()
    {
        On.Monocle.MInput.MouseData.UpdateNull -= MouseData_UpdateNull;
        On.Monocle.MInput.Update -= MInput_Update;
    }
    /**/
}
