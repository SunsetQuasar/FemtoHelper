// Celeste.CustomParallaxBigWaterfall
using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Helpers;

[CustomEntity("FemtoHelper/CustomParallaxBigWaterfall")]

public class CustomParallaxBigWaterfall : Entity
{
    private enum Layers
    {
        Fg,
        Bg
    }

    private enum DisplacementType
    {
        None,
        Vanilla,
        Custom,
    }

    private readonly Layers layer;

    private readonly DisplacementType displacementType;

    private readonly float width;

    private readonly float height;

    private readonly float parallax;

    private readonly float fallSpeedMultiplier;

    private readonly List<float> lines = [];

    private readonly Color surfaceColor;

    private readonly Color fillColor;

    private float sine;

    private readonly SoundSource loopingSfx;

    private float fade;

    private readonly bool smooth;

    public bool UseDisplacement => displacementType != DisplacementType.None;

    private Vector2 RenderPosition => RenderPositionAtCamera((Scene as Level).Camera.Position + new Vector2(160f, 90f));

    public CustomParallaxBigWaterfall(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Tag = Tags.TransitionUpdate;
        layer = data.Enum("layer", Layers.Bg);
        width = data.Width;
        height = data.Height;
        fallSpeedMultiplier = data.Float("fallSpeedMultiplier");
        var surfaceOpacity = data.Float("surfaceOpacity");
        var fillOpacity = data.Float("fillOpacity");

        Depth = data.Int("depth", layer == Layers.Fg ? -49900 : 10010);

        smooth = data.Bool("smooth", layer == Layers.Fg);

        bool playSoundSource = !data.Bool("silent", layer == Layers.Bg);

        if (playSoundSource)
        {
            Add(loopingSfx = new SoundSource());
            loopingSfx.Play("event:/env/local/waterfall_big_main");
        }

        displacementType = data.Enum("displacementType", layer == Layers.Fg ? DisplacementType.Vanilla : DisplacementType.None);

        if (UseDisplacement)
        {
            Add(new DisplacementRenderHook(RenderDisplacement));
        }

        if (displacementType == DisplacementType.Vanilla)
        {
            lines.Add(3f);
            lines.Add(width - 4f);
        }
        else
        {
            lines.Add(6f);
            lines.Add(width - 7f);
        }
        parallax = data.Float("parallax");
        surfaceColor = Calc.HexToColor(data.Attr("surfaceColor")) * surfaceOpacity;
        fillColor = Calc.HexToColor(data.Attr("fillColor")) * fillOpacity;

        fade = 1f;
        Add(new TransitionListener
        {
            OnIn = delegate (float f)
            {
                fade = f;
            },
            OnOut = delegate (float f)
            {
                fade = 1f - f;
            }
        });
        if (!(width > 16f)) return;
        int num = Calc.Random.Next((int)(width / 16f));
        for (int i = 0; i < num; i++)
        {
            lines.Add(8f + Calc.Random.NextFloat(width - 16f));
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if ((Scene as Level).Transitioning)
        {
            fade = 0f;
        }
    }

    public Vector2 RenderPositionAtCamera(Vector2 camera)
    {
        Vector2 value = Position + new Vector2(width, height) / 2f - camera;
        Vector2 zero = Vector2.Zero;
        zero -= value * (1f - parallax);
        return Position + zero;
    }

    public void RenderDisplacement()
    {
        float x = RenderPosition.X;

        if (!CullHelper.IsRectangleVisible(x, Y, width, height))
        {
            return;
        }

        if (displacementType == DisplacementType.Vanilla)
        {
            Draw.Rect(x, Y, width, height, new Color(0.5f, 0.5f, 1f, 1f));
            return;
        }

        if (displacementType == DisplacementType.None)
        {
            return;
        }

        Vector2 position = (Scene as Level).Camera.Position;
        int segLen = smooth ? 1 : 3;
        float startY = Math.Max(Y, (float)Math.Floor(position.Y / segLen) * segLen);
        float endY = Math.Min(Y + height, position.Y + 180f);
        for (float y = startY; y < endY; y += segLen)
        {
            int distort = (int)(Math.Sin(y / 6f - sine * 8f) * 2.0);
            Draw.Rect(x+1, y, width-2, segLen, new Color(0.5f + (distort / 32f), 0.5f, 0f, 1f));
        }
    }

    public override void Update()
    {
        sine += Engine.DeltaTime * fallSpeedMultiplier;
        if (loopingSfx != null)
        {
            Vector2 position = (Scene as Level).Camera.Position;
            loopingSfx.Position = new Vector2(RenderPosition.X - X, Calc.Clamp(position.Y + 90f, Y, height) - Y);
        }
        base.Update();
    }

    public override void Render()
    {
        float x = RenderPosition.X;

        if (!CullHelper.IsRectangleVisible(x, Y, width, height))
        {
            return;
        }

        Color fill = fillColor * fade;
        Color surface = surfaceColor * fade;
        Draw.Rect(x, Y, width, height, fill);
        if (UseDisplacement)
        {
            float overdraw = displacementType == DisplacementType.Custom ? 0f : 1f;

            Draw.Rect(x - overdraw, Y, 3f, height, surface);
            Draw.Rect(x + (width - 3f) + overdraw, Y, 3f, height, surface);
            foreach (float line in lines)
            {
                Draw.Rect(x + line, Y, 1f, height, surface);
            }
            return;
        } 
        Vector2 position = (Scene as Level).Camera.Position;
        int segLen = smooth ? 1 : 3;
        float startY = Math.Max(Y, (float)Math.Floor(position.Y / segLen) * segLen);
        float endY = Math.Min(Y + height, position.Y + 180f);
        for (float y = startY; y < endY; y += segLen)
        {
            int distort = (int)(Math.Sin(y / 6f - sine * 8f) * 2.0);
            Draw.Rect(x, y, 4 + distort, segLen, surface);
            Draw.Rect(x + width - 4f + distort, y, 4 - distort, segLen, surface);
            foreach (float line2 in lines)
            {
                Draw.Rect(x + distort + line2, y, 1f, segLen, surface);
            }
        }
    }
}
