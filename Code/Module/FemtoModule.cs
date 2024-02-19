using Monocle;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections;
using Microsoft.Xna.Framework;
using Celeste.Mod.FemtoHelper.Effects;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.Entities;
using Celeste.Mod.FemtoHelper.Entities;
using System.Reflection;
using MonoMod.Utils;
using MonoMod.ModInterop;
using System.Collections.Generic;
using Celeste.Mod.FemtoHelper.Code.Effects;
using System.Linq;

namespace Celeste.Mod.FemtoHelper
{
    public class FemtoModule : EverestModule
    {

        [ModImportName("GravityHelper")]
        public static class GravityHelperSupport
        {
            public static Func<int> GetPlayerGravity;
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
        public static FemtoHelperSession session2 => (FemtoHelperSession)Instance._Session;

        public FemtoModule()
        {
            Instance = this;
        }
        private static void ModifyBossSpritesOnCustomFinalBoss(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("badeline_boss")))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<string, FinalBoss, string>>(ChangeFBSpriteRef);
            }
        }

        private static string ChangeFBSpriteRef(string orig, FinalBoss b)
        {
            if (b is CrystalHeartBoss) return "badeline_boss_femtohelper"; //Change this value as needed
            return orig;
        }

        private static void CrystalHeartBossExtraEffects(On.Celeste.FinalBoss.orig_OnPlayer orig, FinalBoss CustomFinalBoss, Player player)
        {
            orig.Invoke(CustomFinalBoss, player);
            if (CustomFinalBoss is CrystalHeartBoss)
            {
                for (int i = 0; i < 4; i++)
                {
                    (player.Scene as Level).Add(new AbsorbOrb(CustomFinalBoss.Position, CustomFinalBoss));
                    Audio.Play("event:/FemtoHelper/boss_spikes_burst_quiet", CustomFinalBoss.Position);
                }
            }
        }
        private static void CrystalHeartBossShrinkHitbox(On.Celeste.FinalBoss.orig_Added orig, FinalBoss CustomFinalBoss, Scene scene)
        {
            orig.Invoke(CustomFinalBoss, scene);
            if (CustomFinalBoss is CrystalHeartBoss)
            {
                CustomFinalBoss.Collider.Width /= 1.5f;
            }
        }

        private static void Puffer_KaizoCollideHHook(On.Celeste.Puffer.orig_OnCollideH orig, Puffer self, CollisionData data)
        {
            if(self.ToString() == "Celeste.Puffer")
            {
                Type selfRef = self.GetType();

                FieldInfo SpeedInfo = selfRef.GetField("hitSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

                Vector2 RefSpeed = (Vector2)(SpeedInfo?.GetValue(self));
                if (data.Hit is Generic_SMWBlock && !(data.Hit as Generic_SMWBlock).active)
                {
                    if (RefSpeed.X > 0)
                    {
                        if ((data.Hit as Generic_SMWBlock).canHitLeft) (data.Hit as Generic_SMWBlock).Hit(null, 1);
                    }
                    if (RefSpeed.X < 0)
                    {
                        if ((data.Hit as Generic_SMWBlock).canHitRight) (data.Hit as Generic_SMWBlock).Hit(null, 2);
                    }

                }
            }
            orig.Invoke(self, data);
        }
        private static void Puffer_KaizoCollideVHook(On.Celeste.Puffer.orig_OnCollideV orig, Puffer self, CollisionData data)
        {
            if (self.ToString() == "Celeste.Puffer")
            {
                Type selfRef = self.GetType();

                FieldInfo SpeedInfo = selfRef.GetField("hitSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

                Vector2 RefSpeed = (Vector2)(SpeedInfo?.GetValue(self));


                if (data.Hit is Generic_SMWBlock && !(data.Hit as Generic_SMWBlock).active)
                {
                    if (RefSpeed.Y > 0)
                    {
                        if ((data.Hit as Generic_SMWBlock).canHitTop) (data.Hit as Generic_SMWBlock).Hit(null, 3);
                    }
                    if (RefSpeed.Y < 0)
                    {
                        if ((data.Hit as Generic_SMWBlock).canHitBottom) (data.Hit as Generic_SMWBlock).Hit(null, 0);
                    }

                }
            }
            orig.Invoke(self, data);
        }

        private static void Puffer_SplodeKaizoHook(On.Celeste.Puffer.orig_Explode orig, Puffer self)
        {
            orig.Invoke(self);
            Collider collider = self.Collider;
            self.Collider = new Circle(40f);
            foreach (Generic_SMWBlock entity in self.Scene.Tracker.GetEntities<Generic_SMWBlock>())
            {
                if(entity != null)
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

        public static bool onCollideHHook(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher)
        {
            if(self is Debris)
            {
                return orig(self, moveH, onCollide, pusher);
            }
            int orig_moveH = moveH;
            Holdable h = self.Get<Holdable>();
            if (h != null)
            {
                Vector2 targetPosition = self.Position + Vector2.UnitX * moveH;
                int num = Math.Sign(moveH);
                int num2 = 0;
                float orig_x = self.X;
                
                while (moveH != 0)
                {
                    Solid solid = self.CollideFirst<Solid>(self.Position + Vector2.UnitX * num);
                    if (solid != null)
                    {

                        if (false) // disabled for now
                        {
                            if (solid is CrushBlock)
                            {
                                if (Math.Abs(h.GetSpeed().X) >= 120)
                                {
                                    bool activate = (bool)(solid as CrushBlock).GetType().GetMethod("CanActivate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(solid, new object[] { -Vector2.UnitX * num });
                                    if (activate) (solid as CrushBlock).GetType().GetMethod("Attack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(solid, new object[] { -Vector2.UnitX * num });
                                }
                            }

                            if (solid is DashBlock)
                            {
                                if (Math.Abs(h.GetSpeed().X) >= 120) (solid as DashBlock).Break(self.Center, Vector2.UnitX * num, true, true);
                            }
                        }

                        if ((solid is Generic_SMWBlock))
                        {
                            if (!(solid as Generic_SMWBlock).active)
                            {
                                if (h.GetSpeed().X > 20)
                                {
                                    if ((solid as Generic_SMWBlock).canHitLeft) (solid as Generic_SMWBlock).Hit(null, 1);
                                }
                                if (h.GetSpeed().X < -20)
                                {
                                    if ((solid as Generic_SMWBlock).canHitRight) (solid as Generic_SMWBlock).Hit(null, 2);
                                }
                                self.X = orig_x;
                                return orig(self, orig_moveH, onCollide, pusher);
                            }
                        }
                    }
                    num2 += num;
                    moveH -= num;
                    self.X += num;
                }
                self.X = orig_x;
            }
            return orig(self, orig_moveH, onCollide, pusher);
        }

        public static bool onCollideVHook(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher)
        {
            if (self is Debris)
            {
                return orig(self, moveV, onCollide, pusher);
            }
            Holdable h = self.Get<Holdable>();
            int orig_moveV = moveV;
            if (h != null)
            {
                Vector2 targetPosition = self.Position + Vector2.UnitY * moveV;
                int num = Math.Sign(moveV);
                int num2 = 0;
                float orig_y = self.Y;
                
                while (moveV != 0)
                {
                    Platform platform = self.CollideFirst<Solid>(self.Position + Vector2.UnitY * num);
                    if (platform != null)
                    {

                        if (false) //disabled for now
                        {
                            if (platform is CrushBlock)
                            {
                                if ((platform as CrushBlock).CanActivate(-Vector2.UnitY * num))
                                {
                                    if (Math.Abs(h.GetSpeed().Y) >= 120) (platform as CrushBlock).Attack(-Vector2.UnitY * num);
                                }
                            }

                            if (platform is DashBlock)
                            {
                                if (Math.Abs(h.GetSpeed().Y) >= 120) (platform as DashBlock).Break(self.Center, Vector2.UnitY * num, true, true);
                            }

                        }

                        if ((platform is Generic_SMWBlock))
                        {
                            if (!(platform as Generic_SMWBlock).active)
                            {
                                Logger.Log(LogLevel.Info, "FemtoHelper/onCollideVHook", "speed: " + h.GetSpeed().Y.ToString());
                                if (h.GetSpeed().Y > 80)
                                {
                                    if ((platform as Generic_SMWBlock).canHitTop) (platform as Generic_SMWBlock).Hit(null, 3);
                                }
                                if (h.GetSpeed().Y < 0)
                                {
                                    if ((platform as Generic_SMWBlock).canHitBottom) (platform as Generic_SMWBlock).Hit(null, 0);
                                }
                                self.Y = orig_y;
                                return orig(self, orig_moveV, onCollide, pusher);
                            }
                        }

                    }
                    num2 += num;
                    moveV -= num;
                    self.Y += num;
                }
                self.Y = orig_y;
            }

            return orig(self, orig_moveV, onCollide, pusher);
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
            IL.Celeste.FinalBoss.CreateBossSprite += ModifyBossSpritesOnCustomFinalBoss;
            On.Celeste.FinalBoss.OnPlayer += CrystalHeartBossExtraEffects;
            On.Celeste.FinalBoss.Added += CrystalHeartBossShrinkHitbox;
            On.Celeste.Puffer.OnCollideH += Puffer_KaizoCollideHHook;
            On.Celeste.Puffer.OnCollideV += Puffer_KaizoCollideVHook;
            On.Celeste.Puffer.Explode += Puffer_SplodeKaizoHook;
            On.Celeste.Seeker.RegenerateCoroutine += Seeker_RegenKaizoHook;
            On.Celeste.Actor.MoveHExact += onCollideHHook;
            On.Celeste.Actor.MoveVExact += onCollideVHook;
            CodecumberPortStuff.Load();
            VitalDrainController.Load();

            Everest.Events.Player.OnSpawn += ReloadDistortedParallax;
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
        public override void Unload()
        {
            Everest.Events.Level.OnLoadBackdrop -= Level_OnLoadBackdrop;
            IL.Celeste.FinalBoss.CreateBossSprite -= ModifyBossSpritesOnCustomFinalBoss;
            On.Celeste.FinalBoss.OnPlayer -= CrystalHeartBossExtraEffects;
            On.Celeste.FinalBoss.Added -= CrystalHeartBossShrinkHitbox;
            On.Celeste.Puffer.OnCollideH -= Puffer_KaizoCollideHHook;
            On.Celeste.Puffer.OnCollideV -= Puffer_KaizoCollideVHook;
            On.Celeste.Puffer.Explode -= Puffer_SplodeKaizoHook;
            On.Celeste.Seeker.RegenerateCoroutine -= Seeker_RegenKaizoHook;
            On.Celeste.Actor.MoveHExact -= onCollideHHook;
            On.Celeste.Actor.MoveVExact -= onCollideVHook;
            CodecumberPortStuff.Unload();
            VitalDrainController.Unload();
        }
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
}

