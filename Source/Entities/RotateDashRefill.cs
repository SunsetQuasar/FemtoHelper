using System;
using System.Linq;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities
{
    [Tracked]
    public class ExtraTrailManager : Component
    {
        public Player player;
        public float dashTrailTimer;
        public int dashTrailCounter;
        public int dashParticleCount;
        public ExtraTrailManager() : base(true, true)
        {

        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            player = entity as Player;
        }

        public override void Update()
        {
            base.Update();
            if (player != null)
            {
                if (player.StateMachine.state == Player.StDash)
                {
                    dashTrailTimer = 0f;
                    dashTrailCounter = 0;
                }

                if (dashTrailTimer > 0f)
                {
                    dashTrailTimer -= Engine.DeltaTime;
                    if (dashTrailTimer <= 0f)
                    {
                        player.CreateTrail();
                        dashTrailCounter--;
                        if (dashTrailCounter > 0)
                        {
                            dashTrailTimer = 0.07f;
                        }
                    }
                }
                if (player.Speed != Vector2.Zero && Scene.OnInterval(0.02f) && dashParticleCount > 0)
                {
                    ParticleType type = ((!player.wasDashB) ? Player.P_DashA : ((player.Sprite.Mode != PlayerSpriteMode.MadelineAsBadeline) ? Player.P_DashB : Player.P_DashBadB));
                    player.level.ParticlesFG.Emit(type, player.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), player.DashDir.Angle());
                    dashParticleCount--;
                }
            } 
            else
            {
                dashTrailTimer = 0f;
                dashTrailCounter = 0;
                dashParticleCount = 0;
            }
        }
    }

    [Tracked]
    [CustomEntity("FemtoHelper/RotateDashRefill")]
    public class RotateDashRefill : Entity
    {
        public static ParticleType P_Shatter;

        public static ParticleType P_Regen;

        public static ParticleType P_Glow;

        public static ParticleType P_ShatterTwo;

        public static ParticleType P_RegenTwo;

        public static ParticleType P_GlowTwo;

        private Sprite sprite;

        private Sprite flash;

        private Image outline;

        private Wiggler wiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Level level;

        private SineWave sine;

        private bool twoDashes;

        private bool oneUse;

        private ParticleType p_shatter;

        private ParticleType p_regen;

        private ParticleType p_glow;

        private float respawnTimer;

        public float angle;

        public float scalar;

        public Color[] EffectColors;

        public RotateDashRefill(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            twoDashes = false;
            oneUse = data.Bool("oneUse", false);
            angle = data.Float("angle", 90);
            scalar = data.Float("scalar", 1.5f);
            EffectColors = data.Attr("effectColors", "7958ad,cbace6,634691").Split(',').Select(Calc.HexToColor).ToArray();
            if (EffectColors.Length != 3) EffectColors = [Calc.HexToColor("7958ad"), Calc.HexToColor("cbace6"), Calc.HexToColor("634691")];
            string text = data.Attr("texture", "objects/refill/");
            string[] colors = data.Attr("colors", "dba0d0,ca6dd1,e6aec1,e376df").Split(',');
            if (colors.Length != 4) colors = "dba0d0,ca6dd1,e6aec1,e376df".Split(',');
            p_shatter = new ParticleType(Refill.P_Shatter);
            p_regen = new ParticleType(Refill.P_Regen);
            p_glow = new ParticleType(Refill.P_Glow);
            p_shatter.Color = Calc.HexToColor(colors[0]);
            p_shatter.Color2 = Calc.HexToColor(colors[1]);
            p_regen.Color = p_glow.Color = Calc.HexToColor(colors[2]);
            p_regen.Color2 = p_glow.Color2 = Calc.HexToColor(colors[3]);
            Add(outline = new Image(GFX.Game[text + "outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, text + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, text + "flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate
            {
                flash.Visible = false;
            };
            flash.CenterOrigin();
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
            }));
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();
            UpdateY();
            base.Depth = -100;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
            else if (base.Scene.OnInterval(0.1f))
            {
                level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
            }
            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
            if (base.Scene.OnInterval(2f) && sprite.Visible)
            {
                flash.Play("flash", restart: true);
                flash.Visible = true;
            }
        }

        private void Respawn()
        {
            if (!Collidable)
            {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                base.Depth = -100;
                wiggler.Start();
                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
                level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateY()
        {
            Sprite obj = flash;
            Sprite obj2 = sprite;
            float num2 = (bloom.Y = sine.Value * 2f);
            float num5 = (obj.Y = (obj2.Y = num2));
        }

        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline();
            }
            base.Render();
        }

        private void OnPlayer(Player player)
        {
            if (!FemtoModule.Session.HasRotateDash || FemtoModule.Session.RotateDashAngle != Calc.ToRad(angle) || FemtoModule.Session.RotateDashScalar != scalar)
            {
                player.UseRefill(twoDashes);
                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                FemtoModule.Session.HasRotateDash = true;
                FemtoModule.Session.RotateDashAngle = Calc.ToRad(angle);
                FemtoModule.Session.RotateDashScalar = scalar;
                FemtoModule.Session.RotateDashColors = EffectColors;
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player)));
                respawnTimer = 2.5f;
            }
        }

        private IEnumerator RefillRoutine(Player player)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = (flash.Visible = false);
            if (!oneUse)
            {
                outline.Visible = true;
            }
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - (float)Math.PI / 2f);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + (float)Math.PI / 2f);
            SlashFx.Burst(Position, num);
            if (oneUse)
            {
                RemoveSelf();
            }
        }

        public static void Load()
        {
            On.Celeste.LevelLoader.LoadingThread += RotateDashInitialize;
            On.Celeste.PlayerHair.GetHairColor += RotateDashCustomColor;
            On.Celeste.Player.Die += RotateDashDeathHook;
            On.Celeste.Player.DashBegin += RotateDashBeginHook;
            On.Celeste.Player.DashCoroutine += RotateDashCoroutineHook;
            On.Celeste.Player.Added += RotateDashAddComponent;
            On.Celeste.Player.Update += RotateDashBugCheck;
        }

        public static void Unload()
        {
            On.Celeste.LevelLoader.LoadingThread -= RotateDashInitialize;
            On.Celeste.PlayerHair.GetHairColor -= RotateDashCustomColor;
            On.Celeste.Player.Die -= RotateDashDeathHook;
            On.Celeste.Player.DashBegin -= RotateDashBeginHook;
            On.Celeste.Player.DashCoroutine -= RotateDashCoroutineHook;
            On.Celeste.Player.Added -= RotateDashAddComponent;
            On.Celeste.Player.Update -= RotateDashBugCheck;
        }



        private static void RotateDashInitialize(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
        {
            orig(self);
            RotateDashInitialize();
        }

        private static PlayerDeadBody RotateDashDeathHook(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            RotateDashInitialize();
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        private static Color RotateDashCustomColor(On.Celeste.PlayerHair.orig_GetHairColor orig, PlayerHair self, int index)
        {
            if (FemtoModule.Session.HasRotateDash)
            {
                return Color.Lerp(FemtoModule.Session.RotateDashColors[0], FemtoModule.Session.RotateDashColors[1], (float)(Math.Sin(self.Scene.TimeActive * 4) * 0.5f) + 0.5f);
            }
            return orig(self, index);
        }
        private static IEnumerator RotateDashCoroutineHook(On.Celeste.Player.orig_DashCoroutine orig, Player self)
        {
            if (FemtoModule.Session.HasRotateDash)
            {
                Engine.TimeRate = 0.05f;
                self.Add(new Coroutine(CodecumberPortStuff.RotateDashAlignAnim(self)));
                yield return 0.005f;
                Engine.TimeRate = 1;

                self.Speed = Vector2.Transform(self.Speed, Matrix.CreateRotationZ(-FemtoModule.Session.RotateDashAngle));
                (self.Scene as Level).DirectionalShake(self.Speed.SafeNormalize());
                self.Speed *= FemtoModule.Session.RotateDashScalar;
                self.StateMachine.State = 0;
                FemtoModule.Session.HasRotateDash = false;
                FemtoModule.Session.HasStartedRotateDashing = false;
                ExtraTrailManager t = self.Get<ExtraTrailManager>();
                if(t != null)
                {
                    t.dashTrailTimer = 0.06f;
                    t.dashTrailCounter = 3;
                    t.dashParticleCount = 10;
                }
                self.level.Displacement.AddBurst(self.Center, 0.4f, 8f, 64f, 0.5f, Ease.QuadOut, Ease.QuadOut);
                yield return null;

            }
            else
            {
                yield return new SwapImmediately(orig(self));
            }
        }
        

        private static void RotateDashBeginHook(On.Celeste.Player.orig_DashBegin orig, Player self)
        {
            Vector2 tempSpeed = Vector2.Zero;
            tempSpeed = self.Speed;
            orig(self);
            if (FemtoModule.Session.HasRotateDash)
            {
                self.Speed = tempSpeed;
                FemtoModule.Session.HasStartedRotateDashing = true;
            }
        }

        private static void RotateDashBugCheck(On.Celeste.Player.orig_Update orig, Player self)
        {
            if (FemtoModule.Session.HasStartedRotateDashing && FemtoModule.Session.HasRotateDash && self.StateMachine.State != 2)
            {
                FemtoModule.Session.HasRotateDash = false;
                FemtoModule.Session.HasStartedRotateDashing = false;
                Engine.TimeRate = 1;
            }
            orig(self);
        }

        private static void RotateDashAddComponent(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);
            self.Add(new RotateDashIndicator());
            self.Add(new ExtraTrailManager());
        }

        public static void RotateDashInitialize()
        {
            FemtoModule.Session.HasRotateDash = false;
            FemtoModule.Session.RotateDashAngle = 0;
            FemtoModule.Session.RotateDashScalar = 1;
            FemtoModule.Session.HasStartedRotateDashing = false;
        }
    }
}
