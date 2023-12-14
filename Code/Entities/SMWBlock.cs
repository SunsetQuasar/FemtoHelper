using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections;
using System.Reflection;
using MonoMod.ModInterop;
using Celeste.Mod.FemtoHelper;
using Celeste.Mod.FemtoHelper.Entities;

namespace Celeste.Mod.FemtoHelper.Entities
{

    /// <summary>
    /// this sucks ass oh my god please look away i'm so embarassed
    /// </summary>
    [Tracked]
    [CustomEntity("FemtoHelper/SMWBlock")]
    public class Generic_SMWBlock : Solid
    {
        public class Coin : Entity
        {
            public Vector2 Speed;

            public MTexture coinSheet;
            public MTexture coinSlice;

            public float coinTimer;

            public int coinFrames;

            public float life;

            public Coin(Vector2 pos, string path) : base(pos)
            {
                coinSheet = GFX.Game[path + "coins"];
                coinSlice = coinSheet.GetSubtexture(0, 0, coinSheet.Height, coinSheet.Height);
                Speed = new Vector2(0, -175);
                coinFrames = coinSheet.Width / coinSheet.Height;
                life = 0.6f;
            }
            public override void Update()
            {
                base.Update();
                coinTimer += Engine.DeltaTime * 24;
                Position += Speed * Engine.DeltaTime;
                Speed.Y += 390 * Engine.DeltaTime;
                life -= Engine.DeltaTime;
                if(life < 0)
                {
                    RemoveSelf();
                    for(int i = 0; i< 6; i++)
                    {
                        Dust.Burst(Position + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4)), Calc.Random.NextFloat((float)Math.PI * 2), 1);
                    }
                }
            }
            public override void Render()
            {
                base.Render();
                coinSlice = coinSheet.GetSubtexture(((int)Math.Floor(coinTimer) % coinFrames) * coinSheet.Height, 0, coinSheet.Height, coinSheet.Height);

                coinSlice.DrawCentered(Position);
            }
        }

        public bool active;

        public bool hasIndicator;

        public float animationRate;

        public MTexture indicator;
        public int indiframes;
        public float inditimer;

        public MTexture kaizo;

        public MTexture used;
        public int usedframes;
        public float usedtimer;

        public float bouncetimer;

        public string path;

        public bool solidbeforehit;

        public Collider plCol;

        public MTexture used2, indicator2;

        public Vector2 node;

        public List<Entity> rewards;

        public List<Vector2> offsets;

        public Rectangle rewardcatcher;

        public bool catched;

        public int bumpdir;

        public float distance;

        public Vector2 ejectOffset;

        public Vector2 ejectOffset2;

        public float ejectDuration;

        public int ejectDirection;

        public bool canHitTop, canHitBottom, canHitLeft, canHitRight;

        public string hitFlag;

        public int hitFlagBehavior;

        public bool switchMode;

        public bool giveCoyoteFramesOnHit;

        public string audioPath;

        public string neededFlag;

        public bool neededflagplus = true;

        public bool ejectFromPoint, ejectToPoint;

        public Vector2 spriteOffset;

        public bool hasBeenHitOnce;

        public bool specialHandling;

        public Generic_SMWBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
        {
            Depth = data.Int("depth", -15000);
            specialHandling = data.Bool("specialEntityHandling", true);
            catched = false;
            ejectDuration = data.Float("ejectDuration", 0.5f);
            ejectDirection = data.Int("ejectDirection", 4);
            path = data.Attr("path", "objects/FemtoHelper/SMWBlock/solid/");
            animationRate = data.Float("animationRate", 8f);
            indicator = GFX.Game[path + "indicator"];
            indicator2 = indicator.GetSubtexture(0, 0, indicator.Height, indicator.Height);
            indiframes = indicator.Width / indicator.Height;
            inditimer = 0;
            kaizo = GFX.Game[path + "active"];
            used = GFX.Game[path + "used"];
            used2 = used.GetSubtexture(0, 0, used.Height, used.Height);
            usedframes = used.Width / used.Height;
            usedtimer = 0;
            active = false;
            hasIndicator = data.Bool("indicate", false);
            bouncetimer = 0;
            Collidable = solidbeforehit = data.Bool("solidBeforeHit", false);
            if (!Collidable) DisableStaticMovers();
            plCol = new Hitbox(Width - 2, 2, 1, Height);

            Add(new PlayerCollider(OnPlayerCollide, plCol));

            OnDashCollide = OnDashed;

            rewards = new List<Entity>();
            offsets = new List<Vector2>();

            node = data.NodesOffset(offset)[0];

            if (node == null) node = Position;

            rewardcatcher = new Rectangle((int)node.X, (int)node.Y, data.Int("rewardContainerWidth", 16), data.Int("rewardContainerHeight", 16));

            bumpdir = 0;

            distance = data.Float("ejectDistance", 24);

            ejectOffset = new Vector2(data.Float("ejectOffsetX", 0), data.Float("ejectOffsetY", 0));
            ejectOffset2 = new Vector2(data.Float("ejectDestinationOffsetX", 0), data.Float("ejectDestinationOffsetY", 0));

            canHitTop = data.Bool("canHitTop", true);
            canHitBottom = data.Bool("canHitBottom", true);
            canHitLeft = data.Bool("canHitLeft", true);
            canHitRight = data.Bool("canHitRight", true);

            hitFlag = data.Attr("hitFlag", "smwblock_flag");
            hitFlagBehavior = data.Int("hitFlagBehavior", 2);

            switchMode = data.Bool("switchMode", false);

            giveCoyoteFramesOnHit = data.Bool("giveCoyoteFramesOnHit", false);

            audioPath = data.Attr("audioPath", "event:/FemtoHelper/");

            neededFlag = data.Attr("neededFlag", "");

            ejectFromPoint = data.Bool("ejectFromPoint", true);
            ejectToPoint = data.Bool("ejectToPoint", false);

            spriteOffset = new Vector2(data.Float("spriteOffsetX", 0), data.Float("spriteOffsetY", 0));

            Component crystalCollider = FemtoModule.CavernHelperSupport.GetCrystalBombExplosionCollider?.Invoke(OnCrystalExplosion, Collider);


            if(crystalCollider != null) Add(crystalCollider);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            neededflagplus = neededFlag == "" ? true : (neededFlag.StartsWith("!") ? !(Scene as Level).Session.GetFlag(neededFlag.Substring(1)) : (Scene as Level).Session.GetFlag(neededFlag));
        }

        public override void Update()
        {
            neededflagplus = neededFlag == "" ? true : (neededFlag.StartsWith("!") ? !(Scene as Level).Session.GetFlag(neededFlag.Substring(1)) : (Scene as Level).Session.GetFlag(neededFlag));

            if (catched == false && !switchMode)
            {
                foreach (Entity entity in Scene.Entities)
                {
                    if (Collide.CheckRect(entity, rewardcatcher) || rewardcatcher.Contains((int)entity.X, (int)entity.Y))
                    {
                        if(entity != (Scene as Level).SolidTiles)
                        {
                            offsets.Add(entity.Position - new Vector2(rewardcatcher.Center.X, rewardcatcher.Center.Y));
                            rewards.Add(entity);
                            entity.Active = entity.Visible = false;
                        }
                    }
                }
                catched = true;
            }

            if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
            {
                plCol.Position.Y = - 2;
            } else
            {
                plCol.Position.Y = Height;
            }

            base.Update();

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
                {
                    if (CollideCheck<Player>(Position - Vector2.UnitY) && player.Bottom - 4 <= Top && player.Speed.Y < 0 && !(active || bouncetimer > 0) && !solidbeforehit)
                    {
                        if (canHitBottom) Hit(player, 3);
                    }
                } else
                {
                    if (CollideCheck<Player>(Position + Vector2.UnitY) && player.Top + 4 >= Bottom && player.Speed.Y < 0 && !(active || bouncetimer > 0) && !solidbeforehit)
                    {
                        if (canHitBottom) Hit(player, 0);
                    }
                }
                
            }
            if (bouncetimer > 0) bouncetimer -= Engine.DeltaTime * 60;

            if (!Collidable)
            {
                DisableStaticMovers();
            }

            inditimer += Engine.DeltaTime * animationRate * (neededflagplus ? 1 : 0);
            usedtimer += Engine.DeltaTime * animationRate * (neededflagplus ? 1 : 0);
        }

        public override void Render()
        {
            base.Render();

            Color color = Color.White;

            if (neededflagplus)
            {
                color = Color.White;
            } else
            {
                color = Calc.HexToColor("808080");
            }

            used2 = used.GetSubtexture(((int)Math.Floor(usedtimer) % usedframes) * used.Height, 0, used.Height, used.Height);
            indicator2 = indicator.GetSubtexture(((int)Math.Floor(inditimer) % indiframes) * indicator.Height, 0, indicator.Height, indicator.Height);

            if (active || switchMode)
            {
                if (bouncetimer <= 0)
                {
                    if (!switchMode)
                    {
                        used2.Draw(Position + spriteOffset, Vector2.Zero, color);
                    }
                    else
                    {
                        if(hasIndicator || hasBeenHitOnce)
                        {
                            if ((Scene as Level).Session.GetFlag(hitFlag))
                            {
                                used2.Draw(Position + spriteOffset, Vector2.Zero, color);
                            }
                            else
                            {
                                indicator2.Draw(Position + spriteOffset, Vector2.Zero, color);
                            }
                        }
                    }

                }
                else
                {
                    switch (bumpdir)
                    {
                        case 0:
                            kaizo.Draw(Position + spriteOffset - (Vector2.UnitY * (float)Math.Sin(bouncetimer / 10 * Math.PI) * 6), Vector2.Zero, color);
                            break;
                        case 1:
                            kaizo.Draw(Position + spriteOffset + (Vector2.UnitX * (float)Math.Sin(bouncetimer / 10 * Math.PI) * 6), Vector2.Zero, color);
                            break;
                        case 2:
                            kaizo.Draw(Position + spriteOffset - (Vector2.UnitX * (float)Math.Sin(bouncetimer / 10 * Math.PI) * 6), Vector2.Zero, color);
                            break;
                        case 3:
                            kaizo.Draw(Position + spriteOffset + (Vector2.UnitY * (float)Math.Sin(bouncetimer / 10 * Math.PI) * 6), Vector2.Zero, color);
                            break;
                    }
                    
                }
            }
            else
            {
                if (hasIndicator) indicator2.Draw(Position + spriteOffset, Vector2.Zero, color);
            }
        }
        public void Hit(Player player, int dir)
        {
            if (!neededflagplus) return;

            bumpdir = dir;

            int dir2 = ejectDirection == 4 ? dir : ejectDirection;

            Audio.Play(audioPath + "blockhit");

            if(hitFlag != "")(Scene as Level).Session.SetFlag(hitFlag, hitFlagBehavior == 0 ? true : hitFlagBehavior == 1 ? false : !(Scene as Level).Session.GetFlag(hitFlag));
            
            hasBeenHitOnce = true;
            if(!switchMode) active = true;
            Collidable = true;
            EnableStaticMovers();
            bouncetimer = 8;
            if (player != null)
            {
                if (dir == 0)
                {
                    player.Position.Y += Bottom - player.Top;
                    player.Speed = Vector2.Zero;
                }
                if (dir == 3 && FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
                {
                    player.Position.Y += Top - player.Bottom;
                    player.Speed = Vector2.Zero;
                }
                if (!(FemtoModule.CommunalHelperSupport.GetDreamTunnelDashState?.Invoke() == 1 || FemtoModule.CommunalHelperSupport.HasDreamTunnelDash?.Invoke() == true)) player.StateMachine.State = 0;
                if (player.Ducking && dir == 0) { player.Ducking = false; player.Position.Y += 5; };
                if (player.Ducking && dir == 3 && FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1) { player.Ducking = false; player.Position.Y -= 5; };
                if (giveCoyoteFramesOnHit)player.StartJumpGraceTime();
            }
            Celeste.Freeze(0.05f);

            if (rewards.Count != 0)
            {
                Audio.Play(audioPath + "blockreward");
            }
            else
            {
                if (switchMode)
                {
                    Audio.Play(audioPath + "blockswitch");
                }
                else
                {
                    Audio.Play(audioPath + "blockcoin");
                    Scene.Add(new Coin(Position + new Vector2(Width / 2, Height / 2), path));
                }
            }

            for (int i = 0; i < rewards.Count && i < offsets.Count; i++)
            {
                Entity reward = rewards[i];

                Vector2 offset = offsets[i];

                if (reward != null)
                {
                    switch (dir2)
                    {
                        case 0:
                            Add(new Coroutine(popupRoutine(reward, new Vector2(Center.X, Top + 8) + ejectOffset2 + new Vector2(0, -distance), dir2, offset)));
                            break;
                        case 1:
                            Add(new Coroutine(popupRoutine(reward, new Vector2(Right - 8, Center.Y) + ejectOffset2 + new Vector2(distance, 0), dir2, offset)));
                            break;
                        case 2:
                            Add(new Coroutine(popupRoutine(reward, new Vector2(Left + 8, Center.Y) + ejectOffset2 + new Vector2(-distance, 0), dir2, offset)));
                            break;
                        case 3:
                            Add(new Coroutine(popupRoutine(reward, new Vector2(Center.X, Bottom - 8) + ejectOffset2 + new Vector2(0, distance), dir2, offset)));
                            break;
                    }
                    
                }
            }
        }

        public void OnCrystalExplosion(Vector2 dir)
        {
            if(!active) Hit(null, 0);
        }

        public void OnPlayerCollide(Player player)
        {
            if (!solidbeforehit) return;
            if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
            {
                if (player.Bottom - 2 <= Top && !(active || bouncetimer > 0))
                {
                    if (canHitBottom && !(FemtoModule.CommunalHelperSupport.GetDreamTunnelDashState?.Invoke() == 1 || FemtoModule.CommunalHelperSupport.HasDreamTunnelDash.Invoke())) Hit(player, 3);
                }
            } else
            {
                if (player.Top + 2 >= Bottom && !(active || bouncetimer > 0))
                {
                    if (canHitBottom && !(FemtoModule.CommunalHelperSupport.GetDreamTunnelDashState?.Invoke() == 1 || FemtoModule.CommunalHelperSupport.HasDreamTunnelDash.Invoke())) Hit(player, 0);
                }
            }      
        }

        public IEnumerator popupRoutine(Entity entity, Vector2 to, int dir, Vector2 offset)
        {
            float[,] stupid = new float[2, 4] {
                { Center.X, Right - 8, Left + 8, Center.X },
                { Top + 8, Center.Y, Center.Y, Bottom - 8 }
            };

            Vector2 offset_start = !ejectFromPoint ? offset : Vector2.Zero;
            Vector2 offset_end = !ejectToPoint ? offset : Vector2.Zero;

            if (entity is CrystalStaticSpinner)
            {
                MethodInfo weh = (entity as CrystalStaticSpinner).GetType().GetMethod("CreateSprites", BindingFlags.NonPublic | BindingFlags.Instance);
                weh.Invoke(entity, null);
            }
            Vector2 PlatformOffset = Vector2.Zero;
            if (entity.ToString() == "Celeste.MovingPlatform")
            {
                FieldInfo wuhh = (entity as MovingPlatform).GetType().GetField("end", BindingFlags.NonPublic | BindingFlags.Instance);
                PlatformOffset = (Vector2)wuhh.GetValue(entity as MovingPlatform) - (entity as MovingPlatform).Position;
            }
            if (entity.ToString() == "Celeste.MovingPlatformLine")
            {
                FieldInfo wuhh = (entity as MovingPlatformLine).GetType().GetField("end", BindingFlags.NonPublic | BindingFlags.Instance);
                PlatformOffset = (Vector2)wuhh.GetValue(entity as MovingPlatformLine) - (entity as MovingPlatformLine).Position;
            }
            if (entity.ToString() == "Celeste.SwitchGate")
            {
                FieldInfo wuhh = (entity as SwitchGate).GetType().GetField("node", BindingFlags.NonPublic | BindingFlags.Instance);
                PlatformOffset = (Vector2)wuhh.GetValue(entity as SwitchGate) - (entity as SwitchGate).Position;
            }


            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, ejectDuration, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                if (specialHandling)
                {
                    FieldInfo wuh;
                    if (entity is Platform)
                    {
                        (entity as Platform).MoveTo(new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));
                    }
                    if (entity.ToString() == "Celeste.Cloud")
                    {
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased);
                        wuh = (entity as Cloud).GetType().GetField("startY", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue((entity as Cloud), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased));
                    }
                    else if (entity.ToString() == "Celeste.BounceBlock")
                    {
                        wuh = (entity as BounceBlock).GetType().GetField("startPos", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue((entity as BounceBlock), new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));
                    }
                    else if (entity.ToString() == "Celeste.Booster")
                    {
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased);
                        wuh = (entity as Booster).GetType().GetField("outline", BindingFlags.NonPublic | BindingFlags.Instance);
                        Entity pleasekillme = (Entity)wuh.GetValue(entity as Booster);
                        FieldInfo pkm2 = pleasekillme.GetType().GetField("Position", BindingFlags.Public | BindingFlags.Instance);
                        pkm2.SetValue(pleasekillme, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));

                    }
                    else if (entity.ToString() == "Celeste.Bumper")
                    {
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased);
                        wuh = (entity as Bumper).GetType().GetField("anchor", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as Bumper, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));

                    }
                    else if (entity.ToString() == "Celeste.FloatingDebris")
                    {
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased);
                        wuh = (entity as FloatingDebris).GetType().GetField("start", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as FloatingDebris, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));

                    }
                    else if (entity.ToString() == "Celeste.IntroCar")
                    {
                        wuh = (entity as IntroCar).GetType().GetField("startY", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as IntroCar, MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased));
                        wuh = (entity as IntroCar).GetType().GetField("wheels", BindingFlags.NonPublic | BindingFlags.Instance);
                        Entity pleasekillme = (Entity)wuh.GetValue(entity as IntroCar);
                        FieldInfo pkm2 = pleasekillme.GetType().GetField("Position", BindingFlags.Public | BindingFlags.Instance);
                        pkm2.SetValue(pleasekillme, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));

                    }
                    else if (entity.ToString() == "Celeste.LightningBreakerBox")
                    {
                        wuh = (entity as LightningBreakerBox).GetType().GetField("start", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as LightningBreakerBox, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));

                    }

                    else if (entity.ToString() == "Celeste.MoonCreature")
                    {
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased);
                        wuh = (entity as MoonCreature).GetType().GetField("start", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as MoonCreature, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));
                        wuh = (entity as MoonCreature).GetType().GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as MoonCreature, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));
                    }
                    else if (entity is CustomMoonCreature)
                    {
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased);
                        (entity as CustomMoonCreature).start = (entity as CustomMoonCreature).target = new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased));
                        for (int i = 0; i < (entity as CustomMoonCreature).trail.Length; i++)
                        {
                            (entity as CustomMoonCreature).trail[i].Position = new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased));
                        }
                    }
                    else if (entity.ToString() == "Celeste.MoveBlock")
                    {
                        wuh = (entity as MoveBlock).GetType().GetField("startPosition", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as MoveBlock, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));

                    }
                    else if (entity.ToString() == "Celeste.MovingPlatform")
                    {
                        wuh = (entity as MovingPlatform).GetType().GetField("start", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as MovingPlatform, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));
                        wuh = (entity as MovingPlatform).GetType().GetField("end", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as MovingPlatform, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X + PlatformOffset.X, to.X + offset_end.X + PlatformOffset.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y + PlatformOffset.Y, to.Y + offset_end.Y + PlatformOffset.Y, t.Eased)));

                    }

                    else if (entity.ToString() == "Celeste.MovingPlatformLine")
                    {
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased);
                        wuh = (entity as MovingPlatformLine).GetType().GetField("end", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as MovingPlatformLine, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X + PlatformOffset.X, to.X + offset_end.X + PlatformOffset.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y + PlatformOffset.Y, to.Y + offset_end.Y + PlatformOffset.Y, t.Eased)));

                    }

                    else if (entity.ToString() == "Celeste.SinkingPlatform")
                    {
                        wuh = (entity as SinkingPlatform).GetType().GetField("startY", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as SinkingPlatform, MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased));

                    }
                    else if (entity.ToString() == "Celeste.StarJumpBlock")
                    {
                        wuh = (entity as StarJumpBlock).GetType().GetField("startY", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as StarJumpBlock, MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased));

                    }
                    else if (entity.ToString() == "Celeste.Puffer")
                    {
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased);
                        wuh = (entity as Puffer).GetType().GetField("startPosition", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as Puffer, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));
                        wuh = (entity as Puffer).GetType().GetField("anchorPosition", BindingFlags.NonPublic | BindingFlags.Instance);
                        wuh.SetValue(entity as Puffer, new Vector2(MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased)));

                    }
                    else

                    {
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased);
                    }
                }
                else
                {
                    entity.Position.X = MathHelper.Lerp(stupid[0, dir] + ejectOffset.X + offset_start.X, to.X + offset_end.X, t.Eased);
                    entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + ejectOffset.Y + offset_start.Y, to.Y + offset_end.Y, t.Eased);
                }
            };
            Add(tween);
            entity.Active = entity.Visible = true;
            yield return ejectDuration;
        }

        public DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if(((FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1 ? direction.Y > 0 : direction.Y < 0) && direction.X == 0) || !neededflagplus)
            {
                return DashCollisionResults.NormalCollision;
                    
            }

            if (!solidbeforehit) return DashCollisionResults.NormalCollision;

            if (!(active || bouncetimer > 0))
            {
                if(direction.X > 0)
                {
                    if (canHitLeft)
                    {
                        Hit(player, 1);
                    } else return DashCollisionResults.NormalCollision;
                }
                else if (direction.X < 0)
                {
                    if (canHitRight)
                    {
                        Hit(player, 2);
                    } else return DashCollisionResults.NormalCollision;
                }
                if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
                {
                    if (direction.Y < 0)
                    {
                        if (canHitBottom)
                        {
                            Hit(player, 0);
                        }
                        else return DashCollisionResults.NormalCollision;
                    }
                } else
                {
                    if (direction.Y > 0)
                    {
                        if (canHitTop)
                        {
                            Hit(player, 3);
                        }
                        else return DashCollisionResults.NormalCollision;
                    }
                }
                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;

        }
    }
}
