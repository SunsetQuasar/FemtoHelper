using Celeste.Mod.FemtoHelper.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/SessionHeartGate")]

public class SessionHeartGate : Entity
{
    private struct Particle
    {
        public Vector2 Position;

        public float Speed;

        public Color Color;
    }

    private class WhiteLine : Entity
    {
        private float fade = 1f;

        private readonly int blockSize;

        private readonly SessionHeartGate parent;

        public WhiteLine(SessionHeartGate parent, Vector2 origin, int blockSize)
            : base(origin)
        {
            this.parent = parent;
            Depth = -1000000;
            this.blockSize = blockSize;
        }

        public override void Update()
        {
            base.Update();
            fade = Calc.Approach(fade, 0f, Engine.DeltaTime);
            if (!(fade <= 0f))
            {
                return;
            }
            RemoveSelf();
            Level level = SceneAs<Level>();
            for (float num = (int)level.Camera.Left; num < level.Camera.Right; num += 1f)
            {
                if (num < X || num >= X + (float)blockSize)
                {
                    level.Particles.Emit(parent.P_Slice, new Vector2(num, Y));
                }
            }
        }

        public override void Render()
        {
            Vector2 position = (Scene as Level).Camera.Position;
            float num = Math.Max(1f, 4f * fade);
            Draw.Rect(position.X - 10f, Y - num / 2f, 340f, num, Color.White);
        }
    }

    public ParticleType P_Shimmer = new(HeartGemDoor.P_Shimmer)
    {
        Color2 = Color.PaleTurquoise
    };

    public ParticleType P_Slice = new(HeartGemDoor.P_Slice)
    {
        Color2 = Color.PaleTurquoise * 0.65f
    };

    public readonly int Requires;

    public int Size;

    private readonly float openDistance;

    private float openPercent;

    private Solid TopSolid;

    private Solid BotSolid;

    private float offset;

    private Vector2 mist;

    private readonly MTexture temp = new MTexture();

    private readonly List<MTexture> icon;

    private readonly Particle[] particles = new Particle[50];

    private bool startHidden;

    private float heartAlpha = 1f;

    private readonly string group, spritePath;

    public int HeartGems
    {
        get
        {
            if (SaveData.Instance.CheatMode)
            {
                return Requires;
            }
            return FemtoModule.Session.SessionHeartCount(group);
        }
    }

    public float Counter { get; private set; }

    public bool Opened { get; private set; }

    private float openAmount => openPercent * openDistance;

    private readonly bool requireLeft;

    private readonly Color mistColor, interiorColor, interiorParticleColor;
    private readonly float mistAlpha = 0.6f, mistSpeed, bloom;

    public SessionHeartGate(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        Requires = data.Int("requires");
        Add(new CustomBloom(RenderBloom));
        Size = data.Width;
        openDistance = 32f;
        Vector2? vector = data.FirstNodeNullable(offset);
        if (vector.HasValue)
        {
            openDistance = Math.Abs(vector.Value.Y - Y);
        }

        spritePath = data.String("spritePath", "objects/heartdoor/");

        icon = GFX.Game.GetAtlasSubtextures($"{spritePath}icon");
        startHidden = data.Bool("startHidden");
        group = data.String("group", "");
        requireLeft = data.Bool("requireLeft", false);
        mistColor = data.HexColor("mistColor", Color.PaleTurquoise);
        interiorColor = data.HexColor("interiorColor", Calc.HexToColor("41665F"));
        interiorParticleColor = data.HexColor("interiorParticleColor", Color.White);
        mistAlpha = data.Float("mistAlpha", 0.6f);
        mistSpeed = data.Float("mistSpeed", 1f);
        bloom = data.Float("bloomAlpha", 1f);

        P_Shimmer.Color = data.HexColor("shimmerColorA", Color.Lerp(mistColor, Color.White, 0.5f));
        P_Shimmer.Color2 = data.HexColor("shimmerColorB", mistColor);
        P_Slice.Color = data.HexColor("sliceColorA", Color.White);
        P_Slice.Color2 = data.HexColor("sliceColorB", mistColor) * 0.65f;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level level = scene as Level;
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Position = new Vector2(Calc.Random.NextFloat(Size), Calc.Random.NextFloat(level.Bounds.Height));
            particles[i].Speed = Calc.Random.Range(4, 12);
            particles[i].Color = interiorParticleColor * Calc.Random.Range(0.2f, 0.6f);
        }
        level.Add(TopSolid = new Solid(new Vector2(X, level.Bounds.Top - 32), Size, Y - (float)level.Bounds.Top + 32f, safe: true));
        TopSolid.SurfaceSoundIndex = 32;
        TopSolid.SquishEvenInAssistMode = true;
        TopSolid.EnableAssistModeChecks = false;
        level.Add(BotSolid = new Solid(new Vector2(X, Y), Size, (float)level.Bounds.Bottom - Y + 32f, safe: true));
        BotSolid.SurfaceSoundIndex = 32;
        BotSolid.SquishEvenInAssistMode = true;
        BotSolid.EnableAssistModeChecks = false;
        if ((Scene as Level).Session.GetFlag($"opened_sessionheart_gate_{group}_{Requires}"))
        {
            Opened = true;
            Visible = true;
            openPercent = 1f;
            Counter = Requires;
            TopSolid.Y -= openDistance;
            BotSolid.Y += openDistance;
        }
        else
        {
            Add(new Coroutine(Routine()));
        }
    }

    public override void Awake(Scene scene)
    {
        if (string.IsNullOrWhiteSpace(group) || (FemtoHelperMetadata.SessionHeartMeta?.Count ?? 0) == 0)
        {
            scene.Add(new MiniTextbox("FEMTOHELPER_ERRORHANDLER_EMPTY_SESSION_HEART_GATE"));
            RemoveSelf();
            TopSolid.RemoveSelf();
            BotSolid.RemoveSelf();
            return;
        }

        if (!FemtoHelperMetadata.SessionHeartGroupNames.Contains(group))
        {
            scene.Add(new MiniTextbox("FEMTOHELPER_ERRORHANDLER_INVALID_SESSION_HEART_GATE"));
            Log($"Failed to load Session Heart Gate'{SourceData.ID}': '{group}' is not defined as a Session Heart Group in {(scene as Level).Session.MapData.Filename}.meta.yaml", LogLevel.Error);
            RemoveSelf();
            TopSolid.RemoveSelf();
            BotSolid.RemoveSelf();
            return;
        }

        base.Awake(scene);
        if (Opened)
        {
            Scene.CollideFirst<DashBlock>(BotSolid.Collider.Bounds)?.RemoveSelf();
        }
        else if (startHidden)
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && (entity.X > X && requireLeft))
            {
                startHidden = false;
                Scene.CollideFirst<DashBlock>(BotSolid.Collider.Bounds)?.RemoveSelf();
            }
            else
            {
                Visible = false;
            }
        }
    }

    private IEnumerator Routine()
    {
        Level level = Scene as Level;
        float botFrom;
        float topFrom;
        float botTo;
        float topTo;
        if (startHidden)
        {
            Player entity;
            do
            {
                yield return null;
                entity = Scene.Tracker.GetEntity<Player>();
            }
            while (entity == null || !(Math.Abs(entity.X - Center.X) < 100f));
            Audio.Play("event:/new_content/game/10_farewell/heart_door", Position);
            Visible = true;
            heartAlpha = 0f;
            topTo = TopSolid.Y;
            botTo = BotSolid.Y;
            topFrom = (TopSolid.Y -= 240f);
            botFrom = (BotSolid.Y -= 240f);
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 1.2f)
            {
                float num = Ease.CubeIn(p);
                TopSolid.MoveToY(topFrom + (topTo - topFrom) * num);
                BotSolid.MoveToY(botFrom + (botTo - botFrom) * num);
                DashBlock dashBlock = Scene.CollideFirst<DashBlock>(BotSolid.Collider.Bounds);
                if (dashBlock != null)
                {
                    level.Shake(0.5f);
                    Celeste.Freeze(0.1f);
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    dashBlock.Break(BotSolid.BottomCenter, new Vector2(0f, 1f), playSound: true, playDebrisSound: false);
                    Player entity2 = Scene.Tracker.GetEntity<Player>();
                    if (entity2 != null && Math.Abs(entity2.X - Center.X) < 40f)
                    {
                        entity2.PointBounce(entity2.Position + Vector2.UnitX * 8f * -Math.Sign(entity2.X - Center.X));
                    }
                }
                yield return null;
            }
            level.Shake(0.5f);
            Celeste.Freeze(0.1f);
            TopSolid.Y = topTo;
            BotSolid.Y = botTo;
            while (heartAlpha < 1f)
            {
                heartAlpha = Calc.Approach(heartAlpha, 1f, Engine.DeltaTime * 2f);
                yield return null;
            }
            yield return 0.6f;
        }
        while (!Opened && Counter < (float)Requires)
        {
            Player p = Scene.Tracker.GetEntity<Player>();
            if (p != null && Math.Abs(p.X - Center.X) < 80f && (p.X < X || !requireLeft))
            {
                if (Counter == 0f && HeartGems > 0)
                {
                    Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
                }
                /*
                if (HeartGems < Requires)
                {
                    level.Session.SetFlag("granny_door");
                }
                */
                int num2 = (int)Counter;
                int target = Math.Min(HeartGems, Requires);
                Counter = Calc.Approach(Counter, target, Engine.DeltaTime * (float)Requires * 0.8f);
                if (num2 != (int)Counter)
                {
                    yield return 0.1f;
                    if (Counter < (float)target)
                    {
                        Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
                    }
                }
            }
            else
            {
                Counter = Calc.Approach(Counter, 0f, Engine.DeltaTime * (float)Requires * 4f);
            }
            yield return null;
        }
        yield return 0.5f;
        Scene.Add(new WhiteLine(this, Position, Size));
        level.Shake();
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
        level.Flash(Color.White * 0.5f);
        Audio.Play("event:/game/09_core/frontdoor_unlock", Position);
        Opened = true;
        level.Session.SetFlag($"opened_sessionheart_gate_{group}_{Requires}");
        offset = 0f;
        yield return 0.6f;
        botFrom = TopSolid.Y;
        topFrom = TopSolid.Y - openDistance;
        botTo = BotSolid.Y;
        topTo = BotSolid.Y + openDistance;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime)
        {
            level.Shake();
            openPercent = Ease.CubeIn(p);
            TopSolid.MoveToY(MathHelper.Lerp(botFrom, topFrom, openPercent));
            BotSolid.MoveToY(MathHelper.Lerp(botTo, topTo, openPercent));
            if (p >= 0.4f && level.OnInterval(0.1f))
            {
                for (int i = 4; i < Size; i += 4)
                {
                    level.ParticlesBG.Emit(P_Shimmer, 1, new Vector2(TopSolid.Left + (float)i + 1f, TopSolid.Bottom - 2f), new Vector2(2f, 2f), -MathF.PI / 2f);
                    level.ParticlesBG.Emit(P_Shimmer, 1, new Vector2(BotSolid.Left + (float)i + 1f, BotSolid.Top + 2f), new Vector2(2f, 2f), MathF.PI / 2f);
                }
            }
            yield return null;
        }
        TopSolid.MoveToY(topFrom);
        BotSolid.MoveToY(topTo);
        openPercent = 1f;
    }
    public override void Update()
    {
        base.Update();
        if (!Opened)
        {
            offset += 12f * Engine.DeltaTime;
            mist.X -= 4f * Engine.DeltaTime * mistSpeed;
            mist.Y -= 24f * Engine.DeltaTime * mistSpeed;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
            }
        }
    }
    public void RenderBloom()
    {
        if (!Opened && Visible && bloom > 0)
        {
            DrawBloom(new Rectangle((int)TopSolid.X, (int)TopSolid.Y, Size, (int)(TopSolid.Height + BotSolid.Height)));
        }
    }
    private void DrawBloom(Rectangle bounds)
    {
        Draw.Rect(bounds.Left - 4, bounds.Top, 2f, bounds.Height, Color.White * 0.25f * bloom);
        Draw.Rect(bounds.Left - 2, bounds.Top, 2f, bounds.Height, Color.White * 0.5f * bloom);
        Draw.Rect(bounds, Color.White * 0.75f * bloom);
        Draw.Rect(bounds.Right, bounds.Top, 2f, bounds.Height, Color.White * 0.5f * bloom);
        Draw.Rect(bounds.Right + 2, bounds.Top, 2f, bounds.Height, Color.White * 0.25f * bloom);
    }
    private void DrawMist(Rectangle bounds, Vector2 mist)
    {
        Color color = mistColor * mistAlpha;
        MTexture mTexture = GFX.Game[$"{spritePath}mist"];
        int num = mTexture.Width / 2;
        int num2 = mTexture.Height / 2;
        for (int i = 0; i < bounds.Width; i += num)
        {
            for (int j = 0; j < bounds.Height; j += num2)
            {
                mTexture.GetSubtexture((int)Utils.Mod(mist.X, num), (int)Utils.Mod(mist.Y, num2), Math.Min(num, bounds.Width - i), Math.Min(num2, bounds.Height - j), temp);
                temp.Draw(new Vector2(bounds.X + i, bounds.Y + j), Vector2.Zero, color);
            }
        }
    }
    private void DrawInterior(Rectangle bounds)
    {
        Draw.Rect(bounds, interiorColor);
        DrawMist(bounds, mist);
        DrawMist(bounds, new Vector2(mist.Y, mist.X) * 1.5f);
        Vector2 vector = (Scene as Level).Camera.Position;
        if (Opened)
        {
            vector = Vector2.Zero;
        }
        for (int i = 0; i < particles.Length; i++)
        {
            Vector2 vector2 = particles[i].Position + vector * 0.2f;
            vector2.X = Utils.Mod(vector2.X, bounds.Width);
            vector2.Y = Utils.Mod(vector2.Y, bounds.Height);
            Draw.Pixel.Draw(new Vector2(bounds.X, bounds.Y) + vector2, Vector2.Zero, particles[i].Color);
        }
    }
    private void DrawEdges(Rectangle bounds, Color color)
    {
        MTexture mTexture = GFX.Game[$"{spritePath}edge"];
        MTexture mTexture2 = GFX.Game[$"{spritePath}top"];
        int num = (int)(offset % 8f);
        if (num > 0)
        {
            mTexture.GetSubtexture(0, 8 - num, 7, num, temp);
            temp.DrawJustified(new Vector2(bounds.Left + 4, bounds.Top), new Vector2(0.5f, 0f), color, new Vector2(-1f, 1f));
            temp.DrawJustified(new Vector2(bounds.Right - 4, bounds.Top), new Vector2(0.5f, 0f), color, new Vector2(1f, 1f));
        }
        for (int i = num; i < bounds.Height; i += 8)
        {
            mTexture.GetSubtexture(0, 0, 8, Math.Min(8, bounds.Height - i), temp);
            temp.DrawJustified(new Vector2(bounds.Left + 4, bounds.Top + i), new Vector2(0.5f, 0f), color, new Vector2(-1f, 1f));
            temp.DrawJustified(new Vector2(bounds.Right - 4, bounds.Top + i), new Vector2(0.5f, 0f), color, new Vector2(1f, 1f));
        }
        for (int j = 0; j < bounds.Width; j += 8)
        {
            mTexture2.DrawCentered(new Vector2(bounds.Left + 4 + j, bounds.Top + 4), color);
            mTexture2.DrawCentered(new Vector2(bounds.Left + 4 + j, bounds.Bottom - 4), color, new Vector2(1f, -1f));
        }
    }
    public override void Render()
    {
        Color color = (Opened ? (Color.White * 0.25f) : Color.White);
        if (!Opened && TopSolid.Visible && BotSolid.Visible)
        {
            Rectangle bounds = new Rectangle((int)TopSolid.X, (int)TopSolid.Y, Size, (int)(TopSolid.Height + BotSolid.Height));
            DrawInterior(bounds);
            DrawEdges(bounds, color);
        }
        else
        {
            if (TopSolid.Visible)
            {
                Rectangle bounds2 = new Rectangle((int)TopSolid.X, (int)TopSolid.Y, Size, (int)TopSolid.Height);
                DrawInterior(bounds2);
                DrawEdges(bounds2, color);
            }
            if (BotSolid.Visible)
            {
                Rectangle bounds3 = new Rectangle((int)BotSolid.X, (int)BotSolid.Y, Size, (int)BotSolid.Height);
                DrawInterior(bounds3);
                DrawEdges(bounds3, color);
            }
        }
        if (!(heartAlpha > 0f))
        {
            return;
        }
        float num = 12f;
        int num2 = (int)((float)(Size - 8) / num);
        int num3 = (int)Math.Ceiling((float)Requires / (float)num2);
        Color color2 = color * heartAlpha;
        for (int i = 0; i < num3; i++)
        {
            int num4 = (((i + 1) * num2 < Requires) ? num2 : (Requires - i * num2));
            Vector2 vector = new Vector2(X + (float)Size * 0.5f, Y) + new Vector2((float)(-num4) / 2f + 0.5f, (float)(-num3) / 2f + (float)i + 0.5f) * num;
            if (Opened)
            {
                if (i < num3 / 2)
                {
                    vector.Y -= openAmount + 8f;
                }
                else
                {
                    vector.Y += openAmount + 8f;
                }
            }
            for (int j = 0; j < num4; j++)
            {
                int num5 = i * num2 + j;
                float num6 = Ease.CubeIn(Calc.ClampedMap(Counter, num5, (float)num5 + 1f));
                icon[(int)(num6 * (float)(icon.Count - 1))].DrawCentered(vector + new Vector2((float)j * num, 0f), color2);
            }
        }
    }
}
