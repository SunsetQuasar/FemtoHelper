using Celeste.Mod.Entities;
using IL.MonoMod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/GloriousPassage")]
public class GloriousPassage : Entity
{
    public string flag;
    public int lastinput;
    public string roomName;
    public bool yeahforsure;
    public Player player;
    public string audio;
    public MTexture closed;
    public MTexture open;
    public bool simple;
    public bool done;
    public bool faceLeft;
    public int spawnIndex;
    public bool interactToOpen;
    public bool keepDashes;
    public bool SameRoom;
    public bool CarryHoldablesOver;
    public TalkComponent talk;
    public GloriousPassage(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);
        Add(new PlayerCollider(onPlayer, Collider));

        Depth = data.Int("depth", 120);
        flag = data.Attr("flag", "door_check");
        roomName = data.Attr("roomName", "").Trim();
        audio = data.Attr("audio", "event:/FemtoHelper/smw_door_opens");
        closed = GFX.Game[data.Attr("closedPath", "objects/FemtoHelper/SMWDoor/closed")];
        open = GFX.Game[data.Attr("openPath", "objects/FemtoHelper/SMWDoor/open")];
        simple = data.Bool("simpleTrigger", false);
        faceLeft = data.Bool("faceLeft", false);
        spawnIndex = data.Int("spawnpointIndex");
        interactToOpen = !data.Bool("pressUpToOpen", false);
        keepDashes = data.Bool("keepDashes", false);
        SameRoom = data.Bool("sameRoom", true);
        CarryHoldablesOver = data.Bool("carryHoldablesOver", true);
        if (interactToOpen)
        {
            Add(talk = new TalkComponent(new Rectangle(0, 0, (int)Collider.Width, (int)Collider.Height), new Vector2(Width / 2, -8), onTalk));
            talk.PlayerMustBeFacing = false;
        }
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        (Scene as Level).Session.SetFlag(flag, false);
    }

    public void onTalk(Player player)
    {
        if (!done)
        {
            Add(new Coroutine(Routine(player)));
        }
    }

    public void onPlayer(Player player)
    {
        if (simple && !done)
        {
            Add(new Coroutine(Routine(player)));
            return;
        }
        yeahforsure = true;
        this.player = player;
    }

    public override void Update()
    {
        base.Update();
        if (yeahforsure && !done && !interactToOpen && player != null && player.OnGround() && Input.MoveY.Value == -1 && lastinput != -1)
        {
            Add(new Coroutine(Routine(player)));
        }
        lastinput = Input.MoveY.Value;
        yeahforsure = false;
    }

    public IEnumerator Routine(Player player)
    {
        if(!string.IsNullOrEmpty(flag))(Scene as Level).Session.SetFlag(flag, true);
        if (!string.IsNullOrEmpty(audio)) Audio.Play(audio);
        done = true;
        Level level = Scene as Level;
        player.StateMachine.state = Player.StDummy;

        for (float i = 0; i < 1; i += Engine.RawDeltaTime * 4)
        {
            Engine.TimeRate = Calc.Approach(Engine.TimeRate, 0, Engine.RawDeltaTime * 4);
            yield return null;
        }
        level.DoScreenWipe(wipeIn: false, new Action(tp));

        yield return null;
    }

    public void tp()
    {
        if (player != null && Scene != null)
        {
            Engine.TimeRate = 1;
            Level level = Scene as Level;
            level.Session.SetFlag("transition_assist", false);
            Player player = Scene.Tracker.GetEntity<Player>();
            level.OnEndOfFrame += () =>
            {
                Vector2 pos, playerDelta;
                Leader leader;

                if(level.Session.Level == roomName && SameRoom)
                {
                    level.Session.RespawnPoint = level.Session.LevelData.Spawns[Calc.Clamp(spawnIndex, 0, level.Session.LevelData.Spawns.Count - 1)];

                    pos = player.Position;

                    player.Position = level.Session.RespawnPoint.Value;

                    playerDelta = player.Position - pos;

                    player.Hair.MoveHairBy(playerDelta);

                    if (!keepDashes) player.Dashes = player.MaxDashes;

                    leader = player.Get<Leader>();

                    foreach (Follower item2 in leader.Followers.Where((Follower f) => f.Entity != null))
                    {
                        item2.Entity.Position += playerDelta;
                    }

                    for (int i = 0; i < leader.PastPoints.Count; i++)
                    {
                        leader.PastPoints[i] += playerDelta;
                    }

                    if(player.Holding != null)
                    {
                        player.Holding.Entity.Position += playerDelta;
                    }

                    level.Session.SetFlag("transition_assist", true);
                    player.Speed = Vector2.Zero;
                    level.DoScreenWipe(wipeIn: true);
                    level.Add(new DelayedCameraRequest(player));

                    done = false;
                    return;
                }

                new Vector2(level.LevelOffset.X + (float)level.Bounds.Width - player.X, player.Y - level.LevelOffset.Y);
                Vector2 levelOffset = level.LevelOffset;
                Vector2 vector2 = level.Camera.Position - level.LevelOffset;
                Facings facing = faceLeft ? Facings.Left : Facings.Right;
                pos = player.Position;

                leader = player.Get<Leader>();
                foreach (Follower item in leader.Followers.Where((Follower f) => f.Entity != null))
                {
                    item.Entity.AddTag(Tags.Global);
                    level.Session.DoNotLoad.Add(item.ParentEntityID);
                }

                Holdable hold = player.Holding;

                if (hold != null)
                {
                    hold.Entity.AddTag(Tags.Global);
                }

                Vector2 cameraDelta = level.Camera.Position - pos;
                int dashes = player.Dashes;
                level.Remove(player);
                level.UnloadLevel();
                LevelData newLevelData = level.Session.MapData.Get(roomName);
                if (newLevelData != null)
                {
                    level.Session.Level = roomName;
                }
                else
                {
                    foreach (LevelData d in level.Session.MapData.Levels)
                    {
                        if (d.Name.Trim() == roomName) {
                            level.Session.Level = d.Name;
                        }
                    }
                }


                level.Session.RespawnPoint = level.Session.LevelData.Spawns[Calc.Clamp(spawnIndex, 0, level.Session.LevelData.Spawns.Count - 1)];

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
                if(keepDashes) player.Dashes = dashes;

                if(hold != null)
                {
                    hold.Entity.Position += playerDelta;
                    hold.Entity.RemoveTag(Tags.Global);
                }

                foreach (Follower item2 in leader.Followers.Where((Follower f) => f.Entity != null))
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
                level.Add(new DelayedCameraRequest(player));
            };
        }
    }

    public override void Render()
    {
        base.Render();
        if (simple) return;
        if (done)
        {
            open.DrawCentered(Center);
        }
        else
        {
            closed.DrawCentered(Center);
        }
    }
}
