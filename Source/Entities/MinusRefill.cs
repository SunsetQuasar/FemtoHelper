using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.FemtoHelper.Entities;
[CustomEntity("FemtoHelper/MinusRefill")]
public class MinusRefill : Entity
{
	private Sprite sprite;

	private Sprite flash;

	private Image outline;

	private Wiggler wiggler;

	private BloomPoint bloom;

	private VertexLight light;

	private Level level;

	private SineWave sine;

	private bool oneUse;

	private static ParticleType _pShatter = new(Refill.P_Shatter)
    {
        Color = Calc.HexToColor("d3e8ff"),
        Color2 = Calc.HexToColor("85b0fc")
    };

	private static ParticleType _pRegen = new(Refill.P_Regen)
    {
        Color = Calc.HexToColor("a5d1ff"),
        Color2 = Calc.HexToColor("6da0e0")
    };

	private static ParticleType _pGlow = new(Refill.P_Glow)
    {
        Color = Calc.HexToColor("a5d1ff"),
        Color2 = Calc.HexToColor("6da0e0")
    };

	private float respawnTimer;
	private readonly float respawnTime;

	
	public MinusRefill(Vector2 position, EntityData data)
		: base(position)
	{
		base.Collider = new Hitbox(16f, 16f, -8f, -8f);
		Add(new PlayerCollider(OnPlayer));
		this.oneUse = data.Bool("oneUse", false);
		string text;
		text = "objects/FemtoHelper/minusRefill/";
		Add(outline = new Image(GFX.Game[text + "outline"]));
		outline.CenterOrigin();
		outline.Visible = false;
		Add(sprite = new Sprite(GFX.Game, text + "idle"));
		sprite.AddLoop("idle", "", 0.1f);
		sprite.Play("idle");
		sprite.CenterOrigin();
		Add(flash = new Sprite(GFX.Game, text + "flash"));
		flash.Add("flash", "", 0.05f);
		flash.OnFinish = (_) =>
        {
			flash.Visible = false;
		};
		flash.CenterOrigin();
		Add(wiggler = Wiggler.Create(1f, 4f,  (float v) =>
		{
			sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
		}));
		Add(new MirrorReflection());
		Add(bloom = new BloomPoint(0.8f, 16f));
		Add(light = new VertexLight(Color.White, 1f, 16, 48));
		Add(sine = new SineWave(0.6f, 0f));
		sine.Randomize();
		UpdateY();
		base.Depth = -100;
		respawnTime = data.Float("respawnTime", 2.5f);
    }

	
	public MinusRefill(EntityData data, Vector2 offset)
		: this(data.Position + offset, data)
	{
	}

	
	public override void Added(Scene scene)
	{
		base.Added(scene);
		level = SceneAs<Level>();
	}

	
	public override void Update()
	{
		base.Update();
		if (respawnTimer > 0f)
		{
			respawnTimer -= Engine.DeltaTime;
			if (respawnTimer <= 0f)
			{
				Respawn();
			}
		}
		else if (base.Scene.OnInterval(0.1f))
		{
			level.ParticlesFG.Emit(_pGlow, 1, Position, Vector2.One * 5f);
		}
		UpdateY();
		light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
		bloom.Alpha = light.Alpha * 0.8f;
		if (base.Scene.OnInterval(2f) && sprite.Visible)
		{
			flash.Play("flash", restart: true);
			flash.Visible = true;
		}
	}

	
	private void Respawn()
	{
		if (!Collidable)
		{
			Collidable = true;
			sprite.Visible = true;
			outline.Visible = false;
			base.Depth = -100;
			wiggler.Start();
			Audio.Play("event:/game/general/diamond_return", Position);
			level.ParticlesFG.Emit(_pRegen, 16, Position, Vector2.One * 2f);
		}
	}

	
	private void UpdateY()
	{
		Sprite obj = flash;
		Sprite obj2 = sprite;
		float num2 = (bloom.Y = sine.Value * 2f);
		float y = (obj2.Y = num2);
		obj.Y = y;
	}

	
	public override void Render()
	{
		if (sprite.Visible)
		{
			sprite.DrawOutline();
		}
		base.Render();
	}

	
	private void OnPlayer(Player player)
	{
		
		if(player.Dashes-- <= 0)
		{
			player.Die(-(Center - player.Center).SafeNormalize());
		}
		Audio.Play("event:/game/general/diamond_touch", Position);
		Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
		Collidable = false;
		Add(new Coroutine(MinusRefillRoutine(player)));
		respawnTimer = respawnTime;
	}

	
	private IEnumerator MinusRefillRoutine(Player player)
	{
		Celeste.Freeze(0.05f);
		yield return null;
		level.Shake();
		Sprite obj = sprite;
		Sprite obj2 = flash;
		bool visible = false;
		obj2.Visible = false;
		obj.Visible = visible;
		if (!oneUse)
		{
			outline.Visible = true;
		}
		Depth = 8999;
		yield return 0.05f;
		float num = player.Speed.Angle();
		level.ParticlesFG.Emit(_pShatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
		level.ParticlesFG.Emit(_pShatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
		SlashFx.Burst(Position, num);
		if (oneUse)
		{
			RemoveSelf();
		}
	}
}
