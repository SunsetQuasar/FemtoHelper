using Celeste.Mod.Helpers;
using System;
using static Monocle.Ease;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/DirectionalLine")]
public class DirectionalLine : Entity
{
    public readonly MTexture Sprite;
    public readonly int Count;
    public readonly Tween Tween;
    public readonly Easer PositionEaser;
    public readonly Easer AlphaEaserIn;
    public readonly Easer AlphaEaserOut;
    public readonly bool OrientSprite;
    public Vector2 Endpoint;
    public Color Color;
    public float ActivationAlpha;
    public readonly string ActivationFlag;
    public readonly string DeactivationFlag;
    public readonly float ActivationTime;
    public readonly float DeactivationTime;
    public readonly float AlphaInPercent;
    public readonly float AlphaOutPercent;
    public readonly float AlphaMult;
    public DirectionalLine(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Count = Math.Max(data.Int("spriteCount", 5), 1);
        Tween = Tween.Create(Tween.TweenMode.Looping, Linear, data.Float("duration", 1f), true);
        Add(Tween);
        PositionEaser = GetEaser(data.Attr("positionEase", "CubeInOut"));
        AlphaEaserIn = GetEaser(data.Attr("alphaInEase", "CubeInOut"));
        AlphaEaserOut = GetEaser(data.Attr("alphaOutEase", "CubeInOut"));
        AlphaInPercent = Calc.Clamp(data.Float("alphaInPercent", 0.3f), 0, 1);
        AlphaOutPercent = Calc.Clamp(data.Float("alphaOutPercent", 0.3f), 0, 1);
        AlphaInPercent = Calc.Clamp(AlphaInPercent, 0, 1 - AlphaOutPercent);
        AlphaOutPercent = Calc.Clamp(AlphaOutPercent, 0, 1 - AlphaInPercent);
        if (AlphaInPercent + AlphaOutPercent > 1) AlphaInPercent = AlphaOutPercent = 0.5f;
        Sprite = GFX.Game[data.Attr("texture", "objects/FemtoHelper/directionalArrow/arrow")];
        Color = Calc.HexToColorWithAlpha(data.Attr("color", "ffffffff"));
        Endpoint = data.NodesOffset(offset)[0];
        OrientSprite = data.Bool("orientSprite", true);
        ActivationFlag = data.Attr("activationFlag", "");
        DeactivationFlag = data.Attr("deactivationFlag", "");
        ActivationTime = data.Float("activationTime", 1);
        DeactivationTime = data.Float("deactivationTime", 1);

        ActivationAlpha = string.IsNullOrEmpty(ActivationFlag) ? 1f : 0f;

        AlphaMult = data.Float("alphaMultiplier", 1);

        Depth = data.Int("depth", -250);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level level = scene as Level;
        if (!string.IsNullOrEmpty(DeactivationFlag) && level.Session.GetFlag(DeactivationFlag))
        {
            ActivationAlpha = 0;
        }
        else if (ActivationAlpha < 1 && (string.IsNullOrEmpty(ActivationFlag) || level.Session.GetFlag(ActivationFlag)))
        {
            ActivationAlpha = 1;
        }
    }

    public override void Update()
    {
        base.Update();
        Level level = Scene as Level;
        if (!string.IsNullOrEmpty(DeactivationFlag) && level.Session.GetFlag(DeactivationFlag))
        {
            ActivationAlpha = Math.Max(ActivationAlpha - Engine.DeltaTime / Math.Max(DeactivationTime, float.Epsilon), 0);
        } 
        else if (ActivationAlpha < 1 && (string.IsNullOrEmpty(ActivationFlag) || level.Session.GetFlag(ActivationFlag)))
        {
            ActivationAlpha = Math.Min(ActivationAlpha + Engine.DeltaTime / Math.Max(ActivationTime, float.Epsilon), 1);
        }
    }

    public override void Render()
    {
        base.Render();
        if (ActivationAlpha == 0 || AlphaMult == 0) return;
        float x = (Endpoint - Position).X;
        float y = (Endpoint - Position).Y;
        for (float i = 0; i < 1; i += 1 / (float)Count)
        {
            Vector2 pos = GetEasedPos(Tween.Percent + i);
            if (!CullHelper.IsRectangleVisible(pos.X, pos.Y, Sprite.Width, Sprite.Height)) continue;
            Sprite.DrawCentered(pos, Color * GetAlphaEase(Tween.Percent + i) * ActivationAlpha * AlphaMult, 1, OrientSprite ? (float)Math.Atan2(y, x) : 0);
        }
    }

    public float GetAlphaEase(float t)
    {
        t = t % 1;
        if(t < AlphaInPercent)
        {
            return AlphaEaserIn(t * (1 / Math.Max(AlphaInPercent, float.Epsilon)));
        }
        else if (t > AlphaInPercent && t < 1 - AlphaOutPercent)
        {
            return 1;
        }
        else
        {
            return AlphaEaserOut((1 - t) * (1 / Math.Max(AlphaOutPercent, float.Epsilon)));
        }
    }

    public Vector2 GetEasedPos(float t)
    {
        return Vector2.Lerp(Position, Endpoint, PositionEaser(t%1));
    }

    public static Easer GetEaser(string ease)
    {
        return ease.Trim().ToLower() switch
        {
            "sinein" => SineIn,
            "sineout" => SineOut,
            "sineinout" => SineInOut,

            "quadin" => QuadIn,
            "quadout" => QuadOut,
            "quadinout" => QuadInOut,

            "cubein" => CubeIn,
            "cubeout" => CubeOut,
            "cubeinout" => CubeInOut,

            "quintin" => QuintIn,
            "quintout" => QuintOut,
            "quintinout" => QuintInOut,

            "expoin" => ExpoIn,
            "expoout" => ExpoOut,
            "expoinout" => ExpoInOut,

            "backin" => BackIn,
            "backout" => BackOut,
            "backinout" => BackInOut,

            "bigbackin" => BigBackIn,
            "bigbackout" => BigBackOut,
            "bigbackinout" => BigBackInOut,

            "elasticin" => ElasticIn,
            "elasticout" => ElasticOut,
            "elasticinout" => ElasticInOut,

            "bouncein" => BounceIn,
            "bounceout" => BounceOut,
            "bounceinout" => BounceInOut,

            _ => Linear,
        };
    }
}

