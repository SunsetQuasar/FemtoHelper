using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/OcularBarrier")]
public class OcularBarrier : Solid
{
    public string flag;
    public bool invert;
    public MTexture[,] nineSliceOn;
    public MTexture[,] nineSliceOff;
    public MTexture centerOn;
    public MTexture centerOff;
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;
    public float flashTimer;
    public BloomPoint bloom;
    public Color ReturnColor => invert ? Color.Lerp(Collidable ? color3 : color4, Color.White, Ease.CubeOut(flashTimer)) : Color.Lerp(Collidable ? color1 : color2, Color.White, Ease.CubeOut(flashTimer));

    public OcularBarrier(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {
        string path = data.Attr("texturePath", "objects/FemtoHelper/OcularBarrier/");
        flag = data.Attr("flag", "lookout_interacting");
        invert = data.Bool("invert", false);
        MTexture mTexture = GFX.Game[path + "nineSliceOn"];
        MTexture mTexture2 = GFX.Game[path + "nineSliceOff"];
        centerOn = GFX.Game[path + "centerOn"];
        centerOff = GFX.Game[path + "centerOff"];
        nineSliceOn = new MTexture[3, 3];
        nineSliceOff = new MTexture[3, 3];
        color1 = Calc.HexToColorWithAlpha(data.Attr("activeColor", "99FF66FF"));
        color2 = Calc.HexToColorWithAlpha(data.Attr("inactiveColor", "005500FF"));
        color3 = Calc.HexToColorWithAlpha(data.Attr("invertedActiveColor", "6699FFFF"));
        color4 = Calc.HexToColorWithAlpha(data.Attr("invertedInactiveColor", "000055FF"));
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                nineSliceOn[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                nineSliceOff[i, j] = mTexture2.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }
        flashTimer = 0;
        Add(bloom = new BloomPoint(0.3f, 9));
        bloom.Position = new Vector2(Width / 2f, Height / 2f);
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
                spikes.EnabledColor = invert ? color3 : color1;
                spikes.DisabledColor = invert ? color4 : color2;
                spikes.VisibleWhenDisabled = true;
                spikes.SetSpikeColor(ReturnColor);
                spikes.Depth = base.Depth + 1;
            }
            Spring spring = staticMover.Entity as Spring;
            if (spring != null)
            {
                spring.DisabledColor = invert ? color4 : color2;
                spring.VisibleWhenDisabled = true;
                spring.Depth = base.Depth + 1;
            }
        }
    }

    public override void Update()
    {
        base.Update();
        bool col2 = Collidable;
        Collidable = invert ? (!(Scene as Level).Session.GetFlag(flag) && !BlockedCheck()) : ((Scene as Level).Session.GetFlag(flag) && !BlockedCheck());
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
        flashTimer = Calc.Approach(flashTimer, 0, Engine.DeltaTime * 3f);
        bloom.Visible = Collidable;
    }

    public void Enable()
    {
        flashTimer = 1f;
        EnableStaticMovers();
        Depth = -9000;
        foreach (StaticMover staticMover in staticMovers)
        {
            staticMover.Entity.Depth = base.Depth + 1;
        }
    }

    public void Disable()
    {
        DisableStaticMovers();
        Depth = 100;
        foreach (StaticMover staticMover in staticMovers)
        {
            staticMover.Entity.Depth = base.Depth + 1;
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
        MTexture[,] ninSlice = active ? nineSliceOn : nineSliceOff;
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
            centerOn.DrawCentered(pos + new Vector2(width / 2f, height / 2f), color);
        }
        else
        {
            centerOff.DrawCentered(pos + new Vector2(width / 2f, height / 2f), color);
        }
    }
}
