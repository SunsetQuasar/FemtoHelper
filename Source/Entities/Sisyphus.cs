using System.Collections;
using System.Linq;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
public class Sisyphus : Entity
{
    public float Alpha;
    public readonly MTexture Texture;
    public readonly float Rand;
    public float Sisyfade;
    public Sisyphus() : base(Vector2.Zero)
    {
        Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate;
        Alpha = 0;
        Rand = Calc.Random.Range(0.7f, 1.2f);
        Texture = GFX.Game["objects/FemtoHelper/sisyphus/image"];
    }
    public override void Update()
    {
        if (!(Scene as Level).Paused)
        {
            base.Update();
        }
        Sisyfade = Calc.Approach(Sisyfade, (Scene as Level).Paused ? 1f : 0f, 8f * Engine.RawDeltaTime);
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        Add(new Coroutine(Routine()));
    }
    public IEnumerator Routine()
    {
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, (2 * Rand), start: true);
        tween.OnUpdate = delegate (Tween t)
        {
            Alpha = t.Eased;
        };
        Add(tween);
        yield return (2 * Rand) + 0.7f;
        Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, null, (2 * Rand), start: true);
        tween2.OnUpdate = delegate (Tween t)
        {
            Alpha = 1 - t.Eased;
        };
        Add(tween2);
        yield return (2 * Rand);
        RemoveSelf();
    }

    public override void Render()
    {
        base.Render();
        Color color = Color.Lerp(Color.White, Color.Black, Sisyfade * 0.7f);

        Texture.Draw(Vector2.Zero, Vector2.Zero, color * Alpha);
    }
    //Engine.Scene.Add(new Celeste.Mod.Codecumber.Entities.Sisyphus())
}

[CustomEntity("FemtoHelper/SisyphusTrigger")]
public class SisyphusTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    public float Timer = 0f;
    public bool Yeah = false;
    public float Randtimer = Calc.Random.Range(4.5f, 7f);
    public bool Yess;

    public override void Added(Scene scene)
    {
        base.Added(scene);
        foreach (var s in Scene.Tracker.GetEntities<Sisyphus>().Cast<Sisyphus>().Where(s => s != null))
        {
            s.RemoveSelf();
        }
    }
    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        if (!Yess)
        {
            Level obj = Scene as Level;
            obj.Session.Audio.Music.Event = "event:/spear_queer/sisyphus";
            obj.Session.Audio.Music.Progress = 0;
            obj.Session.Audio.Apply(forceSixteenthNoteHack: false);
            Yess = true;
            X -= 88;
            Add(new Coroutine(Waitlmao()));
        }
    }
    public IEnumerator Waitlmao()
    {
        yield return 0.5f;
        Scene.Add(new Sisyphus());
        Yeah = true;
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);
        if (!Yeah) return;
        Timer += Engine.DeltaTime;
        if (!(Timer >= Randtimer)) return;
        if (Calc.Random.Chance(0.8f))
        {
            Scene.Add(new Sisyphus());
        }
        Timer = 0;
        Randtimer = Calc.Random.Range(4.5f, 7f);
        if (Calc.Random.Chance(0.4f)) Randtimer = Calc.Random.Range(6f, 9f);

    }
}
