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
using MonoMod.Utils;
using MonoMod.ModInterop;
using System.Collections.Generic;
using Celeste.Mod.FemtoHelper.Code.Effects;
using System.Linq;
using Celeste.Mod.FemtoHelper.Wipes;
using Celeste.Mod.FemtoHelper.Code.Entities;
using MonoMod.RuntimeDetour;
using System.Reflection;
using static Celeste.Mod.FemtoHelper.Entities.SparkRefill;
using System.Diagnostics.CodeAnalysis;

namespace Celeste.Mod.FemtoHelper;

public class FemtoModule : EverestModule
{

    [ModImportName("GravityHelper")]
    public static class GravityHelperSupport
    {
        public static Func<int> GetPlayerGravity;
        public static Action<int, float> SetPlayerGravity;
        public static Func<Actor, Action<Entity, int, float>, Component> CreateGravityListener;
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

    [ModImportName("FrostHelper")]
    public static class FrostHelperSupport
    {
        public delegate bool TryCreateSessionExpressionDelegate(string str, [NotNullWhen(true)] out object expression);
        public static TryCreateSessionExpressionDelegate TryCreateSessionExpression;

        public static Func<object, Session, int> GetIntSessionExpressionValue;
    }

    // Only one alive module instance can exist at any given time.
    public static FemtoModule Instance;
    public static SpriteBank FemtoSpriteBank;
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
            if (data.Hit is Generic_SMWBlock { Active: false } block)
            {
                switch (self.hitSpeed.X)
                {
                    case > 0:
                    {
                        if (block.CanHitLeft) block.Hit(null, 1);
                        break;
                    }
                    case < 0:
                    {
                        if (block.CanHitRight) block.Hit(null, 2);
                        break;
                    }
                }
            }
        }
        orig(self, data);
    }
    private static void Puffer_KaizoCollideVHook(On.Celeste.Puffer.orig_OnCollideV orig, Puffer self, CollisionData data)
    {
        if (self.ToString() == "Celeste.Puffer")
        {
            if (data.Hit is Generic_SMWBlock { Active: false } block)
            {
                switch (self.hitSpeed.Y)
                {
                    case > 0:
                    {
                        if (block.CanHitTop) block.Hit(null, 3);
                        break;
                    }
                    case < 0:
                    {
                        if (block.CanHitBottom) block.Hit(null, 0);
                        break;
                    }
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
        foreach (var entity in from Generic_SMWBlock entity in self.Scene.Tracker.GetEntities<Generic_SMWBlock>() where entity != null where self.CollideCheck(entity) && !entity.Active select entity)
        {
            entity.Hit(null, 0);
        }
        self.Collider = collider;
    }
    private static IEnumerator Seeker_RegenKaizoHook(On.Celeste.Seeker.orig_RegenerateCoroutine orig, Seeker self)
    {
        IEnumerator origEnum = orig(self);
        while (origEnum.MoveNext()) yield return origEnum.Current;
        self.Collider = new Circle(40f);
        foreach (var entity in from Generic_SMWBlock entity in self.Scene.Tracker.GetEntities<Generic_SMWBlock>() where entity != null where self.CollideCheck(entity) && !entity.Active select entity)
        {
            entity.Hit(null, 0);
        }
        self.Collider = new Hitbox(6f, 6f, -3f, -3f);
    }

    [Command("test_femto_wipes", "test Femto Helper custom wipes")]
    private static void CmdFemtoWipes()
    {
        Engine.Scene = new TestFemtoWipes();
    }

    // Set up any hooks, event handlers and your mod in general here.
    // Load runs before Celeste itself has initialized properly.

    private static ILHook dashCoroutineHook;
    private static ILHook redDashCoroutineHook;

    public Hook canDashHook;
    private delegate bool orig_CanDash(Player self);
    private bool modCanDash(orig_CanDash orig, Player self)
    {
        if (self.Get<LimitRefill.DirectionConstraint>() is { } d)
        {
            Vector2 aim = Input.GetAimVector();

            // block the dash directly if the player is holding a forbidden direction, and does not have Dash Assist enabled.
            return orig(self) && (SaveData.Instance.Assists.DashAssist || isDashDirectionAllowed(aim, d));
        }
        return orig(self);
    }
    private bool isDashDirectionAllowed(Vector2 direction, LimitRefill.DirectionConstraint d)
    {
        // if directions are not integers, make them integers.
        direction = new Vector2(Math.Sign(direction.X), Math.Sign(direction.Y));

        // bottom-left (-1, 1) is row 2, column 0.
        return d.dirs[(int)(direction.Y + 1),(int)(direction.X + 1)];
    }

    private static void modDashSpeed(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        // find 240f in the method (dash speed) and multiply it with our modifier.
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(240f)))
        {
            cursor.EmitLdloc1();
            cursor.EmitDelegate(GetMultiplier);
            cursor.EmitMul();
        }
    }

    private static float GetMultiplier(Player player)
    {
        if (player.Get<SparkDash>() is { } s) return s.ThisDashHasStarted ? 2f : 1f;
        return 1f;
    }

    private void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);
        if (self.Get<SparkDash>() is { } s) s.ThisDashHasStarted = true;
    }

    private static void Player_DashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        orig(self);
        if (self.Get<SparkDash>() is { } s && s.ThisDashHasStarted) s.RemoveSelf();
    }

    public override void Load()
    {
        canDashHook = new Hook(typeof(Player).GetMethod("get_CanDash"), typeof(FemtoModule).GetMethod("modCanDash", BindingFlags.NonPublic | BindingFlags.Instance), this);

        dashCoroutineHook = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(), modDashSpeed);
        redDashCoroutineHook = new ILHook(typeof(Player).GetMethod("RedDashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(), modDashSpeed);
        On.Celeste.Player.DashEnd += Player_DashEnd;
        On.Celeste.Player.DashBegin += Player_DashBegin;

        typeof(FemtoHelperExports).ModInterop();

        typeof(GravityHelperSupport).ModInterop(); //:3

        typeof(CommunalHelperSupport).ModInterop(); //:33

        typeof(CavernHelperSupport).ModInterop(); //:333

        typeof(FrostHelperSupport).ModInterop();

        Everest.Events.Level.OnLoadBackdrop += Level_OnLoadBackdrop;
        On.Celeste.Puffer.OnCollideH += Puffer_KaizoCollideHHook;
        On.Celeste.Puffer.OnCollideV += Puffer_KaizoCollideVHook;
        On.Celeste.Puffer.Explode += Puffer_SplodeKaizoHook;
        On.Celeste.Seeker.RegenerateCoroutine += Seeker_RegenKaizoHook;
        IL.Celeste.Actor.MoveHExact += onCollideH_IL;
        IL.Celeste.Actor.MoveVExact += onCollideV_IL;
        CodecumberPortStuff.Load();
        VitalDrainController.Load();
        CrystalHeartBoss.Load();
        PlutoniumText.Load();
        ClutterShadowController.Load();
        SMWHoldable.Load();
        ExtraHoldableInteractionsController.Load();
        Monopticon.Load();
        GenericWaterBlock.Load();
        TheContraption.Load();
        LimitRefill.Load();
        BoundRefill.Load();

        Everest.Events.Player.OnSpawn += ReloadDistortedParallax;
    }

    public override void Unload()
    {
        canDashHook?.Dispose();
        canDashHook = null;

        dashCoroutineHook?.Dispose();
        redDashCoroutineHook?.Dispose();

        dashCoroutineHook = null;
        redDashCoroutineHook = null;

        On.Celeste.Player.DashEnd -= Player_DashEnd;
        On.Celeste.Player.DashBegin -= Player_DashBegin;

        Everest.Events.Level.OnLoadBackdrop -= Level_OnLoadBackdrop;
        On.Celeste.Puffer.OnCollideH -= Puffer_KaizoCollideHHook;
        On.Celeste.Puffer.OnCollideV -= Puffer_KaizoCollideVHook;
        On.Celeste.Puffer.Explode -= Puffer_SplodeKaizoHook;
        On.Celeste.Seeker.RegenerateCoroutine -= Seeker_RegenKaizoHook;
        IL.Celeste.Actor.MoveHExact -= onCollideH_IL;
        IL.Celeste.Actor.MoveVExact -= onCollideV_IL;
        CodecumberPortStuff.Unload();
        VitalDrainController.Unload();
        CrystalHeartBoss.Unload();
        PlutoniumText.Unload();
        ClutterShadowController.Unload();
        SMWHoldable.Unload();
        ExtraHoldableInteractionsController.Unload();
        Monopticon.Unload();
        GenericWaterBlock.Unload();
        TheContraption.Unload();
        LimitRefill.Unload();
        BoundRefill.Unload();
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
    
    private static bool CheckSpeed(float speed, float req, bool equal, float tolerance)
    {
        if (!equal)
        {
            return speed >= req;
        }
        return Math.Abs(speed - req) < tolerance;
    }

    public static CollisionData InvokeExtraCollisionActionsH(CollisionData data, Actor self)
    {
        if (self is Debris) return data;
        Holdable h = self.Get<Holdable>();
        if (h == null) return data;

        ExtraHoldableInteractionsController controller = self.Scene.Tracker.GetEntity<ExtraHoldableInteractionsController>();

        if (controller != null)
        {
            switch (data.Hit)
            {
                case CrushBlock block when controller.InteractWithCrushBlocks:
                    if (block.CanActivate(-data.Direction) && CheckSpeed(Math.Abs(h.GetSpeed().X), controller.CrushBlockSpeedReq.X, controller.ExactSpeedMatch, controller.ExactSpeedTolerance)) block.Attack(-data.Direction);
                    break;
                case DashBlock dashBlock when controller.InteractWithDashBlocks:
                    if(CheckSpeed(Math.Abs(h.GetSpeed().X), controller.DashBlockSpeedReq.X, controller.ExactSpeedMatch, controller.ExactSpeedTolerance)) dashBlock.Break(self.Center, data.Direction, true, true);
                    break;
                case LightningBreakerBox box when controller.InteractWithBreakerBoxes:
                    if (!(data.Direction == Vector2.UnitX && box.spikesLeft) && !(data.Direction == -Vector2.UnitX && box.spikesRight) && CheckSpeed(Math.Abs(h.GetSpeed().X), controller.BreakerBoxSpeedReq.X, controller.ExactSpeedMatch, controller.ExactSpeedTolerance))
                    {
                        (box.Scene as Level)?.DirectionalShake(data.Direction);
                        box.sprite.Scale = new Vector2(1f + Math.Abs(data.Direction.Y) * 0.4f - Math.Abs(data.Direction.X) * 0.4f, 1f + Math.Abs(data.Direction.X) * 0.4f - Math.Abs(data.Direction.Y) * 0.4f);
                        box.health--;
                        if (box.health > 0)
                        {
                            box.Add(box.firstHitSfx = new SoundSource("event:/new_content/game/10_farewell/fusebox_hit_1"));
                            Celeste.Freeze(0.1f);
                            box.shakeCounter = 0.2f;
                            box.shaker.On = true;
                            box.bounceDir = data.Direction;
                            box.bounce.Start();
                            box.smashParticles = true;
                            box.Pulse();
                            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        }
                        else
                        {
                            box.firstHitSfx?.Stop();
                            Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", box.Position);
                            Celeste.Freeze(0.2f);
                            box.Break();
                            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                            box.SmashParticles(data.Direction.Perpendicular());
                            box.SmashParticles(-data.Direction.Perpendicular());
                        }
                    }
                    break;
                case SwapBlock swapBlock when controller.InteractWithSwapBlocks:
                    if (CheckSpeed(Math.Abs(h.GetSpeed().X), controller.SwapBlockSpeedReq.X, controller.ExactSpeedMatch, controller.ExactSpeedTolerance))
                    {
                        swapBlock.Swapping = swapBlock.lerp < 1f;
                        swapBlock.target = 1;
                        swapBlock.returnTimer = 0.8f;
                        swapBlock.burst = (swapBlock.Scene as Level)?.Displacement.AddBurst(swapBlock.Center, 0.2f, 0f, 16f);
                        swapBlock.speed = swapBlock.lerp >= 0.2f ? swapBlock.maxForwardSpeed : MathHelper.Lerp(swapBlock.maxForwardSpeed * 0.333f, swapBlock.maxForwardSpeed, swapBlock.lerp / 0.2f);
                        Audio.Stop(swapBlock.returnSfx);
                        Audio.Stop(swapBlock.moveSfx);
                        if (!swapBlock.Swapping)
                        {
                            Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", swapBlock.Center);
                        }
                        else
                        {
                            swapBlock.moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", swapBlock.Center);
                        }
                    }
                    break;
                case MoveBlock moveBlock when controller.InteractWithMoveBlocks && CheckSpeed(Math.Abs(h.GetSpeed().X), controller.MoveBlockSpeedReq.X, controller.ExactSpeedMatch, controller.ExactSpeedTolerance):
                    moveBlock.triggered = true;
                    break;
                case FallingBlock fallingBlock when controller.InteractWithMoveBlocks && CheckSpeed(Math.Abs(h.GetSpeed().X), controller.FallingBlockSpeedReq.X, controller.ExactSpeedMatch, controller.ExactSpeedTolerance):
                    fallingBlock.Triggered = true;
                    break;
            }
        }

        if (data.Hit is not Generic_SMWBlock smwblock || smwblock.Active) return data;
        
        if (h.GetSpeed().X > 20)
        {
            if (smwblock.CanHitLeft) smwblock.Hit(null, 1);
        }
        if (h.GetSpeed().X < -20)
        {
            if (smwblock.CanHitRight) smwblock.Hit(null, 2);
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
            switch (data.Hit)
            {
                case CrushBlock block when controller.InteractWithCrushBlocks:
                    if (block.CanActivate(-data.Direction))
                    {
                        if (CheckSpeed(Math.Abs(h.GetSpeed().Y), controller.CrushBlockSpeedReq.Y, controller.ExactSpeedMatch, controller.ExactSpeedTolerance)) block.Attack(-data.Direction);
                    }
                    break;
                case DashBlock dashBlock when controller.InteractWithDashBlocks:
                    if (CheckSpeed(Math.Abs(h.GetSpeed().Y), controller.DashBlockSpeedReq.Y, controller.ExactSpeedMatch, controller.ExactSpeedTolerance)) dashBlock.Break(self.Center, data.Direction, true, true);
                    break;
                case LightningBreakerBox box when controller.InteractWithBreakerBoxes:
                    if (!(data.Direction == Vector2.UnitX && box.spikesLeft) && !(data.Direction == -Vector2.UnitX && box.spikesRight) && CheckSpeed(Math.Abs(h.GetSpeed().Y), controller.BreakerBoxSpeedReq.Y, controller.ExactSpeedMatch, controller.ExactSpeedTolerance))
                    {
                        (box.Scene as Level)?.DirectionalShake(data.Direction);
                        box.sprite.Scale = new Vector2(1f + Math.Abs(data.Direction.Y) * 0.4f - Math.Abs(data.Direction.X) * 0.4f, 1f + Math.Abs(data.Direction.X) * 0.4f - Math.Abs(data.Direction.Y) * 0.4f);
                        box.health--;
                        if (box.health > 0)
                        {
                            box.Add(box.firstHitSfx = new SoundSource("event:/new_content/game/10_farewell/fusebox_hit_1"));
                            Celeste.Freeze(0.1f);
                            box.shakeCounter = 0.2f;
                            box.shaker.On = true;
                            box.bounceDir = data.Direction;
                            box.bounce.Start();
                            box.smashParticles = true;
                            box.Pulse();
                            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        }
                        else
                        {
                            box.firstHitSfx?.Stop();
                            Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", box.Position);
                            Celeste.Freeze(0.2f);
                            box.Break();
                            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                            box.SmashParticles(data.Direction.Perpendicular());
                            box.SmashParticles(-data.Direction.Perpendicular());
                        }
                    }
                    break;
                case SwapBlock swapBlock when controller.InteractWithSwapBlocks:
                    if (CheckSpeed(Math.Abs(h.GetSpeed().Y), controller.SwapBlockSpeedReq.Y, controller.ExactSpeedMatch, controller.ExactSpeedTolerance))
                    {
                        swapBlock.Swapping = swapBlock.lerp < 1f;
                        swapBlock.target = 1;
                        swapBlock.returnTimer = 0.8f;
                        swapBlock.burst = (swapBlock.Scene as Level)?.Displacement.AddBurst(swapBlock.Center, 0.2f, 0f, 16f);
                        swapBlock.speed = swapBlock.lerp >= 0.2f ? swapBlock.maxForwardSpeed : MathHelper.Lerp(swapBlock.maxForwardSpeed * 0.333f, swapBlock.maxForwardSpeed, swapBlock.lerp / 0.2f);
                        Audio.Stop(swapBlock.returnSfx);
                        Audio.Stop(swapBlock.moveSfx);
                        if (!swapBlock.Swapping)
                        {
                            Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", swapBlock.Center);
                        }
                        else
                        {
                            swapBlock.moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", swapBlock.Center);
                        }
                    }
                    break;
                case MoveBlock moveBlock when controller.InteractWithMoveBlocks:
                    if (CheckSpeed(Math.Abs(h.GetSpeed().Y), controller.MoveBlockSpeedReq.Y, controller.ExactSpeedMatch, controller.ExactSpeedTolerance))
                    {
                        moveBlock.triggered = true;
                    }
                    break;
                case FallingBlock fallingBlock when controller.InteractWithFallingBlocksV:
                    if (CheckSpeed(Math.Abs(h.GetSpeed().Y), controller.FallingBlockSpeedReq.Y, controller.ExactSpeedMatch, controller.ExactSpeedTolerance))
                    {
                        fallingBlock.Triggered = true;
                    }
                    break;
            }
            
        }
        
        //smw block handling

        if (data.Hit is not Generic_SMWBlock smwblock || smwblock.Active) return data;
        
        if (h.GetSpeed().Y > 80)
        {
            if (smwblock.CanHitTop) smwblock.Hit(null, 3);
        }
        if (h.GetSpeed().Y < 0)
        {
            if (smwblock.CanHitBottom) smwblock.Hit(null, 0);
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
        FemtoSpriteBank = new SpriteBank(GFX.Game, "Graphics/FemtoHelper/Sprites.xml");
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

                child.AttrInt("sliceMode", 0) == 0 ? DistortedParallax.SliceModes.TransLong : child.AttrInt("sliceMode", 0) == 1 ? DistortedParallax.SliceModes.LongTrans : child.AttrInt("sliceMode", 0) == 2 ? DistortedParallax.SliceModes.TransTrans : DistortedParallax.SliceModes.LongLong
                );
        }
        return null;
    }
}

