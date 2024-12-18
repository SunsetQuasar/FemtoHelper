using System;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/SMWShell")]
public class SMWShell : Actor
{
	private enum States
	{
		Dropped = 0,
		Kicked = 1,
		Dead = 2
	}
	
	private States state = States.Dropped;
	
    private SMWHoldable hold;
    private Vector2 speed;
    private readonly Collision onCollideH;
    private readonly Collision onCollideV;
    private float noGravityTimer;
    private Vector2 prevLiftSpeed;

    private float dontKillTimer;
    
    private const float HorizontalFriction = 60f;
    private const float MaxFallSpeed = 240f;
    private const float Gravity = 450f;
    
    public SMWShell(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        speed = Vector2.Zero;
        
        Collider = new Hitbox(12f, 10f, -6f, -4f);
        Add(new PlayerCollider(OnPlayer));
        
        Add(new PlayerCollider(OnPlayerBonk, new Hitbox(12f, 4f, -6f, -8f)));
        
        Depth = -10;
        Add(hold = new SMWHoldable());
        
        hold.PickupCollider = new Hitbox(20f, 14f, -10f, -4f);
        hold.SlowFall = false;
        hold.SlowRun = false;
        hold.OnPickup = OnPickup;
        hold.OnRelease = OnRelease;
        hold.SpeedGetter = () => speed;
        hold.OnHitSpring = HitSpring;
        hold.SpeedSetter = (spd) => speed = spd;
        
        onCollideH = OnCollideH;
        onCollideV = OnCollideV;
    }

    private void OnPlayerBonk(Player p)
    {
	    if (state != States.Kicked || dontKillTimer > 0) return;
	    p.Bounce(Top);
	    state = States.Dropped;
    }
    
    private void OnPlayer(Player p)
    {
	    if (state == States.Kicked)
	    {
		    if (dontKillTimer <= 0)
		    {
			    p.Die((Position - p.Center).SafeNormalize());
		    }
	    }
	    else
	    {
		    float kickSpeed = 0;
		    if (p.CenterX > CenterX)
		    {
			    kickSpeed = -200;
		    } 
		    else if (p.CenterX < CenterX)
		    {
			    kickSpeed = 200;
		    }
		    else
		    {
			    if (p.Facing == Facings.Left)
			    {
				    kickSpeed = -200;
			    }
			    else
			    {
				    kickSpeed = 200;
			    }
		    }
		    
		    Kick(Vector2.UnitX * kickSpeed);
	    }
    }

    private void Kick(Vector2? spd = null)
    {
	    if(spd != null) speed = Vector2.UnitX * spd ?? Vector2.Zero;
	    state = States.Kicked;
	    dontKillTimer = 0.1f;
	    hold.cannotHoldTimer = 0.02f;
    }

    public override void Update()
    {
        base.Update();
        if (state == States.Kicked)
        {
	        hold.cannotHoldTimer = 0.02f;
        }
        if (dontKillTimer > 0)
        {
	        dontKillTimer -= Engine.DeltaTime;
	        if(hold.IsHeld) dontKillTimer = 0;
        } 
		if (hold.IsHeld)
		{
			prevLiftSpeed = Vector2.Zero;
		}
		else
		{
			if (OnGround())
			{
				float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
				if(state != States.Kicked) speed.X = Calc.Approach(speed.X, target, 800f * Engine.DeltaTime);
				Vector2 liftSpeed = base.LiftSpeed;
				if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
				{
					speed = prevLiftSpeed;
					prevLiftSpeed = Vector2.Zero;
					speed.Y = Math.Min(speed.Y * 0.6f, 0f);
					if (speed.X != 0f && speed.Y == 0f)
					{
						speed.Y = -60f;
					}
					if (speed.Y < 0f)
					{
						noGravityTimer = 0.15f;
					}
				}
				else
				{
					prevLiftSpeed = liftSpeed;
					if (liftSpeed.Y < 0f && speed.Y < 0f)
					{
						speed.Y = 0f;
					}
				}
			}
			else if (hold.ShouldHaveGravity)
			{
				float num = 800f;
				if (Math.Abs(speed.Y) <= 30f)
				{
					num *= 0.5f;
				}
				float num2 = 250f;
				if (speed.Y < 0f)
				{
					num2 *= 0.5f;
				}
				if(state != States.Kicked) speed.X = Calc.Approach(speed.X, 0f, num2 * Engine.DeltaTime);
				if (noGravityTimer > 0f)
				{
					noGravityTimer -= Engine.DeltaTime;
				}
				else
				{
					speed.Y = Calc.Approach(speed.Y, 200f, num * Engine.DeltaTime);
				}
			}
			MoveH(speed.X * Engine.DeltaTime, onCollideH);
			MoveV(speed.Y * Engine.DeltaTime, onCollideV);
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			TempleGate templeGate = CollideFirst<TempleGate>();
			if (templeGate != null && entity != null)
			{
				templeGate.Collidable = false;
				MoveH((float)(Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime);
				templeGate.Collidable = true;
			}
		}
	    hold.CheckAgainstColliders();
    }
    
    private void OnCollideH(CollisionData data)
    {
        speed.X *= state == States.Kicked ? -1 : -0.5f;
    }
    
    private void OnCollideV(CollisionData data)
    {
        if(Math.Abs(speed.Y) > 40 && state != States.Kicked)
        {
	        speed.Y *= Math.Sign(speed.Y) == 1 ? -0.3f : -0.1f;
        } else
        {
	        speed.Y = 0f;
        }
        if(state != States.Kicked)speed.X *= 0.5f;
    }
    
    private bool HitSpring(Spring spring)
    {
        if (hold.IsHeld) return false;
        switch (spring.Orientation)
        {
            case Spring.Orientations.Floor when speed.Y >= 0f:
                speed.X *= 0.5f;
                speed.Y = -160f;
                return true;
            case Spring.Orientations.WallLeft when speed.X <= 0f:
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = 160f;
                speed.Y = -80f;
                return true;
            case Spring.Orientations.WallRight when speed.X >= 0f:
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = -160f;
                speed.Y = -80f;
                return true;
            default:
                return false;
        }
    }
    
    private void OnPickup()
    {
        speed = Vector2.Zero;
        state = States.Dropped;
        AddTag(Tags.Persistent);
    }
    
    private void OnRelease(Vector2 force)
    {
	    bool kicked = true;
	    Player player = Scene.Tracker.GetNearestEntity<Player>(Position);
	    if (player == null) return;
	    force.Y *= 0.5f;
	    if (force.X != 0f)
	    {
		    if (force.Y == 0)
		    {
			    force.Y = -0.4f;
			    if (Input.Aim.Value.Y < 0f)
			    {
				    kicked = false;
				    force.Y = -3f;
				    force.X = player.Speed.X / 200f;
			    }
		    }
	    }

	    if (force is { X: 0, Y: 0 })
	    {
		    kicked = false;
		    force.X = player.Speed.X / 400f;
		    force.X += player.Facing == Facings.Right ? 0.25f : -0.25f;
	    }
        if(kicked) Kick();
	    speed = force * new Vector2(200, 100);
	    RemoveTag(Tags.Persistent);
	    if (TrySquishWiggle(CollisionData.Empty))
	    {
		    RemoveSelf();
	    }
    }

    public override void Render()
    {
        base.Render();
        Draw.Rect(Collider, Color.Aqua);
        Draw.HollowRect(X - 8, Y - 8, 16, 16, Color.Gray);
    }
}