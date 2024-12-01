using System;
using Celeste.Mod.FemtoHelper.Utils;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/SMWFish")]
public class TrollFish : Entity
{
    public Vector2 Speed;
    public float Gravity;
    public readonly string Flag;

    public readonly Collider Bonkbox;

    public readonly bool Blurp;

    public readonly bool Big;

    public bool Neededflagplus => (Scene as Level).FancyCheckFlag(Flag);

    public bool Dead;

    public float Deadspin;

    public readonly MTexture Texture;
    public MTexture Textureslice;
    public float Texturetimer;
    public readonly int Textureframes;

    public readonly string AudioPath;

    public readonly bool FaceRight;

    public TrollFish(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Depth = data.Int("depth", -120000);
        Speed = new Vector2(data.Float("initialSpeedX", 0), data.Float("initialSpeedY", 0));
        Gravity = data.Float("gravity", 260);
        Flag = data.Attr("activationFlag", "fish_flag");
        Blurp = data.Bool("blurp", false);
        Big = data.Bool("big", false);
        FaceRight = data.Bool("faceRight", false);

        Texture = Blurp ? GFX.Game[data.Attr("path", "objects/FemtoHelper/SMWFish/normal/") + "blurp"] : GFX.Game[data.Attr("path", "objects/FemtoHelper/SMWFish/normal/") + "cheep"];

        Textureslice = Texture.GetSubtexture(0, 0, Texture.Height, Texture.Height);

        Textureframes = Texture.Width / Texture.Height;

        Collider = new Circle(Big ? 16 : 8, 0, 0);
        if (!Blurp) Bonkbox = new Hitbox(Big ? 32 : 16, (Big ? 8 : 4), (Big ? -16 : -8), (Big ? -16 : -8));

        AudioPath = data.Attr("audioPath", "event:/FemtoHelper/");

        if (!Blurp) Add(new PlayerCollider(Bonk, Bonkbox));
        if (!data.Bool("harmless", false)) Add(new PlayerCollider(SomeoneGotTrolled, Collider));
    }

    private void SomeoneGotTrolled(Player player)
    {
        player.Die(Vector2.Normalize(player.Position - Position), false, true);
        if (!SaveData.Instance.Assists.Invincible) return;
        Dead = true;
        Speed = (Vector2.Normalize(Position - player.Position) * 63.24f);
        Gravity = 100;
        Collidable = false;
        Audio.Play(AudioPath + "enemykill");

    }

    private void Bonk(Player player)
    {
        Audio.Play(AudioPath + "enemykill");
        Celeste.Freeze(0.1f);
        player.Bounce(Top + 2f);
        Dead = true;
        Speed = (Vector2.Normalize(Position - player.Position) * 63.24f);
        Gravity = 180;
        Collidable = false;
    }

    public override void Update()
    {
        base.Update();

        if (X > (Scene as Level).Bounds.Right + 128 || X < (Scene as Level).Bounds.Left - 128 || Y > (Scene as Level).Bounds.Bottom + 128 || Y < (Scene as Level).Bounds.Top - 128)
        {
            RemoveSelf();
        }

        if (!Neededflagplus)
        {
            Collidable = Visible = false;
        }
        else
        {

            if (!Collidable && !Dead) Collidable = true;
            if (!Visible) Visible = true;
            Position += Speed * Engine.DeltaTime;
            Speed.Y += Gravity * Engine.DeltaTime;

            if (Dead)
            {
                Deadspin += Engine.DeltaTime * 16;
            }
            else
            {
                Texturetimer += Engine.DeltaTime * 7;
            }
        }
    }

    public override void Render()
    {
        base.Render();
        Textureslice = Texture.GetSubtexture(((int)Math.Floor(Texturetimer) % Textureframes) * Texture.Height, 0, Texture.Height, Texture.Height);

        Vector2 size = FaceRight ? new Vector2(-1, 1) : Vector2.One;
        if (Big) size *= 2;

        Textureslice.DrawCentered(Position, Dead ? Calc.HexToColor("CCCCCC") : Color.White, size, Deadspin);
    }
}
