// Celeste.FinalBoss
using Celeste;
using Mono.Cecil.Cil;
using MonoMod.Cil;

[CustomEntity("FemtoHelper/CrystalHeartBoss")]
[Tracked(false)]
public class CrystalHeartBoss : FinalBoss
{
	public CrystalHeartBoss(Vector2 position, Vector2[] nodes, int patternIndex, float cameraYPastMax, bool dialog, bool startHit, bool cameraLockY)
		: base(position, nodes, patternIndex, cameraYPastMax, dialog, startHit, cameraLockY)
	{
		this.patternIndex = patternIndex;
		CameraYPastMax = cameraYPastMax;
		this.dialog = dialog;
		this.startHit = startHit;
		Add(light = new VertexLight(Color.White, 1f, 32, 64));
		Collider = (circle = new Circle(8f, 0f, -4f));
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
        GetType().GetField("canChangeMusic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, e.Bool("canChangeMusic", defaultValue: true));
	}

    public static void Load()
    {
        IL.Celeste.FinalBoss.CreateBossSprite += ModifyBossSpritesOnCustomFinalBoss;
        On.Celeste.FinalBoss.OnPlayer += CrystalHeartBossExtraEffects;
        On.Celeste.FinalBoss.Added += CrystalHeartBossShrinkHitbox;
    }

    public static void Unload()
    {
        IL.Celeste.FinalBoss.CreateBossSprite -= ModifyBossSpritesOnCustomFinalBoss;
        On.Celeste.FinalBoss.OnPlayer -= CrystalHeartBossExtraEffects;
        On.Celeste.FinalBoss.Added -= CrystalHeartBossShrinkHitbox;
    }

    private static void ModifyBossSpritesOnCustomFinalBoss(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("badeline_boss")))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(ChangeFbSpriteRef);
        }
    }

    private static string ChangeFbSpriteRef(string orig, FinalBoss b)
    {
        if (b is CrystalHeartBoss) return "badeline_boss_femtohelper"; //Change this value as needed
        return orig;
    }

    private static void CrystalHeartBossExtraEffects(On.Celeste.FinalBoss.orig_OnPlayer orig, FinalBoss customFinalBoss, Player player)
    {
        orig(customFinalBoss, player);
        if (customFinalBoss is CrystalHeartBoss)
        {
            for (int i = 0; i < 4; i++)
            {
                (player.Scene as Level)?.Add(new AbsorbOrb(customFinalBoss.Position, customFinalBoss));
                Audio.Play("event:/FemtoHelper/boss_spikes_burst_quiet", customFinalBoss.Position);
            }
        }
    }
    private static void CrystalHeartBossShrinkHitbox(On.Celeste.FinalBoss.orig_Added orig, FinalBoss customFinalBoss, Scene scene)
    {
        orig(customFinalBoss, scene);
        if (customFinalBoss is CrystalHeartBoss)
        {
            customFinalBoss.Collider.Width /= 1.5f;
        }
    }
}
