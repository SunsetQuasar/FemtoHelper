// Celeste.FinalBoss
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

[CustomEntity("FemtoHelper/CrystalHeartBoss")]
[Tracked(false)]
public class CrystalHeartBoss : FinalBoss
{
	public static ParticleType P_Burst;

	public const float CameraXPastMax = 140f;

	private const float MoveSpeed = 600f;

	private const float AvoidRadius = 12f;

	public Sprite Sprite;

	public PlayerSprite NormalSprite;

	private PlayerHair normalHair;

	private Vector2 avoidPos;

	public float CameraYPastMax;

	public bool Moving;

	public bool Sitting;

	private int facing;

	private Level level;

	private Circle circle;

	private Vector2[] nodes;

	private int nodeIndex;

	private int patternIndex;

	private Coroutine attackCoroutine;

	private Coroutine triggerBlocksCoroutine;

	private List<Entity> fallingBlocks;

	private List<Entity> movingBlocks;

	private bool playerHasMoved;

	private SineWave floatSine;

	private bool dialog;

	private bool startHit;

	private VertexLight light;

	private Wiggler scaleWiggler;

	private FinalBossStarfield bossBg;

	private SoundSource chargeSfx;

	private SoundSource laserSfx;

	private bool canChangeMusic;

	public Vector2 BeamOrigin => base.Center + Sprite.Position + new Vector2(0f, -14f);

	public Vector2 ShotOrigin => base.Center + Sprite.Position + new Vector2(6f * Sprite.Scale.X, 2f);
	public CrystalHeartBoss(Vector2 position, Vector2[] nodes, int patternIndex, float cameraYPastMax, bool dialog, bool startHit, bool cameraLockY)
		: base(position, nodes, patternIndex, cameraYPastMax, dialog, startHit, cameraLockY)
	{
		this.patternIndex = patternIndex;
		CameraYPastMax = cameraYPastMax;
		this.dialog = dialog;
		this.startHit = startHit;
		Add(light = new VertexLight(Color.White, 1f, 32, 64));
		base.Collider = (circle = new Circle(8f, 0f, -4f));
		Add(new PlayerCollider(OnPlayer));
		this.nodes = new Vector2[nodes.Length + 1];
		this.nodes[0] = Position;
		for (int i = 0; i < nodes.Length; i++)
		{
			this.nodes[i + 1] = nodes[i];
		}
		attackCoroutine = new Coroutine(removeOnComplete: false);
		Add(attackCoroutine);
		triggerBlocksCoroutine = new Coroutine(removeOnComplete: false);
		Add(triggerBlocksCoroutine);
		Add(new CameraLocker(cameraLockY ? Level.CameraLockModes.FinalBoss : Level.CameraLockModes.FinalBossNoY, 140f, cameraYPastMax));
		Add(floatSine = new SineWave(0.6f, 0f));
		Add(scaleWiggler = Wiggler.Create(0.6f, 3f));
		Add(chargeSfx = new SoundSource());
		Add(laserSfx = new SoundSource());
	}

	public CrystalHeartBoss(EntityData e, Vector2 offset) : base (e, offset)
	{
		orig_ctor(e, offset);
		canChangeMusic = e.Bool("canChangeMusic", defaultValue: true);
	}
}
