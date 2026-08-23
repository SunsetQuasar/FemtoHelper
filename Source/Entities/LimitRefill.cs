using Celeste;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/LimitRefill")]
public class LimitRefill : CustomRefill
{
    public class DirectionConstraint : Component
    {
        public bool[,] Dirs = new bool[3, 3];

        public Vector2? lastAim = null;
        public Vector2? dashOverrideReturn = null;
        public DirectionConstraint(Directions dir) : base(false, false)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Dirs[i, j] = false;
                }
            }
            switch (dir)
            {
                default:
                    Dirs[0, 1] = true;
                    break;
                case Directions.UpRight:
                    Dirs[0, 2] = true;
                    break;
                case Directions.Right:
                    Dirs[1, 2] = true;
                    break;
                case Directions.DownRight:
                    Dirs[2, 2] = true;
                    break;
                case Directions.Down:
                    Dirs[2, 1] = true;
                    break;
                case Directions.DownLeft:
                    Dirs[2, 0] = true;
                    break;
                case Directions.Left:
                    Dirs[1, 0] = true;
                    break;
                case Directions.UpLeft:
                    Dirs[0, 0] = true;
                    break;
            }
        }
    }
    private static ParticleType _pShatter;
    private static ParticleType _pRegen;
    private static ParticleType _pGlow;

    public enum Directions
    {
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,
    }

    private readonly Directions direction;

    public LimitRefill(Vector2 position, EntityData data)
        : base(position, false, data.Bool("oneUse", false))
    {
        direction = data.Enum("direction", Directions.Up);
        this.SetTexture("objects/FemtoHelper/limitRefill/" + direction switch
        {
            Directions.UpRight => "upright/",
            Directions.Right => "right/",
            Directions.DownRight => "downright/",
            Directions.Down => "down/",
            Directions.DownLeft => "downleft/",
            Directions.Left => "left/",
            Directions.UpLeft => "upleft/",
            _ => "up/"
        })
            .SetParticles(_pShatter, _pRegen, _pGlow)
            .SetOnCollect(OnCollect);

        AlwaysUse = true;

        RespawnTime = data.Float("respawnTime", 2.5f);
    }

    public LimitRefill(EntityData data, Vector2 offset)
        : this(data.Position + offset, data)
    {
    }

    private void OnCollect(Player player)
    {
        player.UseRefill(false);
        if (player.Get<DirectionConstraint>() is DirectionConstraint d) d.RemoveSelf();
        player.Add(new DirectionConstraint(direction));
    }
    private static Hook CanDashHook;
    private delegate bool OrigCanDash(Player self);

    [OnLoadContent]
    public static void LoadLimitRefillContent(bool firstLoad)
    {
        _pShatter = new(P_Shatter)
        {
            Color = Calc.HexToColor("7affe8"),
            Color2 = Calc.HexToColor("7affe8")
        };

        _pRegen = new(P_Regen)
        {
            Color = Calc.HexToColor("00cca9"),
            Color2 = Calc.HexToColor("00cca9")
        };

        _pGlow = new(P_Glow)
        {
            Color = Calc.HexToColor("00cca9"),
            Color2 = Calc.HexToColor("00cca9")
        };
    }

    [OnLoad]
    public static void LoadHooks()
    {
        CanDashHook = new Hook(typeof(Player).GetMethod("get_CanDash"), ModCanDash);
        On.Celeste.Player.DashBegin += Player_DashBegin;
        On.Celeste.Player.DashCoroutine += Player_DashCoroutine;
    }

    private static bool ModCanDash(OrigCanDash orig, Player self)
    {
        if (self.Get<DirectionConstraint>() is { } d)
        {
            Vector2 aim = Input.GetAimVector();

            // block the dash directly if the player is holding a forbidden direction, and does not have Dash Assist enabled.
            return orig(self) && (SaveData.Instance.Assists.DashAssist || IsDashDirectionAllowed(aim, d));
        }
        return orig(self);
    }

    private static bool IsDashDirectionAllowed(Vector2 direction, DirectionConstraint d)
    {
        // if directions are not integers, make them integers.
        direction = new Vector2(Math.Sign(direction.X), Math.Sign(direction.Y));

        // bottom-left (-1, 1) is row 2, column 0.
        return d.Dirs[(int)(direction.Y + 1), (int)(direction.X + 1)];
    }

    private static IEnumerator Player_DashCoroutine(On.Celeste.Player.orig_DashCoroutine orig, Player self)
    {
        Vector2 temp = self.lastAim;
        if (self.Get<DirectionConstraint>() is { } d && d.lastAim is not null)
        {
            self.OverrideDashDirection = d.lastAim ?? default;
            self.lastAim = d.lastAim ?? default;
        }
        yield return new SwapImmediately(orig(self));
        self.lastAim = temp;
    }

    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        if (self.Get<DirectionConstraint>() is { } d)
        {
            d.lastAim = self.lastAim;
            d.dashOverrideReturn = self.OverrideDashDirection;
            Alarm.Set(self, 0f, () =>
            {
                d.RemoveSelf();
                self.OverrideDashDirection = d.dashOverrideReturn;
            });
        }
        orig(self);
    }

    [OnUnload]
    public static void UnloadHooks()
    {
        CanDashHook?.Dispose();
        CanDashHook = null;

        On.Celeste.Player.DashBegin -= Player_DashBegin;
        On.Celeste.Player.DashCoroutine -= Player_DashCoroutine;
    }
}
