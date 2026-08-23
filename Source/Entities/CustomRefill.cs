using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[TrackedAs(typeof(Refill), true)]
public class CustomRefill : Refill
{
    internal PlayerCollider playerCollider;
    internal Func<Player, IEnumerator> enumeratorGetter;
    internal Func<Player, bool> collectLogic;
    internal Action onRespawn;
    internal Action onRender;
    internal Action<Player> onCollect;
    internal Func<bool> respawnGate;
    public float RespawnTime;
    public int DashCount;
    public bool RefillDash;
    public bool RefillStamina;
    public bool AlwaysUse;
    public string AudioPath = "event:/game/general/";
    public string TwoDashAudioPath = "event:/new_content/game/10_farewell/";
    public Vector2 VisualOffset = Vector2.Zero;
    public CustomRefill(Vector2 position, bool fixedDashes, bool oneUse) : base(position, fixedDashes, oneUse)
    {
        //replace the vanilla PlayerCollider
        Get<PlayerCollider>()?.RemoveSelf();
        Add(playerCollider = new PlayerCollider(ReplacedOnPlayer));
        collectLogic = DefaultCollectLogic;
        DashCount = fixedDashes ? 2 : 1;
        RefillDash = true;
        RefillStamina = true;
        RespawnTime = 2.5f;
        AlwaysUse = false;
    }

    public bool DefaultCollectLogic(Player player)
    {
        // TIL: short-circuit boolean operators skip even non-pure functions (wait did i already know this?)
        return AlwaysUse | ReplacedUseRefill(player, twoDashes);
    }

    public bool ReplacedUseRefill(Player player, bool fixedDashes)
    {
        bool flag = false;
        int num = player.MaxDashes;
        if (fixedDashes)
        {
            num = DashCount;
        }
        if (player.Dashes < num || player.Stamina < 20f)
        {
            if (RefillDash) player.Dashes = num;
            if (RefillStamina) player.RefillStamina();
            flag = true;
        }

        return flag || AlwaysUse;
    }

    public CustomRefill SetPlayerCollider(Action<Player> replaceWith)
    {
        playerCollider = new(replaceWith);
        return this;
    }

    public CustomRefill SetEnumerator(Func<Player, IEnumerator> replaceWith)
    {
        enumeratorGetter = replaceWith;
        return this;
    }

    public CustomRefill SetCollectLogic(Func<Player, bool> replaceWith)
    {
        collectLogic = replaceWith;
        return this;
    }

    public CustomRefill SetParticles(ParticleType shatter, ParticleType regen, ParticleType glow)
    {
        p_shatter = shatter;
        p_regen = regen;
        p_glow = glow;
        return this;
    }

    public CustomRefill SetOnRespawn(Action replaceWith)
    {
        onRespawn = replaceWith;
        return this;
    }

    public CustomRefill SetOnRender(Action replaceWith)
    {
        onRender = replaceWith;
        return this;
    }

    public CustomRefill SetOnCollect(Action<Player> replaceWith)
    {
        onCollect = replaceWith;
        return this;
    }

    public CustomRefill SetRespawnGate(Func<bool> logic)
    {
        respawnGate = logic;
        return this;
    }

    public CustomRefill SetTexture(string text)
    {
        Remove(outline);
        Remove(sprite);
        Remove(flash);

        Add(outline = new Image(GFX.Game[text + "outline"]));
        outline.CenterOrigin();
        outline.Visible = false;
        Add(sprite = new Sprite(GFX.Game, text + "idle"));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        Add(flash = new Sprite(GFX.Game, text + "flash"));
        flash.Add("flash", "", 0.05f);
        flash.OnFinish = (_) =>
        {
            flash.Visible = false;
        };
        flash.CenterOrigin();
        return this;
    }

    public void ReplacedOnPlayer(Player player)
    {
        if (collectLogic(player))
        {
            onCollect?.Invoke(player);
            Audio.Play(twoDashes ? $"{TwoDashAudioPath}pinkdiamond_touch" : $"{AudioPath}diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(ReplacedRefillRoutine(player)));
            respawnTimer = RespawnTime;
        }
    }

    public IEnumerator ReplacedRefillRoutine(Player player)
    {
        global::Celeste.Celeste.Freeze(0.05f);
        yield return null;
        level.Shake();
        Sprite obj = sprite;
        Sprite obj2 = flash;
        bool visible = false;
        obj2.Visible = false;
        obj.Visible = visible;
        if (!oneUse)
        {
            outline.Visible = true;
        }
        Depth = 8999;
        if (enumeratorGetter is not null)
        {
            yield return new SwapImmediately(enumeratorGetter.Invoke(player));
        }
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse)
        {
            RemoveSelf();
        }
    }

    [OnInitialize]
    public static void Initialize()
    {
        // manually add Refill to the tracker
        // GetEntitiesTrackIfNeeded doesn't work for... reasons
        Tracker.AddTypeToTracker(typeof(Refill));

        // unnecessary?
        Tracker.Refresh();
    }

    [OnLoad]
    public static void Load()
    {
        IL.Celeste.Refill.Respawn += Refill_Respawn;
        IL.Celeste.Refill.Update += Refill_Update;
    }

    [OnUnload]
    public static void Unload()
    {
        IL.Celeste.Refill.Respawn -= Refill_Respawn;
        IL.Celeste.Refill.Update -= Refill_Update;
    }

    private static void Refill_Respawn(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.EmitLdarg0();
        cursor.EmitDelegate(RefillRespawnUsedToBeAnOnHook);

        while (cursor.TryGotoNextBestFit(MoveType.After, instr => instr.MatchLdstr(out string _)))
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(ReplaceRespawnAudio);
        }
    }

    private static string ReplaceRespawnAudio(string orig, Refill self)
    {
        if (self is CustomRefill cr)
        {
            if (orig.EndsWith("diamond_return"))
            {
                return $"{cr.AudioPath}diamond_return";
            }

            if (orig.EndsWith("pinkdiamond_return"))
            {
                return $"{cr.TwoDashAudioPath}diamond_return";
            }
        }
        return orig;
    }

    private static void RefillRespawnUsedToBeAnOnHook(Refill self)
    {
        if (self is CustomRefill cr)
        {
            cr.onRespawn?.Invoke();
        }
    }

    private static void Refill_Update(ILContext il)
    {
        ILCursor cursor = new(il);
        if (cursor.TryGotoNextBestFit(MoveType.After, instr => instr.MatchLdfld<Refill>("respawnTimer"), instr => instr.MatchLdcR4(0.0f), instr => instr.MatchBleUn(out ILLabel _)))
        {
            cursor.Index--;
            cursor.EmitLdarg0();
            cursor.EmitDelegate(ModRespawnTimerCheck);
        }
    }

    private static float ModRespawnTimerCheck(float orig, Refill self)
    {
        if (self is CustomRefill cr && !(cr.respawnGate?.Invoke() ?? true))
        {
            return float.MaxValue;
        }
        return orig;
    }

    public override void Render()
    {
        Position += VisualOffset;
        onRender?.Invoke();
        base.Render();
        Position -= VisualOffset;
    }
}
