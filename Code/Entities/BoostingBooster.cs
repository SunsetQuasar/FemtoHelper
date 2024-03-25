
using System;
using System.Collections;
using System.Reflection;
using Celeste;
using Celeste.Mod.Entities;
using IL.Celeste;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using On.Celeste;

namespace Celeste.Mod.FemtoHelper.Entities
{

    public class PointFourCircle : Component
    {
        public float timer;
        public Color color;

        public PointFourCircle(Color color) : base(true, true)
        {
            timer = 0.4f;
            this.color = color;
        }

        public override void Update()
        {
            base.Update();
            if (timer > 0)
            {
                timer -= Engine.DeltaTime;
            }
            else
            {
                RemoveSelf();
            }

        }

        public override void Render()
        {
            base.Render();
            Draw.Circle(Entity.Center, timer * 48f, color * 0.8f, 16);
        }
    }


    [CustomEntity("FemtoHelper/BoostingBooster")]
    public class BoostingBoosterSorryIStoleFromCommunalHelper : Booster
    {
        protected DynData<Booster> BoosterData;

        public ParticleType P_CustomAppear;

        public ParticleType P_CustomBurst;

        public ParticleType P_CustomBurst2;

        private bool hasCustomSounds;

        private string enterSoundEvent;

        private string moveSoundEvent;

        private bool playMoveEventEnd;

        private static readonly MethodInfo m_Player_orig_Update = typeof(Player).GetMethod("orig_Update", BindingFlags.Instance | BindingFlags.Public);

        private static ILHook IL_Player_orig_Update;

        public bool RedBoost => (BoosterData.Get<bool>("red") ? ((byte)1) : ((byte)0)) != 0;

        public Sprite Sprite => BoosterData.Get<Sprite>("sprite");

        public float MovementInBubbleFactor { get; set; } = 3f;

        public float thetimer;

        public bool oopsnotvisible;

        public bool patient;

        public BoostingBoosterSorryIStoleFromCommunalHelper(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Bool("red", false))
        {
            thetimer = Calc.Random.Range(0, (float)Math.PI * 2);
            oopsnotvisible = false;
            patient = data.Bool("patient", false);
            BoosterData = new DynData<Booster>(this);
            P_CustomAppear = new ParticleType
            {
                Size = 1f,
                Color = Calc.HexToColor("FF594A"),
                DirectionRange = (float)Math.PI / 30f,
                LifeMin = 0.6f,
                LifeMax = 1f,
                SpeedMin = 40f,
                SpeedMax = 50f,
                SpeedMultiplier = 0.25f,
                FadeMode = ParticleType.FadeModes.Late
            };
            P_CustomBurst = new ParticleType
            {
                Source = GFX.Game["particles/blob"],
                Color = Calc.HexToColor("942c3e"),
                FadeMode = ParticleType.FadeModes.None,
                LifeMin = 0.5f,
                LifeMax = 0.8f,
                Size = 0.7f,
                SizeRange = 0.25f,
                ScaleOut = true,
                Direction = 4.712389f,
                DirectionRange = 0.17453292f,
                SpeedMin = 10f,
                SpeedMax = 20f,
                SpeedMultiplier = 0.01f,
                Acceleration = new Vector2(0f, 90f)
            };
            P_CustomBurst2 = new ParticleType
            {
                Source = GFX.Game["particles/blob"],
                Color = Calc.HexToColor("7EDC89"),
                FadeMode = ParticleType.FadeModes.None,
                LifeMin = 0.5f,
                LifeMax = 0.8f,
                Size = 0.7f,
                SizeRange = 0.25f,
                ScaleOut = true,
                Direction = 4.712389f,
                DirectionRange = 0.17453292f,
                SpeedMin = 80f,
                SpeedMax = 180f,
                SpeedMultiplier = 0.01f,
                Acceleration = new Vector2(0f, 0f)
            };
            if (RedBoost)
            {
                ReplaceSprite(FemtoModule.femtoSpriteBank.Create("FemtoHelper_boosting_booster_purp"));
                SetParticleColors(Calc.HexToColor("DB7F86"), Calc.HexToColor("FFCDC9"));
                P_CustomBurst2.Color = Calc.HexToColor("DB7F86");
            }
            else
            {
                ReplaceSprite(FemtoModule.femtoSpriteBank.Create("FemtoHelper_boosting_booster_red"));
                SetParticleColors(Calc.HexToColor("7EDC89"), Calc.HexToColor("C3FA8E"));
            }

        }

        protected void ReplaceSprite(Sprite newSprite)
        {
            Sprite component = BoosterData.Get<Sprite>("sprite");
            Remove(component);
            BoosterData["sprite"] = newSprite;
            Add(newSprite);
        }

        protected void SetParticleColors(Color burstColor, Color appearColor)
        {
            DynData<Booster> boosterData = BoosterData;
            ParticleType obj = new ParticleType(Booster.P_Burst)
            {
                Color = burstColor
            };
            ParticleType value = obj;
            P_CustomBurst = obj;
            boosterData["particleType"] = value;
            P_CustomAppear = new ParticleType(Booster.P_Appear)
            {
                Color = appearColor
            };
        }

        protected void SetSoundEvent(string enterSound, string moveSound, bool playMoveEnd = false)
        {
            enterSoundEvent = enterSound;
            moveSoundEvent = moveSound;
            playMoveEventEnd = playMoveEnd;
            hasCustomSounds = true;
        }

        public void LoopingSfxParam(string path, float value)
        {
            BoosterData.Get<SoundSource>("loopingSfx")!.Param(path, value);
        }

        protected virtual void OnRespawn()
        {
            Sprite.Play("appear");
            oopsnotvisible = false;
        }

        protected virtual void OnPlayerEnter(Player player)
        {
        }

        protected virtual void OnPlayerExit(Player player)
        {
        }

        protected virtual int? RedDashUpdateBefore(Player player)
        {
            return null;
        }

        protected virtual int? RedDashUpdateAfter(Player player)
        {
            return null;
        }

        protected virtual IEnumerator RedDashCoroutineAfter(Player player)
        {
            yield return null;
        }

        protected virtual IEnumerator BoostRoutine(Player player)
        {
            yield return 0.25f;
            player.StateMachine.State = (RedBoost ? 5 : 2);
        }

        public static void Load()
        {
            On.Celeste.Booster.Respawn += Booster_Respawn;
            On.Celeste.Booster.AppearParticles += Booster_AppearParticles;
            On.Celeste.Booster.OnPlayer += Booster_OnPlayer;
            On.Celeste.Booster.PlayerBoosted += Booster_PlayerBoosted;
            On.Celeste.Booster.PlayerReleased += Booster_PlayerReleased;
            On.Celeste.Booster.BoostRoutine += Booster_BoostRoutine;
            IL.Celeste.Player.BoostUpdate += Player_BoostUpdate;
            On.Celeste.Player.BoostCoroutine += Player_BoostCoroutine;
            On.Celeste.Player.RedDashUpdate += Player_RedDashUpdate;
            On.Celeste.Player.RedDashCoroutine += Player_RedDashCoroutine;
            IL_Player_orig_Update = new ILHook(m_Player_orig_Update, new ILContext.Manipulator(Player_orig_Update));
        }

        public static void Unload()
        {
            On.Celeste.Booster.Respawn -= Booster_Respawn;
            On.Celeste.Booster.AppearParticles -= Booster_AppearParticles;
            On.Celeste.Booster.OnPlayer -= Booster_OnPlayer;
            On.Celeste.Booster.PlayerBoosted -= Booster_PlayerBoosted;
            On.Celeste.Booster.PlayerReleased -= Booster_PlayerReleased;
            On.Celeste.Booster.BoostRoutine -= Booster_BoostRoutine;
            On.Celeste.Player.BoostCoroutine -= Player_BoostCoroutine;
            On.Celeste.Player.RedDashUpdate -= Player_RedDashUpdate;
            On.Celeste.Player.RedDashCoroutine -= Player_RedDashCoroutine;
            IL_Player_orig_Update.Dispose();
        }

        private static void Booster_Respawn(On.Celeste.Booster.orig_Respawn orig, Booster self)
        {
            orig(self);
            (self as BoostingBoosterSorryIStoleFromCommunalHelper)?.OnRespawn();
        }

        private static IEnumerator Booster_BoostRoutine(On.Celeste.Booster.orig_BoostRoutine orig, Booster self, Player player, Vector2 dir)
        {
            IEnumerator origEnum = orig(self, player, dir);
            while (origEnum.MoveNext())
            {
                yield return origEnum.Current;
            }
            (self as BoostingBoosterSorryIStoleFromCommunalHelper)?.OnPlayerExit(player);
            BoostingBoosterSorryIStoleFromCommunalHelper customBooster = self as BoostingBoosterSorryIStoleFromCommunalHelper;
            if (customBooster != null && player != null)
            {
                if (player.Dead) yield break;
                player.Add(new PointFourCircle(customBooster.P_CustomBurst2.Color));
                customBooster.oopsnotvisible = true;
                yield return 0.4f;
                if (player.Dead) yield break;
                Audio.Play("event:/new_content/game/10_farewell/puffer_splode", player.Position);
                Vector2 vector2 = player.ExplodeLaunch(player.Center - (new Vector2(Input.MoveX, Input.MoveY) * 4), snapUp: false);
                (customBooster.Scene as Level).DirectionalShake(vector2, 0.15f);
                (customBooster.Scene as Level).Displacement.AddBurst(player.Center, 0.3f, 8f, 32f, 0.8f);
                (customBooster.Scene as Level).Particles.Emit(customBooster.P_CustomBurst2, 12, player.Center, Vector2.One * 3f, vector2.Angle());
                ParticleSystem particlesBG = self.SceneAs<Level>().ParticlesBG;
                for (int i = 0; i < 360; i += 30)
                {
                    particlesBG.Emit(customBooster.P_CustomAppear, 1, player.Center, Vector2.One * 2f, (float)i * ((float)Math.PI / 180f));
                }
            }

        }

        private static void Booster_PlayerReleased(On.Celeste.Booster.orig_PlayerReleased orig, Booster self)
        {
            orig(self);
            BoostingBoosterSorryIStoleFromCommunalHelper customBooster = self as BoostingBoosterSorryIStoleFromCommunalHelper;
            if (customBooster != null && customBooster.RedBoost && customBooster.hasCustomSounds && customBooster.playMoveEventEnd)
            {
                customBooster.BoosterData.Get<SoundSource>("loopingSfx")!.Play(customBooster.moveSoundEvent, "end", 1f);
            }

        }

        private static void Booster_PlayerBoosted(On.Celeste.Booster.orig_PlayerBoosted orig, Booster self, Player player, Vector2 direction)
        {
            orig(self, player, direction);
            BoostingBoosterSorryIStoleFromCommunalHelper customBooster = self as BoostingBoosterSorryIStoleFromCommunalHelper;
            if (customBooster != null && customBooster.hasCustomSounds && customBooster.RedBoost)
            {
                customBooster.BoosterData.Get<SoundSource>("loopingSfx")!.Play(customBooster.moveSoundEvent);
            }
        }

        private static void Booster_OnPlayer(On.Celeste.Booster.orig_OnPlayer orig, Booster self, Player player)
        {
            BoostingBoosterSorryIStoleFromCommunalHelper customBooster = self as BoostingBoosterSorryIStoleFromCommunalHelper;
            if (customBooster != null)
            {
                bool flag = customBooster.BoosterData.Get<float>("respawnTimer") <= 0f && customBooster.BoosterData.Get<float>("cannotUseTimer") <= 0f && !self.BoostingPlayer;
                if (customBooster.hasCustomSounds)
                {
                    if (flag)
                    {
                        customBooster.BoosterData["cannotUseTimer"] = 0.45f;
                        if (customBooster.RedBoost)
                        {
                            player.RedBoost(self);
                        }
                        else
                        {
                            player.Boost(self);
                        }
                        Audio.Play(customBooster.enterSoundEvent, self.Position);
                        customBooster.BoosterData.Get<Wiggler>("wiggler")!.Start();
                        Sprite sprite = customBooster.BoosterData.Get<Sprite>("sprite");
                        sprite.Play("inside");
                        sprite.FlipX = player.Facing == Facings.Left;
                    }
                }
                else
                {
                    orig(self, player);
                }
                if (flag)
                {
                    customBooster.OnPlayerEnter(player);
                }
            }
            else
            {
                orig(self, player);
            }
        }

        private static void Booster_AppearParticles(On.Celeste.Booster.orig_AppearParticles orig, Booster self)
        {
            BoostingBoosterSorryIStoleFromCommunalHelper customBooster = self as BoostingBoosterSorryIStoleFromCommunalHelper;
            if (customBooster != null)
            {
                ParticleSystem particlesBG = self.SceneAs<Level>().ParticlesBG;
                for (int i = 0; i < 360; i += 30)
                {
                    particlesBG.Emit(customBooster.P_CustomAppear, 1, self.Center, Vector2.One * 2f, (float)i * ((float)Math.PI / 180f));
                }
            }
            else
            {
                orig(self);
            }
        }

        private static void Player_BoostUpdate(ILContext il)
        {
            ILCursor iLCursor = new ILCursor(il);
            iLCursor.GotoNext(MoveType.After, (Instruction instr) => instr.MatchLdcR4(3f));
            iLCursor.Emit(OpCodes.Ldarg_0);
            iLCursor.EmitDelegate<Func<float, Player, float>>(boosthookthing);
        }

        private static float boosthookthing(float f, Player player)
        {
            return (player.LastBooster as BoostingBoosterSorryIStoleFromCommunalHelper)?.MovementInBubbleFactor ?? f;
        }

        private static IEnumerator Player_BoostCoroutine(On.Celeste.Player.orig_BoostCoroutine orig, Player self)
        {
            Booster lastBooster = self.LastBooster;
            BoostingBoosterSorryIStoleFromCommunalHelper booster = lastBooster as BoostingBoosterSorryIStoleFromCommunalHelper;
            IEnumerator routine;
            if (booster != null)
            {
                if (!booster.patient)
                {
                    routine = ((booster != null) ? booster.BoostRoutine(self) : orig(self));
                    while (routine.MoveNext())
                    {
                        yield return routine.Current;
                    }
                }
            }
            else
            {
                routine = ((booster != null) ? booster.BoostRoutine(self) : orig(self));
                while (routine.MoveNext())
                {
                    yield return routine.Current;
                }
            }

        }

        private static int Player_RedDashUpdate(On.Celeste.Player.orig_RedDashUpdate orig, Player self)
        {
            BoostingBoosterSorryIStoleFromCommunalHelper customBooster = self.LastBooster as BoostingBoosterSorryIStoleFromCommunalHelper;
            if (customBooster == null)
            {
                return orig(self);
            }
            int? num = customBooster.RedDashUpdateBefore(self);
            int num2 = orig(self);
            return customBooster.RedDashUpdateAfter(self) ?? num ?? num2;
        }

        private static IEnumerator Player_RedDashCoroutine(On.Celeste.Player.orig_RedDashCoroutine orig, Player self)
        {
            Booster currentBooster = self.CurrentBooster;
            IEnumerator origRoutine = orig(self);
            while (origRoutine.MoveNext())
            {
                yield return origRoutine.Current;
            }
            BoostingBoosterSorryIStoleFromCommunalHelper booster = currentBooster as BoostingBoosterSorryIStoleFromCommunalHelper;
            if (booster != null)
            {
                IEnumerator routine = booster.RedDashCoroutineAfter(self);
                while (routine.MoveNext())
                {
                    yield return routine.Current;
                }
            }
        }

        private static void Player_orig_Update(ILContext il)
        {
            ILCursor iLCursor = new ILCursor(il);
            ILLabel label = null;
            iLCursor.GotoNext((Instruction instr) => instr.MatchCall<Actor>("Update"));
            iLCursor.GotoNext((Instruction instr) => instr.MatchCall<Actor>("MoveH"));
            iLCursor.GotoPrev(MoveType.After, (Instruction instr) => instr.MatchBeq(out label));
            iLCursor.Emit(OpCodes.Ldarg_0);
            iLCursor.EmitDelegate<Predicate<Player>>(UnaffectedBySpeed);
            iLCursor.Emit(OpCodes.Brtrue_S, label);
            iLCursor.GotoNext((Instruction instr) => instr.MatchCall<Actor>("MoveV"));
            iLCursor.GotoPrev(MoveType.After, (Instruction instr) => instr.MatchBeq(out label));
            iLCursor.Emit(OpCodes.Ldarg_0);
            iLCursor.EmitDelegate<Predicate<Player>>(UnaffectedBySpeed);
            iLCursor.Emit(OpCodes.Brtrue_S, label);
        }

        private static bool UnaffectedBySpeed(Player player)
        {
            int result;
            if (player.StateMachine.State == 5)
            {
                Booster lastBooster = player.LastBooster;
                result = 0;
            }
            else
            {
                result = 0;
            }
            return (byte)result != 0;
        }

        public override void Update()
        {
            base.Update();
            thetimer += Engine.DeltaTime * 2.76f;
        }
        public override void Render()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            MTexture t = RedBoost ? GFX.Game["objects/FemtoHelper/booster/tinyPurp"] : GFX.Game["objects/FemtoHelper/booster/tiny"];
            float num = RedBoost ? -0.8f : 0.8f;

            if (player != null || Sprite.Position == Vector2.Zero)
            {
                for (float i = 0; i < Math.PI * 2; i += (float)Math.PI / 3f)
                {
                    if (mod(thetimer + i + (float)(Math.PI / 2f), (float)Math.PI * 2) > Math.PI && !oopsnotvisible)
                    {
                        t.DrawOutlineCentered(Position + Sprite.Position + new Vector2((float)Math.Sin(thetimer + i) * 14, (float)Math.Cos(thetimer + i + num) * 6));
                        t.DrawCentered(Position + Sprite.Position + new Vector2((float)Math.Sin(thetimer + i) * 14, (float)Math.Cos(thetimer + i + num) * 6), Color.White);
                    }
                }
            }
            base.Render();
            if (player != null || Sprite.Position == Vector2.Zero)
            {
                for (float i = 0; i < Math.PI * 2; i += (float)Math.PI / 3f)
                {
                    if (mod(thetimer + i + (float)(Math.PI / 2f), (float)Math.PI * 2) <= Math.PI && !oopsnotvisible)
                    {
                        t.DrawOutlineCentered(Position + Sprite.Position + new Vector2((float)Math.Sin(thetimer + i) * 14, (float)Math.Cos(thetimer + i + num) * 6));
                        t.DrawCentered(Position + Sprite.Position + new Vector2((float)Math.Sin(thetimer + i) * 14, (float)Math.Cos(thetimer + i + num) * 6), Color.White);
                    }
                }
            }
        }
        private float mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }


}
