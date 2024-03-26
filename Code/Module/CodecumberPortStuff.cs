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
using Celeste.Mod.FemtoHelper.Entities;
using Celeste.Mod.Femtohelper.Entities;

namespace Celeste.Mod.FemtoHelper
{
    public class CodecumberPortStuff
    {
        public static void Load()
        {
            MovingWaterBlock.Load();
            RotateDashRefill.Load();
            BoostingBoosterSorryIStoleFromCommunalHelper.Load();
            EvilTheoCrystal.Load();
            On.Celeste.Solid.Update += NoAssistSquishHook;
            On.Celeste.Player.OnSquish += NoAssistSquishPlayer;
            On.Celeste.Player.DreamDashUpdate += DreamDashBounceHook;
        }

        public static void Unload()
        {
            MovingWaterBlock.Unload();
            RotateDashRefill.Unload();
            BoostingBoosterSorryIStoleFromCommunalHelper.Unload();
            EvilTheoCrystal.Unload();
            On.Celeste.Solid.Update -= NoAssistSquishHook;
            On.Celeste.Player.OnSquish -= NoAssistSquishPlayer;
            On.Celeste.Player.DreamDashUpdate -= DreamDashBounceHook;
        }

        public static void NoAssistSquishHook(On.Celeste.Solid.orig_Update orig, Solid self)
        {
            bool assist1 = SaveData.Instance.Assists.Invincible;
            if (self.Scene.Tracker.GetEntity<AssistHazardController>() != null)
            {
                SaveData.Instance.Assists.Invincible = true;
            }
            orig(self);
            SaveData.Instance.Assists.Invincible = assist1;
        }

        public static void NoAssistSquishPlayer(On.Celeste.Player.orig_OnSquish orig, Player self, CollisionData data)
        {
            bool assist1 = SaveData.Instance.Assists.Invincible;
            if (self.Scene.Tracker.GetEntity<AssistHazardController>() != null)
            {
                SaveData.Instance.Assists.Invincible = true;
            }
            orig(self, data);
            SaveData.Instance.Assists.Invincible = assist1;
        }

        public static int DreamDashBounceHook(On.Celeste.Player.orig_DreamDashUpdate orig, Player self)
        {
            bool assist1 = SaveData.Instance.Assists.Invincible;
            if (self.Scene.Tracker.GetEntity<AssistHazardController>() != null)
            {
                SaveData.Instance.Assists.Invincible = true;
            }
            int a = orig(self);
            SaveData.Instance.Assists.Invincible = assist1;
            return a;
        }
        
        public static IEnumerator RotateDashAlignAnim(Player player)
        {
            RotateDashIndicator indicator = player.Components.Get<RotateDashIndicator>();
            indicator.Anim = true;
            indicator.ArrowAngle = VectorToAngle(player.Speed);
            float angle2 = VectorToAngle(player.Speed);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 0.01f, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                indicator.timer = Math.Min(t.Eased * 6, 1);
                indicator.ArrowAngle = Calc.LerpClamp(angle2, angle2 - FemtoModule.Session.RotateDashAngle, t.Eased);
            };
            indicator.Entity.Add(tween);
            yield return 0.005f;
            indicator.Anim = false;
            indicator.timer = 1f;
        }
        
        public static float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }
    }
}
