using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using On.Celeste;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using System.Collections;
using Monocle;
using IL.Celeste;
using MonoMod.Cil;
using MonoMod;
using Celeste.Mod.Entities;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.FemtoHelper
{
    public class CodecumberPortStuff
    {
        public static void Load()
        {
            //Entities.BoostingBoosterSorryIStoleFromCommunalHelper.Load();
            //Entities.MovingWaterBlock.Load();
            //On.Celeste.LevelLoader.LoadingThread += RotateDashInitialize;
            //On.Celeste.PlayerHair.GetHairColor += RotateDashCustomColor;
            //On.Celeste.Player.Die += RotateDashDeathHook;
            //On.Celeste.Player.DashBegin += RotateDashBeginHook;
            //On.Celeste.Player.DashCoroutine += RotateDashCoroutineHook;
            //On.Celeste.Player.Added += RotateDashAddComponent;
            On.Celeste.Solid.Update += NoAssistSquishHook;
            On.Celeste.Player.OnSquish += NoAssistSquishPlayer;
            //On.Celeste.Player.Update += RotateDashBugCheck;
            On.Celeste.Player.DreamDashUpdate += DreamDashBounceHook;
        }

        public static void Unload()
        {
            //Entities.BoostingBoosterSorryIStoleFromCommunalHelper.Unload();
            //Entities.MovingWaterBlock.Unload();
            //On.Celeste.LevelLoader.LoadingThread -= RotateDashInitialize;
            //On.Celeste.PlayerHair.GetHairColor -= RotateDashCustomColor;
            //On.Celeste.Player.Die -= RotateDashDeathHook;
            //On.Celeste.Player.DashBegin -= RotateDashBeginHook;
            //On.Celeste.Player.DashCoroutine -= RotateDashCoroutineHook;
            //On.Celeste.Player.Added -= RotateDashAddComponent;
            On.Celeste.Solid.Update -= NoAssistSquishHook;
            On.Celeste.Player.OnSquish -= NoAssistSquishPlayer;
            //On.Celeste.Player.Update -= RotateDashBugCheck;
            On.Celeste.Player.DreamDashUpdate -= DreamDashBounceHook;
        }


        /*
        private static void RotateDashInitialize(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
        {
            orig(self);
            RotateDashInitialize();
        }

        private static PlayerDeadBody RotateDashDeathHook(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            RotateDashInitialize();
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        
        private static Color RotateDashCustomColor(On.Celeste.PlayerHair.orig_GetHairColor orig, PlayerHair self, int index)
        {
            if (CodecumberModule.Session.HasRotateDash)
            {
                return Color.Lerp(Calc.HexToColor("7958ad"), Calc.HexToColor("cbace6"), (float)(Math.Sin(self.Scene.TimeActive * 2) * 0.5f) + 0.5f);
            }

            return orig(self, index);
        }
        private static IEnumerator RotateDashCoroutineHook(On.Celeste.Player.orig_DashCoroutine orig, Player self)
        {
            if (CodecumberModule.Session.HasRotateDash)
            {
                Engine.TimeRate = 0.05f;
                self.Add(new Coroutine(RotateDashAlignAnim(self)));
                yield return 0.01f;
                Engine.TimeRate = 1;

                self.Speed = Vector2.Transform(self.Speed, Matrix.CreateRotationZ(-CodecumberModule.Session.RotateDashAngle));
                self.Speed *= CodecumberModule.Session.RotateDashScalar;
                self.StateMachine.State = 0;
                CodecumberModule.Session.HasRotateDash = false;
                CodecumberModule.Session.HasStartedRotateDashing = false;
                yield return null;

            }
            else
            {
                yield return new SwapImmediately(orig(self));
            }
        }
        

        private static void RotateDashBeginHook(On.Celeste.Player.orig_DashBegin orig, Player self)
        {
            Vector2 tempSpeed = Vector2.Zero;
            tempSpeed = self.Speed;
            orig(self);
            if (CodecumberModule.Session.HasRotateDash)
            {
                self.Speed = tempSpeed;
                CodecumberModule.Session.HasStartedRotateDashing = true;
            }
        }

        private static void RotateDashBugCheck(On.Celeste.Player.orig_Update orig, Player self)
        {
            if (CodecumberModule.Session.HasStartedRotateDashing && CodecumberModule.Session.HasRotateDash && self.StateMachine.State != 2)
            {
                CodecumberModule.Session.HasRotateDash = false;
                CodecumberModule.Session.HasStartedRotateDashing = false;
                Engine.TimeRate = 1;
            }
            orig(self);
        }

        private static void RotateDashAddComponent(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);
            self.Add(new Entities.RotateDashIndicator());
        }

        public static void RotateDashInitialize()
        {
            CodecumberModule.Session.HasRotateDash = false;
            CodecumberModule.Session.RotateDashAngle = 0;
            CodecumberModule.Session.RotateDashScalar = 1;
            CodecumberModule.Session.HasStartedRotateDashing = false;
        }
        */

        public static void NoAssistSquishHook(On.Celeste.Solid.orig_Update orig, Solid self)
        {
            bool assist1 = SaveData.Instance.Assists.Invincible;
            if (self.Scene.Tracker.GetEntity<Entities.AssistHazardController>() != null)
            {
                SaveData.Instance.Assists.Invincible = true;
            }
            orig(self);
            SaveData.Instance.Assists.Invincible = assist1;
        }

        public static void NoAssistSquishPlayer(On.Celeste.Player.orig_OnSquish orig, Player self, CollisionData data)
        {
            bool assist1 = SaveData.Instance.Assists.Invincible;
            if (self.Scene.Tracker.GetEntity<Entities.AssistHazardController>() != null)
            {
                SaveData.Instance.Assists.Invincible = true;
            }
            orig(self, data);
            SaveData.Instance.Assists.Invincible = assist1;
        }

        public static int DreamDashBounceHook(On.Celeste.Player.orig_DreamDashUpdate orig, Player self)
        {
            bool assist1 = SaveData.Instance.Assists.Invincible;
            if (self.Scene.Tracker.GetEntity<Entities.AssistHazardController>() != null)
            {
                SaveData.Instance.Assists.Invincible = true;
            }
            int a = orig(self);
            SaveData.Instance.Assists.Invincible = assist1;
            return a;
        }
        /*
        public static IEnumerator RotateDashAlignAnim(Player player)
        {
            Entities.RotateDashIndicator indicator = player.Components.Get<Entities.RotateDashIndicator>();
            indicator.Anim = true;
            indicator.ArrowAngle = VectorToAngle(player.Speed);
            float angle2 = VectorToAngle(player.Speed);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 0.01f, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                indicator.timer = Math.Min(t.Eased * 6, 1);
                indicator.ArrowAngle = Calc.LerpClamp(angle2, angle2 - CodecumberModule.Session.RotateDashAngle, t.Eased);
            };
            indicator.Entity.Add(tween);
            yield return 0.01f;
            indicator.Anim = false;
            indicator.timer = 1f;
        }
        */
        public static float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }
    }
}
