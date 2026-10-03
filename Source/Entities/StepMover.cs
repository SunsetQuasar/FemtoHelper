using Celeste.Mod.Helpers;
using Celeste.Mod.Registry;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/StepMover")]
public class StepMover : Entity
{
    [Tracked]
    public class StepMoverTrigger(Action callback) : Component(false, false)
    {
        Action onTrigger = callback;
        public void Trigger()
        {
            onTrigger?.Invoke();
        }

        [OnLoad]
        public static void Load()
        {
            IL.Celeste.Refill.OnPlayer += Refill_OnPlayer;
        }

        private static void Refill_OnPlayer(ILContext il)
        {
            ILCursor cursor = new(il);
            if (cursor.TryGotoNextBestFit(MoveType.After, instr => instr.MatchCallOrCallvirt<Player>("UseRefill"), instr => instr.MatchBrfalse(out _)))
            {
                cursor.EmitLdarg0();
                cursor.EmitDelegate(TriggerStepMover);
            }
        }

        public static void TriggerStepMover(Refill refill)
        {
            foreach (StepMoverTrigger trigger in refill.Components.GetAll<StepMoverTrigger>())
            {
                trigger.Trigger();
            }
        }

        [OnUnload]
        public static void Unload()
        {
            IL.Celeste.Refill.OnPlayer -= Refill_OnPlayer;
        }
    }
    public List<Entity> affected;
    public Hitbox catcher;
    public Vector2 delta;

    private readonly List<(Type, string)> targetTypesAndSIDs;
    private readonly List<(Type, string)> triggerTypesAndSIDs;
    private readonly bool strict;
    private readonly string msg = null;
    private bool moved;
    private readonly float moveTime;
    private readonly MTexture icon;
    private readonly Vector2 iconPos;
    private float iconAlpha = 1f;

    private readonly int iconCountX = 1;
    private readonly int iconCountY = 1;
    private readonly int iconSpacingX = 8;
    private readonly int iconSpacingY = 8;

    public StepMover(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {
        Depth = 7000;

        targetTypesAndSIDs = [.. data.String("types", "refill,crumbleBlock")
            .Split(',')
            .SelectMany<string, (Type, string)>(
                (str) => EntityRegistry.GetKnownTypesFromSid(str).Select(t => (t, str))
            )];

        triggerTypesAndSIDs = [.. data.String("linkActivationTo", "refill,crumbleBlock")
            .Split(',')
            .SelectMany<string, (Type, string)>(
                (str) => EntityRegistry.GetKnownTypesFromSid(str).Select(t => (t, str))
            )];

        targetTypesAndSIDs.Add((typeof(StepMover), "FemtoHelper/StepMover"));

        strict = data.Bool("strictWhitelist", true);

        Vector2[] nodes = data.NodesWithPosition(offset);

        affected = [];

        if (nodes.Length < 3)
        {
            Error($"StepMover {id} has {nodes.Length - 1} node{(nodes.Length == 2 ? "" : "s")}, which is less than the required 2!");
            msg = "needs 3 nodes";

            Collider = catcher = new Hitbox(data.Width, data.Height, 0, 0);
        }
        else
        {
            if (nodes.Length > 4)
            {
                Warn($"StepMover {id} has {nodes.Length - 1} nodes, ignoring anything but the first 3!");
            }
            Collider = new Hitbox(data.Width, data.Height, 0, 0);
            catcher = new Hitbox(data.Float("targetBoxWidth", data.Width), data.Float("targetBoxHeight", data.Height), nodes[1].X, nodes[1].Y);
            delta = nodes[2] - nodes[1];

            Add(new PlayerCollider(OnTrigger, Collider));

            iconPos = nodes.Length == 4 ? nodes[3] : (nodes[2] + new Vector2(catcher.Width / 2, catcher.Height / 2));
        }

        moveTime = data.Float("moveTime", 0.5f);

        string iconPath = data.String("icon", "objects/FemtoHelper/StepMover/refilltarget");

        if (!string.IsNullOrWhiteSpace(iconPath))
        {
            icon = GFX.Game[iconPath];
        }

        iconCountX = data.Int("iconCountX", 1);
        iconCountY = data.Int("iconCountY", 1);
        iconSpacingX = data.Int("iconSpacingX", icon.Width);
        iconSpacingY = data.Int("iconSpacingY", icon.Height);
    }
    public void OnTrigger(Player player = null)
    {
        if (moved) return;
        moved = true;
        Collidable = false;

        Tween.Set(this, Tween.TweenMode.Oneshot, moveTime, Ease.ExpoIn, (t) =>
        {
            iconAlpha = 1 - t.Eased;
        });

        foreach (Entity entity in affected)
        {
            Vector2 anchor = entity.Position;

            if (entity is Refill refill)
            {
                DynamicData.For(refill).Set("respawn_position", anchor + delta);
            }

            bool prev = entity.Collidable;
            if (entity is not Platform) entity.Collidable = false;

            Tween.Set(this, Tween.TweenMode.Oneshot, moveTime, Ease.CubeOut, (t) =>
            {
                Vector2 dest = Vector2.Lerp(anchor, anchor + delta, t.Eased);

                switch (entity)
                {
                    case Platform platform:
                        platform.MoveTo(dest);
                        break;
                    case Actor actor:
                        Vector2 difference = dest - actor.Position;
                        actor.NaiveMove(difference);
                        break;
                    case Booster booster:
                        booster.outline.Position = dest;
                        booster.Position = dest;
                        break;
                    case MoverRefill moverRefill:
                        Vector2 moverDiff = moverRefill.Destination - moverRefill.Position;
                        moverRefill.Position = dest;
                        moverRefill.Destination = dest + moverDiff;
                        moverRefill.line.Position = moverRefill.Position;
                        moverRefill.line.Endpoint = moverRefill.Destination;
                        break;
                    default:
                        entity.Position = dest;
                        break;
                }
            }, (t) =>
            {
                if (entity is StepMover) entity.Collidable = true;
                else if (entity is not Platform) entity.Collidable |= prev;
            });
        }
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (var (type, _) in targetTypesAndSIDs.Union(triggerTypesAndSIDs))
        {
            Tracker.AddTypeToTracker(type);
        }

        Scene.OnEndOfFrame += () =>
        {
            Tracker.Refresh();

            bool linked = false;
            foreach (var (type, str) in triggerTypesAndSIDs)
            {
                List<Entity> l = Scene.Tracker.GetEntitiesTrackIfNeeded(type);
                foreach (Entity e in l)
                {
                    if (strict && !str.Equals(e.SourceData?.Name ?? "", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    bool thisprev = Collidable;
                    bool eprev = e.Collidable;
                    Collidable = true;
                    e.Collidable = true;

                    if (Collider.Collide(e))
                    {
                        if (e is Solid solid)
                        {
                            linked = true;
                            solid.Add(new Coroutine(SolidDetectRoutine(solid, () => OnTrigger())));
                        }
                        else if (e is JumpThru jumpthru)
                        {
                            linked = true;
                            jumpthru.Add(new Coroutine(JumpThruDetectRoutine(jumpthru, () => OnTrigger())));
                        }
                        else if (e is Refill refill)
                        {
                            linked = true;
                            refill.Add(new StepMoverTrigger(() => OnTrigger()));
                        } 
                        else if (e.Collider is not null)
                        {
                            linked = true;
                            e.Add(new PlayerCollider(OnTrigger));
                        }
                    }

                    Collidable = thisprev;
                    e.Collidable = eprev;
                }
            }

            if (linked)
            {
                Remove(Get<PlayerCollider>());
            }

            foreach (var (type, str) in targetTypesAndSIDs)
            {
                List<Entity> l = Scene.Tracker.GetEntitiesTrackIfNeeded(type);
                foreach (Entity e in l)
                {
                    if (e == this) continue;

                    if (strict && !str.Equals(e.SourceData?.Name ?? "", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    if (!catcher.Collide(e)) continue;

                    if (e is StepMover)
                    {
                        e.Collidable = false;
                    }

                    affected.Add(e);
                }
            }
        };
    }

    public static IEnumerator SolidDetectRoutine(Solid solid, Action onTrigger)
    {
        while (!solid.HasPlayerRider() && !solid.HasPlayerClimbing())
        {
            yield return null;
        }
        onTrigger?.Invoke();
    }

    public static IEnumerator JumpThruDetectRoutine(JumpThru jumpThru, Action onTrigger)
    {
        while (!jumpThru.HasPlayerRider())
        {
            yield return null;
        }
        onTrigger?.Invoke();
    }

    public override void Render()
    {
        base.Render();
        if (icon is not null)
        {
            for (int i = 0; i < iconCountX; i++)
            {
                for (int j = 0; j < iconCountY; j++)
                {
                    icon.DrawCentered(iconPos + new Vector2(i * iconSpacingX, j * iconSpacingY), Color.White * iconAlpha);
                }
            }
        }
        
        if (msg is not null)
        {
            Draw.SpriteBatch.DrawString(Draw.DefaultFont, msg, Position, Color.White);
        }
    }
}
