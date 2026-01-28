namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/OcularBarrier")]
public class OcularBarrier : Solid
{
    public string Flag;
    public bool Invert;
    public MTexture[,] NineSliceOn;
    public MTexture[,] NineSliceOff;
    public MTexture CenterOn;
    public MTexture CenterOff;
    public Color Color1;
    public Color Color2;
    public Color Color3;
    public Color Color4;
    public float FlashTimer;
    public BloomPoint Bloom;
    public Color ReturnColor => Invert ? Color.Lerp(Collidable ? Color3 : Color4, Color.White, Ease.CubeOut(FlashTimer)) : Color.Lerp(Collidable ? Color1 : Color2, Color.White, Ease.CubeOut(FlashTimer));

    public OcularBarrier(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {
        string path = data.Attr("texturePath", "objects/FemtoHelper/OcularBarrier/");
        Flag = data.Attr("flag", "lookout_interacting");
        Invert = data.Bool("invert", false);
        MTexture mTexture = GFX.Game[path + "nineSliceOn"];
        MTexture mTexture2 = GFX.Game[path + "nineSliceOff"];
        CenterOn = GFX.Game[path + "centerOn"];
        CenterOff = GFX.Game[path + "centerOff"];
        NineSliceOn = new MTexture[3, 3];
        NineSliceOff = new MTexture[3, 3];
        Color1 = Calc.HexToColorWithAlpha(data.Attr("activeColor", "99FF66FF"));
        Color2 = Calc.HexToColorWithAlpha(data.Attr("inactiveColor", "005500FF"));
        Color3 = Calc.HexToColorWithAlpha(data.Attr("invertedActiveColor", "6699FFFF"));
        Color4 = Calc.HexToColorWithAlpha(data.Attr("invertedInactiveColor", "000055FF"));
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                NineSliceOn[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                NineSliceOff[i, j] = mTexture2.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }
        FlashTimer = 0;
        Add(Bloom = new BloomPoint(0.3f, 9));
        Bloom.Position = new Vector2(Width / 2f, Height / 2f);
        AddTag(Tags.TransitionUpdate);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (StaticMover staticMover in staticMovers)
        {
            Spikes spikes = staticMover.Entity as Spikes;
            if (spikes != null)
            {
                spikes.EnabledColor = Invert ? Color3 : Color1;
                spikes.DisabledColor = Invert ? Color4 : Color2;
                spikes.VisibleWhenDisabled = true;
                spikes.SetSpikeColor(ReturnColor);
                spikes.Depth = Depth + 1;
            }
            Spring spring = staticMover.Entity as Spring;
            if (spring != null)
            {
                spring.DisabledColor = Invert ? Color4 : Color2;
                spring.VisibleWhenDisabled = true;
                spring.Depth = Depth + 1;
            }
        }
    }

    public override void Update()
    {
        base.Update();
        bool col2 = Collidable;
        Collidable = Invert ? (!(Scene as Level).Session.GetFlag(Flag) && !BlockedCheck()) : ((Scene as Level).Session.GetFlag(Flag) && !BlockedCheck());
        if (Collidable != col2)
        {
            if (Collidable)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }
        FlashTimer = Calc.Approach(FlashTimer, 0, Engine.DeltaTime * 3f);
        Bloom.Visible = Collidable;
    }

    public void Enable()
    {
        FlashTimer = 1f;
        EnableStaticMovers();
        Depth = -9000;
        foreach (StaticMover staticMover in staticMovers)
        {
            staticMover.Entity.Depth = Depth + 1;
        }
    }

    public void Disable()
    {
        DisableStaticMovers();
        Depth = 100;
        foreach (StaticMover staticMover in staticMovers)
        {
            staticMover.Entity.Depth = Depth + 1;
        }
    }

    public override void Render()
    {
        DrawBlockStyle(Position, Width, Height, Collidable);
        base.Render();
    }


    public bool BlockedCheck()
    {
        TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
        if (theoCrystal != null && !TryActorWiggleUp(theoCrystal))
        {
            return true;
        }
        Player player = CollideFirst<Player>();
        if (player != null && !TryActorWiggleUp(player))
        {
            return true;
        }
        return false;
    }

    private bool TryActorWiggleUp(Entity actor)
    {
        bool collidable = Collidable;
        Collidable = true;
        for (int i = 1; i <= 4; i++)
        {
            if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
            {
                actor.Position -= Vector2.UnitY * i;
                Collidable = collidable;
                return true;
            }
        }
        Collidable = collidable;
        return false;
    }

    private void DrawBlockStyle(Vector2 pos, float width, float height, bool active)
    {
        MTexture[,] ninSlice = active ? NineSliceOn : NineSliceOff;
        Color color = ReturnColor;
        int num = (int)(width / 8f);
        int num2 = (int)(height / 8f);
        ninSlice[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
        ninSlice[2, 0].Draw(pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
        ninSlice[0, 2].Draw(pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
        ninSlice[2, 2].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);
        for (int i = 1; i < num - 1; i++)
        {
            ninSlice[1, 0].Draw(pos + new Vector2(i * 8, 0f), Vector2.Zero, color);
            ninSlice[1, 2].Draw(pos + new Vector2(i * 8, height - 8f), Vector2.Zero, color);
        }
        for (int j = 1; j < num2 - 1; j++)
        {
            ninSlice[0, 1].Draw(pos + new Vector2(0f, j * 8), Vector2.Zero, color);
            ninSlice[2, 1].Draw(pos + new Vector2(width - 8f, j * 8), Vector2.Zero, color);
        }
        for (int k = 1; k < num - 1; k++)
        {
            for (int l = 1; l < num2 - 1; l++)
            {
                ninSlice[1, 1].Draw(pos + new Vector2(k, l) * 8f, Vector2.Zero, color);
            }
        }
        if (active)
        {
            CenterOn.DrawCentered(pos + new Vector2(width / 2f, height / 2f), color);
        }
        else
        {
            CenterOff.DrawCentered(pos + new Vector2(width / 2f, height / 2f), color);
        }
    }
}
