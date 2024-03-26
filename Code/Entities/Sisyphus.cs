using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities
{
    [Tracked]
    public class Sisyphus : Entity
    {
        public float alpha;
        public MTexture texture;
        public float rand;
        public float sisyfade;
        public Sisyphus() : base(Vector2.Zero)
        {
            Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate;
            alpha = 0;
            rand = Calc.Random.Range(0.7f, 1.2f);
            texture = GFX.Game["objects/FemtoHelper/sisyphus/image"];
        }
        public override void Update()
        {
            if (!(Scene as Level).Paused)
            {
                base.Update();
            }
            sisyfade = Calc.Approach(sisyfade, (Scene as Level).Paused ? 1f : 0f, 8f * Engine.RawDeltaTime);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(Routine()));
        }
        public IEnumerator Routine()
        {
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, (2 * rand), start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                alpha = t.Eased;
            };
            Add(tween);
            yield return (2 * rand) + 0.7f;
            Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, null, (2 * rand), start: true);
            tween2.OnUpdate = delegate (Tween t)
            {
                alpha = 1 - t.Eased;
            };
            Add(tween2);
            yield return (2 * rand);
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            Color color = Color.Lerp(Color.White, Color.Black, sisyfade * 0.7f);

            texture.Draw(Vector2.Zero, Vector2.Zero, color * alpha);
        }
        //Engine.Scene.Add(new Celeste.Mod.Codecumber.Entities.Sisyphus())
    }

    [CustomEntity("FemtoHelper/SisyphusTrigger")]
    public class SisyphusTrigger : Trigger
    {
        public float timer;
        public bool yeah;
        public float randtimer;
        public bool yess;
        public SisyphusTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            timer = 0f;
            yeah = false;
            randtimer = Calc.Random.Range(4.5f, 7f);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (Sisyphus s in Scene.Tracker.GetEntities<Sisyphus>())
            {
                if (s != null)
                {
                    s.RemoveSelf();
                }
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (!yess)
            {
                Level obj = base.Scene as Level;
                obj.Session.Audio.Music.Event = "event:/spear_queer/sisyphus";
                obj.Session.Audio.Music.Progress = 0;
                obj.Session.Audio.Apply(forceSixteenthNoteHack: false);
                yess = true;
                X -= 88;
                Add(new Coroutine(waitlmao()));
            }
        }
        public IEnumerator waitlmao()
        {
            yield return 0.5f;
            Scene.Add(new Sisyphus());
            yeah = true;
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (yeah)
            {
                timer += Engine.DeltaTime;
                if (timer >= randtimer)
                {
                    if (Calc.Random.Chance(0.8f))
                    {
                        Scene.Add(new Sisyphus());
                    }
                    timer = 0;
                    randtimer = Calc.Random.Range(4.5f, 7f);
                    if (Calc.Random.Chance(0.4f)) randtimer = Calc.Random.Range(6f, 9f);
                }
            }

        }
    }

}
