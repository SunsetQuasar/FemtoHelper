using Celeste.Mod.Entities;
using IL.MonoMod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public GloriousPassage(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);
        Add(new PlayerCollider(onPlayer, Collider));
        Depth = 120;

        flag = data.Attr("flag", "door_check");
        roomName = data.Attr("roomName", "").Trim();
        audio = data.Attr("audio", "event:/paeceful_sibs_chamber/smw_door_opens");
        closed = GFX.Game[data.Attr("closedPath", "objects/ss2024/gloriousPassage/closed")];
        open = GFX.Game[data.Attr("openPath", "objects/ss2024/gloriousPassage/open")];
        simple = data.Bool("simpleTrigger", false);
        faceLeft = data.Bool("faceLeft", false);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        (Scene as Level).Session.SetFlag(flag, false);
    }

    public void onPlayer(Player player)
    {
        if (simple && !done)
        {
            (Scene as Level).Session.SetFlag(flag, true);
            Add(new Coroutine(Routine(player)));
            done = true;
            return;
        }
        yeahforsure = true;
        this.player = player;
    }

    public override void Update()
    {
        base.Update();
        if (yeahforsure && !done)
        {
            if (player.OnGround())
            {
                if (Input.MoveY.Value == -1 && lastinput != -1)
                {
                    (Scene as Level).Session.SetFlag(flag, true);
                    Audio.Play(audio);
                    Add(new Coroutine(Routine(player)));
                    done = true;
                }
            }
        }
        lastinput = Input.MoveY.Value;
        yeahforsure = false;
    }

    public IEnumerator Routine(Player player)
    {

        Level level = Scene as Level;
        player.StateMachine.state = 11;

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
            Player player = Scene.Tracker.GetEntity<Player>();
            level.OnEndOfFrame += delegate
            {
                new Vector2(level.LevelOffset.X + (float)level.Bounds.Width - player.X, player.Y - level.LevelOffset.Y);
                Vector2 levelOffset = level.LevelOffset;
                Vector2 vector2 = level.Camera.Position - level.LevelOffset;
                Facings facing = faceLeft ? Facings.Left : Facings.Right;
                Vector2 pos = player.Position;

                Leader leader = player.Get<Leader>();
                foreach (Follower item in leader.Followers.Where((Follower f) => f.Entity != null))
                {
                    item.Entity.AddTag(Tags.Global);
                    level.Session.DoNotLoad.Add(item.ParentEntityID);
                }

                Vector2 cameraDelta = level.Camera.Position - pos;
                level.Remove(player);
                level.UnloadLevel();
                if (level.Session.MapData.Get(roomName) != null)
                {
                    level.Session.Level = roomName;
                }
                else
                {
                    foreach (LevelData d in level.Session.MapData.Levels)
                    {
                        if (d.Name.Trim() == roomName) level.Session.Level = d.Name;
                    }
                }



                level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));

                level.Session.FirstLevel = false;
                level.Add(player);
                level.LoadLevel(Player.IntroTypes.Transition);



                player.Position = level.Session.RespawnPoint.Value;

                Vector2 playerDelta = player.Position - pos;
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
