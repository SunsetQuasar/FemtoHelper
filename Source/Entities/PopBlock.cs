using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.FemtoHelper.Entities.EntityKillZone;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/PopBlock")]
public class PopBlock : Solid
{
    public class Side : Entity
    {
        private readonly bool edgeLeft;
        private readonly bool edgeRight;
        private readonly PopBlock parent;
        public Side(PopBlock parent) : base()
        {
            Depth = 7120;
            this.parent = parent;
            Position = parent.BottomLeft;
            edgeLeft = !parent.CheckForSame(parent.Left - 8f, parent.Bottom - 8f);
            edgeRight = !parent.CheckForSame(parent.Right, parent.Bottom - 8f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override void Render()
        {
            base.Render();
            float height = Calc.Min(8, parent.Height);
            Draw.Rect(Position + new Vector2(0f, 2f - height), parent.Width, height, parent.disabledColor2);
            Draw.Rect(Position + new Vector2(edgeLeft ? 1f : 0f, 2f - height), parent.Width - (edgeLeft ? 1f : 0f) - (edgeRight ? 1f : 0f), height, parent.disabledColor);
        }
    }

    [Tracked]
    public class PopBlockManager : Entity
    {
        Queue<PopBlock> popQueue = [];
        List<PopBlock> soundList = [];
        public PopBlockManager() : base()
        {
            //right before player
            Depth = -1;
            AddTag(Tags.Global);
        }

        public override void Update()
        {
            base.Update();
            while (popQueue.TryDequeue(out var block))
            {
                block.PopActors();
            }

            Vector2 screenCenter = SceneAs<Level>().Camera.Position + new Vector2((GameplayBuffers.Gameplay?.Width ?? 320) / 2f, (GameplayBuffers.Gameplay?.Height ?? 180) / 2f);
            soundList.Sort((a, b) => (int)(Vector2.DistanceSquared(screenCenter, a.Center) - Vector2.DistanceSquared(screenCenter, b.Center)));

            float volume = 1f;

            foreach (PopBlock block in soundList)
            {
                block.PlaySound(volume);
                volume *= 0.4f;
            }

            soundList.Clear();
        }

        public static PopBlockManager GetManager(Level level)
        {
            if (level.Tracker.GetEntity<PopBlockManager>() is PopBlockManager manager)
            {
                return manager;
            }
            PopBlockManager @new = [];
            level.Add(@new);
            return @new;
        }

        public void QueuePop(PopBlock block)
        {
            popQueue.Enqueue(block);
        }
        public void AddSound(PopBlock block)
        {
            soundList.Add(block);
        }
    }

    public enum PushModes
    {
        OppositeFacing,
        Facing,
        Right,
        Down,
        Left,
        Up
    };

    public readonly float Delay;
    public readonly PushModes PushMode;
    public bool Popping = false;
    public bool Toggle = false;
    private Color color;
    private Color disabledColor;
    private Color disabledColor2;
    private Color staticMoverColor;
    private Color staticMoverDisabledColor;
    public Color HighlightColor => Collidable ? new(0.35f, 0.3f, 0.25f, 0f) : new(0.1f, 0.12f, 0.14f, 0f);

    private List<PopBlock> group;
    private bool groupLeader;
    private Vector2 groupOrigin;
    //private Rectangle groupBounds;

    private Wiggler wiggler;
    private Vector2 wigglerScaler;

    private Side side;

    public MTexture SolidTexture;
    public MTexture PressedTexture;
    public List<Image> Solid = [];
    public List<Image> Pressed = [];
    public List<Image> All = [];
    public List<Image> Highlight = [];
    public List<Image> HighlightAlt = [];

    private float facingPercent;
    private readonly SoundSource sfx = new();

    public PopBlock(EntityData data, Vector2 offset) : base(data.Position, data.Width, data.Height, false)
    {
        Depth = 500;
        Add(new Coroutine(Routine()));
        Delay = data.Float("delay", 1f);
        PushMode = data.Enum("pushMode", PushModes.OppositeFacing);
        Collidable = Toggle = true;
        SolidTexture = GFX.Game["objects/FemtoHelper/PopBlock/solid"];
        PressedTexture = GFX.Game["objects/FemtoHelper/PopBlock/pressed"];

        Color darkTint = Calc.HexToColor("667DA5");
        Color darkerTint = Calc.HexToColor("324E69");

        color = data.HexColor("color", Color.RosyBrown);
        disabledColor = data.HexColor("disabledColor", new((color.R / 255f) * darkTint.R / 255f, (color.G / 255f) * darkTint.G / 255f, (color.B / 255f) * darkTint.B / 255f, (color.A / 255f) * darkTint.A / 255f));
        disabledColor2 = data.HexColor("disabledColor2", new((color.R / 255f) * darkerTint.R / 255f, (color.G / 255f) * darkerTint.G / 255f, (color.B / 255f) * darkerTint.B / 255f, (color.A / 255f) * darkerTint.A / 255f));

        staticMoverColor = data.HexColor("staticMoverColor", color);
        staticMoverDisabledColor = data.HexColor("staticMoverDisabledColor", disabledColor);

        sfx.Position = new Vector2(base.Width, base.Height) / 2f;
        Add(sfx);
    }

    public void AddTile(float x, float y, int tx, int ty)
    {
        Pressed.Add(GetImage(x, y, tx, ty, PressedTexture));
        Solid.Add(GetImage(x, y, tx, ty, SolidTexture));
    }

    public void AddHighlight(float x, float y, bool alt = false)
    {
        Image image;
        switch (PushMode)
        {
            case PushModes.OppositeFacing:
                image = GetImage(x, y, alt ? 2 : 0, 4, SolidTexture);
                image.Color = HighlightColor;
                (alt ? HighlightAlt : Highlight).Add(image);
                break;
            case PushModes.Facing:
                image = GetImage(x, y, alt ? 0 : 2, 4, SolidTexture);
                image.Color = HighlightColor;
                (alt ? HighlightAlt : Highlight).Add(image);
                break;
            case PushModes.Right:
                image = GetImage(x, y, 2, 4, SolidTexture);
                image.Color = HighlightColor;
                Highlight.Add(image);
                break;
            case PushModes.Down:
                image = GetImage(x, y, 1, 4, SolidTexture);
                image.Color = HighlightColor;
                Highlight.Add(image);
                break;
            case PushModes.Left:
                image = GetImage(x, y, 0, 4, SolidTexture);
                image.Color = HighlightColor;
                Highlight.Add(image);
                break;
            case PushModes.Up:
                image = GetImage(x, y, 3, 4, SolidTexture);
                image.Color = HighlightColor;
                Highlight.Add(image);
                break;
        }
    }
    public Image GetImage(float x, float y, int tx, int ty, MTexture tex)
    {
        Vector2 vector = new(x - X, y - Y);
        Image image = new(tex.GetSubtexture(tx * 8, ty * 8, 8, 8));
        Vector2 vector2 = groupOrigin - Position;
        image.Origin = vector2 - vector;
        image.Position = vector2;
        image.Color = color;
        Add(image);
        All.Add(image);
        return image;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        side.RemoveSelf();
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        scene.Add(side = new Side(this));

        foreach (StaticMover staticMover in staticMovers)
        {
            if (staticMover.Entity is Spikes spikes)
            {
                spikes.EnabledColor = staticMoverColor;
                spikes.DisabledColor = staticMoverDisabledColor;
                spikes.VisibleWhenDisabled = true;
                spikes.SetSpikeColor(color);
            }
            if (staticMover.Entity is Spring spring)
            {
                spring.DisabledColor = staticMoverDisabledColor;
                spring.VisibleWhenDisabled = true;
            }
        }
        if (group == null)
        {
            groupLeader = true;
            group = [this];
            FindInGroup(this);
            float boundsLeft = float.MaxValue;
            float boundsRight = float.MinValue;
            float boundsTop = float.MaxValue;
            float boundsBottom = float.MinValue;
            foreach (PopBlock item in group)
            {
                if (item.Left < boundsLeft)
                {
                    boundsLeft = item.Left;
                }
                if (item.Right > boundsRight)
                {
                    boundsRight = item.Right;
                }
                if (item.Bottom > boundsBottom)
                {
                    boundsBottom = item.Bottom;
                }
                if (item.Top < boundsTop)
                {
                    boundsTop = item.Top;
                }
            }
            groupOrigin = new Vector2((int)(boundsLeft + (boundsRight - boundsLeft) / 2f), (int)boundsBottom);
            wigglerScaler = new Vector2(Calc.ClampedMap(boundsRight - boundsLeft, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(boundsBottom - boundsTop, 32f, 96f, 1f, 0.2f));
            Add(wiggler = Wiggler.Create(Delay / 3f, 3f));
            foreach (PopBlock item2 in group)
            {
                item2.wiggler = wiggler;
                item2.wigglerScaler = wigglerScaler;
                item2.groupOrigin = groupOrigin;
            }
        }
        foreach (StaticMover staticMover2 in staticMovers)
        {
            if (staticMover2.Entity is Spikes spikes2)
            {
                spikes2.SetOrigins(groupOrigin);
            }
        }

        Point useCenter = PushMode switch
        {
            PushModes.Right => new(1, 1),
            PushModes.Down => new(2, 3),
            PushModes.Left => new(1, 3),
            PushModes.Up => new(0, 3),
            PushModes.Facing => new(4, 1),
            _ => new(4, 0),
        };

        for (float tileX = Left; tileX < Right; tileX += 8f)
        {
            for (float TileY = Top; TileY < Bottom; TileY += 8f)
            {
                bool leftCheck = CheckForSame(tileX - 8f, TileY);
                bool rightCheck = CheckForSame(tileX + 8f, TileY);
                bool upCheck = CheckForSame(tileX, TileY - 8f);
                bool downCheck = CheckForSame(tileX, TileY + 8f);

                // we always add center tile anyway
                AddTile(tileX, TileY, useCenter.X, useCenter.Y);

                if (leftCheck && rightCheck && upCheck && downCheck)
                {
                    if (!CheckForSame(tileX + 8f, TileY - 8f))
                    {
                        AddTile(tileX, TileY, 3, 0);
                    }
                    else if (!CheckForSame(tileX - 8f, TileY - 8f))
                    {
                        AddTile(tileX, TileY, 3, 1);
                    }
                    else if (!CheckForSame(tileX + 8f, TileY + 8f))
                    {
                        AddTile(tileX, TileY, 3, 2);
                    }
                    else if (!CheckForSame(tileX - 8f, TileY + 8f))
                    {
                        AddTile(tileX, TileY, 3, 3);
                    }
                    /* //center tile
                    else
                    {
                        AddTile(tileX, TileY, useCenter.X, useCenter.Y);
                    }
                    */
                }
                else if (leftCheck && rightCheck && !upCheck && downCheck)
                {
                    AddTile(tileX, TileY, 1, 0);
                    if (PushMode == PushModes.Up) AddHighlight(tileX, TileY);
                }
                else if (leftCheck && rightCheck && upCheck && !downCheck)
                {
                    AddTile(tileX, TileY, 1, 2);
                    if (PushMode == PushModes.Down) AddHighlight(tileX, TileY);
                }
                else if (leftCheck && !rightCheck && upCheck && downCheck)
                {
                    AddTile(tileX, TileY, 2, 1);
                    if (PushMode == PushModes.Right) AddHighlight(tileX, TileY);
                    if (PushMode == PushModes.OppositeFacing) AddHighlight(tileX, TileY, true);
                    if (PushMode == PushModes.Facing) AddHighlight(tileX, TileY);
                }
                else if (!leftCheck && rightCheck && upCheck && downCheck)
                {
                    AddTile(tileX, TileY, 0, 1);
                    if (PushMode == PushModes.Left) AddHighlight(tileX, TileY);
                    if (PushMode == PushModes.OppositeFacing) AddHighlight(tileX, TileY);
                    if (PushMode == PushModes.Facing) AddHighlight(tileX, TileY, true);

                }
                else if (leftCheck && !rightCheck && !upCheck && downCheck)
                {
                    AddTile(tileX, TileY, 2, 0);
                    if (PushMode == PushModes.Right) AddHighlight(tileX, TileY);
                    if (PushMode == PushModes.OppositeFacing) AddHighlight(tileX, TileY, true);
                    if (PushMode == PushModes.Facing) AddHighlight(tileX, TileY);
                    if (PushMode == PushModes.Up) AddHighlight(tileX, TileY);
                }
                else if (!leftCheck && rightCheck && !upCheck && downCheck)
                {
                    AddTile(tileX, TileY, 0, 0);
                    if (PushMode == PushModes.Left) AddHighlight(tileX, TileY);
                    if (PushMode == PushModes.OppositeFacing) AddHighlight(tileX, TileY);
                    if (PushMode == PushModes.Facing) AddHighlight(tileX, TileY, true);
                    if (PushMode == PushModes.Up) AddHighlight(tileX, TileY);
                }
                else if (leftCheck && !rightCheck && upCheck && !downCheck)
                {
                    AddTile(tileX, TileY, 2, 2);
                    if (PushMode == PushModes.Right) AddHighlight(tileX, TileY);
                    if (PushMode == PushModes.OppositeFacing) AddHighlight(tileX, TileY, true);
                    if (PushMode == PushModes.Facing) AddHighlight(tileX, TileY);
                    if (PushMode == PushModes.Down) AddHighlight(tileX, TileY);
                }
                else if (!leftCheck && rightCheck && upCheck && !downCheck)
                {
                    AddTile(tileX, TileY, 0, 2);
                    if (PushMode == PushModes.Left) AddHighlight(tileX, TileY);
                    if (PushMode == PushModes.OppositeFacing) AddHighlight(tileX, TileY);
                    if (PushMode == PushModes.Facing) AddHighlight(tileX, TileY, true);
                    if (PushMode == PushModes.Down) AddHighlight(tileX, TileY);
                }
            }
        }
        if (!Collidable)
        {
            DisableStaticMovers();
        }
        UpdateVisualState();
    }
    private void FindInGroup(PopBlock block)
    {
        foreach (PopBlock entity in Scene.Tracker.GetEntities<PopBlock>())
        {
            if (entity != this && entity != block && entity.PushMode == PushMode && entity.Delay == Delay && (entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))) && !group.Contains(entity))
            {
                group.Add(entity);
                FindInGroup(entity);
                entity.group = group;
            }
        }
    }

    private bool CheckForSame(float x, float y)
    {
        foreach (PopBlock entity in Scene.Tracker.GetEntities<PopBlock>())
        {
            if (entity.PushMode == PushMode && entity.Delay == Delay && entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
            {
                return true;
            }
        }
        return false;
    }

    public void UpdateVisualState()
    {
        foreach (StaticMover staticMover in staticMovers)
        {
            staticMover.Entity.Depth = Depth + 1;
        }

        foreach (Image solid in Solid)
        {
            solid.Visible = Collidable;
        }
        foreach (Image pressed in Pressed)
        {
            pressed.Visible = !Collidable;
        }
        Color highlightColor = HighlightColor;
        foreach (Image highlight in Highlight)
        {
            highlight.Color = highlightColor;
            highlight.Color *= PushMode switch
            {
                PushModes.Facing or PushModes.OppositeFacing => Ease.CubeInOut(facingPercent),
                _ => 1f
            };
        }
        foreach (Image highlightAlt in HighlightAlt)
        {
            highlightAlt.Color = highlightColor;
            highlightAlt.Color *= PushMode switch
            {
                PushModes.Facing or PushModes.OppositeFacing => 1 - Ease.CubeInOut(facingPercent),
                _ => 1f
            };
        }

        if (!groupLeader)
        {
            return;
        }
        Vector2 scale = new Vector2(1f + wiggler.Value * 0.025f * wigglerScaler.X, 1f + wiggler.Value * 0.075f * wigglerScaler.Y);
        foreach (PopBlock blocks in group)
        {
            foreach (Image item4 in blocks.All)
            {
                item4.Scale = scale;
            }
            foreach (StaticMover staticMover2 in blocks.staticMovers)
            {
                if (staticMover2.Entity is not Spikes spikes)
                {
                    continue;
                }
                foreach (Component component in spikes.Components)
                {
                    if (component is Image image)
                    {
                        image.Scale = scale;
                    }
                }
            }
        }
    }

    public IEnumerator Routine()
    {
        while (true)
        {
            yield return (Delay / 5f) * 4f;

            MoveV(Toggle ? 1f : -1f);

            yield return Delay / 5f;

            Toggle = !Toggle;

            if (groupLeader) PopBlockManager.GetManager(Scene as Level).AddSound(this);

            if (Toggle)
            {
                wiggler.Start();
                Popping = true;
                PopBlockManager.GetManager(Scene as Level).QueuePop(this);
                EnableStaticMovers();

                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.Top >= Bottom - 1f)
                {
                    Depth = 25;
                }
                else
                {
                    Depth = -25;
                }
            }
            else
            {
                MoveV(1f);
                Collidable = false;
                DisableStaticMovers();
                Depth = 7000;
            }

            UpdateVisualState();
        }
    }

    public void PlaySound(float volume)
    {
        sfx.Play(Toggle ? "event:/FemtoHelper/pop_block_switch_2" : "event:/FemtoHelper/pop_block_switch_1");
        sfx.instance.setVolume(volume * 2f);
    }

    public override void Update()
    {
        if (Popping)
        {
            MoveVExact(0);
            Popping = false;
        }
        base.Update();

        if (Scene.Tracker.GetEntity<Player>() is Player player)
        {
            facingPercent = Calc.Approach(facingPercent, player.Facing == Facings.Left ? 0f : 1f, Engine.DeltaTime * 4f);
        }

        UpdateVisualState();
    }

    public override void MoveHExact(int move)
    {
        base.MoveHExact(move);
    }

    public override void MoveVExact(int move)
    {
        base.MoveVExact(move);
    }

    public const int MaxIterations = 50;

    public void PopActors()
    {
        Player player = Scene.Tracker.GetEntity<Player>();
        foreach (Actor actor in CollideAll<Actor>())
        {
            MoveActorRecursive(player, actor, 0);
        }
        MoveV(Toggle ? -1f : 1f);
        Collidable = true;
    }

    public void MoveActorRecursive(Player player, Actor actor, int iter)
    {
        if (actor.Get<Holdable>() is { IsHeld: true }) return;

        bool prev = Collidable;
        Collidable = false;

        switch (PushMode)
        {
            case PushModes.OppositeFacing:
                if (player.Facing == Facings.Left)
                {
                    actor.NaiveMove(new(Right - actor.Left, 0));
                }
                else
                {
                    actor.NaiveMove(new(Left - actor.Right, 0));
                }
                break;
            case PushModes.Facing:
                if (player.Facing == Facings.Left)
                {
                    actor.NaiveMove(new(Left - actor.Right, 0));
                }
                else
                {
                    actor.NaiveMove(new(Right - actor.Left, 0));
                }
                break;
            case PushModes.Up:
                actor.NaiveMove(new(0, Top - actor.Bottom));
                break;
            case PushModes.Right:
                actor.NaiveMove(new(Right - actor.Left, 0));
                break;
            case PushModes.Down:
                actor.NaiveMove(new(0, Bottom - actor.Top));
                break;
            case PushModes.Left:
                actor.NaiveMove(new(Left - actor.Right, 0));
                break;
        }
        Collidable = prev;

        foreach (Solid solid in Scene.Tracker.GetEntities<Solid>())
        {
            if (solid.CollideCheck(player))
            {
                Debug(iter);
                if (solid == this || iter > MaxIterations || solid is not PopBlock { Popping: true, Toggle: true } popblock)
                {
                    if (actor is Player p)
                    {
                        p.Die(Vector2.Zero);
                    }
                    else
                    {
                        if (!actor.TrySquishWiggleNoPusher())
                        {
                            PoofEntity(actor);
                        }
                    }
                }
                else
                {
                    popblock.MoveActorRecursive(player, actor, iter + 1);
                }
            }
        }
    }

    public override void Render()
    {
        base.Render();
    }
}
