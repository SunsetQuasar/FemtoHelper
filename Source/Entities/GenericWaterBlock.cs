using Celeste.Mod.FemtoHelper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;
[TrackedAs(typeof(Water))]
public abstract class GenericWaterBlock : Water
{

    public Vector2 movementCounter;

    public Vector2 LiftSpeed;

    public GenericWaterBlock(Vector2 pos, float wid, float hei) : base(pos, false, false, wid, hei)
    {
        FillColor = Color.Transparent;
        DisplacementRenderHook d = Components.Get<DisplacementRenderHook>();
        Remove(d);
        Add(new DisplacementRenderHook(DrawDisplacement));

        RemoveTag(Tags.TransitionUpdate);
    }

    public abstract void DrawDisplacement();

    public void MoveTo(Vector2 pos)
    {
        MoveToX(pos.X);
        MoveToY(pos.Y);
    }

    public void MoveToX(float to)
    {
        MoveH(to - Position.X);
    }

    public void MoveToY(float to)
    {
        MoveV(to - Position.Y);
    }

    public bool MoveToCollideBarriers(Vector2 pos)
    {
        return MoveToXCollideBarriers(pos.X) || MoveToYCollideBarriers(pos.Y);
    }

    public bool MoveToXCollideBarriers(float to)
    {
        return MoveHCollideBarriers(to - Position.X);
    }
    public bool MoveToYCollideBarriers(float to)
    {
        return MoveVCollideBarriers(to - Position.Y);
    }

    public void MoveH(float moveH)
    {
        if (Engine.DeltaTime == 0f)
        {
            LiftSpeed.X = 0f;
        }
        else
        {
            LiftSpeed.X = moveH / Engine.DeltaTime;
        }
        movementCounter.X += moveH;
        int num = (int)Math.Round(movementCounter.X);
        if (num != 0)
        {
            movementCounter.X -= num;
            MoveHExact(num);
        }
    }

    public void MoveH(float moveH, float liftSpeedH)
    {
        LiftSpeed.X = liftSpeedH;
        movementCounter.X += moveH;
        int num = (int)Math.Round(movementCounter.X);
        if (num != 0)
        {
            movementCounter.X -= num;
            MoveHExact(num);
        }
    }

    public void MoveV(float moveV)
    {
        if (Engine.DeltaTime == 0f)
        {
            LiftSpeed.Y = 0f;
        }
        else
        {
            LiftSpeed.Y = moveV / Engine.DeltaTime;
        }
        movementCounter.Y += moveV;
        int num = (int)Math.Round(movementCounter.Y);
        if (num != 0)
        {
            movementCounter.Y -= num;
            MoveVExact(num);
        }
    }
    public void MoveV(float moveV, float liftSpeedV)
    {
        LiftSpeed.Y = liftSpeedV;
        movementCounter.Y += moveV;
        int num = (int)Math.Round(movementCounter.Y);
        if (num != 0)
        {
            movementCounter.Y -= num;
            MoveVExact(num);
        }
    }

    public void MoveHExact(int moveH)
    {

        Position.X += moveH;
        foreach (Entity entity in contains.Select((w) => w.Entity))
        {
            if (entity is Actor)
            {
                (entity as Actor).MoveH(moveH);
                (entity as Actor).LiftSpeed = LiftSpeed;
            }
            else
            {
                entity.Position.X += moveH;
            }
        }

    }

    public void MoveVExact(int moveV)
    {

        Position.Y += moveV;
        foreach (Entity entity in contains.Select((w) => w.Entity))
        {
            if (entity is Actor)
            {
                (entity as Actor).MoveV(moveV);
                (entity as Actor).LiftSpeed = LiftSpeed;
            }
            else
            {
                entity.Position.Y += moveV;
            }
        }

    }

    public bool MoveVCollideBarriers(float moveV, Action<Vector2, Vector2, SeekerBarrier> onCollide = null)
    {
        if (Engine.DeltaTime == 0f)
        {
            LiftSpeed.Y = 0f;
        }
        else
        {
            LiftSpeed.Y = moveV / Engine.DeltaTime;
        }
        movementCounter.Y += moveV;
        int num = (int)Math.Round(movementCounter.Y);
        if (num != 0)
        {
            movementCounter.Y -= num;
            return MoveVExactCollideBarriers(num, onCollide);
        }
        return false;
    }

    public bool MoveVExactCollideBarriers(int moveV, Action<Vector2, Vector2, SeekerBarrier> onCollide = null)
    {
        float y = base.Y;
        int num = Math.Sign(moveV);
        int num2 = 0;
        SeekerBarrier platform = null;
        while (moveV != 0)
        {
            platform = this.CollideFirstIgnoreCollidable<SeekerBarrier>(Position + Vector2.UnitY * num);
            if (platform != null)
            {
                break;
            }
            /*
            if (moveV > 0)
            {
                platform = CollideFirstOutside<JumpThru>(Position + Vector2.UnitY * num);
                if (platform != null)
                {
                    break;
                }
            }
            */
            num2 += num;
            moveV -= num;
            base.Y += num;
        }
        base.Y = y;
        MoveVExact(num2);
        if (platform != null)
        {
            onCollide?.Invoke(Vector2.UnitY * num, Vector2.UnitY * num2, platform);
        }
        return platform != null;
    }


    public bool MoveHCollideBarriers(float moveH, Action<Vector2, Vector2, SeekerBarrier> onCollide = null)
    {
        if (Engine.DeltaTime == 0f)
        {
            LiftSpeed.X = 0f;
        }
        else
        {
            LiftSpeed.X = moveH / Engine.DeltaTime;
        }
        movementCounter.X += moveH;
        int num = (int)Math.Round(movementCounter.X);
        if (num != 0)
        {
            movementCounter.X -= num;
            return MoveHExactCollideBarriers(num, onCollide);
        }
        return false;
    }


    public bool MoveHExactCollideBarriers(int moveH, Action<Vector2, Vector2, SeekerBarrier> onCollide = null)
    {

        float x = base.X;
        int num = Math.Sign(moveH);
        int num2 = 0;
        SeekerBarrier barrier = null;
        while (moveH != 0)
        {
            barrier = this.CollideFirstIgnoreCollidable<SeekerBarrier>(Position + Vector2.UnitX * num);
            if (barrier != null)
            {
                break;
            }
            num2 += num;
            moveH -= num;
            base.X += num;
        }
        base.X = x;
        MoveHExact(num2);
        if (barrier != null)
        {
            onCollide?.Invoke(Vector2.UnitX * num, Vector2.UnitX * num2, barrier);
        }
        return barrier != null;
    }

    public override void Render()
    {
        base.Render();
        Components.Render();
    }
}
