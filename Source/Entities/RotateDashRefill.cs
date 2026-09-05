using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System;
using System.Linq;
using System.Collections;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
public class RotateDash() : Component(true, true)
{
    public readonly MTexture Texture = GFX.Game["objects/FemtoHelper/rotateRefillCCW/indicator_a"];
    private float angleTarget;
    private float displayAngle;
    private float displayPercent = 0;
    private float sineTimer = Calc.Random.NextAngle();
    public float AngleOffset = 0;
    public float SpeedScalar = 1;
    public bool HasStartedRotateDashing = false;

    public Color[] Colors;
    public override void Update()
    {
        base.Update();
        sineTimer += Engine.DeltaTime * (MathF.Tau * 1.2f);
        if ((Entity as Player).Speed != Vector2.Zero)
        {
            float newAngle = VectorToAngle((Entity as Player).Speed) - AngleOffset;
            angleTarget = Calc.AngleApproach(angleTarget, newAngle, Engine.DeltaTime * 16 * Calc.AbsAngleDiff(angleTarget, newAngle));
            displayPercent = Calc.Approach(displayPercent, 1, Engine.DeltaTime * 6);
        }
        else
        {
            displayPercent = Calc.Approach(displayPercent, 0, Engine.DeltaTime * 6);
        }
        displayAngle = angleTarget + MathF.Sin(sineTimer) / 5;
    }

    public override void Render()
    {
        if (displayPercent > 0)
        {
            Texture.DrawOutlineCentered(Entity.Center + Vector2.UnitY, Color.Black, Ease.CubeInOut(displayPercent), displayAngle);
            Texture.DrawOutlineCentered(Entity.Center, Color.Black, Ease.CubeInOut(displayPercent), displayAngle);
            Texture.DrawCentered(Entity.Center + Vector2.UnitY, Colors[2], Ease.CubeInOut(displayPercent), displayAngle);
            Texture.DrawCentered(Entity.Center, Colors[1], Ease.CubeInOut(displayPercent), displayAngle);
        }
        base.Render();
    }
}

[Tracked]
public class ExtraTrailManager() : Component(true, true)
{
    public Player Player;
    public float DashTrailTimer;
    public int DashTrailCounter;
    public int DashParticleCount;

    public override void Added(Entity entity)
    {
        base.Added(entity);
        Player = entity as Player;
    }

    public override void Update()
    {
        base.Update();
        if (Player != null)
        {
            if (Player.StateMachine.state == Player.StDash)
            {
                DashTrailTimer = 0f;
                DashTrailCounter = 0;
            }

            if (DashTrailTimer > 0f)
            {
                DashTrailTimer -= Engine.DeltaTime;
                if (DashTrailTimer <= 0f)
                {
                    Player.CreateTrail();
                    DashTrailCounter--;
                    if (DashTrailCounter > 0)
                    {
                        DashTrailTimer = 0.07f;
                    }
                }
            }
            if (Player.Speed != Vector2.Zero && Scene.OnInterval(0.02f) && DashParticleCount > 0)
            {
                ParticleType type = !Player.wasDashB ? Player.P_DashA : Player.Sprite.Mode != PlayerSpriteMode.MadelineAsBadeline ? Player.P_DashB : Player.P_DashBadB;
                Player.level.ParticlesFG.Emit(type, Player.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), Player.DashDir.Angle());
                DashParticleCount--;
            }
        } 
        else
        {
            DashTrailTimer = 0f;
            DashTrailCounter = 0;
            DashParticleCount = 0;
        }
    }
}

[Tracked]
[CustomEntity("FemtoHelper/RotateDashRefill")]
public class RotateDashRefill : CustomRefill
{
    private readonly ParticleType pShatter;

    private readonly ParticleType pRegen;

    private readonly ParticleType pGlow;

    public readonly float Angle;

    public readonly float Scalar;

    public readonly Color[] EffectColors;

    public RotateDashRefill(EntityData data, Vector2 offset) : base(data.Position + offset, false, data.Bool("oneUse", false))
    {
        pShatter = new ParticleType(P_Shatter);
        pRegen = new ParticleType(P_Regen);
        pGlow = new ParticleType(P_Glow);

        string[] efColors = data.Attr("effectColors", "7958ad,cbace6,634691").Split(',');
        if (efColors.Length != 3) efColors = ["7958ad", "cbace6", "634691"];
        string[] colors = data.Attr("particleColors", "dba0d0,ca6dd1,e6aec1,e376df").Split(',');
        if (colors.Length != 4) colors = ["dba0d0", "ca6dd1", "e6aec1", "e376df"];
        EffectColors = [.. efColors.Select(Calc.HexToColor)];

        pShatter.Color = Calc.HexToColor(colors[0]);
        pShatter.Color2 = Calc.HexToColor(colors[1]);
        pRegen.Color = pGlow.Color = Calc.HexToColor(colors[2]);
        pRegen.Color2 = pGlow.Color2 = Calc.HexToColor(colors[3]);

        Angle = data.Float("angle", 90);
        Scalar = data.Float("scalar", 1.5f);

        this.SetTexture(data.Attr("texture", "objects/FemtoHelper/rotateRefillCCW/"))
            .SetParticles(pShatter, pRegen, pGlow)
            .SetCollectLogic(CollectCheck)
            .SetOnCollect(OnCollect);

        VisualOffset = data.Vector2("visualOffsetX", "visualOffsetY", Vector2.Zero);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Render()
    {
        base.Render();
    }

    public bool CollectCheck(Player player)
    {
        if (player.Get<RotateDash>() is RotateDash rotateDash)
        {
            if (rotateDash.AngleOffset == Angle.ToRad() && rotateDash.SpeedScalar == Scalar)
            {
                return false;
            }
            player.Remove(rotateDash);
        }
        return true;
    }

    private void OnCollect(Player player)
    {
        player.UseRefill(false);
        player.Add(new RotateDash()
        {
            AngleOffset = Angle.ToRad(),
            SpeedScalar = Scalar,
            Colors = EffectColors
        });
    }

    [OnLoad]
    public static void LoadHooks()
    {
        On.Celeste.PlayerHair.GetHairColor += RotateDashCustomColor;
        On.Celeste.Player.DashBegin += RotateDashBeginHook;
        On.Celeste.Player.DashCoroutine += RotateDashCoroutineHook;
        On.Celeste.Player.Update += RotateDashBugCheck;
    }
    [OnUnload]
    public static void UnloadHooks()
    {
        On.Celeste.PlayerHair.GetHairColor -= RotateDashCustomColor;
        On.Celeste.Player.DashBegin -= RotateDashBeginHook;
        On.Celeste.Player.DashCoroutine -= RotateDashCoroutineHook;
        On.Celeste.Player.Update -= RotateDashBugCheck;
    }

    private static Color RotateDashCustomColor(On.Celeste.PlayerHair.orig_GetHairColor orig, PlayerHair self, int index)
    {
        return self.Entity.Get<RotateDash>() is RotateDash rotateDash ? Color.Lerp(rotateDash.Colors[0], rotateDash.Colors[1], (float)(Math.Sin(self.Scene.TimeActive * 4) * 0.5f) + 0.5f) : orig(self, index);
    }
    private static IEnumerator RotateDashCoroutineHook(On.Celeste.Player.orig_DashCoroutine orig, Player self)
    {
        if (self.Get<RotateDash>() is RotateDash rotateDash)
        {
            Celeste.Freeze(0.1f);

            self.Speed = Vector2.Transform(self.Speed, Matrix.CreateRotationZ(-rotateDash.AngleOffset));
            (self.Scene as Level).DirectionalShake(self.Speed.SafeNormalize());
            self.Speed *= rotateDash.SpeedScalar;
            self.StateMachine.State = 0;
            self.Remove(rotateDash);
            ExtraTrailManager t = self.Get<ExtraTrailManager>();
            if(t == null)
            {
                self.Add(t = new ExtraTrailManager());
            }
            t.DashTrailTimer = 0.06f;
            t.DashTrailCounter = 3;
            t.DashParticleCount = 10;

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
        Vector2 tempSpeed = self.Speed;

        orig(self);

        if (self.Get<RotateDash>() is RotateDash rotateDash)
        {
            self.Speed = tempSpeed;
            rotateDash.HasStartedRotateDashing = true;
        }
    }

    private static void RotateDashBugCheck(On.Celeste.Player.orig_Update orig, Player self)
    {
        if (self.Get<RotateDash>() is RotateDash rotateDash && rotateDash.HasStartedRotateDashing && self.StateMachine.State != 2)
        {
            self.Remove(rotateDash);
        }
        orig(self);
    }
}