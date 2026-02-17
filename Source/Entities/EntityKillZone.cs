using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/EntityKillZone")]
public class EntityKillZone : Entity
{
    public class Poof : Entity
    {
        public Poof(Vector2 Position) : base(Position)
        {
            Depth = -51000;
            Sprite puff = new(GFX.Game, "objects/FemtoHelper/EntityKillZone/");
            puff.Add("puff", "puff", 0.1f);
            puff.OnFinish = (anim) => RemoveSelf();
            puff.CenterOrigin();
            Add(puff);
            puff.Play("puff");
        }
    }

    private static Dictionary<string, Action<Entity>> killInterop = [];

    public static readonly ParticleType Stars = new()
    {
        Source = GFX.Game["particles/FemtoHelper/star"],
        Color = Color.Yellow,
        Friction = 640f,
        LifeMin = 0.4f,
        LifeMax = 0.4f,
        SpeedMin = 160f,
        SpeedMax = 160f,
        Size = 1f,
        SizeRange = 0f,
    };

    private readonly HashSet<string> whitelist;
    private readonly HashSet<string> blacklist;

    public Color FillColor;
    public Color OutlineColor;

    public Vector2 ShakeOffset;
    public EntityKillZone(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        whitelist = [.. data.String("whitelist", "").Split(',')];
        blacklist = [.. data.String("blacklist", "").Split(',')];
        whitelist.Remove("");
        blacklist.Remove("");

        FillColor = Calc.HexToColorWithAlpha(data.Attr("fillColor", "2C22424D")) * data.Float("fillAlpha", 1f);
        OutlineColor = Calc.HexToColorWithAlpha(data.Attr("outlineColor", "9470DCFF")) * data.Float("outlineAlpha", 1f);

        Depth = -1000;

        Collider = new Hitbox(data.Width, data.Height);

        if (data.Bool("attachToSolid"))
        {
            Add(new StaticMover()
            {
                SolidChecker = CollideCheck,
                JumpThruChecker = CollideCheck,
                OnDestroy = RemoveSelf,
                OnShake = v => ShakeOffset += v
            });
        }
    }
    public override void Render()
    {
        Vector2 temp = Position;
        Position += ShakeOffset;
        base.Render();
        Rectangle rect = Collider.Bounds;
        rect.Inflate(-1, -1);
        Draw.Rect(rect, FillColor);
        Draw.HollowRect(Collider, OutlineColor);
        Position = temp;
    }

    public override void Update()
    {
        base.Update();

        List<Entity> toKill = [];

        foreach (Entity entity in Scene.Entities)
        {
            if (!CollideCheck(entity)) continue;
            if (entity == (Scene as Level).SolidTiles || entity is Poof) continue;

            if (whitelist.Count > 0)
            {
                bool whitelisted = false;
                foreach (string str in whitelist)
                {
                    string pattern = "^" + Regex.Escape(str).Replace("\\*", ".*") + "$";

                    if (Regex.IsMatch(entity.GetType().FullName, pattern) || (entity.SourceData != null && entity.SourceData.Name != null && Regex.IsMatch(entity.SourceData.Name, pattern)) || (whitelist.Contains("@triggers") && entity is Trigger))
                    {
                        whitelisted = true;
                        break;
                    }
                }
                if (!whitelisted) continue;
            }
            else
            {
                bool blacklisted = false;
                foreach (string str in blacklist)
                {
                    string pattern = "^" + Regex.Escape(str).Replace("\\*", ".*") + "$";

                    if (Regex.IsMatch(entity.GetType().FullName, pattern) || (entity.SourceData != null && entity.SourceData.Name != null && Regex.IsMatch(entity.SourceData.Name, pattern)) || (blacklist.Contains("@triggers") && entity is Trigger))
                    {
                        blacklisted = true;
                        break;
                    }
                }
                if (blacklisted) continue;
            }

            if (entity is Player p)
            {
                p.Die(Vector2.Zero);
                return;
            }

            Audio.Play("event:/FemtoHelper/stomp_poof", entity.Center);

            Scene.Add(new Poof(entity.Center));

            SceneAs<Level>().ParticlesFG.Emit(Stars, entity.Center, (-22.5f).ToRad());
            SceneAs<Level>().ParticlesFG.Emit(Stars, entity.Center, (22.5f).ToRad());
            SceneAs<Level>().ParticlesFG.Emit(Stars, entity.Center, (180 - 22.5f).ToRad());
            SceneAs<Level>().ParticlesFG.Emit(Stars, entity.Center, (180 + 22.5f).ToRad());

            if (entity is SmwShell shell)
            {
                shell.counter?.RemoveSelf();
            }

            entity.RemoveSelf();
        }
    }
}
