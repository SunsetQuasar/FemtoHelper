using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Celeste.Mod.FemtoHelper.Utils;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/SMWBlock")]
public class Generic_SMWBlock : Solid
{
    public class Coin : Entity
    {
        public Vector2 Speed;

        public readonly MTexture CoinSheet;
        public MTexture CoinSlice;

        public float CoinTimer;

        public readonly int CoinFrames;

        public float Life;

        public Coin(Vector2 pos, string path) : base(pos)
        {
            CoinSheet = GFX.Game[path + "coins"];
            CoinSlice = CoinSheet.GetSubtexture(0, 0, CoinSheet.Height, CoinSheet.Height);
            Speed = new Vector2(0, -175);
            CoinFrames = CoinSheet.Width / CoinSheet.Height;
            Life = 0.6f;
        }
        public override void Update()
        {
            base.Update();
            CoinTimer += Engine.DeltaTime * 24;
            Position += Speed * Engine.DeltaTime;
            Speed.Y += 390 * Engine.DeltaTime;
            Life -= Engine.DeltaTime;
            if (!(Life < 0)) return;
            RemoveSelf();
            for(int i = 0; i< 6; i++)
            {
                Dust.Burst(Position + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4)), Calc.Random.NextFloat((float)Math.PI * 2), 1);
            }
        }
        public override void Render()
        {
            base.Render();
            CoinSlice = CoinSheet.GetSubtexture((int)Math.Floor(CoinTimer) % CoinFrames * CoinSheet.Height, 0, CoinSheet.Height, CoinSheet.Height);

            CoinSlice.DrawCentered(Position);
        }
    }

    public bool Active;

    public readonly bool HasIndicator;

    public readonly float AnimationRate;

    public readonly MTexture Indicator;
    public readonly int Indiframes;
    public float Inditimer;

    public readonly MTexture Kaizo;

    public readonly MTexture Used;
    public readonly int Usedframes;
    public float Usedtimer;

    public float Bouncetimer;

    public readonly string Path;

    public readonly bool Solidbeforehit;

    public readonly Collider PlCol;

    public MTexture Used2, Indicator2;

    public Vector2 Node;

    public readonly List<Entity> Rewards;

    public readonly List<Vector2> Offsets;

    public Rectangle Rewardcatcher;

    public bool Catched;

    public int Bumpdir;

    public readonly float Distance;

    public Vector2 EjectOffset;

    public Vector2 EjectOffset2;

    public readonly float EjectDuration;

    public readonly int EjectDirection;

    public readonly bool CanHitTop;
    public readonly bool CanHitBottom;
    public readonly bool CanHitLeft;
    public readonly bool CanHitRight;

    public readonly string HitFlag;
    public readonly string SwitchModeRenderFlag;

    public readonly int HitFlagBehavior;

    public readonly bool SwitchMode;

    public readonly bool GiveCoyoteFramesOnHit;

    public readonly string AudioPath;

    public readonly string NeededFlag;

    public bool Neededflagplus => (Scene as Level).FancyCheckFlag(NeededFlag);

    public readonly bool EjectFromPoint;
    public readonly bool EjectToPoint;

    public Vector2 SpriteOffset;

    public bool HasBeenHitOnce;

    public readonly bool SpecialHandling;

    public readonly bool InvisibleReward;
    public readonly bool InactiveReward;
    public readonly bool UncollidableReward;

    private readonly bool DashableKaizo;

    public Generic_SMWBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {
        Depth = data.Int("depth", -15000);
        SpecialHandling = data.Bool("specialEntityHandling", true);
        Catched = false;
        EjectDuration = data.Float("ejectDuration", 0.5f);
        EjectDirection = data.Int("ejectDirection", 4);
        Path = data.Attr("path", "objects/FemtoHelper/SMWBlock/solid/");
        AnimationRate = data.Float("animationRate", 8f);
        Indicator = GFX.Game[Path + "indicator"];
        Indicator2 = Indicator.GetSubtexture(0, 0, Indicator.Height, Indicator.Height);
        Indiframes = Indicator.Width / Indicator.Height;
        Inditimer = 0;
        Kaizo = GFX.Game[Path + "active"];
        Used = GFX.Game[Path + "used"];
        Used2 = Used.GetSubtexture(0, 0, Used.Height, Used.Height);
        Usedframes = Used.Width / Used.Height;
        Usedtimer = 0;
        Active = false;
        HasIndicator = data.Bool("indicate", false);
        Bouncetimer = 0;
        Collidable = Solidbeforehit = data.Bool("solidBeforeHit", false);
        DashableKaizo = data.Bool("dashableKaizo", false);
        if (!Collidable) DisableStaticMovers();
        PlCol = new Hitbox(Width - 2, 2, 1, Height);

        Add(new PlayerCollider(OnPlayerCollide, PlCol));

        OnDashCollide = OnDashed;

        Rewards = new List<Entity>();
        Offsets = new List<Vector2>();

        Node = data.NodesOffset(offset)[0];

        Rewardcatcher = new Rectangle((int)Node.X, (int)Node.Y, data.Int("rewardContainerWidth", 16), data.Int("rewardContainerHeight", 16));

        InvisibleReward = data.Bool("affectVisible", true);
        InactiveReward = data.Bool("affectActive", true);
        UncollidableReward = data.Bool("affectCollidable", false);

        Bumpdir = 0;

        Distance = data.Float("ejectDistance", 24);

        EjectOffset = new Vector2(data.Float("ejectOffsetX", 0), data.Float("ejectOffsetY", 0));
        EjectOffset2 = new Vector2(data.Float("ejectDestinationOffsetX", 0), data.Float("ejectDestinationOffsetY", 0));

        CanHitTop = data.Bool("canHitTop", true);
        CanHitBottom = data.Bool("canHitBottom", true);
        CanHitLeft = data.Bool("canHitLeft", true);
        CanHitRight = data.Bool("canHitRight", true);

        HitFlag = data.Attr("hitFlag", "smwblock_flag");
        HitFlagBehavior = data.Int("hitFlagBehavior", 2);

        SwitchMode = data.Bool("switchMode", false);
        SwitchModeRenderFlag = data.Attr("switchAppearanceFlag", "");

        GiveCoyoteFramesOnHit = data.Bool("giveCoyoteFramesOnHit", false);

        AudioPath = data.Attr("audioPath", "event:/FemtoHelper/");

        NeededFlag = data.Attr("neededFlag", "");

        EjectFromPoint = data.Bool("ejectFromPoint", true);
        EjectToPoint = data.Bool("ejectToPoint", false);

        SpriteOffset = new Vector2(data.Float("spriteOffsetX", 0), data.Float("spriteOffsetY", 0));

        Component crystalCollider = FemtoModule.CavernHelperSupport.GetCrystalBombExplosionCollider?.Invoke(OnCrystalExplosion, Collider);


        if(crystalCollider != null) Add(crystalCollider);
    }
    
    public override void Update()
    {
        Player player = Scene.Tracker.GetEntity<Player>();

        if (Catched == false && !SwitchMode)
        {
            foreach (Entity entity in Scene.Entities)
            {
                if (!Collide.CheckRect(entity, Rewardcatcher) && !Rewardcatcher.Contains((int)entity.X, (int)entity.Y)) continue;
                if (entity == (Scene as Level).SolidTiles) continue;
                Offsets.Add(entity.Position - new Vector2(Rewardcatcher.Center.X, Rewardcatcher.Center.Y));
                Rewards.Add(entity);
                if(InactiveReward) entity.Active = false;
                if(InvisibleReward) entity.Visible = false;
                if(UncollidableReward) entity.Collidable = false;
            }
            Catched = true;
        }

        if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
        {
            PlCol.Position.Y = -2;
        }
        else
        {
            PlCol.Position.Y = Height;
        }

        base.Update();

        if (!Solidbeforehit && player != null)
        {
            //bottom (top in inverted gravity)

            if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
            {
                if (CollideCheck<Player>(Position - Vector2.UnitY) && player.Bottom - 4 <= Top && player.Speed.Y < 0 && !(Active || Bouncetimer > 0))
                {
                    if (CanHitBottom) Hit(player, 3);
                }
            } else
            {
                if (CollideCheck<Player>(Position + Vector2.UnitY) && player.Top + 4 >= Bottom && player.Speed.Y < 0 && !(Active || Bouncetimer > 0))
                {
                    if (CanHitBottom) Hit(player, 0);
                }
            }

            if (DashableKaizo && player.DashAttacking)
            {
                //top (bottom in inverted gravity)

                if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
                {
                    if (CollideCheck<Player>(Position + Vector2.UnitY) && player.Top + 4 >= Bottom && player.Speed.Y > 0 && !(Active || Bouncetimer > 0))
                    {
                        if (CanHitTop) Hit(player, 0);
                    }
                }
                else
                {
                    if (CollideCheck<Player>(Position - Vector2.UnitY) && player.Bottom - 4 <= Top && player.Speed.Y > 0 && !(Active || Bouncetimer > 0))
                    {
                        if (CanHitTop) Hit(player, 3);
                    }
                }

                // to the right (left wall)

                if (CollideCheck<Player>(Position - Vector2.UnitX) && player.Right - 4 <= Left && player.Speed.X > 0 && !(Active || Bouncetimer > 0))
                {
                    if (CanHitLeft) Hit(player, 1);
                }

                // to the left (right wall)

                if (CollideCheck<Player>(Position + Vector2.UnitX) && player.Left + 4 >= Right && player.Speed.X < 0 && !(Active || Bouncetimer > 0))
                {
                    if (CanHitRight) Hit(player, 2);
                }
            }
        }
        if (Bouncetimer > 0) Bouncetimer -= Engine.DeltaTime * 60;

        if (!Collidable)
        {
            DisableStaticMovers();
        }

        Inditimer += Engine.DeltaTime * AnimationRate * (Neededflagplus ? 1 : 0);
        Usedtimer += Engine.DeltaTime * AnimationRate * (Neededflagplus ? 1 : 0);
    }

    public override void Render()
    {
        base.Render();

        Color color = Color.White;

        color = Neededflagplus ? Color.White : Calc.HexToColor("808080");

        Used2 = Used.GetSubtexture((int)Math.Floor(Usedtimer) % Usedframes * Used.Height, 0, Used.Height, Used.Height);
        Indicator2 = Indicator.GetSubtexture((int)Math.Floor(Inditimer) % Indiframes * Indicator.Height, 0, Indicator.Height, Indicator.Height);

        if (Active || SwitchMode)
        {
            if (Bouncetimer <= 0)
            {
                if (!SwitchMode)
                {
                    Used2.Draw(Position + SpriteOffset, Vector2.Zero, color);
                }
                else
                {
                    if(HasIndicator || HasBeenHitOnce)
                    {
                        if (string.IsNullOrEmpty(SwitchModeRenderFlag) ? (Scene as Level).Session.GetFlag(HitFlag) : (Scene as Level).Session.GetFlag(SwitchModeRenderFlag))
                        {
                            Used2.Draw(Position + SpriteOffset, Vector2.Zero, color);
                        }
                        else
                        {
                            Indicator2.Draw(Position + SpriteOffset, Vector2.Zero, color);
                        }
                    }
                }

            }
            else
            {
                switch (Bumpdir)
                {
                    case 0:
                        Kaizo.Draw(Position + SpriteOffset - Vector2.UnitY * (float)Math.Sin(Bouncetimer / 10 * Math.PI) * 6, Vector2.Zero, color);
                        break;
                    case 1:
                        Kaizo.Draw(Position + SpriteOffset + Vector2.UnitX * (float)Math.Sin(Bouncetimer / 10 * Math.PI) * 6, Vector2.Zero, color);
                        break;
                    case 2:
                        Kaizo.Draw(Position + SpriteOffset - Vector2.UnitX * (float)Math.Sin(Bouncetimer / 10 * Math.PI) * 6, Vector2.Zero, color);
                        break;
                    case 3:
                        Kaizo.Draw(Position + SpriteOffset + Vector2.UnitY * (float)Math.Sin(Bouncetimer / 10 * Math.PI) * 6, Vector2.Zero, color);
                        break;
                }
                
            }
        }
        else
        {
            if (HasIndicator) Indicator2.Draw(Position + SpriteOffset, Vector2.Zero, color);
        }
    }
    public void Hit(Player player, int dir)
    {
        if (!Neededflagplus) return;

        Bumpdir = dir;

        int dir2 = EjectDirection == 4 ? dir : EjectDirection;

        Audio.Play(AudioPath + "blockhit");

        if(HitFlag != "")(Scene as Level).Session.SetFlag(HitFlag, HitFlagBehavior == 0 ? true : HitFlagBehavior == 1 ? false : !(Scene as Level).Session.GetFlag(HitFlag));
        
        HasBeenHitOnce = true;
        if(!SwitchMode) Active = true;
        EnableStaticMovers();
        Bouncetimer = 8;
        if (player != null)
        {
            switch (dir)
            {
                case 0:
                    player.Position.Y += Bottom - player.Top;
                    player.Speed = Vector2.Zero;
                    break;
                case 1 when DashableKaizo:
                    player.MoveToX(Left - (player.Width / 2));
                    player.Rebound(-1);
                    break;
                case 2 when DashableKaizo:
                    player.MoveToX(Right + (player.Width / 2));
                    player.Rebound(1);
                    break;
                case 3 when FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1 || DashableKaizo:
                    player.Position.Y += Top - player.Bottom;
                    player.Speed = Vector2.Zero;
                    if (DashableKaizo)
                    {
                        player.Rebound(0);
                    }
                    break;
            }

            if (!(FemtoModule.CommunalHelperSupport.GetDreamTunnelDashState?.Invoke() == 1 || FemtoModule.CommunalHelperSupport.HasDreamTunnelDash?.Invoke() == true)) player.StateMachine.State = 0;
            if (player.Ducking && dir == 0) { player.Ducking = false; player.Position.Y += 5; };
            if (player.Ducking && dir == 3 && FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1) { player.Ducking = false; player.Position.Y -= 5; };
            if (GiveCoyoteFramesOnHit)player.StartJumpGraceTime();
        }
        Collidable = true;
        Celeste.Freeze(0.05f);

        if (Rewards.Count != 0)
        {
            Audio.Play(AudioPath + "blockreward");
        }
        else
        {
            if (SwitchMode)
            {
                Audio.Play(AudioPath + "blockswitch");
            }
            else
            {
                Audio.Play(AudioPath + "blockcoin");
                Scene.Add(new Coin(Position + new Vector2(Width / 2, Height / 2), Path));
            }
        }

        for (int i = 0; i < Rewards.Count && i < Offsets.Count; i++)
        {
            Entity reward = Rewards[i];

            Vector2 offset = Offsets[i];

            if (reward != null)
            {
                switch (dir2)
                {
                    case 0:
                        Add(new Coroutine(PopupRoutine(reward, new Vector2(Center.X, Top + 8) + EjectOffset2 + new Vector2(0, -Distance), dir2, offset)));
                        break;
                    case 1:
                        Add(new Coroutine(PopupRoutine(reward, new Vector2(Right - 8, Center.Y) + EjectOffset2 + new Vector2(Distance, 0), dir2, offset)));
                        break;
                    case 2:
                        Add(new Coroutine(PopupRoutine(reward, new Vector2(Left + 8, Center.Y) + EjectOffset2 + new Vector2(-Distance, 0), dir2, offset)));
                        break;
                    case 3:
                        Add(new Coroutine(PopupRoutine(reward, new Vector2(Center.X, Bottom - 8) + EjectOffset2 + new Vector2(0, Distance), dir2, offset)));
                        break;
                }
                
            }
        }
    }

    public void OnCrystalExplosion(Vector2 dir)
    {
        if(!Active) Hit(null, 0);
    }

    public void OnPlayerCollide(Player player)
    {
        if (!Solidbeforehit) return;
        if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
        {
            if (!(player.Bottom - 2 <= Top) || Active || Bouncetimer > 0) return;
            if (CanHitBottom && !(FemtoModule.CommunalHelperSupport.GetDreamTunnelDashState?.Invoke() == 1 || (FemtoModule.CommunalHelperSupport.HasDreamTunnelDash?.Invoke() ?? false))) Hit(player, 3);
        } else
        {
            if (!(player.Top + 2 >= Bottom) || Active || Bouncetimer > 0) return;
            if (CanHitBottom && !(FemtoModule.CommunalHelperSupport.GetDreamTunnelDashState?.Invoke() == 1 || (FemtoModule.CommunalHelperSupport.HasDreamTunnelDash?.Invoke() ?? false))) Hit(player, 0);
        }      
    }

    public IEnumerator PopupRoutine(Entity entity, Vector2 to, int dir, Vector2 offset)
    {
        float[,] stupid = new float[2, 4] {
            { Center.X, Right - 8, Left + 8, Center.X },
            { Top + 8, Center.Y, Center.Y, Bottom - 8 }
        };

        Vector2 offsetStart = !EjectFromPoint ? offset : Vector2.Zero;
        Vector2 offsetEnd = !EjectToPoint ? offset : Vector2.Zero;
        Vector2 platformOffset = entity.ToString() switch
        {
            "Celeste.MovingPlatform" => (entity as MovingPlatform).end - (entity as MovingPlatform).Position,
            "Celeste.MovingPlatformLine" => (entity as MovingPlatformLine).end - (entity as MovingPlatformLine).Position,
            "Celeste.SwitchGate" => (entity as SwitchGate).node - (entity as SwitchGate).Position,
            _ => Vector2.Zero
        };

        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, EjectDuration, start: true);
        tween.OnUpdate = delegate (Tween t)
        {
            if (SpecialHandling)
            {
                if (entity is Platform platform)
                {
                    platform.MoveTo(new Vector2(MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased)));
                }
                switch (entity.ToString())
                {
                    case "Celeste.Cloud":
                        (entity as Cloud).startY = entity.Position.Y;
                        break;
                    case "Celeste.BounceBlock":
                        (entity as BounceBlock).startPos = entity.Position;
                        break;
                    case "Celeste.Booster":
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased);
                        (entity as Booster).outline.Position = entity.Position;
                        break;
                    case "Celeste.Bumper":
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased);
                        (entity as Bumper).anchor = entity.Position;
                        break;
                    case "Celeste.FloatingDebris":
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased);
                        (entity as FloatingDebris).start = entity.Position;
                        break;
                    case "Celeste.IntroCar":
                        (entity as IntroCar).startY = entity.Position.Y;
                        (entity as IntroCar).wheels.Position = entity.Position;
                        break;
                    case "Celeste.LightningBreakerBox":
                        (entity as LightningBreakerBox).start = entity.Position;
                        break;
                    case "Celeste.MoonCreature":
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased);
                        (entity as MoonCreature).start = (entity as MoonCreature).target = entity.Position;
                        break;
                    case "CustomMoonCreature":
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased);
                        (entity as CustomMoonCreature).Start = (entity as CustomMoonCreature).Target = entity.Position;
                        for (int i = 0; i < (entity as CustomMoonCreature).Trail.Length; i++)
                        {
                            (entity as CustomMoonCreature).Trail[i].Position = new Vector2(MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased));
                        }
                        break;
                    case "Celeste.MoveBlock":
                        (entity as MoveBlock).startPosition = entity.Position;
                        break;
                    case "Celeste.MovingPlatform":
                        (entity as MovingPlatform).start = new Vector2(MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased));
                        (entity as MovingPlatform).end = new Vector2(MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X + platformOffset.X, to.X + offsetEnd.X + platformOffset.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y + platformOffset.Y, to.Y + offsetEnd.Y + platformOffset.Y, t.Eased));
                        break;
                    case "Celeste.MovingPlatformLine":
                        (entity as MovingPlatformLine).end = new Vector2(MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X + platformOffset.X, to.X + offsetEnd.X + platformOffset.X, t.Eased), MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y + platformOffset.Y, to.Y + offsetEnd.Y + platformOffset.Y, t.Eased));
                        break;
                    case "Celeste.SinkingPlatform":
                        (entity as SinkingPlatform).startY = entity.Position.Y;
                        break;
                    case "Celeste.StarJumpBlock":
                        (entity as StarJumpBlock).startY = entity.Position.Y;
                        break;
                    case "Celeste.Puffer":
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased);
                        (entity as Puffer).anchorPosition = (entity as Puffer).startPosition = entity.Position;
                        break;
                    case "Celeste.DashSwitch":
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased);
                        (entity as DashSwitch).pressedTarget = (entity as DashSwitch).side switch
                        {
                            DashSwitch.Sides.Down => entity.Position + Vector2.UnitY * 8f,
                            DashSwitch.Sides.Up => entity.Position + Vector2.UnitY * -8f,
                            DashSwitch.Sides.Right => entity.Position + Position + Vector2.UnitX * 8f,
                            DashSwitch.Sides.Left => entity.Position + Vector2.UnitX * -8f,
                            _ => Position
                        };
                        break;
                    case "Celeste.CrystalStaticSpinner":
                        CrystalStaticSpinner spinner = entity as CrystalStaticSpinner;
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased);
                        if (spinner.filler != null)
                        {
                            spinner.filler.RemoveSelf();
                        }
                        spinner.filler = null;
                        if (spinner.border != null)
                        {
                            spinner.border.RemoveSelf();
                        }
                        spinner.border = null;
                        List<Image> list = spinner.Components.GetAll<Image>().ToList();
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].RemoveSelf();
                        }
                        spinner.expanded = false;
                        (entity as CrystalStaticSpinner).CreateSprites();
                        break;
                    case "Celeste.SwitchGate":
                        (entity as SwitchGate).node = entity.Position + platformOffset;
                        (entity as SwitchGate).Get<Coroutine>().RemoveSelf();
                        (entity as SwitchGate).Add(new Coroutine((entity as SwitchGate).Sequence((entity as SwitchGate).node)));
                        break;
                    default:
                        entity.Position.X = MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased);
                        entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased);
                        break;
                }
            }
            else
            {
                entity.Position.X = MathHelper.Lerp(stupid[0, dir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased);
                entity.Position.Y = MathHelper.Lerp(stupid[1, dir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased);
            }
        };
        Add(tween);
        if (InactiveReward) entity.Active = true;
        if (InvisibleReward) entity.Visible = true;
        if (UncollidableReward) entity.Collidable = true;
        yield return EjectDuration;
    }

    

    public DashCollisionResults OnDashed(Player player, Vector2 direction)
    {
        if(((FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1 ? direction.Y > 0 : direction.Y < 0) && direction.X == 0) || !Neededflagplus)
        {
            return DashCollisionResults.NormalCollision;
                
        }

        if (!Solidbeforehit) return DashCollisionResults.NormalCollision;

        if (Active || Bouncetimer > 0) return DashCollisionResults.NormalCollision;
        switch (direction.X)
        {
            case > 0 when CanHitLeft:
                Hit(player, 1);
                break;
            case > 0:
                return DashCollisionResults.NormalCollision;
            case < 0 when CanHitRight:
                Hit(player, 2);
                break;
            case < 0:
                return DashCollisionResults.NormalCollision;
        }
        if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
        {
            if (!(direction.Y < 0)) return DashCollisionResults.Rebound;
            if (CanHitBottom)
            {
                Hit(player, 0);
            }
            else return DashCollisionResults.NormalCollision;
        } else
        {
            if (!(direction.Y > 0)) return DashCollisionResults.Rebound;
            if (CanHitTop)
            {
                Hit(player, 3);
            }
            else return DashCollisionResults.NormalCollision;
        }
        return DashCollisionResults.Rebound;

    }
}