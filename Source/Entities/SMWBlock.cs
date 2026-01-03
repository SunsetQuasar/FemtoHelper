using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Celeste.Mod.FemtoHelper.Utils;
using MonoMod.Utils;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/SMWBlock")]
public class GenericSmwBlock : Solid
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
            for (int i = 0; i < 6; i++)
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

    public readonly struct SheetFrames
    {
        readonly List<MTexture> frames;
        readonly int sourceWidth, sourceHeight, count;

        public SheetFrames(MTexture tex)
        {
            frames = [];
            sourceWidth = tex.Width;
            sourceHeight = tex.Height;

            for (int i = 0; i < tex.Width; i += tex.Height)
            {
                frames.Add(tex.GetSubtexture(i, 0, tex.Height, tex.Height));
            }

            count = frames.Count;
        }

        public MTexture this[int frame]
        {
            get
            {
                return frames[frame % count];
            }
        }
    }

    public bool Activated;

    public readonly bool HasIndicator;

    public readonly float AnimationRate;

    public readonly SheetFrames Indicator;
    //public readonly MTexture Indicator;
    //public readonly int Indiframes;
    public float Inditimer;

    public readonly MTexture HitSprite;

    public readonly SheetFrames Used;
    //public readonly MTexture Used;
    //public readonly int Usedframes;
    public float Usedtimer;

    public float Bouncetimer;

    public readonly string Path;

    public readonly bool Solidbeforehit;

    public readonly Collider PlCol;

    //public MTexture Used2, Indicator2;

    public Vector2 Node;

    public readonly List<Entity> Rewards;

    public readonly List<Vector2> Offsets;

    public Rectangle Rewardcatcher;

    public bool Caught;

    public Direction Bumpdir;

    public readonly float Distance;

    public Vector2 EjectOffset;

    public Vector2 EjectOffset2;

    public readonly float EjectDuration;

    public readonly Direction EjectDirection;

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
    public readonly float LaunchMultiplier;

    public readonly bool InvisibleReward;
    public readonly bool InactiveReward;
    public readonly bool UncollidableReward;

    private readonly bool dashableKaizo;

    private readonly HashSet<string> whitelist;
    private readonly HashSet<string> blacklist;
    public enum Direction
    {
        Up,
        Right,
        Left,
        Down,
        Opposite,
    }

    public GenericSmwBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {
        whitelist = [.. data.String("whitelist", "").Split(',')];
        blacklist = [.. data.String("blacklist", "").Split(',')];
        whitelist.Remove("");
        blacklist.Remove("");

        Depth = data.Int("depth", -15000);

        SpecialHandling = data.Bool("specialEntityHandling", true);

        LaunchMultiplier = data.Float("launchMultiplier", 0f);

        Caught = false;

        EjectDuration = data.Float("ejectDuration", 0.5f);
        EjectDirection = (Direction)data.Int("ejectDirection", 4);

        Path = data.Attr("path", "objects/FemtoHelper/SMWBlock/solid/");

        AnimationRate = data.Float("animationRate", 8f);

        Indicator = new(GFX.Game[Path + "indicator"]);
        Inditimer = 0;

        Used = new(GFX.Game[Path + "used"]);
        Usedtimer = 0;

        HitSprite = GFX.Game[Path + "active"];

        Activated = false;

        HasIndicator = data.Bool("indicate", false);

        Bouncetimer = 0;

        Collidable = Solidbeforehit = data.Bool("solidBeforeHit", false);

        dashableKaizo = data.Bool("dashableKaizo", false);
        if (!Collidable) DisableStaticMovers();
        PlCol = new Hitbox(Width - 2, 2, 1, Height);

        Add(new PlayerCollider(OnPlayerCollide, PlCol));

        OnDashCollide = OnDashed;

        Rewards = [];
        Offsets = [];

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


        if (crystalCollider != null) Add(crystalCollider);
    }

    public override void Update()
    {
        Player player = Scene.Tracker.GetEntity<Player>();

        if (Caught == false && !SwitchMode)
        {
            foreach (Entity entity in Scene.Entities)
            {
                if (!Collide.CheckRect(entity, Rewardcatcher) && !Rewardcatcher.Contains((int)entity.X, (int)entity.Y)) continue;
                if (entity == (Scene as Level).SolidTiles) continue;

                if (whitelist.Count > 0)
                {
                    bool whitelisted = false;
                    foreach (string str in whitelist)
                    {
                        if (entity.GetType().FullName == str)
                        {
                            whitelisted = true;
                            break;
                        }
                    }
                    if (!whitelisted) continue;
                }
                else
                {
                    bool blacklisted = false;
                    foreach (string str in blacklist)
                    {
                        if (entity.GetType().FullName == str)
                        {
                            blacklisted = true;
                            break;
                        }
                    }
                    if (blacklisted) continue;
                }

                Offsets.Add(entity.Position - new Vector2(Rewardcatcher.Center.X, Rewardcatcher.Center.Y));
                Rewards.Add(entity);
                if (InactiveReward) entity.Active = false;
                if (InvisibleReward) entity.Visible = false;
                if (UncollidableReward) entity.Collidable = false;
            }
            Caught = true;
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
                if (CollideCheck<Player>(Position - Vector2.UnitY) && player.Bottom - 4 <= Top && player.Speed.Y < 0 && !(Activated || Bouncetimer > 0))
                {
                    if (CanHitBottom) Hit(player, Direction.Down);
                }
            }
            else
            {
                if (CollideCheck<Player>(Position + Vector2.UnitY) && player.Top + 4 >= Bottom && player.Speed.Y < 0 && !(Activated || Bouncetimer > 0))
                {
                    if (CanHitBottom) Hit(player, Direction.Up);
                }
            }

            if (dashableKaizo && player.DashAttacking)
            {
                //top (bottom in inverted gravity)

                if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
                {
                    if (CollideCheck<Player>(Position + Vector2.UnitY) && player.Top + 4 >= Bottom && player.Speed.Y > 0 && !(Activated || Bouncetimer > 0))
                    {
                        if (CanHitTop) Hit(player, Direction.Up);
                    }
                }
                else
                {
                    if (CollideCheck<Player>(Position - Vector2.UnitY) && player.Bottom - 4 <= Top && player.Speed.Y > 0 && !(Activated || Bouncetimer > 0))
                    {
                        if (CanHitTop) Hit(player, Direction.Down);
                    }
                }

                // to the right (left wall)

                if (CollideCheck<Player>(Position - Vector2.UnitX) && player.Right - 4 <= Left && player.Speed.X > 0 && !(Activated || Bouncetimer > 0))
                {
                    if (CanHitLeft) Hit(player, Direction.Right);
                }

                // to the left (right wall)

                if (CollideCheck<Player>(Position + Vector2.UnitX) && player.Left + 4 >= Right && player.Speed.X < 0 && !(Activated || Bouncetimer > 0))
                {
                    if (CanHitRight) Hit(player, Direction.Left);
                }
            }
        }
        if (Bouncetimer > 0)
        {
            Bouncetimer -= Engine.DeltaTime * 60;
        }

        if (!Collidable)
        {
            DisableStaticMovers();
        }

        Inditimer += Engine.DeltaTime * AnimationRate * (Neededflagplus ? 1 : 0);
        Usedtimer += Engine.DeltaTime * AnimationRate * (Neededflagplus ? 1 : 0);
    }

    public void TryDrawTiled(MTexture texture, Color color, Vector2 offset = default)
    {
        for(int i = 0; i < Width; i += texture.Width)
        {
            for (int j = 0; j < Height; j += texture.Height)
            {
                texture.Draw(Position + SpriteOffset + offset + new Vector2(i, j), Vector2.Zero, color);
            }
        }
    }

    public override void Render()
    {
        base.Render();

        Color color = Neededflagplus ? Color.White : Calc.HexToColor("808080");

        if (Activated || SwitchMode)
        {
            if (Bouncetimer <= 0)
            {
                if (!SwitchMode)
                {
                    TryDrawTiled(Used[(int)Math.Floor(Usedtimer)], color);
                }
                else
                {
                    if (HasIndicator || HasBeenHitOnce)
                    {
                        if (string.IsNullOrEmpty(SwitchModeRenderFlag) ? (Scene as Level).Session.GetFlag(HitFlag) : (Scene as Level).Session.GetFlag(SwitchModeRenderFlag))
                        {
                            TryDrawTiled(Used[(int)Math.Floor(Usedtimer)], color);
                        }
                        else
                        {
                            TryDrawTiled(Indicator[(int)Math.Floor(Inditimer)], color);
                        }
                    }
                }

            }
            else
            {
                float distance = (float)Math.Sin(Bouncetimer / 10 * Math.PI) * 6;
                switch (Bumpdir)
                {
                    default:
                    case Direction.Up:
                        TryDrawTiled(HitSprite, color, -Vector2.UnitY * distance);
                        break;
                    case Direction.Right:
                        TryDrawTiled(HitSprite, color, Vector2.UnitX * distance);
                        break;
                    case Direction.Left:
                        TryDrawTiled(HitSprite, color, -Vector2.UnitX * distance);
                        break;
                    case Direction.Down:
                        TryDrawTiled(HitSprite, color, Vector2.UnitY * distance);
                        break;
                }

            }
        }
        else
        {
            if (HasIndicator) TryDrawTiled(Indicator[(int)Math.Floor(Inditimer)], color);
        }
    }
    public void Hit(Player player, Direction dir)
    {
        if (!Neededflagplus) return;

        Bumpdir = dir;

        Direction dir2 = EjectDirection == Direction.Opposite ? dir : EjectDirection;

        Audio.Play(AudioPath + "blockhit");

        if (HitFlag != "") (Scene as Level).Session.SetFlag(HitFlag, HitFlagBehavior == 0 ? true : HitFlagBehavior == 1 ? false : !(Scene as Level).Session.GetFlag(HitFlag));

        HasBeenHitOnce = true;
        if (!SwitchMode) Activated = true;
        EnableStaticMovers();
        Bouncetimer = 8;
        if (player != null)
        {
            switch (dir)
            {
                case Direction.Right when dashableKaizo:
                    player.MoveToX(Left - (player.Width / 2));
                    player.Rebound(-1);
                    break;
                case Direction.Left when dashableKaizo:
                    player.MoveToX(Right + (player.Width / 2));
                    player.Rebound(1);
                    break;
                case Direction.Down when FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1 || dashableKaizo:
                    player.Position.Y += Top - player.Bottom;
                    player.Speed = Vector2.Zero;
                    if (dashableKaizo)
                    {
                        player.Rebound(0);
                    }
                    break;
                case Direction.Up:
                    player.Position.Y += Bottom - player.Top;
                    player.Speed = Vector2.Zero;
                    break;
            }

            if (!(FemtoModule.CommunalHelperSupport.GetDreamTunnelDashState?.Invoke() == 1 || FemtoModule.CommunalHelperSupport.HasDreamTunnelDash?.Invoke() == true)) player.StateMachine.State = 0;

            if (player.Ducking && dir == Direction.Up)
            {
                player.Ducking = false;
                player.Position.Y += 5;
            }

            if (player.Ducking && dir == Direction.Down && FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
            {
                player.Ducking = false;
                player.Position.Y -= 5;
            }

            if (GiveCoyoteFramesOnHit) player.StartJumpGraceTime();
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
                    case Direction.Right:
                        Add(new Coroutine(PopupRoutine(reward, new Vector2(Right - 8, Center.Y) + EjectOffset2 + new Vector2(Distance, 0), dir2, offset)));
                        break;
                    case Direction.Left:
                        Add(new Coroutine(PopupRoutine(reward, new Vector2(Left + 8, Center.Y) + EjectOffset2 + new Vector2(-Distance, 0), dir2, offset)));
                        break;
                    case Direction.Down:
                        Add(new Coroutine(PopupRoutine(reward, new Vector2(Center.X, Bottom - 8) + EjectOffset2 + new Vector2(0, Distance), dir2, offset)));
                        break;
                    default:
                    case Direction.Up:
                        Add(new Coroutine(PopupRoutine(reward, new Vector2(Center.X, Top + 8) + EjectOffset2 + new Vector2(0, -Distance), dir2, offset)));
                        break;
                }

            }
        }
    }

    public void OnCrystalExplosion(Vector2 dir)
    {
        if (!Activated) Hit(null, 0);
    }

    public void OnPlayerCollide(Player player)
    {
        if (!Solidbeforehit) return;
        if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
        {
            if (!(player.Bottom - 2 <= Top) || Activated || Bouncetimer > 0) return;
            if (CanHitBottom && !(FemtoModule.CommunalHelperSupport.GetDreamTunnelDashState?.Invoke() == 1 || (FemtoModule.CommunalHelperSupport.HasDreamTunnelDash?.Invoke() ?? false))) Hit(player, Direction.Down);
        }
        else
        {
            if (!(player.Top + 2 >= Bottom) || Activated || Bouncetimer > 0) return;
            if (CanHitBottom && !(FemtoModule.CommunalHelperSupport.GetDreamTunnelDashState?.Invoke() == 1 || (FemtoModule.CommunalHelperSupport.HasDreamTunnelDash?.Invoke() ?? false))) Hit(player, Direction.Up);
        }
    }

    public void UpdateCameraForPlayer(Player player)
    {
        if (player.Scene == null) return;
        Level level = Scene as Level;
        Vector2 position = level.Camera.Position;
        Vector2 cameraTarget = player?.CameraTarget ?? position;
        float num = (player.StateMachine.State == 20) ? 8f : 1f;
        level.Camera.Position = position + (cameraTarget - position) * (1f - (float)Math.Pow(0.01f / num, Engine.DeltaTime));
    }

    public void CheckTriggersForPlayer(Player player)
    {
        foreach (Trigger entity in Scene.Tracker.GetEntities<Trigger>())
        {
            if (CollideCheck(entity))
            {
                if (!entity.Triggered)
                {
                    entity.Triggered = true;
                    player.triggersInside.Add(entity);
                    entity.OnEnter(player);
                }
                entity.OnStay(player);
            }
            else if (entity.Triggered)
            {
                player.triggersInside.Remove(entity);
                entity.Triggered = false;
                entity.OnLeave(player);
            }
        }
    }

    public IEnumerator PopupRoutine(Entity entity, Vector2 to, Direction dir, Vector2 offset)
    {

        if (entity is null) yield break;

        int intdir = (int)dir;

        bool inactiveActor = false;

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

        if (((entity is Actor && entity.Get<Holdable>() is not null) || entity is Player) && LaunchMultiplier > 0f)
        {
            switch (dir)
            {
                case Direction.Right:
                    entity.CenterY = CenterY;
                    entity.Left = Right;
                    break;
                case Direction.Left:
                    entity.CenterY = CenterY;
                    entity.Right = Left;
                    break;
                case Direction.Down:
                    entity.CenterX = CenterX;
                    entity.Top = Bottom;
                    break;
                case Direction.Up:
                    entity.CenterX = CenterX;
                    entity.Bottom = Top;
                    break;
            }

            entity.Position += EjectOffset;

            Vector2 go = dir switch
            {
                Direction.Up => new(0, -1),
                Direction.Right => new(1, 0),
                Direction.Left => new(-1, 0),
                Direction.Down => new(0, 1),
                _ => new()
            };

            if (entity is Player p)
            {
                p.ExplodeLaunch(entity.Center - go, true, false);
                p.Speed *= LaunchMultiplier;
            }
            else if (entity.Get<Holdable>() is { } h)
            {
                h.ExplodeLaunch(entity.Center - go, true, false);
                h.SetSpeed(h.GetSpeed() * LaunchMultiplier);
            }
        }
        else
        {
            if (SpecialHandling)
            {
                //necessary because aqua does some funny business, only do it once because dynamicdata moment
                if (entity is Refill refill)
                {
                    DynamicData.For(refill).Set("respawn_position", to + offsetEnd);
                }

                if (!InactiveReward)
                {
                    if (entity is Player p)
                    {
                        p.Active = false;
                        inactiveActor = true;
                    }
                    if (entity.Get<Holdable>() is { } h)
                    {
                        h.Holder?.Drop();
                        entity.Active = false;
                        inactiveActor = true;
                    }
                }
            }

            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, EjectDuration, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                if (entity is null || entity.Scene is null) return;

                float lerpx = MathHelper.Lerp(stupid[0, intdir] + EjectOffset.X + offsetStart.X, to.X + offsetEnd.X, t.Eased);
                float lerpy = MathHelper.Lerp(stupid[1, intdir] + EjectOffset.Y + offsetStart.Y, to.Y + offsetEnd.Y, t.Eased);

                if (SpecialHandling)
                {
                    if (entity is Platform platform)
                    {
                        platform.MoveTo(new Vector2(lerpx, lerpy));
                        //platform.Position = new Vector2(lerpx, lerpy);
                    }

                    if (entity.Get<Holdable>() is { } h)
                    {
                        if (InactiveReward)
                        {
                            h.SetSpeed(Vector2.Zero);
                        }
                        h.cannotHoldTimer = (2 * Engine.DeltaTime);
                        if (h.Holder is not null)
                        {
                            t.RemoveSelf();
                        }
                    }

                    if (entity is Player p)
                    {
                        CheckTriggersForPlayer(p);
                        UpdateHairForPlayer(p);
                        UpdateCameraForPlayer(p);
                        p.UpdateHair(true);
                        p.Hair.Update();
                        p.Hair.AfterUpdate();
                        p.UpdateCarry();
                        if (InactiveReward)
                        {
                            p.Speed = Vector2.Zero;
                        }
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
                            entity.Position.X = lerpx;
                            entity.Position.Y = lerpy;
                            (entity as Booster).outline.Position = entity.Position;
                            break;
                        case "Celeste.Bumper":
                            entity.Position.X = lerpx;
                            entity.Position.Y = lerpy;
                            (entity as Bumper).anchor = entity.Position;
                            break;
                        case "Celeste.FloatingDebris":
                            entity.Position.X = lerpx;
                            entity.Position.Y = lerpy;
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
                            entity.Position.X = lerpx;
                            entity.Position.Y = lerpy;
                            (entity as MoonCreature).start = (entity as MoonCreature).target = entity.Position;
                            break;
                        case "CustomMoonCreature":
                            entity.Position.X = lerpx;
                            entity.Position.Y = lerpy;
                            (entity as CustomMoonCreature).Start = (entity as CustomMoonCreature).Target = entity.Position;
                            for (int i = 0; i < (entity as CustomMoonCreature).Trail.Length; i++)
                            {
                                (entity as CustomMoonCreature).Trail[i].Position = new Vector2(lerpx, lerpy);
                            }
                            break;
                        case "Celeste.MoveBlock":
                            (entity as MoveBlock).startPosition = entity.Position;
                            break;
                        case "Celeste.MovingPlatform":
                            (entity as MovingPlatform).start = new Vector2(lerpx, lerpy);
                            (entity as MovingPlatform).end = new Vector2(MathHelper.Lerp(stupid[0, intdir] + EjectOffset.X + offsetStart.X + platformOffset.X, to.X + offsetEnd.X + platformOffset.X, t.Eased), MathHelper.Lerp(stupid[1, intdir] + EjectOffset.Y + offsetStart.Y + platformOffset.Y, to.Y + offsetEnd.Y + platformOffset.Y, t.Eased));
                            break;
                        case "Celeste.MovingPlatformLine":
                            (entity as MovingPlatformLine).end = new Vector2(MathHelper.Lerp(stupid[0, intdir] + EjectOffset.X + offsetStart.X + platformOffset.X, to.X + offsetEnd.X + platformOffset.X, t.Eased), MathHelper.Lerp(stupid[1, intdir] + EjectOffset.Y + offsetStart.Y + platformOffset.Y, to.Y + offsetEnd.Y + platformOffset.Y, t.Eased));
                            break;
                        case "Celeste.SinkingPlatform":
                            (entity as SinkingPlatform).startY = entity.Position.Y;
                            break;
                        case "Celeste.StarJumpBlock":
                            (entity as StarJumpBlock).startY = entity.Position.Y;
                            break;
                        case "Celeste.Puffer":
                            entity.Position.X = lerpx;
                            entity.Position.Y = lerpy;
                            (entity as Puffer).anchorPosition = (entity as Puffer).startPosition = entity.Position;
                            break;
                        case "Celeste.DashSwitch":
                            entity.Position.X = lerpx;
                            entity.Position.Y = lerpy;
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
                            entity.Position.X = lerpx;
                            entity.Position.Y = lerpy;
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
                            entity.Position.X = lerpx;
                            entity.Position.Y = lerpy;
                            break;
                    }
                }
                else
                {
                    entity.Position.X = lerpx;
                    entity.Position.Y = lerpy;
                }
            };
            Add(tween);
        }

        if (InactiveReward && !inactiveActor) entity.Active = true;
        if (InvisibleReward) entity.Visible = true;
        if (UncollidableReward) entity.Collidable = true;
        yield return EjectDuration;
        if (entity is null) yield break;
        if (inactiveActor) entity.Active = true;
    }

    private void UpdateHairForPlayer(Player p)
    {
        Vector2 forceStrongWindHair = p.windDirection;
        if (p.ForceStrongWindHair.Length() > 0f)
        {
            forceStrongWindHair = p.ForceStrongWindHair;
        }
        if (p.windTimeout > 0f && forceStrongWindHair.X != 0f)
        {
            p.windHairTimer += Engine.DeltaTime * 8f;
            p.Hair.StepPerSegment = new Vector2(forceStrongWindHair.X * 5f, (float)Math.Sin(p.windHairTimer));
            p.Hair.StepInFacingPerSegment = 0f;
            p.Hair.StepApproach = 128f;
            p.Hair.StepYSinePerSegment = 0f;
        }
        else if (p.Dashes > 1)
        {
            p.Hair.StepPerSegment = new Vector2((float)Math.Sin(Scene.TimeActive * 2f) * 0.7f - (float)((int)p.Facing * 3), (float)Math.Sin(Scene.TimeActive * 1f));
            p.Hair.StepInFacingPerSegment = 0f;
            p.Hair.StepApproach = 90f;
            p.Hair.StepYSinePerSegment = 1f;
            p.Hair.StepPerSegment.Y += forceStrongWindHair.Y * 2f;
        }
        else
        {
            p.Hair.StepPerSegment = new Vector2(0f, 2f);
            p.Hair.StepInFacingPerSegment = 0.5f;
            p.Hair.StepApproach = 64f;
            p.Hair.StepYSinePerSegment = 0f;
            p.Hair.StepPerSegment.Y += forceStrongWindHair.Y * 0.5f;
        }
    }

    public DashCollisionResults OnDashed(Player player, Vector2 direction)
    {
        if (((FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1 ? direction.Y > 0 : direction.Y < 0) && direction.X == 0) || !Neededflagplus)
        {
            return DashCollisionResults.NormalCollision;

        }

        if (!Solidbeforehit) return DashCollisionResults.NormalCollision;

        if (Activated || Bouncetimer > 0) return DashCollisionResults.NormalCollision;
        switch (direction.X)
        {
            case > 0 when CanHitLeft:
                Hit(player, Direction.Right);
                break;
            case > 0:
                return DashCollisionResults.NormalCollision;
            case < 0 when CanHitRight:
                Hit(player, Direction.Left);
                break;
            case < 0:
                return DashCollisionResults.NormalCollision;
        }
        if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
        {
            if (!(direction.Y < 0)) return DashCollisionResults.Rebound;
            if (CanHitBottom)
            {
                Hit(player, Direction.Up);
            }
            else return DashCollisionResults.NormalCollision;
        }
        else
        {
            if (!(direction.Y > 0)) return DashCollisionResults.Rebound;
            if (CanHitTop)
            {
                Hit(player, Direction.Down);
            }
            else return DashCollisionResults.NormalCollision;
        }
        return DashCollisionResults.Rebound;

    }

    public static void Load()
    {
        On.Monocle.Entity.Render += Entity_Render;
    }

    private static void Entity_Render(On.Monocle.Entity.orig_Render orig, Entity self)
    {
        bool yep = false;
        Vector2 pos = self.Position;
        if ((self.Get<StaticMover>() is { } sm) && sm.Platform is GenericSmwBlock block && block.Bouncetimer > 0)
        {
            yep = true;
            switch (block.Bumpdir)
            {
                default:
                case Direction.Up:
                    self.Position -= Vector2.UnitY * (float)Math.Sin(block.Bouncetimer / 10 * Math.PI) * 6;
                    break;
                case Direction.Right:
                    self.Position += Vector2.UnitX * (float)Math.Sin(block.Bouncetimer / 10 * Math.PI) * 6;
                    break;
                case Direction.Left:
                    self.Position -= Vector2.UnitX * (float)Math.Sin(block.Bouncetimer / 10 * Math.PI) * 6;
                    break;
                case Direction.Down:
                    self.Position += Vector2.UnitY * (float)Math.Sin(block.Bouncetimer / 10 * Math.PI) * 6;
                    break;
            }
        }
        orig(self);
        if (yep)
        {
            self.Position = pos;
        }
    }

    public static void Unload()
    {
        On.Monocle.Entity.Render -= Entity_Render;
    }
}