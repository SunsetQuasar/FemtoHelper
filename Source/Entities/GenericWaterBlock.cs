using Celeste.Mod.FemtoHelper.Utils;
using Celeste.Mod.Helpers;
using MonoMod;
using MonoMod.Cil;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.FemtoHelper.Entities;
[TrackedAs(typeof(Water))]
public abstract class GenericWaterBlock : Water
{

    public Vector2 movementCounter;

    public Vector2 LiftSpeed;

    private readonly bool canCarry;

    public bool shaking;
    public float shakeTimer;

    private Vector2 shakeAmount;
    public Vector2 Shake => shakeAmount;

    public static ParticleType Dissipate = new ParticleType(Booster.P_Burst)
    {
        Color = Calc.HexToColor("81F4F0") * 0.25f
    };

    public static ParticleType Tinydrops = new ParticleType
    {
        Size = 1f,

        Color = Color.LightSkyBlue * 0.6f,
        DirectionRange = MathF.PI / 30f,
        LifeMin = 0.3f,
        LifeMax = 0.6f,
        SpeedMin = 5f,
        SpeedMax = 10f,
        SpeedMultiplier = 0.10f,
        FadeMode = ParticleType.FadeModes.Linear,
    };
    public static ParticleType Tinydrops2 = new ParticleType
    {
        Size = 1f,
        Color = Color.LightSkyBlue,
        DirectionRange = MathF.PI / 30f,
        LifeMin = 0.6f,
        LifeMax = 1f,
        SpeedMin = 40f,
        SpeedMax = 50f,
        SpeedMultiplier = 0.25f,
        FadeMode = ParticleType.FadeModes.Late
    };
    public GenericWaterBlock(Vector2 pos, float wid, float hei, bool can_carry) : base(pos, false, false, wid, hei)
    {
        DisplacementRenderHook d = Components.Get<DisplacementRenderHook>();
        Remove(d);
        Add(new DisplacementRenderHook(DrawDisplacement));

        RemoveTag(Tags.TransitionUpdate);
        canCarry = can_carry;
    }

    public override void Update()
    {
        base.Update();
        if (!shaking)
        {
            return;
        }
        if (base.Scene.OnInterval(0.04f))
        {
            Vector2 vector = shakeAmount;
            shakeAmount = Calc.Random.ShakeVector();
            OnShake(shakeAmount - vector);
        }
        if (shakeTimer > 0f)
        {
            shakeTimer -= Engine.DeltaTime;
            if (shakeTimer <= 0f)
            {
                shaking = false;
                StopShaking();
            }
        }
    }

    public virtual void DrawDisplacement()
    {

    }

    public void StartShaking(float time = 0f)
    {
        shaking = true;
        shakeTimer = time;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void StopShaking()
    {
        shaking = false;
        if (shakeAmount != Vector2.Zero)
        {
            OnShake(-shakeAmount);
            shakeAmount = Vector2.Zero;
        }
    }

    public virtual void OnShake(Vector2 amount)
    {
    }

    public static void Load()
    {
        IL.Celeste.Player.NormalUpdate += Player_NormalUpdate;
    }

    private static void Player_NormalUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        while (cursor.TryGotoNextBestFit(MoveType.After, instr => instr.MatchLdcR4(1), instr => instr.MatchCallOrCallvirt<Water.Surface>("DoRipple"), instr => instr.MatchLdcI4(0)))
        {
            ILLabel label = cursor.DefineLabel();
            cursor.Index--;
            cursor.EmitBr(label);
            cursor.Index++;
            cursor.EmitPop();
            cursor.EmitLdarg0();
            cursor.EmitLdloc(14);
            cursor.EmitDelegate(DoSomeWaterStuff);
            cursor.MarkLabel(label);
            cursor.EmitLdcI4(0);
        }
    }

    public static void DoSomeWaterStuff(Player player, bool canUnDuck)
    {
        if (player == null) return;
        if (canUnDuck && player.WaterWallJumpCheck(1))
        {
            if (player.DashAttacking && player.SuperWallJumpAngleCheck)
            {
                player.SuperWallJump(-1);
            }
            else
            {
                player.WallJump(-1);
            }
        }
        else if (canUnDuck && player.WaterWallJumpCheck(-1))
        {
            if (player.DashAttacking && player.SuperWallJumpAngleCheck)
            {
                player.SuperWallJump(1);
            }
            else
            {
                player.WallJump(1);
            }
        }
    }

    public static void Unload()
    {
        IL.Celeste.Player.NormalUpdate -= Player_NormalUpdate;
    }

    public void MoveTo(Vector2 pos)
    {
        MoveToX(pos.X);
        MoveToY(pos.Y);
    }

    public void MoveTo(Vector2 pos, Vector2 liftSpeed)
    {
        MoveToX(pos.X, liftSpeed.X);
        MoveToY(pos.Y, liftSpeed.Y);
    }

    public void MoveToX(float x)
    {
        MoveH((float)((double)x - (double)Position.X - (double)movementCounter.X));
    }

    public void MoveToX(float x, float liftSpeedX)
    {
        MoveH((float)((double)x - (double)Position.X - (double)movementCounter.X), liftSpeedX);
    }

    public void MoveToY(float y)
    {
        MoveV((float)((double)y - (double)Position.Y - (double)movementCounter.Y));
    }

    public void MoveToY(float y, float liftSpeedY)
    {
        MoveV((float)((double)y - (double)Position.Y - (double)movementCounter.Y), liftSpeedY);
    }


    public bool MoveToCollideBarriers(Vector2 pos, bool thruDashBlocks = false, bool evenSolids = false)
    {
        return MoveToXCollideBarriers(pos.X, thruDashBlocks, evenSolids) || MoveToYCollideBarriers(pos.Y, thruDashBlocks, evenSolids);
    }

    public bool MoveToXCollideBarriers(float to, bool thruDashBlocks = false, bool evenSolids = false)
    {
        return MoveHCollideBarriers(to - Position.X, null, thruDashBlocks, evenSolids);
    }
    public bool MoveToYCollideBarriers(float to, bool thruDashBlocks = false, bool evenSolids = false)
    {
        return MoveVCollideBarriers(to - Position.Y, null, thruDashBlocks, evenSolids);
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
        if (num == 0) return;
        movementCounter.X -= num;
        MoveHExact(num);
    }

    public void MoveH(float moveH, float liftSpeedH)
    {
        LiftSpeed.X = liftSpeedH;
        movementCounter.X += moveH;
        int num = (int)Math.Round(movementCounter.X);
        if (num == 0) return;
        movementCounter.X -= num;
        MoveHExact(num);
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
        if (num == 0) return;
        movementCounter.Y -= num;
        MoveVExact(num);
    }
    public void MoveV(float moveV, float liftSpeedV)
    {
        LiftSpeed.Y = liftSpeedV;
        movementCounter.Y += moveV;
        int num = (int)Math.Round(movementCounter.Y);
        if (num == 0) return;
        movementCounter.Y -= num;
        MoveVExact(num);
    }

    public void MoveHExact(int moveH)
    {

        Position.X += moveH;
        if (!canCarry) return;
        foreach (Entity entity in contains.Select((w) => w.Entity))
        {
            if (entity is Actor actor)
            {
                actor.MoveH(moveH);
                actor.LiftSpeed = LiftSpeed;
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
        if (!canCarry) return;
        foreach (Entity entity in contains.Select((w) => w.Entity))
        {
            if (entity is Actor actor)
            {
                actor.MoveV(moveV);
                actor.LiftSpeed = LiftSpeed;
            }
            else
            {
                entity.Position.Y += moveV;
            }
        }

    }

    public bool MoveVCollideBarriers(float moveV, Action<Vector2, Vector2, object> onCollide = null, bool thruDashBlocks = false, bool evenSolids = false)
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
            return MoveVExactCollideBarriers(num, onCollide, thruDashBlocks, evenSolids);
        }
        return false;
    }

    public bool MoveVExactCollideBarriers(int moveV, Action<Vector2, Vector2, object> onCollide = null, bool thruDashBlocks = false, bool evenSolids = false)
    {
        float y = Y;
        int num = Math.Sign(moveV);
        int num2 = 0;
        object platform = null;
        while (moveV != 0)
        {
            platform = this.CollideFirstIgnoreCollidable<SeekerBarrier>(Position + Vector2.UnitY * num);
            if (platform != null)
            {
                break;
            }

            if (evenSolids)
            {
                if (thruDashBlocks)
                {
                    foreach (DashBlock entity in base.Scene.Tracker.GetEntities<DashBlock>())
                    {
                        if (CollideCheck(entity, Position + Vector2.UnitY * num))
                        {
                            entity.Break(base.Center, Vector2.UnitY * num, true, true);
                            SceneAs<Level>().Shake(0.2f);
                            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        }
                    }
                }
                platform = CollideFirst<Solid>(Position + Vector2.UnitY * num);
                if (platform != null)
                {
                    break;
                }

                if (moveV > 0)
                {
                    platform = CollideFirstOutside<JumpThru>(Position + Vector2.UnitY * num);
                    if (platform != null)
                    {
                        break;
                    }
                }
            }
            
            num2 += num;
            moveV -= num;
            Y += num;
        }
        Y = y;
        MoveVExact(num2);
        if (platform != null)
        {
            onCollide?.Invoke(Vector2.UnitY * num, Vector2.UnitY * num2, platform);
        }
        return platform != null;
    }


    public bool MoveHCollideBarriers(float moveH, Action<Vector2, Vector2, object> onCollide = null, bool thruDashBlocks = false, bool evenSolids = false)
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
        if (num == 0) return false;
        movementCounter.X -= num;
        return MoveHExactCollideBarriers(num, onCollide, thruDashBlocks, evenSolids);
    }


    public bool MoveHExactCollideBarriers(int moveH, Action<Vector2, Vector2, object> onCollide = null, bool thruDashBlocks = false, bool evenSolids = false)
    {

        float x = X;
        int num = Math.Sign(moveH);
        int num2 = 0;
        object barrier = null;
        while (moveH != 0)
        {
            barrier = this.CollideFirstIgnoreCollidable<SeekerBarrier>(Position + Vector2.UnitX * num);
            if (barrier != null)
            {
                break;
            }

            if (evenSolids)
            {
                if (thruDashBlocks)
                {
                    foreach (DashBlock entity in base.Scene.Tracker.GetEntities<DashBlock>())
                    {
                        if (CollideCheck(entity, Position + Vector2.UnitX * num))
                        {
                            entity.Break(base.Center, Vector2.UnitX * num, true, true);
                            SceneAs<Level>().Shake(0.2f);
                            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        }
                    }
                }
                barrier = CollideFirst<Solid>(Position + Vector2.UnitX * num);
                if (barrier != null)
                {
                    break;
                }
            }

            num2 += num;
            moveH -= num;
            X += num;
        }
        X = x;
        MoveHExact(num2);
        if (barrier != null)
        {
            onCollide?.Invoke(Vector2.UnitX * num, Vector2.UnitX * num2, barrier);
        }
        return barrier != null;
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Render()")]
    public void base_Render() { }

    public override void Render()
    {
        base_Render();
    }
}
