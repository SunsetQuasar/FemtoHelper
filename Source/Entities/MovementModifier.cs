using Celeste.Mod.FemtoHelper.Utils;
using IL.Celeste.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/MovementModifier")]
public class MovementModifier : Entity
{
    public enum Operations
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

    [Tracked]
    public class AlreadyModifiedIt() : Component(false, false);

    public Operations OperationX;
    public Operations OperationY;

    public Vector2 Value;

    public Color FillColor;
    public Color OutlineColor;

    public Vector2 ShakeOffset;

    public MovementModifier(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);
        Depth = -100;
        OperationX = data.Enum("operationX", Operations.Multiply);
        OperationY = data.Enum("operationY", Operations.Multiply);
        Value = data.Vector2("valueX", "valueY", Vector2.One);

        FillColor = Calc.HexToColorWithAlpha(data.Attr("fillColor", "2C22424D")) * data.Float("fillAlpha", 1f);
        OutlineColor = Calc.HexToColorWithAlpha(data.Attr("outlineColor", "9470DCFF")) * data.Float("outlineAlpha", 1f);

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

    public Vector2 MovementMod(Vector2 @in)
    {
        Vector2 @out = @in;

        @out.X = OperationX switch
        {
            Operations.Set => (Value.X * Engine.DeltaTime),
            Operations.Add => @out.X + (Value.X * Engine.DeltaTime),
            Operations.AddSigned => @out.X + ((Value.X * Engine.DeltaTime) * MathF.Sign(@out.X)),
            Operations.Subtract => @out.X - (Value.X * Engine.DeltaTime),
            Operations.SubtractSigned => @out.X - ((Value.X * Engine.DeltaTime) * MathF.Sign(@out.X)),
            Operations.Divide => @out.X / Value.X,
            Operations.Max => MathF.Max(MathF.Abs(@out.X), Value.X * Engine.DeltaTime) * MathF.Sign(@out.X),
            Operations.Min => MathF.Min(MathF.Abs(@out.X), Value.X * Engine.DeltaTime) * MathF.Sign(@out.X),
            _ => @out.X * Value.X,
        };

        @out.Y = OperationY switch
        {
            Operations.Set => (Value.Y * Engine.DeltaTime),
            Operations.Add => @out.Y + (Value.Y * Engine.DeltaTime),
            Operations.AddSigned => @out.Y + ((Value.Y * Engine.DeltaTime) * MathF.Sign(@out.Y)),
            Operations.Subtract => @out.Y - (Value.Y * Engine.DeltaTime),
            Operations.SubtractSigned => @out.Y - ((Value.Y * Engine.DeltaTime) * MathF.Sign(@out.Y)),
            Operations.Divide => @out.Y / Value.Y,
            Operations.Max => MathF.Max(MathF.Abs(@out.Y), Value.Y * Engine.DeltaTime) * MathF.Sign(@out.Y),
            Operations.Min => MathF.Min(MathF.Abs(@out.Y), Value.Y * Engine.DeltaTime) * MathF.Sign(@out.Y),
            _ => @out.Y * Value.Y,
        };

        return @out;
    }

    public static void Load()
    {
        On.Celeste.Actor.MoveH += Actor_MoveH;
        On.Celeste.Actor.MoveV += Actor_MoveV;
        On.Celeste.Actor.MoveHExact += Actor_MoveHExact;
        On.Celeste.Actor.MoveVExact += Actor_MoveVExact;
        On.Celeste.Actor.NaiveMove += Actor_NaiveMove;
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
            if (mms.Count != 0)
            {
                foreach (MovementModifier mm in mms)
                {
                    moveH = mm.MovementMod(new(moveH, 0)).X;
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
            if (mms.Count != 0)
            {
                foreach (MovementModifier mm in mms)
                {
                    moveV = mm.MovementMod(new(0, moveV)).Y;
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
            if (mms.Count != 0)
            {
                float floatMoveH = moveH;
                foreach (MovementModifier mm in mms)
                {
                    floatMoveH = mm.MovementMod(new(floatMoveH, 0)).X;
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
            if (mms.Count != 0)
            {
                float floatMoveV = moveV;
                foreach (MovementModifier mm in mms)
                {
                    floatMoveV = mm.MovementMod(new(0, floatMoveV)).Y;
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
        foreach (MovementModifier mm in self.CollideAll<MovementModifier>())
        {
            amount = mm.MovementMod(amount);
        }
        orig(self, amount);
    }

    public static void Unload()
    {
        On.Celeste.Actor.MoveH -= Actor_MoveH;
        On.Celeste.Actor.MoveV -= Actor_MoveV;
        On.Celeste.Actor.MoveHExact -= Actor_MoveHExact;
        On.Celeste.Actor.MoveVExact -= Actor_MoveVExact;
        On.Celeste.Actor.NaiveMove -= Actor_NaiveMove;
    }
}
