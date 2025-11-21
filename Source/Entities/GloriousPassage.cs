using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using Celeste.Mod.FemtoHelper.Utils;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/GloriousPassage")]
public class GloriousPassage : Entity
{
    public readonly string Flag;
    public int Lastinput;
    public readonly string RoomName;
    public bool PlayerInside;
    public Player Player;
    public readonly string Audio;
    public readonly MTexture Closed;
    public readonly MTexture Open;
    public readonly bool Simple;
    public bool Done;
    public readonly bool FaceLeft;
    public readonly int SpawnIndex;
    public readonly bool InteractToOpen;
    public readonly bool KeepDashes;
    public readonly bool SameRoom;
    public bool CarryHoldablesOver;
    public readonly TalkComponent Talk;
    private TimeRateModifier timeRateModifier;
    private readonly string enableFlag;
    private readonly string visibilityFlag;
    public GloriousPassage(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);
        Add(new PlayerCollider(OnPlayer, Collider));

        Depth = data.Int("depth", 120);
        Flag = data.Attr("flag", "door_check");
        RoomName = data.Attr("roomName", "").Trim();
        Audio = data.Attr("audio", "event:/FemtoHelper/smw_door_opens");
        Closed = GFX.Game[data.Attr("closedPath", "objects/FemtoHelper/SMWDoor/closed")];
        Open = GFX.Game[data.Attr("openPath", "objects/FemtoHelper/SMWDoor/open")];
        Simple = data.Bool("simpleTrigger", false);
        FaceLeft = data.Bool("faceLeft", false);
        SpawnIndex = data.Int("spawnpointIndex");
        InteractToOpen = !data.Bool("pressUpToOpen", false);
        KeepDashes = data.Bool("keepDashes", false);
        SameRoom = data.Bool("sameRoom", false);
        CarryHoldablesOver = data.Bool("carryHoldablesOver", false);
        enableFlag = data.String("enableFlag", "");
        visibilityFlag = data.String("visibilityFlag", "");
        if (!InteractToOpen) return;
        Add(Talk = new TalkComponent(new Rectangle(0, 0, (int)Collider.Width, (int)Collider.Height), new Vector2(Width / 2, -8), OnTalk));
        Talk.PlayerMustBeFacing = false;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        (Scene as Level).Session.SetFlag(Flag, false);
        Add(timeRateModifier = new TimeRateModifier(1f));
    }

    public void OnTalk(Player player)
    {
        if (!Done)
        {
            Add(new Coroutine(Routine(player)));
        }
    }

    public void OnPlayer(Player player)
    {
        if (Simple && !Done && Util.EvaluateExpressionAsBoolOrFancyFlag(enableFlag, SceneAs<Level>().Session))
        {
            Add(new Coroutine(Routine(player)));
            return;
        }
        PlayerInside = true;
        Player = player;
    }

    public override void Update()
    {
        if (!Util.EvaluateExpressionAsBoolOrFancyFlag(enableFlag, SceneAs<Level>().Session))
        {
            if(Talk != null && Talk.UI != null)
            {
                Talk.UI.Visible = false;
            }
            PlayerInside = false;
            return;
        } else {
            if (Talk != null && Talk.UI != null)
            {
                Talk.UI.Visible = true;
            }
        }
        base.Update();
        if (PlayerInside && !Done && !InteractToOpen && Player != null && Player.OnGround() && Input.MoveY.Value == -1 && Lastinput != -1)
        {
            Add(new Coroutine(Routine(Player)));
        }
        Lastinput = Input.MoveY.Value;
        PlayerInside = false;
    }

    public IEnumerator Routine(Player player)
    {
        if (!string.IsNullOrEmpty(Flag)) (Scene as Level).Session.SetFlag(Flag, true);
        if (!string.IsNullOrEmpty(Audio)) global::Celeste.Audio.Play(Audio);
        Done = true;
        Level level = Scene as Level;
        player.StateMachine.state = Player.StDummy;

        for (float i = 0; i < 1; i += Engine.RawDeltaTime * 4)
        {
            if (timeRateModifier is not null) timeRateModifier.Multiplier = Calc.Approach(timeRateModifier.Multiplier, 0, Engine.RawDeltaTime * 4);
            yield return null;
        }
        level.DoScreenWipe(wipeIn: false, new Action(Tp));

        yield return null;
    }

    public void Tp()
    {
        if (Player == null || Scene == null) return;
        if (timeRateModifier is not null) timeRateModifier.Multiplier = 1;
        Level level = Scene as Level;
        level.Session.SetFlag("transition_assist", false);
        Player player = Scene.Tracker.GetEntity<Player>();
        level.OnEndOfFrame += () =>
        {
            Vector2 pos, playerDelta;
            Leader leader;

            if (level.Session.Level == RoomName && SameRoom)
            {
                level.Session.RespawnPoint = level.Session.LevelData.Spawns[Calc.Clamp(SpawnIndex, 0, level.Session.LevelData.Spawns.Count - 1)];

                pos = player.Position;

                player.Position = level.Session.RespawnPoint.Value;

                playerDelta = player.Position - pos;

                player.Hair.MoveHairBy(playerDelta);

                if (!KeepDashes) player.Dashes = player.MaxDashes;

                leader = player.Get<Leader>();

                foreach (Follower item2 in leader.Followers.Where(f => f.Entity != null))
                {
                    item2.Entity.Position += playerDelta;
                }

                for (int i = 0; i < leader.PastPoints.Count; i++)
                {
                    leader.PastPoints[i] += playerDelta;
                }

                if (player.Holding != null)
                {
                    player.Holding.Entity.Position += playerDelta;
                }

                level.Session.SetFlag("transition_assist", true);
                player.Speed = Vector2.Zero;
                level.DoScreenWipe(wipeIn: true);
                level.Add(new DelayedCameraRequest(player, false));

                Done = false;
                return;
            }

            //what is this?
            //new Vector2(level.LevelOffset.X + level.Bounds.Width - player.X, player.Y - level.LevelOffset.Y);
            Vector2 levelOffset = level.LevelOffset;
            Vector2 vector2 = level.Camera.Position - level.LevelOffset;
            Facings facing = FaceLeft ? Facings.Left : Facings.Right;
            pos = player.Position;

            leader = player.Get<Leader>();
            foreach (Follower item in leader.Followers.Where(f => f.Entity != null))
            {
                item.Entity.AddTag(Tags.Global);
                level.Session.DoNotLoad.Add(item.ParentEntityID);
            }

            Holdable hold = player.Holding;

            hold?.Entity.AddTag(Tags.Global);

            Vector2 cameraDelta = level.Camera.Position - pos;
            int dashes = player.Dashes;
            level.Remove(player);
            level.UnloadLevel();
            bool error = true;
            LevelData newLevelData = level.Session.MapData.Get(RoomName);
            if (newLevelData != null)
            {
                level.Session.Level = RoomName;
                error = false;
            }
            else
            {
                foreach (var d in level.Session.MapData.Levels.Where(d => d.Name.Trim() == RoomName))
                {
                    level.Session.Level = d.Name;
                    error = false;
                }
            }


            level.Session.RespawnPoint = level.Session.LevelData.Spawns[Calc.Clamp(SpawnIndex, 0, level.Session.LevelData.Spawns.Count - 1)];

            level.Session.FirstLevel = false;
            level.Add(player);
            level.LoadLevel(Player.IntroTypes.Transition);


            player.Position = level.Session.RespawnPoint.Value;

            playerDelta = player.Position - pos;
            level.Camera.Position = player.Position + cameraDelta;
            if (level.Camera.Position.X < level.Bounds.Left) level.Camera.Position = new Vector2(level.Bounds.Left, level.Camera.Position.Y);
            if (level.Camera.Position.Y < level.Bounds.Top) level.Camera.Position = new Vector2(level.Camera.Position.X, level.Bounds.Top);
            if (level.Camera.Position.X + 320 > level.Bounds.Right) level.Camera.Position = new Vector2(level.Bounds.Right, level.Camera.Position.Y);
            if (level.Camera.Position.Y + 180 > level.Bounds.Bottom) level.Camera.Position = new Vector2(level.Camera.Position.X, level.Bounds.Bottom);

            player.Facing = facing;
            player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
            /*
                if (level.Wipe != null)
                {
                    level.Wipe.Cancel();
                }
                */
            player.Visible = true;
            player.Sprite.Visible = true;
            if (KeepDashes) player.Dashes = dashes;

            if (hold != null)
            {
                hold.Entity.Position += playerDelta;
                hold.Entity.RemoveTag(Tags.Global);
            }

            foreach (Follower item2 in leader.Followers.Where(f => f.Entity != null))
            {
                item2.Entity.Position += playerDelta;
                item2.Entity.RemoveTag(Tags.Global);
                level.Session.DoNotLoad.Remove(item2.ParentEntityID);
            }
            for (int i = 0; i < leader.PastPoints.Count; i++)
            {
                leader.PastPoints[i] += playerDelta;
            }
            leader.TransferFollowers();
            level.Session.SetFlag("transition_assist", true);
            player.Speed = Vector2.Zero;
            level.DoScreenWipe(wipeIn: true);
            level.Add(new DelayedCameraRequest(player, error));
        };
    }

    public override void Render()
    {
        if (!Util.EvaluateExpressionAsBoolOrFancyFlag(visibilityFlag, SceneAs<Level>().Session)) return;
        base.Render();
        if (Simple) return;
        if (Done)
        {
            Open.DrawCentered(Center);
        }
        else
        {
            Closed.DrawCentered(Center);
        }
    }
}
public class DelayedCameraRequest(Player player, bool error) : Entity(Vector2.Zero)
{
    public readonly Player Player = player;
    public readonly bool Error = error;

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        (Scene as Level)?.DoScreenWipe(wipeIn: true);
        (Scene as Level).Camera.Position = Player.CameraTarget;
        Add(new Coroutine(TasHelperCameraPatch()));
        Player.StateMachine.State = 0;
        if (Error)
        {
            Scene.Add(new MiniTextbox("FEMTOHELPER_ERRORHANDLER_INVALID_ROOM"));
        }
    }

    public IEnumerator TasHelperCameraPatch()
    {
        (Scene as Level).Camera.Position = Player.CameraTarget;
        yield return null;
        (Scene as Level).Camera.Position = Player.CameraTarget;
        RemoveSelf();
    }

    public override void Render()
    {
        base.Render();
    }
}
