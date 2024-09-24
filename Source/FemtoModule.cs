global using Monocle;
global using Microsoft.Xna.Framework;
global using Celeste.Mod.Entities;

using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections;
using Celeste.Mod.FemtoHelper.Effects;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.FemtoHelper.Entities;
using System.Reflection;
using MonoMod.Utils;
using MonoMod.ModInterop;
using System.Collections.Generic;
using Celeste.Mod.FemtoHelper.Code.Effects;
using System.Linq;
using Celeste.Mod.FemtoHelper.Wipes;
using Celeste.Mod.UI;
using Celeste.Mod.FemtoHelper.Code.Entities;

namespace Celeste.Mod.FemtoHelper;

public class FemtoModule : EverestModule
{

    [ModImportName("GravityHelper")]
    public static class GravityHelperSupport
    {
        public static Func<int> GetPlayerGravity;
        public static Action<int, float> SetPlayerGravity;
    }

    [ModImportName("CommunalHelper.DashStates")]
    public static class CommunalHelperSupport
    {
        public static Func<int> GetDreamTunnelDashState;
        public static Func<bool> HasDreamTunnelDash;
    }

    [ModImportName("CavernHelper")]
    public static class CavernHelperSupport
    {
        public static Func<Action<Vector2>, Collider, Component> GetCrystalBombExplosionCollider;
    }

    // Only one alive module instance can exist at any given time.
    public static FemtoModule Instance;
    public static SpriteBank femtoSpriteBank;
    public override Type SessionType => typeof(FemtoHelperSession);
    public static FemtoHelperSession Session => (FemtoHelperSession)Instance._Session;

    public FemtoModule()
    {
        Instance = this;
    }


    private static void Puffer_KaizoCollideHHook(On.Celeste.Puffer.orig_OnCollideH orig, Puffer self, CollisionData data)
    {
        if (self.ToString() == "Celeste.Puffer")
        {
            if (data.Hit is Generic_SMWBlock && !(data.Hit as Generic_SMWBlock).active)
            {
                if (self.hitSpeed.X > 0)
                {
                    if ((data.Hit as Generic_SMWBlock).canHitLeft) (data.Hit as Generic_SMWBlock).Hit(null, 1);
                }
                if (self.hitSpeed.X < 0)
                {
                    if ((data.Hit as Generic_SMWBlock).canHitRight) (data.Hit as Generic_SMWBlock).Hit(null, 2);
                }

            }
        }
        orig(self, data);
    }
    private static void Puffer_KaizoCollideVHook(On.Celeste.Puffer.orig_OnCollideV orig, Puffer self, CollisionData data)
    {
        if (self.ToString() == "Celeste.Puffer")
        {
            if (data.Hit is Generic_SMWBlock && !(data.Hit as Generic_SMWBlock).active)
            {
                if (self.hitSpeed.Y > 0)
                {
                    if ((data.Hit as Generic_SMWBlock).canHitTop) (data.Hit as Generic_SMWBlock).Hit(null, 3);
                }
                if (self.hitSpeed.Y < 0)
                {
                    if ((data.Hit as Generic_SMWBlock).canHitBottom) (data.Hit as Generic_SMWBlock).Hit(null, 0);
                }

            }
        }
        orig(self, data);
    }

    private static void Puffer_SplodeKaizoHook(On.Celeste.Puffer.orig_Explode orig, Puffer self)
    {
        orig(self);
        Collider collider = self.Collider;
        self.Collider = new Circle(40f);
        foreach (Generic_SMWBlock entity in self.Scene.Tracker.GetEntities<Generic_SMWBlock>())
        {
            if (entity != null)
            {
                if (self.CollideCheck(entity) && !entity.active)
                {
                    entity.Hit(null, 0);
                }
            }
        }
        self.Collider = collider;
    }
    private static IEnumerator Seeker_RegenKaizoHook(On.Celeste.Seeker.orig_RegenerateCoroutine orig, Seeker self)
    {
        IEnumerator origEnum = orig(self);
        while (origEnum.MoveNext()) yield return origEnum.Current;
        self.Collider = new Circle(40f);
        foreach (Generic_SMWBlock entity in self.Scene.Tracker.GetEntities<Generic_SMWBlock>())
        {
            if (entity != null)
            {
                if (self.CollideCheck(entity) && !entity.active)
                {
                    entity.Hit(null, 0);
                }
            }
        }
        self.Collider = new Hitbox(6f, 6f, -3f, -3f);
    }

    [Command("test_femto_wipes", "test Femto Helper custom wipes")]
    private static void CmdFemtoWipes()
    {
        Engine.Scene = new TestFemtoWipes();
    }

    [Command("soundtest", "(FemtoHelper command) warps to the Everest sound test, type \"overworld\" first for this to work")]
    private static void CmdSoundTest()
    {
        OuiModOptions.Instance.Overworld.Goto<OuiSoundTest>();
    }

    // Set up any hooks, event handlers and your mod in general here.
    // Load runs before Celeste itself has initialized properly.
    public override void Load()
    {

        typeof(FemtoHelperExports).ModInterop();

        typeof(GravityHelperSupport).ModInterop(); //:3

        typeof(CommunalHelperSupport).ModInterop(); //:33

        typeof(CavernHelperSupport).ModInterop(); //:333

        Everest.Events.Level.OnLoadBackdrop += Level_OnLoadBackdrop;
        On.Celeste.Puffer.OnCollideH += Puffer_KaizoCollideHHook;
        On.Celeste.Puffer.OnCollideV += Puffer_KaizoCollideVHook;
        On.Celeste.Puffer.Explode += Puffer_SplodeKaizoHook;
        On.Celeste.Seeker.RegenerateCoroutine += Seeker_RegenKaizoHook;
        IL.Celeste.Actor.MoveHExact += onCollideH_IL;
        IL.Celeste.Actor.MoveVExact += onCollideV_IL;
        On.Celeste.CoreModeToggle.Update += CoreModeToggle_Update;
        CodecumberPortStuff.Load();
        VitalDrainController.Load();
        CrystalHeartBoss.Load();

        Everest.Events.Player.OnSpawn += ReloadDistortedParallax;
    }


    public override void Unload()
    {
        Everest.Events.Level.OnLoadBackdrop -= Level_OnLoadBackdrop;
        On.Celeste.Puffer.OnCollideH -= Puffer_KaizoCollideHHook;
        On.Celeste.Puffer.OnCollideV -= Puffer_KaizoCollideVHook;
        On.Celeste.Puffer.Explode -= Puffer_SplodeKaizoHook;
        On.Celeste.Seeker.RegenerateCoroutine -= Seeker_RegenKaizoHook;
        IL.Celeste.Actor.MoveHExact -= onCollideH_IL;
        IL.Celeste.Actor.MoveVExact -= onCollideV_IL;
        On.Celeste.CoreModeToggle.Update -= CoreModeToggle_Update;
        CodecumberPortStuff.Unload();
        VitalDrainController.Unload();
        CrystalHeartBoss.Unload();

    }

    private static void CoreModeToggle_Update(On.Celeste.CoreModeToggle.orig_Update orig, CoreModeToggle self)
    {
        orig(self);
        ExtraHoldableInteractionsController controller = self.Scene.Tracker.GetEntity<ExtraHoldableInteractionsController>();

        if (controller != null && controller.InteractWithCoreSwitch)
        {
            if (self.CollideCheckByComponent<Holdable>() && self.Usable && self.cooldownTimer <= 0f)
            {
                self.playSounds = true;
                Level level = self.SceneAs<Level>();
                if (level.CoreMode == global::Celeste.Session.CoreModes.Cold)
                {
                    level.CoreMode = global::Celeste.Session.CoreModes.Hot;
                }
                else
                {
                    level.CoreMode = global::Celeste.Session.CoreModes.Cold;
                }
                if (self.persistent)
                {
                    level.Session.CoreMode = level.CoreMode;
                }
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                level.Flash(Color.White * 0.15f, drawPlayerOver: true);
                Celeste.Freeze(0.05f);
                self.cooldownTimer = 1f;
            }
        }
    }


    private static void onCollideH_IL(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdloc(4)))
        {
            //Logger.Log(LogLevel.Info, "FemtoHelper/onCollideH_IL", $"Emitting extra collision actions at {cursor.Index} in CIL code for {cursor.Method.FullName}");

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(InvokeExtraCollisionActionsH);
        }
    }

    public static CollisionData InvokeExtraCollisionActionsH(CollisionData data, Actor self)
    {
        if (self is Debris) return data;
        Holdable h = self.Get<Holdable>();
        if (h == null) return data;

        ExtraHoldableInteractionsController controller = self.Scene.Tracker.GetEntity<ExtraHoldableInteractionsController>();

        if (controller != null)
        {
            if (controller.InteractWithCrushBlocks && data.Hit is CrushBlock)
            {
                if ((data.Hit as CrushBlock).CanActivate(-data.Direction))
                {
                    if (Math.Abs(h.GetSpeed().X) >= controller.CrushBlockSpeedReq.X) (data.Hit as CrushBlock).Attack(-data.Direction);
                }
            }

            if (controller.InteractWithDashBlocks && data.Hit is DashBlock)
            {
                if (Math.Abs(h.GetSpeed().X) >= controller.DashBlockSpeedReq.X) (data.Hit as DashBlock).Break(self.Center, data.Direction, true, true);
            }

            if (controller.InteractWithBreakerBoxes && data.Hit is LightningBreakerBox)
            {
                LightningBreakerBox l = (data.Hit as LightningBreakerBox);
                if (!(data.Direction == Vector2.UnitX && l.spikesLeft) && !(data.Direction == -Vector2.UnitX && l.spikesRight) && Math.Abs(h.GetSpeed().X) >= controller.BreakerBoxSpeedReq.X)
                {
                    (l.Scene as Level).DirectionalShake(data.Direction);
                    l.sprite.Scale = new Vector2(1f + Math.Abs(data.Direction.Y) * 0.4f - Math.Abs(data.Direction.X) * 0.4f, 1f + Math.Abs(data.Direction.X) * 0.4f - Math.Abs(data.Direction.Y) * 0.4f);
                    l.health--;
                    if (l.health > 0)
                    {
                        l.Add(l.firstHitSfx = new SoundSource("event:/new_content/game/10_farewell/fusebox_hit_1"));
                        Celeste.Freeze(0.1f);
                        l.shakeCounter = 0.2f;
                        l.shaker.On = true;
                        l.bounceDir = data.Direction;
                        l.bounce.Start();
                        l.smashParticles = true;
                        l.Pulse();
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    }
                    else
                    {
                        l.firstHitSfx?.Stop();
                        Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", l.Position);
                        Celeste.Freeze(0.2f);
                        l.Break();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                        l.SmashParticles(data.Direction.Perpendicular());
                        l.SmashParticles(-data.Direction.Perpendicular());
                    }
                }
            }
            if(controller.InteractWithSwapBlocks && data.Hit is SwapBlock)
            {
                SwapBlock s = data.Hit as SwapBlock;
                if (Math.Abs(h.GetSpeed().X) >= controller.SwapBlockSpeedReq.X)
                {
                    s.Swapping = s.lerp < 1f;
                    s.target = 1;
                    s.returnTimer = 0.8f;
                    s.burst = (s.Scene as Level).Displacement.AddBurst(s.Center, 0.2f, 0f, 16f);
                    if (s.lerp >= 0.2f)
                    {
                        s.speed = s.maxForwardSpeed;
                    }
                    else
                    {
                        s.speed = MathHelper.Lerp(s.maxForwardSpeed * 0.333f, s.maxForwardSpeed, s.lerp / 0.2f);
                    }
                    Audio.Stop(s.returnSfx);
                    Audio.Stop(s.moveSfx);
                    if (!s.Swapping)
                    {
                        Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", s.Center);
                    }
                    else
                    {
                        s.moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", s.Center);
                    }
                }
            }
            if (controller.InteractWithMoveBlocks && (data.Hit is MoveBlock) && (Math.Abs(h.GetSpeed().X) >= controller.MoveBlockSpeedReq.X))
            {
                (data.Hit as MoveBlock).triggered = true;
            }
            if (controller.InteractWithFallingBlocksH && (data.Hit is FallingBlock) && (Math.Abs(h.GetSpeed().X) >= controller.FallingBlockSpeedReq.X))
            {
                (data.Hit as FallingBlock).Triggered = true;
            }
        }

        if (data.Hit is Generic_SMWBlock)
        {
            Generic_SMWBlock smwblock = (data.Hit as Generic_SMWBlock);
            if (!smwblock.active)
            {
                if (h.GetSpeed().X > 20)
                {
                    if (smwblock.canHitLeft) smwblock.Hit(null, 1);
                }
                if (h.GetSpeed().X < -20)
                {
                    if (smwblock.canHitRight) smwblock.Hit(null, 2);
                }
            }
        }

        return data;
    }

    private static void onCollideV_IL(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdloc(4)))
        {
            //Logger.Log(LogLevel.Info, "FemtoHelper/onCollideV_IL", $"Emitting extra collision actions at {cursor.Index} in CIL code for {cursor.Method.FullName}");

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(InvokeExtraCollisionActionsV);
        }
    }

    public static CollisionData InvokeExtraCollisionActionsV(CollisionData data, Actor self)
    {
        if (self is Debris) return data;
        Holdable h = self.Get<Holdable>();
        if (h == null) return data;

        ExtraHoldableInteractionsController controller = self.Scene.Tracker.GetEntity<ExtraHoldableInteractionsController>();

        if (controller != null)
        {
            if (controller.InteractWithCrushBlocks && data.Hit is CrushBlock)
            {
                if ((data.Hit as CrushBlock).CanActivate(-data.Direction))
                {
                    if (Math.Abs(h.GetSpeed().Y) >= controller.CrushBlockSpeedReq.Y) (data.Hit as CrushBlock).Attack(-data.Direction);
                }
            }

            if (controller.InteractWithDashBlocks && data.Hit is DashBlock)
            {
                if (Math.Abs(h.GetSpeed().Y) >= controller.DashBlockSpeedReq.Y) (data.Hit as DashBlock).Break(self.Center, data.Direction, true, true);
            }

            if (controller.InteractWithBreakerBoxes && data.Hit is LightningBreakerBox)
            {
                LightningBreakerBox l = (data.Hit as LightningBreakerBox);
                if (!(data.Direction == Vector2.UnitX && l.spikesLeft) && !(data.Direction == -Vector2.UnitX && l.spikesRight) && Math.Abs(h.GetSpeed().Y) >= controller.BreakerBoxSpeedReq.Y)
                {
                    (l.Scene as Level).DirectionalShake(data.Direction);
                    l.sprite.Scale = new Vector2(1f + Math.Abs(data.Direction.Y) * 0.4f - Math.Abs(data.Direction.X) * 0.4f, 1f + Math.Abs(data.Direction.X) * 0.4f - Math.Abs(data.Direction.Y) * 0.4f);
                    l.health--;
                    if (l.health > 0)
                    {
                        l.Add(l.firstHitSfx = new SoundSource("event:/new_content/game/10_farewell/fusebox_hit_1"));
                        Celeste.Freeze(0.1f);
                        l.shakeCounter = 0.2f;
                        l.shaker.On = true;
                        l.bounceDir = data.Direction;
                        l.bounce.Start();
                        l.smashParticles = true;
                        l.Pulse();
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    }
                    else
                    {
                        if (l.firstHitSfx != null)
                        {
                            l.firstHitSfx.Stop();
                        }
                        Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", l.Position);
                        Celeste.Freeze(0.2f);
                        l.Break();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                        l.SmashParticles(data.Direction.Perpendicular());
                        l.SmashParticles(-data.Direction.Perpendicular());
                    }
                }
            }
            if (controller.InteractWithSwapBlocks && data.Hit is SwapBlock)
            {
                SwapBlock s = data.Hit as SwapBlock;
                if (Math.Abs(h.GetSpeed().Y) >= controller.SwapBlockSpeedReq.Y)
                {
                    s.Swapping = s.lerp < 1f;
                    s.target = 1;
                    s.returnTimer = 0.8f;
                    s.burst = (s.Scene as Level).Displacement.AddBurst(s.Center, 0.2f, 0f, 16f);
                    if (s.lerp >= 0.2f)
                    {
                        s.speed = s.maxForwardSpeed;
                    }
                    else
                    {
                        s.speed = MathHelper.Lerp(s.maxForwardSpeed * 0.333f, s.maxForwardSpeed, s.lerp / 0.2f);
                    }
                    Audio.Stop(s.returnSfx);
                    Audio.Stop(s.moveSfx);
                    if (!s.Swapping)
                    {
                        Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", s.Center);
                    }
                    else
                    {
                        s.moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", s.Center);
                    }
                }
            }
            if (controller.InteractWithMoveBlocks && (data.Hit is MoveBlock) && (Math.Abs(h.GetSpeed().Y) >= controller.MoveBlockSpeedReq.Y))
            {
                (data.Hit as MoveBlock).triggered = true;
            }
            if (controller.InteractWithFallingBlocksV && (data.Hit is FallingBlock) && (Math.Abs(h.GetSpeed().Y) >= controller.FallingBlockSpeedReq.Y))
            {
                (data.Hit as FallingBlock).Triggered = true;
            }
        }

        if (data.Hit is Generic_SMWBlock)
        {
            Generic_SMWBlock smwblock = (data.Hit as Generic_SMWBlock);
            if (!smwblock.active)
            {
                if (h.GetSpeed().Y > 80)
                {
                    if (smwblock.canHitTop) smwblock.Hit(null, 3);
                }
                if (h.GetSpeed().Y < 0)
                {
                    if (smwblock.canHitBottom) smwblock.Hit(null, 0);
                }
            }
        }

        return data;
    }

    public static void ReloadDistortedParallax(Player player)
    {
        Level level = player.Scene as Level;
        List<NewDistortedParallax> d = level.Background.GetEach<NewDistortedParallax>().ToList();
        foreach (NewDistortedParallax parallax in d)
        {
            parallax.Reset();
        }
    }

    // Optional, initialize anything after Celeste has initialized itself properly.
    public override void Initialize()
    {
    }

    // Optional, do anything requiring either the Celeste or mod content here. 
    // Usually involves Spritebanks or custom particle effects.
    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);
        femtoSpriteBank = new SpriteBank(GFX.Game, "Graphics/FemtoHelper/Sprites.xml");
    }

    // Unload the entirety of your mod's content. Free up any native resources.

    private Backdrop Level_OnLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above)
    {
        if (child.Name.Equals("FemtoHelper/WindPetals", StringComparison.OrdinalIgnoreCase))
        {
            return new WindPetals(child.Attr("colors", "66cc33"), child.AttrFloat("fallingSpeedMin", 8f), child.AttrFloat("fallingSpeedMax", 16f), child.AttrInt("blurCount", 15), child.AttrFloat("blurDensity", 3), child.Attr("texture", "particles/petal"), child.AttrInt("particleCount", 40), child.AttrFloat("parallax", 1f), child.AttrFloat("spinSpeedMultiplier", 1f), child.AttrFloat("spinAmountMultiplier", 1f), child.AttrFloat("alpha", 1f), child.AttrFloat("scale", 1f), child.AttrFloat("minXDriftSpeed", 0f), child.AttrFloat("maxXDriftSpeed", 0f));
        }
        if (child.Name.Equals("FemtoHelper/FemtoStars", StringComparison.OrdinalIgnoreCase))
        {
            return new FemtoStars(child.AttrInt("trailCount", 8), child.Attr("colors", "ffffff"), child.AttrFloat("minXSpeed", 0f), child.AttrFloat("maxXSpeed", 0f), child.AttrFloat("minYSpeed", 6f), child.AttrFloat("maxYSpeed", 12f), child.AttrFloat("extraLoopBorderX", 0), child.AttrFloat("extraLoopBorderY", 0), child.AttrInt("starCount", 100), child.Attr("backgroundColor", "000000"), child.AttrFloat("backgroundAlpha", 1f), child.Attr("sprite", "bgs/02/stars"), child.AttrFloat("scrollX", 0f), child.AttrFloat("scrollY", 0f), child.AttrFloat("alpha", 1f), child.AttrFloat("trailSeparation", 1f), child.AttrFloat("animationRate", 2f + Calc.Random.NextFloat(2f)), child.Attr("alphas", "1"));
        }
        if (child.Name.Equals("FemtoHelper/PolygonStars", StringComparison.OrdinalIgnoreCase))
        {
            return new PolygonStars(child.AttrInt("sideCount", 10), child.AttrFloat("pointinessMultiplier", 2f), child.AttrFloat("minRotationSpeed", 1f), child.AttrFloat("maxRotationSpeed", 2f), child.AttrFloat("minSize", 2f), child.AttrFloat("maxSize", 8f), child.AttrFloat("loopBorder", 64f), child.Attr("colors", "008080"), child.AttrFloat("angle", 270f), child.AttrFloat("alpha", 1f), child.AttrFloat("minSpeed", 24f), child.AttrFloat("maxSpeed", 48f), child.AttrInt("amount", 50), child.AttrFloat("scroll", 0.5f));
        }
        if (child.Name.Equals("FemtoHelper/FadingNorthernLights", StringComparison.OrdinalIgnoreCase))
        {
            return new FadingNorthernLights();
        }
        if (child.Name.Equals("FemtoHelper/VaporWave", StringComparison.OrdinalIgnoreCase))
        {
            return new VaporWave(child.AttrFloat("lineCount", 30), child.AttrFloat("horizon", 64));
        }
        if (child.Name.Equals("FemtoHelper/VectorSpace", StringComparison.OrdinalIgnoreCase))
        {
            return new VectorSpace(child.AttrFloat("spacingX", 30), child.AttrFloat("spacingY", 30), child.AttrFloat("velocityX", -10), child.AttrFloat("velocityY", 10), child.AttrFloat("scroll", 1), child.Attr("color", "ffffff"), child.AttrFloat("sineXOffsetMin", 0), child.AttrFloat("sineXOffsetMax", 180), child.AttrFloat("sineYOffsetMin", 0), child.AttrFloat("sineYOffsetMax", 180), child.AttrFloat("sineXFreqMin", -90), child.AttrFloat("sineXFreqMax", 90), child.AttrFloat("sineYFreqMin", -90), child.AttrFloat("sineYFreqMax", 90), child.AttrFloat("amplitude", 12), child.AttrBool("renderTip", true), child.AttrBool("scaleTip", false), child.AttrFloat("alpha", 1), child.AttrBool("yFreqRelativeToX", false), child.AttrBool("yOffsetRelativeToX", false));
        }
        if (child.Name.Equals("FemtoHelper/DigitalCascade", StringComparison.OrdinalIgnoreCase))
        {
            return new DigitalCascade(
                child.AttrInt("symbolAmount", 50),
                child.AttrInt("afterimagesCap", 10000),
                child.Attr("colors", "00cc00"),
                child.AttrFloat("alpha", 1f),
                child.AttrFloat("minSpeed", 20),
                child.AttrFloat("maxSpeed", 40),
                child.AttrFloat("angle", 0),
                child.AttrFloat("extraLoopBorderX", 20),
                child.AttrFloat("extraLoopBorderY", 20),
                child.Attr("spritePath", "bgs/FemtoHelper/digitalCascade/star"),
                child.AttrFloat("minLifetime", 1),
                child.AttrFloat("maxLifetime", 2),
                child.AttrFloat("fadeTime", 0.1f),
                child.AttrInt("fadeScaleMode", 1),
                child.AttrBool("randomSymbol", false),
                child.AttrFloat("afterimageFadeTime", 1f),
                child.AttrFloat("timeBetweenAfterimages", 0.25f),
                child.AttrFloat("scroll", 1f),
                child.AttrBool("noFlipping", false),
                child.AttrFloat("afterimageAlpha", 0.6f)
                );
        }
        if (child.Name.Equals("FemtoHelper/DistortedParallax", StringComparison.OrdinalIgnoreCase))
        {
            return new DistortedParallax(
                child.Attr("texture", "bgs/10/sky"),

                child.AttrFloat("posX", 0),
                child.AttrFloat("posY", 0),

                child.AttrFloat("speedX", 0),
                child.AttrFloat("speedY", 0),

                child.AttrFloat("scrollX", 1),
                child.AttrFloat("scrollY", 1),

                child.AttrBool("loopX", false),
                child.AttrBool("loopY", false),

                Calc.HexToColor(child.Attr("color", "FFFFFF")) * child.AttrFloat("alpha", 1f),

                child.Attr("blendState") == "additive" ? BlendState.Additive : BlendState.AlphaBlend,

                child.AttrFloat("periodX", 40),
                child.AttrFloat("periodY", 10),

                child.AttrFloat("amplitudeX", 10),
                child.AttrFloat("amplitudeY", 0),

                child.AttrFloat("waveSpeedX", 5),
                child.AttrFloat("waveSpeedY", 5),

                new Vector2(child.AttrFloat("offsetX", 0), child.AttrFloat("offsetY", 0)),

                child.AttrBool("flipX", false),
                child.AttrBool("flipY", false),

                child.AttrInt("sliceMode", 0) == 0 ? DistortedParallax.sliceModes.TransLong : child.AttrInt("sliceMode", 0) == 1 ? DistortedParallax.sliceModes.LongTrans : child.AttrInt("sliceMode", 0) == 2 ? DistortedParallax.sliceModes.TransTrans : DistortedParallax.sliceModes.LongLong
                );
        }
        return null;
    }



}

