
using Celeste.Mod.FemtoHelper.Utils;
using System;
using System.Linq;

namespace Celeste.Mod.FemtoHelper.Code.Entities;

[Tracked]
[CustomEntity("FemtoHelper/EHIController")]
public class ExtraHoldableInteractionsController : Entity
{
    public readonly bool InteractWithDashBlocks;
    public readonly bool InteractWithCrushBlocks;
    public readonly bool InteractWithBreakerBoxes;
    public readonly bool InteractWithCoreSwitch;
    public readonly bool InteractWithMoveBlocks;
    public readonly bool InteractWithSwapBlocks;
    public readonly bool InteractWithFallingBlocksH;
    public readonly bool InteractWithFallingBlocksV;
    public readonly bool InteractWithBumpers;

    public Vector2 DashBlockSpeedReq;
    public Vector2 CrushBlockSpeedReq;
    public Vector2 BreakerBoxSpeedReq;
    public Vector2 MoveBlockSpeedReq;
    public Vector2 SwapBlockSpeedReq;
    public Vector2 FallingBlockSpeedReq;

    public bool ExactSpeedMatch;
    public float ExactSpeedTolerance;

    public ExtraHoldableInteractionsController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Tag |= Tags.Persistent;

        InteractWithDashBlocks = data.Bool("breakDashBlock", true);
        InteractWithCrushBlocks = data.Bool("activateKevins", true);
        InteractWithCoreSwitch = data.Bool("flipCoreSwitches", true);
        InteractWithBreakerBoxes = data.Bool("hitLightningBreakerBoxes", true);
        InteractWithMoveBlocks = data.Bool("activateMoveBlocks", false);
        InteractWithSwapBlocks = data.Bool("activateSwapBlocks", false);
        InteractWithFallingBlocksH = data.Bool("activateFallingBlocksHorizontal", false);
        InteractWithFallingBlocksV = data.Bool("activateFallingBlocksVertical", false);
        InteractWithBumpers = data.Bool("interactWithBumpers", false);

        DashBlockSpeedReq = new Vector2(data.Float("dashBlockXSpeedReq", 120f), data.Float("dashBlockYSpeedReq", 120f));
        CrushBlockSpeedReq = new Vector2(data.Float("crushBlockXSpeedReq", 120f), data.Float("crushBlockYSpeedReq", 120f));
        BreakerBoxSpeedReq = new Vector2(data.Float("breakerBoxXSpeedReq", 120f), data.Float("breakerBoxYSpeedReq", 120f));
        MoveBlockSpeedReq = new Vector2(data.Float("moveBlockXSpeedReq", 120f), data.Float("moveBlockYSpeedReq", 120f));
        SwapBlockSpeedReq = new Vector2(data.Float("swapBlockXSpeedReq", 120f), data.Float("swapBlockYSpeedReq", 120f));
        FallingBlockSpeedReq = new Vector2(data.Float("fallingBlockXSpeedReq", 120f), data.Float("fallingBlockYSpeedReq", 120f));

        ExactSpeedMatch = data.Bool("exactSpeedMatch", false);
        ExactSpeedTolerance = data.Float("exactSpeedTolerance", 1f);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (var controller in scene.Tracker.GetEntities<ExtraHoldableInteractionsController>().Cast<ExtraHoldableInteractionsController>().Where(controller => controller != this))
        {
            controller.RemoveSelf();
        }
    }

    public static void Load()
    {
        On.Celeste.CoreModeToggle.Update += CoreModeToggle_Update;
        On.Celeste.Bumper.Added += Bumper_Added;
    }

    public static void Unload()
    {
        On.Celeste.CoreModeToggle.Update -= CoreModeToggle_Update;
        On.Celeste.Bumper.Added -= Bumper_Added;
    }

    private static void CoreModeToggle_Update(On.Celeste.CoreModeToggle.orig_Update orig, CoreModeToggle self)
    {
        orig(self);
        ExtraHoldableInteractionsController controller = self.Scene.Tracker.GetEntity<ExtraHoldableInteractionsController>();

        if (controller is not { InteractWithCoreSwitch: true }) return;
        if (!self.CollideCheckByComponent<Holdable>() || !self.Usable || !(self.cooldownTimer <= 0f)) return;

        self.playSounds = true;
        Level level = self.SceneAs<Level>();
        level.CoreMode = level.CoreMode == global::Celeste.Session.CoreModes.Cold ? global::Celeste.Session.CoreModes.Hot : global::Celeste.Session.CoreModes.Cold;
        if (self.persistent)
        {
            level.Session.CoreMode = level.CoreMode;
        }
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        level.Flash(Color.White * 0.15f, drawPlayerOver: true);
        Celeste.Freeze(0.05f);
        self.cooldownTimer = 1f;
    }

    private static void Bumper_Added(On.Celeste.Bumper.orig_Added orig, Bumper self, Scene scene)
    {
        orig(self, scene);
        scene.OnEndOfFrame += () =>
        {
            ExtraHoldableInteractionsController controller = scene.Tracker.GetEntity<ExtraHoldableInteractionsController>();
            if (controller is not { InteractWithBumpers: true }) return;

            self.Add(new HoldableCollider((Holdable h) =>
            {
                if (self.respawnTimer > 0f || h.IsHeld) return;
                if ((self.Scene as Level).Session.Area.ID == 9)
                {
                    Audio.Play("event:/game/09_core/pinballbumper_hit", self.Position);
                }
                else
                {
                    Audio.Play("event:/game/06_reflection/pinballbumper_hit", self.Position);
                }
                self.respawnTimer = 0.6f;
                //Vector2 vector2 = player.ExplodeLaunch(Position, snapUp: false, sidesOnly: false);
                Vector2 vector2 = h.ExplodeLaunch(self.Position, snapUp: false, sidesOnly: false);

                self.sprite.Play("hit", restart: true);
                self.spriteEvil.Play("hit", restart: true);
                self.light.Visible = false;
                self.bloom.Visible = false;
                self.SceneAs<Level>().DirectionalShake(vector2, 0.15f);
                self.SceneAs<Level>().Displacement.AddBurst(self.Center, 0.3f, 8f, 32f, 0.8f);
                self.SceneAs<Level>().Particles.Emit(Bumper.P_Launch, 12, self.Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());
            }));
        };
    }
}
