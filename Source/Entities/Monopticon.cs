using System;
using System.Collections.Generic;
using System.Collections;
using MonoMod.Cil;
using Celeste.Mod.Helpers;

namespace Celeste.Mod.FemtoHelper.Entities;

[TrackedAs(typeof(LookoutBlocker))]
[Tracked(false)]
[CustomEntity("FemtoHelper/MonoBlocker")]
public class MonoBlocker : LookoutBlocker
{
    public string Flag;
    public MonoBlocker(EntityData data, Vector2 offset) : base(data, offset)
    {
        Flag = data.Attr("flag", "");
    }
}

[TrackedAs(typeof(Lookout))]
[Tracked]
[CustomEntity("FemtoHelper/Monopticon")]
public class Monopticon : Lookout
{

    public class UnsociableTalkComponentUi : TalkComponent.TalkComponentUI // fish "thanks" underflow
    {
        public UnsociableTalkComponentUi(TalkComponent handler)
            : base(handler)
        {
        }


        public new void Render()
        {
        }
    }

    public bool BlockJump;
    public bool BlockDash;
    public string Intflag;
    public bool ArbInteract;
    public bool DashInputCancel;

    public bool BlockWallKick;
    public bool WallKickCancel;

    public static bool Grabbing;

    public bool JumpCancelCheck;

    public bool DashCancelCheck;

    public float DashDelayConfig;
    public float PreOpenFrames;
    public float OpenFrames;
    public float CloseFrames;
    public float CooldownFrames;
    public bool CanDashCoroutine;

    private readonly bool strictStateReset;

    private readonly float binoAccel;
    private readonly float binoMaxSpeed;

    public Monopticon(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        Depth = -8500;

        AddTag(Tags.TransitionUpdate);

        summit = false;
        onlyY = data.Bool("onlyY");
        Collider = new Hitbox(4f, 4f, -2f, -4f);
        Add(new MirrorReflection());
        VertexLight vertexLight = new VertexLight(new Vector2(-1f, -11f), Color.White, 0.8f, 16, 24);
        Add(vertexLight);
        lightTween = vertexLight.CreatePulseTween();
        Add(lightTween);
        sprite.OnFrameChange = delegate (string s)
        {
            switch (s)
            {
                case "idle":
                case "badeline_idle":
                case "nobackpack_idle":
                    if (sprite.CurrentAnimationFrame == sprite.CurrentAnimationTotalFrames - 1)
                    {
                        lightTween.Start();
                    }
                    break;
            }
        };
        Vector2[] array = data.NodesOffset(offset);
        if (array != null && array.Length != 0)
        {
            nodes = [.. array];
        }
        BlockJump = data.Bool("blockJump", true);
        BlockDash = data.Bool("blockDash", true);
        BlockWallKick = data.Bool("blockWallKicks", true);
        WallKickCancel = data.Bool("wallkicksCancelBino", true);
        DashInputCancel = data.Bool("dashInputCancel", false);
        Intflag = data.Attr("interactFlag", "lookout_interacting");
        DashDelayConfig = data.Float("dashCancelDelay", 0.15f);
        PreOpenFrames = data.Float("preOpenFrames", 12f);
        OpenFrames = data.Float("openFrames", 20f);
        CloseFrames = data.Float("closeFrames", 20f);
        CooldownFrames = data.Float("cooldownFrames", 6f);
        strictStateReset = data.Bool("strictStateReset", false);

        binoAccel = data.Float("binoAcceleration", 800f);
        binoMaxSpeed = data.Float("binoMaxSpeed", 240f);
    }

    public static void Load()
    {
        On.Celeste.Lookout.Interact += MonopticonInteractHook;
        On.Celeste.Player.Jump += Player_Jump;
        On.Celeste.Player.SuperJump += Player_SuperJump;
        On.Celeste.Player.DashBegin += Player_DashBegin;
        On.Celeste.Player.WallJump += Player_WallJump;
        On.Celeste.Player.ClimbJump += Player_ClimbJump;
        On.Celeste.Actor.MoveHExact += Actor_MoveHExact;
        On.Celeste.Actor.MoveVExact += Actor_MoveVExact;
        IL.Celeste.Player.ClimbUpdate += Player_ClimbUpdate;
    }

    private static void Player_ClimbUpdate(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.TryGotoNextBestFit(MoveType.After, 
            (instr) => instr.MatchLdsfld(out var reference) && reference?.Name == "Jump",
            (instr) => instr.MatchCallOrCallvirt(out var reference) && reference?.Name == "get_Pressed"
            );
        cursor.EmitLdarg0();
        cursor.EmitDelegate(ModClimbJumpCheck);
        cursor.EmitAnd();
    }

    public static bool ModClimbJumpCheck(Player player)
    {
        Monopticon mono = player.Scene.Tracker.GetEntity<Monopticon>();
        if (mono != null)
        {
            if (!mono.ArbInteract && mono.BlockJump)
            {
                return false;
            }
        }
        return true;
    }

    private static bool Actor_MoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher)
    {
        bool ret = orig(self, moveH, onCollide, pusher);
        if (self is Player player)
        {
            foreach(Entity e in player.Scene.Tracker.GetEntities<Monopticon>())
            {
                e.Position = player.Position;
            }
        }
        return ret;
    }
    private static bool Actor_MoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher)
    {
        bool ret = orig(self, moveV, onCollide, pusher);
        if (self is Player player)
        {
            foreach (Entity e in player.Scene.Tracker.GetEntities<Monopticon>())
            {
                e.Position = player.Position;
            }
        }
        return ret;
    }

    public static void Unload()
    {
        On.Celeste.Lookout.Interact -= MonopticonInteractHook;
        On.Celeste.Player.Jump -= Player_Jump;
        On.Celeste.Player.SuperJump -= Player_SuperJump;
        On.Celeste.Player.DashBegin -= Player_DashBegin;
        On.Celeste.Player.WallJump -= Player_WallJump;
        On.Celeste.Player.ClimbJump -= Player_ClimbJump;
        On.Celeste.Actor.MoveHExact -= Actor_MoveHExact;
        On.Celeste.Actor.MoveVExact -= Actor_MoveVExact;
        IL.Celeste.Player.ClimbUpdate -= Player_ClimbUpdate;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
    }

    public static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        Monopticon mono = self.Scene.Tracker.GetEntity<Monopticon>();
        if (mono != null)
        {
            mono.Add(new Coroutine(mono.DashCheckDelay(mono, self)));
        }

        orig(self);

    }

    public IEnumerator DashCheckDelay(Monopticon mono, Player player)
    {
        if (!mono.CanDashCoroutine) yield break;
        yield return mono.DashDelayConfig / 60f;
        DashCancelCheck = true;
    }

    public static void Player_Jump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
    {
        Monopticon mono = self.Scene.Tracker.GetEntity<Monopticon>();
        Grabbing = self.CollideCheck<Solid>(self.Position + Vector2.UnitX * (float)self.Facing) && Input.Grab.Check;
        if (mono != null)
        {
            if (mono.ArbInteract || Grabbing || !mono.BlockJump)
            {
                orig(self, particles, playSfx);
                mono.JumpCancelCheck = true;
            }
            else
            {
                return;
            }
        }
        else
        {
            orig(self, particles, playSfx);
        }
    }

    public static void Player_SuperJump(On.Celeste.Player.orig_SuperJump orig, Player self)
    {
        Monopticon mono = self.Scene.Tracker.GetEntity<Monopticon>();
        if (mono != null)
        {
            if (mono.ArbInteract || !mono.BlockJump)
            {
                orig(self);
                mono.JumpCancelCheck = true;
            }
            else
            {
                return;
            }
        }
        else
        {
            orig(self);
        }
    }

    public static void Player_WallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
    {
        Monopticon mono = self.Scene.Tracker.GetEntity<Monopticon>();
        if (mono != null)
        {
            if (mono.ArbInteract || !mono.BlockJump)
            {
                orig(self, dir);
                mono.JumpCancelCheck = true;
            }
            else
            {
                return;
            }
        }
        else
        {
            orig(self, dir);
        }
    }

    public static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
    {
        Monopticon mono = self.Scene.Tracker.GetEntity<Monopticon>();
        if (mono != null)
        {
            if (mono.ArbInteract || !mono.BlockJump)
            {
                orig(self);
                mono.JumpCancelCheck = true;
            }
            else
            {
                return;
            }
        }
        else
        {
            orig(self);
        }
    }

    public static void MonopticonInteractHook(On.Celeste.Lookout.orig_Interact orig, Lookout self, Player player)
    {
        Monopticon mono = self as Monopticon;
        if (mono == null)
        {
            orig(self, player);
            return;
        }
        if (!mono.ArbInteract)
        {
            if (player.Holding == null)
            {
                if (player.DefaultSpriteMode == PlayerSpriteMode.MadelineAsBadeline || SaveData.Instance.Assists.PlayAsBadeline)
                {
                    mono.animPrefix = "badeline_";
                }
                else if (player.DefaultSpriteMode == PlayerSpriteMode.MadelineNoBackpack)
                {
                    mono.animPrefix = "nobackpack_";
                }
                else
                {
                    mono.animPrefix = "";
                }
                Coroutine coroutine = new Coroutine(mono.LookRoutine2(player));
                coroutine.RemoveOnComplete = true;
                mono.Add(coroutine);
                mono.interacting = true;
                mono.talk.cooldown = mono.CooldownFrames / 60f;
            }
        }

    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        Add(new TransitionListener()
        {
            OnInBegin = () =>
            {
                (scene as Level).Session.SetFlag("binoState", false);
                (Scene as Level).Session.SetFlag(Intflag, false);
            }
        });
        talk.UI = new UnsociableTalkComponentUi(talk);
        (scene as Level).Session.SetFlag("binoState", false);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if (interacting)
        {
            Player entity = scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                entity.StateMachine.State = 0;
                entity.Sprite.Visible = (entity.Hair.Visible = true);
            }
        }
    }

    public override void Update()
    {
        //base.Update(); // fuck you !!!!!!!
        Components.Update();

        Player player = Scene.Tracker.GetEntity<Player>();

        if (FemtoModule.GravityHelperSupport.GetPlayerGravity?.Invoke() == 1)
        {
            talk.Bounds.Y = 0;
            sprite.Scale.Y = -1;

        }
        else
        {
            talk.Bounds.Y = -8;
            sprite.Scale.Y = 1;
        }

        if (!ArbInteract)
        {
            if (player != null)
            {
                if (!ArbInteract && BlockDash)
                {
                    player.dashCooldownTimer = Engine.DeltaTime + 1E-06f;
                }

            }
        }
        else
        {

        }
        Position = player?.Position ?? Position;

        if (Input.Talk.Pressed)
        {
            if (player != null && (interacting || !strictStateReset))
            {
                player.StateMachine.State = Player.StNormal;
            }
        }
        //Player entity = base.Scene.Tracker.GetEntity<Player>();
        //if (entity != null)
        //{
        //	sprite.Active = interacting || entity.StateMachine.State != 11;
        //	if (!sprite.Active)
        //	{
        //		sprite.SetAnimationFrame(0);
        //	}
        //}
        //if (talk != null && CollideCheck<Solid>())
        //{
        //	Remove(talk);
        //	talk = null;
        //}
    }

    private IEnumerator LookRoutine2(Player player)
    {
        Level level = SceneAs<Level>();
        SandwichLava sandwichLava = Scene.Entities.FindFirst<SandwichLava>();
        if (sandwichLava != null)
        {
            sandwichLava.Waiting = true;
        }
        if (player.Holding != null)
        {
            player.Drop();
        }
        player.StateMachine.State = 11;
        //yield return player.DummyWalkToExact((int)base.X, walkBackwards: false, 1f, cancelOnFall: true);
        if (player.Dead || !player.OnGround())
        {
            if (!player.Dead)
            {
                player.StateMachine.State = 0;
            }
            yield break;
        }
        (Scene as Level).Session.SetFlag(Intflag, true);
        ArbInteract = true;

        Audio.Play("event:/game/general/lookout_use", Position);
        if (player.Facing == Facings.Right)
        {
            sprite.Play(animPrefix + "lookRight");
        }
        else
        {
            sprite.Play(animPrefix + "lookLeft");
        }
        player.Sprite.Visible = (player.Hair.Visible = false);
        yield return PreOpenFrames / 60;

        nodePercent = 0f;
        node = 0;
        Audio.Play("event:/ui/game/lookout_on");
        yield return OpenFrames / 60f;
        JumpCancelCheck = false;
        DashCancelCheck = false;
        CanDashCoroutine = true;
        Vector2 cam = level.Camera.Position;
        Vector2 speed = Vector2.Zero;
        Vector2 lastDir = Vector2.Zero;
        Vector2 camStart = level.Camera.Position;
        Vector2 camStartCenter = camStart + new Vector2(160f, 90f);
        while (!(Input.MenuCancel || (DashInputCancel && Input.Dash)) && !DashCancelCheck && !JumpCancelCheck && interacting && !player.Dead)
        {

            Vector2 value = Input.Aim.Value;
            if (onlyY)
            {
                value.X = 0f;
            }
            if (Math.Sign(value.X) != Math.Sign(lastDir.X) || Math.Sign(value.Y) != Math.Sign(lastDir.Y))
            {
                Audio.Play("event:/game/general/lookout_move", Position);
            }
            lastDir = value;
            if (sprite.CurrentAnimationID != "lookLeft" && sprite.CurrentAnimationID != "lookRight")
            {
                if (value.X == 0f)
                {
                    if (value.Y == 0f)
                    {
                        sprite.Play(animPrefix + "looking");
                    }
                    else if (value.Y > 0f)
                    {
                        sprite.Play(animPrefix + "lookingDown");
                    }
                    else
                    {
                        sprite.Play(animPrefix + "lookingUp");
                    }
                }
                else if (value.X > 0f)
                {
                    if (value.Y == 0f)
                    {
                        sprite.Play(animPrefix + "lookingRight");
                    }
                    else if (value.Y > 0f)
                    {
                        sprite.Play(animPrefix + "lookingDownRight");
                    }
                    else
                    {
                        sprite.Play(animPrefix + "lookingUpRight");
                    }
                }
                else if (value.X < 0f)
                {
                    if (value.Y == 0f)
                    {
                        sprite.Play(animPrefix + "lookingLeft");
                    }
                    else if (value.Y > 0f)
                    {
                        sprite.Play(animPrefix + "lookingDownLeft");
                    }
                    else
                    {
                        sprite.Play(animPrefix + "lookingUpLeft");
                    }
                }
            }
            if (nodes == null)
            {
                speed += binoAccel * value * Engine.DeltaTime;
                if (value.X == 0f)
                {
                    speed.X = Calc.Approach(speed.X, 0f, binoAccel * 2f * Engine.DeltaTime);
                }
                if (value.Y == 0f)
                {
                    speed.Y = Calc.Approach(speed.Y, 0f, binoAccel * 2f * Engine.DeltaTime);
                }
                if (speed.Length() > binoMaxSpeed)
                {
                    speed = speed.SafeNormalize(binoMaxSpeed);
                }
                Vector2 vector = cam;
                List<Entity> entities = Scene.Tracker.GetEntities<LookoutBlocker>();
                cam.X += speed.X * Engine.DeltaTime;
                if (cam.X < (float)level.Bounds.Left || cam.X + 320f > (float)level.Bounds.Right)
                {
                    speed.X = 0f;
                }
                cam.X = Calc.Clamp(cam.X, level.Bounds.Left, level.Bounds.Right - 320);
                foreach (Entity item in entities)
                {
                    bool on = true;
                    if (item is MonoBlocker)
                    {
                        var flag = (item as MonoBlocker).Flag;
                        on = string.IsNullOrEmpty(flag) || (Scene as Level).Session.GetFlag(flag);
                    }
                    if (on && cam.X + 320f > item.Left && cam.Y + 180f > item.Top && cam.X < item.Right && cam.Y < item.Bottom)
                    {
                        cam.X = vector.X;
                        speed.X = 0f;
                    }
                }
                cam.Y += speed.Y * Engine.DeltaTime;
                if (cam.Y < (float)level.Bounds.Top || cam.Y + 180f > (float)level.Bounds.Bottom)
                {
                    speed.Y = 0f;
                }
                cam.Y = Calc.Clamp(cam.Y, level.Bounds.Top, level.Bounds.Bottom - 180);
                foreach (Entity item2 in entities)
                {
                    bool on = true;
                    if (item2 is MonoBlocker)
                    {
                        var flag = (item2 as MonoBlocker).Flag;
                        on = string.IsNullOrEmpty(flag) || (Scene as Level).Session.GetFlag(flag);
                    }
                    if (on && cam.X + 320f > item2.Left && cam.Y + 180f > item2.Top && cam.X < item2.Right && cam.Y < item2.Bottom)
                    {
                        cam.Y = vector.Y;
                        speed.Y = 0f;
                    }
                }
                level.Camera.Position = cam;
            }
            else
            {
                Vector2 vector2 = ((node <= 0) ? camStartCenter : nodes[node - 1]);
                Vector2 vector3 = nodes[node];
                float num = (vector2 - vector3).Length();
                (vector3 - vector2).SafeNormalize();
                if (nodePercent < 0.25f && node > 0)
                {
                    Vector2 begin = Vector2.Lerp((node <= 1) ? camStartCenter : nodes[node - 2], vector2, 0.75f);
                    Vector2 end = Vector2.Lerp(vector2, vector3, 0.25f);
                    SimpleCurve simpleCurve = new SimpleCurve(begin, end, vector2);
                    level.Camera.Position = simpleCurve.GetPoint(0.5f + nodePercent / 0.25f * 0.5f);
                }
                else if (nodePercent > 0.75f && node < nodes.Count - 1)
                {
                    Vector2 value2 = nodes[node + 1];
                    Vector2 begin2 = Vector2.Lerp(vector2, vector3, 0.75f);
                    Vector2 end2 = Vector2.Lerp(vector3, value2, 0.25f);
                    SimpleCurve simpleCurve2 = new SimpleCurve(begin2, end2, vector3);
                    level.Camera.Position = simpleCurve2.GetPoint((nodePercent - 0.75f) / 0.25f * 0.5f);
                }
                else
                {
                    level.Camera.Position = Vector2.Lerp(vector2, vector3, nodePercent);
                }
                level.Camera.Position += new Vector2(-160f, -90f);
                nodePercent -= value.Y * (binoMaxSpeed / num) * Engine.DeltaTime;
                if (nodePercent < 0f)
                {
                    if (node > 0)
                    {
                        node--;
                        nodePercent = 1f;
                    }
                    else
                    {
                        nodePercent = 0f;
                    }
                }
                else if (nodePercent > 1f)
                {
                    if (node < nodes.Count - 1)
                    {
                        node++;
                        nodePercent = 0f;
                    }
                    else
                    {
                        nodePercent = 1f;
                        if (summit)
                        {
                            break;
                        }
                    }
                }
                float num2 = 0f;
                float num3 = 0f;
                for (int i = 0; i < nodes.Count; i++)
                {
                    float num4 = (((i == 0) ? camStartCenter : nodes[i - 1]) - nodes[i]).Length();
                    num3 += num4;
                    if (i < node)
                    {
                        num2 += num4;
                    }
                    else if (i == node)
                    {
                        num2 += num4 * nodePercent;
                    }
                }
            }
            yield return null;
        }
        JumpCancelCheck = false;
        DashCancelCheck = false;
        player.Sprite.Visible = (player.Hair.Visible = true);
        sprite.Play(animPrefix + "idle");
        Audio.Play("event:/ui/game/lookout_off");

        (Scene as Level).Session.SetFlag(Intflag, false);
        ArbInteract = false;
        interacting = false;
        yield return CloseFrames / 60f;
        CanDashCoroutine = false;
        if (DashInputCancel)
        {
            player.StateMachine.State = 0;
        }

        bool atSummitTop = summit && node >= nodes.Count - 1 && nodePercent >= 0.95f;
        if (atSummitTop)
        {
            yield return 0.5f;
            float duration2 = 3f;
            float approach2 = 0f;
            Coroutine component = new Coroutine(level.ZoomTo(new Vector2(160f, 90f), 2f, duration2));
            Add(component);
            while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed && !Input.Dash.Pressed && !(Input.Jump.Pressed || player.onGround) && interacting)
            {
                approach2 = Calc.Approach(approach2, 1f, Engine.DeltaTime / duration2);
                Audio.SetMusicParam("escape", approach2);
                yield return null;
            }
        }
        if ((camStart - level.Camera.Position).Length() > 600f)
        {
            Vector2 was = level.Camera.Position;
            Vector2 direction = (was - camStart).SafeNormalize();
            float approach2 = (atSummitTop ? 1f : 0.5f);
            new FadeWipe(Scene, wipeIn: false).Duration = approach2;
            for (float duration2 = 0f; duration2 < 1f; duration2 += Engine.DeltaTime / approach2)
            {
                level.Camera.Position = was - direction * MathHelper.Lerp(0f, 64f, Ease.CubeIn(duration2));
                yield return null;
            }
            level.Camera.Position = camStart + direction * 32f;
            new FadeWipe(Scene, wipeIn: true);
        }
        Audio.SetMusicParam("escape", 0f);
        level.ScreenPadding = 0f;
        level.ZoomSnap(Vector2.Zero, 1f);

        yield return null;
    }
}
