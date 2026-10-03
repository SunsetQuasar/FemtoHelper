using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/MovementModifier")]
public class MovementModifier : Entity
{
    public enum Operations : byte
    {
        Set,
        Add,
        AddSigned,
        Subtract,
        SubtractSigned,
        Multiply,
        Divide,
        Max,
        Min,
    }

    public readonly record struct ModifierData(Operations OpX, Operations OpY, Vector2 Value, bool AttachToSolid);
    private static Dictionary<ModifierData, MovementModifier> _modifiers = [];

    [Tracked]
    public class AlreadyModifiedIt() : Component(false, false);

    public readonly ModifierData Data;

    public Color FillColor;
    public Color OutlineColor;

    public Vector2 ShakeOffset;

    public readonly bool TryToMerge;

    public MovementModifier(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new ColliderList(new Hitbox(data.Width, data.Height));
        Depth = -100;

        Data = new(data.Enum("operationX", Operations.Multiply), data.Enum("operationY", Operations.Multiply), data.Vector2("valueX", "valueY", Vector2.One), data.Bool("attachToSolid"));

        FillColor = Calc.HexToColorWithAlpha(data.Attr("fillColor", "2C22424D")) * data.Float("fillAlpha", 1f);
        OutlineColor = Calc.HexToColorWithAlpha(data.Attr("outlineColor", "9470DCFF")) * data.Float("outlineAlpha", 1f);

        TryToMerge = !data.Bool("legacy", true);

        if (Data.AttachToSolid)
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

    public override void Awake(Scene scene)
    {
        if (TryToMerge)
        {
            if (_modifiers.TryGetValue(Data, out var leader))
            {
                // you don't deserve to exist join the hivemind
                leader.Merge(this);
            }
            else
            {
                // leader time
                _modifiers.Add(Data, this);
            }
        }
    }

    public void Merge(MovementModifier other)
    {
        if (other.Collider is not ColliderList list)
        {
            Error("Tried to merge with a leader MovementModifer!");
            return;
        }

        if (list.colliders.Length == 0)
        {
            Error("Tried to merge with a MovementModifer with no colliders!");
            return;
        }

        Collider collider = list.colliders[0];

        Vector2 delta = other.Position - Position;
        collider.Position += delta;

        (Collider as ColliderList).Add(collider);
        other.RemoveSelf();
    }

    public override void Render()
    {
        Vector2 temp = Position;
        Position += ShakeOffset;
        base.Render();

        foreach (Hitbox box in (Collider as ColliderList)?.colliders ?? default)
        {
            Rectangle rect = box.Bounds;
            rect.Inflate(-1, -1);
            Draw.Rect(rect, FillColor);
            Draw.HollowRect(box, OutlineColor);
        }

        Position = temp;
    }

    public float MovementModX(float @in)
    {
        float @out = @in;

        @out = Data.OpX switch
        {
            Operations.Set => (Data.Value.X * Engine.DeltaTime),
            Operations.Add => @out + (Data.Value.X * Engine.DeltaTime),
            Operations.AddSigned => @out + ((Data.Value.X * Engine.DeltaTime) * MathF.Sign(@out)),
            Operations.Subtract => @out - (Data.Value.X * Engine.DeltaTime),
            Operations.SubtractSigned => @out - ((Data.Value.X * Engine.DeltaTime) * MathF.Sign(@out)),
            Operations.Divide => @out / Data.Value.X,
            Operations.Max => MathF.Max(MathF.Abs(@out), Data.Value.X * Engine.DeltaTime) * MathF.Sign(@out),
            Operations.Min => MathF.Min(MathF.Abs(@out), Data.Value.X * Engine.DeltaTime) * MathF.Sign(@out),
            _ => @out * Data.Value.X,
        };

        return @out;
    }

    public float MovementModY(float @in)
    {
        float @out = @in;

        @out = Data.OpY switch
        {
            Operations.Set => (Data.Value.Y * Engine.DeltaTime),
            Operations.Add => @out + (Data.Value.Y * Engine.DeltaTime),
            Operations.AddSigned => @out + ((Data.Value.Y * Engine.DeltaTime) * MathF.Sign(@out)),
            Operations.Subtract => @out - (Data.Value.Y * Engine.DeltaTime),
            Operations.SubtractSigned => @out - ((Data.Value.Y * Engine.DeltaTime) * MathF.Sign(@out)),
            Operations.Divide => @out / Data.Value.Y,
            Operations.Max => MathF.Max(MathF.Abs(@out), Data.Value.Y * Engine.DeltaTime) * MathF.Sign(@out),
            Operations.Min => MathF.Min(MathF.Abs(@out), Data.Value.Y * Engine.DeltaTime) * MathF.Sign(@out),
            _ => @out * Data.Value.Y,
        };

        return @out;
    }

    private static bool _hooksLoaded = false;

    public static void Load()
    {
        if (_hooksLoaded) return;
        Info("Loading MovementModifier hooks");
        _hooksLoaded = true;
        Everest.Events.Level.OnLoadLevel += Level_OnLoadLevel;
        On.Celeste.Actor.MoveH += Actor_MoveH;
        On.Celeste.Actor.MoveV += Actor_MoveV;
        On.Celeste.Actor.MoveHExact += Actor_MoveHExact;
        On.Celeste.Actor.MoveVExact += Actor_MoveVExact;
        On.Celeste.Actor.NaiveMove += Actor_NaiveMove;
    }

    private static void Level_OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        _modifiers.Clear();
    }

    private static bool Actor_MoveH(On.Celeste.Actor.orig_MoveH orig, Actor self, float moveH, Collision onCollide, Solid pusher)
    {
        if (self.Components.current.Any(c => c is AlreadyModifiedIt))
        {
            bool ret = orig(self, moveH, onCollide, pusher);
            self.Components.current.RemoveWhere((c) => c is AlreadyModifiedIt);
            return ret;
        }
        else
        {
            List<Entity> mms = self.CollideAll<MovementModifier>();
            if (mms is not null && mms.Count != 0)
            {
                foreach (MovementModifier mm in mms)
                {
                    moveH = mm.MovementModX(moveH);
                }

                self.Components.current.Add(new AlreadyModifiedIt());
                bool ret = orig(self, moveH, onCollide, pusher);
                self.Components.current.RemoveWhere((c) => c is AlreadyModifiedIt);
                return ret;
            }
        }
        return orig(self, moveH, onCollide, pusher);
    }

    private static bool Actor_MoveV(On.Celeste.Actor.orig_MoveV orig, Actor self, float moveV, Collision onCollide, Solid pusher)
    {
        if (self.Components.current.Any(c => c is AlreadyModifiedIt))
        {
            bool ret = orig(self, moveV, onCollide, pusher);
            self.Components.current.RemoveWhere((c) => c is AlreadyModifiedIt);
            return ret;
        }
        else
        {
            List<Entity> mms = self.CollideAll<MovementModifier>();
            if (mms is not null && mms.Count != 0)
            {
                foreach (MovementModifier mm in mms)
                {
                    moveV = mm.MovementModY(moveV);
                }

                self.Components.current.Add(new AlreadyModifiedIt());
                bool ret = orig(self, moveV, onCollide, pusher);
                self.Components.current.RemoveWhere((c) => c is AlreadyModifiedIt);
                return ret;
            }
        }
        return orig(self, moveV, onCollide, pusher);
    }

    private static bool Actor_MoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher)
    {
        if (!self.Components.current.Any(c => c is AlreadyModifiedIt))
        {
            List<Entity> mms = self.CollideAll<MovementModifier>();
            if (mms is not null && mms.Count != 0)
            {
                float floatMoveH = moveH;
                foreach (MovementModifier mm in mms)
                {
                    floatMoveH = mm.MovementModX(floatMoveH);
                }

                self.Components.current.Add(new AlreadyModifiedIt());
                return self.MoveH(floatMoveH, onCollide, pusher);
            }
        }
        self.Components.current.RemoveWhere((c) => c is AlreadyModifiedIt);
        return orig(self, moveH, onCollide, pusher);
    }

    private static bool Actor_MoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher)
    {
        if (!self.Components.current.Any(c => c is AlreadyModifiedIt))
        {
            List<Entity> mms = self.CollideAll<MovementModifier>();
            if (mms is not null && mms.Count != 0)
            {
                float floatMoveV = moveV;
                foreach (MovementModifier mm in mms)
                {
                    floatMoveV = mm.MovementModY(floatMoveV);
                }

                self.Components.current.Add(new AlreadyModifiedIt());
                return self.MoveH(floatMoveV, onCollide, pusher);
            }
        }
        self.Components.current.RemoveWhere((c) => c is AlreadyModifiedIt);
        return orig(self, moveV, onCollide, pusher);
    }

    private static void Actor_NaiveMove(On.Celeste.Actor.orig_NaiveMove orig, Actor self, Vector2 amount)
    {
        var mms = self.CollideAll<MovementModifier>();
        if (mms is not null)
        {
            foreach (MovementModifier mm in mms)
            {
                amount = new(mm.MovementModX(amount.X), mm.MovementModY(amount.Y));
            }
        }
        orig(self, amount);
    }

    public static void Unload()
    {
        if (!_hooksLoaded) return;
        Info("Unloading MovementModifier hooks");
        _hooksLoaded = false;
        Everest.Events.Level.OnLoadLevel -= Level_OnLoadLevel;
        On.Celeste.Actor.MoveH -= Actor_MoveH;
        On.Celeste.Actor.MoveV -= Actor_MoveV;
        On.Celeste.Actor.MoveHExact -= Actor_MoveHExact;
        On.Celeste.Actor.MoveVExact -= Actor_MoveVExact;
        On.Celeste.Actor.NaiveMove -= Actor_NaiveMove;
    }
}
