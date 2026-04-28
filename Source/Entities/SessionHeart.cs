using Celeste.Editor;
using Celeste.Mod.FemtoHelper.Metadata;
using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked(false)]
[CustomEntity("FemtoHelper/SessionHeart")]
public class SessionHeart : Entity
{
    public class Poem : Entity
    {
        private struct Particle
        {
            public Vector2 Direction;

            public float Percent;

            public float Duration;

            
            public void Reset(float percent)
            {
                Direction = Calc.AngleToVector(Calc.Random.NextFloat(MathF.PI * 2f), 1f);
                Percent = percent;
                Duration = 0.5f + Calc.Random.NextFloat() * 0.5f;
            }
        }

        private const float textScale = 1.5f;

        public float Alpha = 1f;

        public float TextAlpha = 1f;

        public Vector2 Offset;

        public Sprite Heart;

        public float ParticleSpeed = 1f;

        public float Shake;

        private float timer;

        private readonly string text;

        private bool disposed;

        private readonly VirtualRenderTarget poem;

        private readonly VirtualRenderTarget smoke;

        private readonly VirtualRenderTarget temp;

        private readonly Particle[] particles = new Particle[80];

        public Color Color { get; private set; }

        
        public Poem(string text, Color color, string spriteID, float heartAlpha)
        {
            if (text != null)
            {
                this.text = ActiveFont.FontSize.AutoNewline(text, 1024);
            }
            Color = color;
            if(string.IsNullOrEmpty(spriteID))
            {
                Heart = new(GFX.Gui, "FemtoHelper/SessionHearts/defaultGui/");
                Heart.AddLoop("idle", "spin", 1f, 0);
                Heart.AddLoop("spin", "spin", 0.08f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
                Heart.AddLoop("fastspin", "spin", 0.08f, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
                Heart.CenterOrigin();
            } 
            else
            {
                Heart = GFX.GuiSpriteBank.Create(spriteID);
            }
            Heart.Play("spin");
            Heart.Position = new Vector2(1920f, 1080f) * 0.5f;
            Heart.Color = Color.White * heartAlpha;
            int num = Math.Min(1920, Engine.ViewWidth);
            int num2 = Math.Min(1080, Engine.ViewHeight);
            poem = VirtualContent.CreateRenderTarget("poem-a", num, num2);
            smoke = VirtualContent.CreateRenderTarget("poem-b", num / 2, num2 / 2);
            temp = VirtualContent.CreateRenderTarget("poem-c", num / 2, num2 / 2);
            Tag = (int)Tags.HUD | (int)Tags.FrozenUpdate;
            Add(new BeforeRenderHook(BeforeRender));
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Reset(Calc.Random.NextFloat());
            }
        }

        
        public void Dispose()
        {
            if (!disposed)
            {
                poem.Dispose();
                smoke.Dispose();
                temp.Dispose();
                RemoveSelf();
                disposed = true;
            }
        }

        
        private void DrawPoem(Vector2 offset, Color color)
        {
            MTexture mTexture = GFX.Gui["poemside"];
            float num = ActiveFont.Measure(text).X * textScale;
            Vector2 vector = new Vector2(960f, 540f) + offset;
            mTexture.DrawCentered(vector - Vector2.UnitX * (num / 2f + 64f), color);
            ActiveFont.Draw(text, vector, new Vector2(0.5f, 0.5f), Vector2.One * textScale, color);
            mTexture.DrawCentered(vector + Vector2.UnitX * (num / 2f + 64f), color);
        }

        
        public override void Update()
        {
            timer += Engine.DeltaTime;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Percent += Engine.DeltaTime / particles[i].Duration * ParticleSpeed;
                if (particles[i].Percent > 1f)
                {
                    particles[i].Reset(0f);
                }
            }
            Heart.Update();
        }

        
        public void BeforeRender()
        {
            if (!disposed)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(poem);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Matrix transformMatrix = Matrix.CreateScale((float)poem.Width / 1920f);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, transformMatrix);
                Heart.Position = Offset + new Vector2(1920f, 1080f) * 0.5f;
                Heart.Scale = Vector2.One * (1f + Shake * 0.1f);
                MTexture mTexture = OVR.Atlas["snow"];
                for (int i = 0; i < particles.Length; i++)
                {
                    Particle particle = particles[i];
                    float num = Ease.SineIn(particle.Percent);
                    Vector2 position = Heart.Position + particle.Direction * (1f - num) * 1920f;
                    float x = 1f + num * 2f;
                    float y = 0.25f * (0.25f + (1f - num) * 0.75f);
                    float num2 = 1f - num;
                    mTexture.DrawCentered(position, Color * num2, new Vector2(x, y), (-particle.Direction).Angle());
                }
                Heart.Position += new Vector2(Calc.Random.Range(-1f, 1f), Calc.Random.Range(-1f, 1f)) * 16f * Shake;
                Heart.Render();
                if (!string.IsNullOrEmpty(text))
                {
                    DrawPoem(Offset + new Vector2(-2f, 0f), Color.Black * TextAlpha);
                    DrawPoem(Offset + new Vector2(2f, 0f), Color.Black * TextAlpha);
                    DrawPoem(Offset + new Vector2(0f, -2f), Color.Black * TextAlpha);
                    DrawPoem(Offset + new Vector2(0f, 2f), Color.Black * TextAlpha);
                    DrawPoem(Offset + Vector2.Zero, Color * TextAlpha);
                }
                Draw.SpriteBatch.End();
                Engine.Graphics.GraphicsDevice.SetRenderTarget(smoke);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                MagicGlow.Render((RenderTarget2D)poem, timer, -1f, Matrix.CreateScale(0.5f));
                GaussianBlur.Blur((RenderTarget2D)smoke, temp, smoke);
            }
        }

        
        public override void Render()
        {
            if (!disposed && !Scene.Paused)
            {
                float num = 1920f / (float)poem.Width;
                Draw.SpriteBatch.Draw((RenderTarget2D)smoke, Vector2.Zero, smoke.Bounds, Color.White * 0.3f * Alpha, 0f, Vector2.Zero, num * 2f, SpriteEffects.None, 0f);
                Draw.SpriteBatch.Draw((RenderTarget2D)poem, Vector2.Zero, poem.Bounds, Color.White * Alpha, 0f, Vector2.Zero, num, SpriteEffects.None, 0f);
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose();
        }
    }
    public bool IsGhost;

    public const float GhostAlpha = 0.8f;

    private Sprite sprite;

    private Sprite white;

    private ParticleType shineParticle;

    public Wiggler ScaleWiggler;

    private Wiggler moveWiggler;

    private Vector2 moveWiggleDir;

    private BloomPoint bloom;

    private VertexLight light;

    private Poem poem;

    private float timer;

    private bool collected;

    private readonly bool autoPulse;

    private float bounceSfxDelay;

    private readonly bool removeCameraTriggers;

    private SoundEmitter sfx;

    private readonly List<InvisibleBarrier> walls;

    private readonly HoldableCollider holdableCollider;

    private EntityID entityID;

    private readonly TimeRateModifier timeRateModifier;

    private readonly string poemID, poemText, groupName, spriteID, flashSpriteID, pulseSfxPath, bounceSfxPath, collectSfxPath, guiSpriteID, quickSmashSfxPath;

    private readonly Color lightColor, particleColor, guiColor;

    private readonly bool fakeParticles, quickSmash;

    private readonly float bloomAlpha, lightAlpha;

    public SessionHeart(Vector2 position) : base(position)
    {
        autoPulse = true;
        walls = new List<InvisibleBarrier>();
        Add(holdableCollider = new HoldableCollider(OnHoldable));
        Add(new MirrorReflection());
    }

    public SessionHeart(EntityData data, Vector2 offset) : this(data.Position + offset)
    {
        removeCameraTriggers = data.Bool("removeCameraTriggers");
        entityID = new EntityID(data.Level.Name, data.ID);
        Add(timeRateModifier = new TimeRateModifier(1f));
        poemID = data.String("poem", "FemtoHelper_SessionHeart_examplePoem");
        poemText = Dialog.Clean(poemID);
        groupName = data.String("group", "");
        spriteID = data.String("sprite", "");
        lightColor = data.HexColor("lightColor", Calc.HexToColor("ADE2D8"));
        particleColor = data.HexColor("particleColor", Calc.HexToColor("5BC4B1"));
        guiColor = data.HexColor("guiColor", Calc.HexToColor("5BC4B1"));
        fakeParticles = data.Bool("fakeParticles", false);
        bloomAlpha = data.Float("bloomAlpha", 0.75f);
        lightAlpha = data.Float("lightAlpha", 1f);
        pulseSfxPath = data.String("pulseSfx", "event:/game/general/crystalheart_pulse");
        bounceSfxPath = data.String("bounceSfx", "event:/game/general/crystalheart_bounce");
        collectSfxPath = data.String("collectSfx", "event:/FemtoHelper/sessionheart_get");
        flashSpriteID = data.String("flashSprite", "heartGemWhite");
        guiSpriteID = data.String("guiSprite", "");
        quickSmash = data.Bool("quickSmash", false);
        quickSmashSfxPath = data.String("quickSmashSfx", "event:/game/07_summit/gem_get");
    }

    public override void Awake(Scene scene)
    {
        if (string.IsNullOrWhiteSpace(groupName) || (FemtoHelperMetadata.SessionHeartMeta?.Count ?? 0) == 0)
        {
            scene.Add(new MiniTextbox("FEMTOHELPER_ERRORHANDLER_EMPTY_SESSION_HEART"));
            RemoveSelf();
            return;
        }

        if (!FemtoHelperMetadata.SessionHeartGroupNames.Contains(groupName))
        {
            scene.Add(new MiniTextbox("FEMTOHELPER_ERRORHANDLER_INVALID_SESSION_HEART"));
            Log($"Failed to load Session Heart '{entityID}': '{groupName}' is not defined as a Session Heart Group in {(scene as Level).Session.MapData.Filename}.meta.yaml", LogLevel.Error);
            RemoveSelf();
            return;
        }
        base.Awake(scene);
        AreaKey area = (Scene as Level).Session.Area;
        Add(sprite = string.IsNullOrWhiteSpace(spriteID) ? FemtoModule.FemtoSpriteBank.Create("FemtoHelperDefaultSessionHeart") : GFX.SpriteBank.Create(spriteID));
        sprite.Play("spin");
        sprite.OnLoop = (string anim) =>
        {
            if (Visible && anim == "spin" && autoPulse)
            {
                Audio.Play(pulseSfxPath, Position);
                ScaleWiggler.Start();
                (Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
            }
        };
        if (IsGhost)
        {
            sprite.Color = Color.White * 0.8f;
        }
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, (float f) =>
        {
            sprite.Scale = Vector2.One * (1f + f * 0.25f);
        }));
        Add(bloom = new BloomPoint(bloomAlpha, 16f));

        shineParticle = new ParticleType(fakeParticles ? HeartGem.P_FakeShine : HeartGem.P_BlueShine)
        {
            Color = particleColor
        };
        Add(light = new VertexLight(lightColor, lightAlpha, 32, 64));
        moveWiggler = Wiggler.Create(0.8f, 2f);
        moveWiggler.StartZero = true;
        Add(moveWiggler);
    }


    public override void Update()
    {
        bounceSfxDelay -= Engine.DeltaTime;
        timer += Engine.DeltaTime;
        sprite.Position = Vector2.UnitY * (float)Math.Sin(timer * 2f) * 2f + moveWiggleDir * moveWiggler.Value * -8f;
        if (white != null)
        {
            white.Position = sprite.Position;
            white.Scale = sprite.Scale;
            if (white.CurrentAnimationID != sprite.CurrentAnimationID)
            {
                white.Play(sprite.CurrentAnimationID);
            }
            white.SetAnimationFrame(sprite.CurrentAnimationFrame);
        }
        if (collected)
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || entity.Dead)
            {
                EndCutscene();
            }
        }
        base.Update();
        if (!collected && Scene.OnInterval(0.1f))
        {
            SceneAs<Level>().Particles.Emit(shineParticle, 1, Center, Vector2.One * 8f);
        }
    }

    public void OnHoldable(Holdable h)
    {
        Player entity = Scene.Tracker.GetEntity<Player>();
        if (!collected && entity != null && h.Dangerous(holdableCollider))
        {
            Collect(entity);
        }
    }

    public void OnPlayer(Player player)
    {
        if (collected || (Scene as Level).Frozen)
        {
            return;
        }
        if (player.DashAttacking)
        {
            Collect(player);
            return;
        }
        if (bounceSfxDelay <= 0f)
        {
            Audio.Play(bounceSfxPath, Position);
            bounceSfxDelay = 0.1f;
        }
        player.PointBounce(Center);
        moveWiggler.Start();
        ScaleWiggler.Start();
        moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
    }

    private void Collect(Player player)
    {
        Scene.Tracker.GetEntity<AngryOshiro>()?.StopControllingTime();
        Coroutine coroutine = new(CollectRoutine(player))
        {
            UseRawDeltaTime = true
        };
        Add(coroutine);
        collected = true;
        if (!removeCameraTriggers)
        {
            return;
        }
        foreach (CameraOffsetTrigger item in Scene.Entities.FindAll<CameraOffsetTrigger>())
        {
            item.RemoveSelf();
        }
    }

    private IEnumerator CollectRoutine(Player player)
    {
        Level level = Scene as Level;
        AreaKey area = level.Session.Area;

        if(quickSmash)
        {
            Visible = false;
            Collidable = false;
            player.Stamina = 110f;
            SoundEmitter.Play(quickSmashSfxPath, this);
            level.Shake();
            global::Celeste.Celeste.Freeze(0.1f);
            float num = player.Speed.Angle();
            //level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
            //level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
            SlashFx.Burst(Position, num);
            for (int i = 0; i < 10; i++)
            {
                Scene.Add(new AbsorbOrb(Position, player));
            }
            level.Flash(Color.White, drawPlayerOver: true);
            Scene.Add(new SummitGem.BgFlash());
            timeRateModifier.Multiplier = 0.5f;
            while (timeRateModifier.Multiplier < 1f)
            {
                timeRateModifier.Multiplier += Engine.RawDeltaTime * 0.5f;
                yield return null;
            }
            RegisterAsCollected(level);
            RemoveSelf();
            yield break;
        }

        level.CanRetry = false;
        sfx = SoundEmitter.Play(collectSfxPath, this);
        walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Right, level.Bounds.Top), 8f, level.Bounds.Height));
        walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Left - 8, level.Bounds.Top), 8f, level.Bounds.Height));
        walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Left, level.Bounds.Top - 8), level.Bounds.Width, 8f));
        foreach (InvisibleBarrier wall in walls)
        {
            Scene.Add(wall);
        }
        Add(white = GFX.SpriteBank.Create(flashSpriteID));
        Depth = -2000000;
        yield return null;
        Celeste.Freeze(0.2f);
        yield return null;
        timeRateModifier.Multiplier = 0.5f;
        player.Depth = -2000000;
        for (int num = 0; num < 10; num++)
        {
            Scene.Add(new AbsorbOrb(Position));
        }
        level.Shake();
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        level.Flash(Color.White);
        level.FormationBackdrop.Display = true;
        level.FormationBackdrop.Alpha = 1f;
        light.Alpha = (bloom.Alpha = 0f);
        Visible = false;
        for (float t = 0f; t < 2f; t += Engine.RawDeltaTime)
        {
            timeRateModifier.Multiplier = Calc.Approach(timeRateModifier.Multiplier, 0f, Engine.RawDeltaTime * 0.25f);
            yield return null;
        }
        yield return null;
        if (player.Dead)
        {
            yield return 100f;
        }
        timeRateModifier.Multiplier = 1f;
        Tag = Tags.FrozenUpdate;
        level.Frozen = true;
        RegisterAsCollected(level);
        poem = new Poem(poemText, guiColor, guiSpriteID, 1f)
        {
            Alpha = 0f
        };
        Scene.Add(poem);
        for (float t = 0f; t < 1f; t += Engine.RawDeltaTime)
        {
            poem.Alpha = Ease.CubeOut(t);
            yield return null;
        }
        while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
        {
            yield return null;
        }
        sfx.Source.Param("end", 1f);
        level.FormationBackdrop.Display = false;
        for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f)
        {
            poem.Alpha = Ease.CubeIn(1f - t);
            yield return null;
        }
        player.Depth = 0;
        EndCutscene();
    }
    private void EndCutscene()
    {
        Level obj = Scene as Level;
        obj.Frozen = false;
        obj.CanRetry = true;
        obj.FormationBackdrop.Display = false;
        timeRateModifier.Multiplier = 1f;
        if (poem != null)
        {
            poem.RemoveSelf();
        }
        foreach (InvisibleBarrier wall in walls)
        {
            wall.RemoveSelf();
        }
        RemoveSelf();
    }
    private void RegisterAsCollected(Level level)
    {
        if (FemtoModule.Session.SessionHearts.TryGetValue(groupName, out var value))
        {
            if (value is not null)
            {
                if (value.Contains(poemID)) return;
                value.Add(poemID);
            }
            else
            {
                value = [poemID];
            }
        }
        else
        {
            FemtoModule.Session.SessionHearts.Add(groupName, [poemID]);
        }

        level.Session.SetFlag($"SessionHeart_{groupName}_{poemID}");

        level.Session.SetCounter($"SessionHeart_group_{groupName}_count", FemtoModule.Session.SessionHeartCount(groupName));

        level.Session.SetCounter($"SessionHeart_total_count", FemtoModule.Session.SessionHeartCount());

        level.Session.DoNotLoad.Add(entityID);
    }

    internal static class SessionHeartInEditor
    {
        internal struct HeartData
        {
            internal string poem;
            internal Vector2 pos;
            internal Color color;
        }

        private static Dictionary<LevelTemplate, List<HeartData>> hearts = [];
        private static ILHook origRenderHook;
        public static AreaKey Map;

        [OnLoad]
        public static void Load()
        {
            origRenderHook = new ILHook(typeof(MapEditor).GetMethod("orig_Render", BindingFlags.Public | BindingFlags.Instance), hook_orig_Render);
            On.Celeste.Editor.LevelTemplate.ctor_LevelData += LevelTemplate_ctor_LevelData;
            On.Celeste.Editor.LevelTemplate.RenderContents += LevelTemplate_RenderContents;
            On.Celeste.Editor.MapEditor.ctor += MapEditor_ctor;
            IL.Celeste.Editor.MapEditor.RenderManualText += MapEditor_RenderManualText;
        }

        private static void hook_orig_Render(ILContext il)
        {
            ILCursor cursor = new(il);
            while (cursor.TryGotoNextBestFit(MoveType.After, (instr) => instr.MatchEndfinally(), (instr) => instr.MatchLdarg0(), (instr) => instr.MatchLdfld<MapEditor>("hovered")))
            {
                cursor.Index--;
                cursor.EmitLdarg0();
                cursor.EmitDelegate(ImplementHeartKey);
                cursor.Index++;
            }
        }

        internal static void ImplementHeartKey(MapEditor me)
        {
            if (hearts.Count == 0) return;
            if (MInput.Keyboard.Check(Keys.H))
            {
                if (!MInput.Keyboard.Check(Keys.Q) && !MInput.Keyboard.Check(Keys.F1)) Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.25f);
                foreach (LevelTemplate level in me.levels)
                {
                    int i = 0;
                    while (hearts.ContainsKey(level) && i < hearts[level].Count)
                    {
                        Vector2 vector = hearts[level][i].pos;
                        ActiveFont.DrawOutline(hearts[level][i].poem, (new Vector2(level.X + vector.X, level.Y + vector.Y - 2) - MapEditor.Camera.Position) * MapEditor.Camera.Zoom + new Vector2(960f, 540f), new Vector2(0.5f, 1f), Vector2.One * 1f, hearts[level][i].color, 2f, Color.Black);
                        i++;
                    }
                }
            }
        }

        private static void LevelTemplate_ctor_LevelData(On.Celeste.Editor.LevelTemplate.orig_ctor_LevelData orig, LevelTemplate self, LevelData data)
        {
            orig(self, data);
            if (FemtoHelperMetadata.SessionHeartMeta is { } && FemtoHelperMetadata.SessionHeartMeta.Count == 0) return;
            List<HeartData> hs = [];
            foreach (EntityData entity in data.Entities)
            {
                if (entity.Name.Equals("FemtoHelper/SessionHeart"))
                {
                    Color col = Color.White * 0.5f;
                    string group = entity.String("group", "");
                    if (FemtoHelperMetadata.SessionHeartGroupNames.Contains(group)) {
                        if(FemtoHelperMetadata.SessionHeartMetaByGroupName.TryGetValue(group, out var def))
                        {
                            col = Calc.HexToColor(def.Color);
                        }
                    }
                    hs.Add(new()
                    {
                        pos = entity.Position / 8f,
                        poem = Dialog.Clean(entity.String("poem", "FemtoHelper_SessionHeart_examplePoem")),
                        color = col
                    });
                }
            }
            hearts.Add(self, hs);
        }

        private static void LevelTemplate_RenderContents(On.Celeste.Editor.LevelTemplate.orig_RenderContents orig, LevelTemplate self, Monocle.Camera camera, List<LevelTemplate> allLevels)
        {
            orig(self, camera, allLevels);
            if (self.Type == LevelTemplateType.Level)
            {
                if (hearts.TryGetValue(self, out List<HeartData> value))
                {
                    foreach (HeartData heart in value)
                    {
                        GFX.Game["particles/FemtoHelper/tinyHeart"].Draw(new Vector2((float)self.X + heart.pos.X - 1f, (float)self.Y + heart.pos.Y - 2f), Vector2.Zero, heart.color);
                        //Draw.HollowRect(self.X + heart.pos.X - 1f, self.Y + heart.pos.Y - 2f, 3f, 3f, heart.color);
                    }
                }
            }
        }

        private static void MapEditor_ctor(On.Celeste.Editor.MapEditor.orig_ctor orig, MapEditor self, AreaKey area, bool reloadMapData)
        {
            hearts = [];
            Map = area;
            orig(self, area, reloadMapData);
        }

        private static void MapEditor_RenderManualText(ILContext il)
        {
            ILCursor cursor = new(il);
            while (cursor.TryGotoNext(MoveType.After, (instr) => instr.MatchBrfalse(out var _), (instr) => instr.MatchLdstr(out var _)))
            {
                cursor.EmitDelegate(AppendManual);
            }
        }

        private static string AppendManual(string orig)
        {
            if (hearts.Count == 0) return orig;
            return $"H:            Show Session Hearts (FemtoHelper)\n\n{orig}";
        }

        [OnUnload]
        public static void Unload()
        {
            origRenderHook.Dispose();
            origRenderHook = null;
            On.Celeste.Editor.LevelTemplate.ctor_LevelData -= LevelTemplate_ctor_LevelData;
            On.Celeste.Editor.LevelTemplate.RenderContents -= LevelTemplate_RenderContents;
            On.Celeste.Editor.MapEditor.ctor -= MapEditor_ctor;
            IL.Celeste.Editor.MapEditor.RenderManualText -= MapEditor_RenderManualText;
        }
    }
}

