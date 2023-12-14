using System;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.FemtoHelper;

[CustomEntity("FemtoHelper/SmileGhost")]

public class SmileGhost : Entity
{
	public float speed = 65f;
	private Level level;
	private Sprite sprite;
	public Vector2 vel;
	private VertexLight Light;
	public float distance;
	public int identity = 0;
	public float steeringWeakness = 10;

	public SmileGhost(Vector2 offset, int id) : base(offset)
	{
		vel = Vector2.Zero;
		Position = offset;
		identity = id;
		speed += (float)(identity * 5);
		steeringWeakness += (float)(identity * 30);

	}
	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		level = scene as Level;
		base.Collider = new Circle(12f, 0, 0);
		Add(new PlayerCollider(OnPlayer));
		Add(sprite = FemtoModule.femtoSpriteBank.Create("smiler"));
		sprite.Play("idle");
		base.Depth = -3000000;
		Add(Light = new VertexLight(Position, Calc.HexToColor("FFFFFF"), 1f, 32, 64));
	}
	public override void Update()
	{
		Player entity = base.Scene.Tracker.GetEntity<Player>();
		if (entity != null)
		{
			Position -= vel * Engine.DeltaTime;
			vel = ((vel * steeringWeakness) + Vector2.Normalize(Position - entity.Position) * speed) / (steeringWeakness + 1);
			Logger.Log("smileghost", speed + ", " + Position + ", " + identity);
			if (identity == 0)
			{
				distance = Math.Max((float)(Math.Sqrt(Math.Pow(entity.Position.X - Position.X, 2)) + Math.Sqrt(Math.Pow(entity.Position.Y - Position.Y, 2))) - 32, 0) / 500;
				Audio.SetMusicParam("smileDistance", distance);
			}
		}
		//currentMoveLoopSfx.Position = Position;
	}
	public void OnPlayer(Player player)
	{
		Audio.SetMusicParam("smileDistance", 1);
		player.Die(new Vector2(Calc.Random.NextFloat(), Calc.Random.NextFloat()), false, true);
	}
}
