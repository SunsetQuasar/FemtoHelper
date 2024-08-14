using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Monocle.Ease;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/DirectionalLine")]
public class DirectionalLine : Entity
{
    public MTexture sprite;
    public int count;
    public Tween tween;
    public Easer positionEaser;
    public Easer alphaEaserIn;
    public Easer alphaEaserOut;
    public bool orientSprite;
    public Vector2 endpoint;
    public Color color;
    public float activationAlpha;
    public string activationFlag;
    public string deactivationFlag;
    public float activationTime;
    public float deactivationTime;
    public float alphaInPercent;
    public float alphaOutPercent;
    public float alphaMult;
    public DirectionalLine(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        count = Math.Max(data.Int("spriteCount", 5), 1);
        tween = Tween.Create(Tween.TweenMode.Looping, Linear, data.Float("duration", 1f), true);
        Add(tween);
        positionEaser = GetEaser(data.Attr("positionEase", "CubeInOut"));
        alphaEaserIn = GetEaser(data.Attr("alphaInEase", "CubeInOut"));
        alphaEaserOut = GetEaser(data.Attr("alphaOutEase", "CubeInOut"));
        alphaInPercent = Calc.Clamp(data.Float("alphaInPercent", 0.3f), 0, 1);
        alphaOutPercent = Calc.Clamp(data.Float("alphaOutPercent", 0.3f), 0, 1);
        alphaInPercent = Calc.Clamp(alphaInPercent, 0, 1 - alphaOutPercent);
        alphaOutPercent = Calc.Clamp(alphaOutPercent, 0, 1 - alphaInPercent);
        if (alphaInPercent + alphaOutPercent > 1) alphaInPercent = alphaOutPercent = 0.5f;
        sprite = GFX.Game[data.Attr("texture", "objects/FemtoHelper/directionalArrow/arrow")];
        color = Calc.HexToColorWithAlpha(data.Attr("color", "ffffffff"));
        endpoint = data.NodesOffset(offset)[0];
        orientSprite = data.Bool("orientSprite", true);
        activationFlag = data.Attr("activationFlag", "");
        deactivationFlag = data.Attr("deactivationFlag", "");
        activationTime = data.Float("activationTime", 1);
        deactivationTime = data.Float("deactivationTime", 1);

        activationAlpha = string.IsNullOrEmpty(activationFlag) ? 1f : 0f;

        alphaMult = data.Float("alphaMultiplier", 1);

        Depth = data.Int("depth", -250);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level level = scene as Level;
        if (!string.IsNullOrEmpty(deactivationFlag) && level.Session.GetFlag(deactivationFlag))
        {
            activationAlpha = 0;
        }
        else if (activationAlpha < 1 && (string.IsNullOrEmpty(activationFlag) || level.Session.GetFlag(activationFlag)))
        {
            activationAlpha = 1;
        }
    }

    public override void Update()
    {
        base.Update();
        Level level = Scene as Level;
        if (!string.IsNullOrEmpty(deactivationFlag) && level.Session.GetFlag(deactivationFlag))
        {
            activationAlpha = Math.Max(activationAlpha - Engine.DeltaTime / Math.Max(deactivationTime, float.Epsilon), 0);
        } 
        else if (activationAlpha < 1 && (string.IsNullOrEmpty(activationFlag) || level.Session.GetFlag(activationFlag)))
        {
            activationAlpha = Math.Min(activationAlpha + Engine.DeltaTime / Math.Max(activationTime, float.Epsilon), 1);
        }
    }

    public override void Render()
    {
        base.Render();
        if (activationAlpha == 0 || alphaMult == 0) return;
        float x = (endpoint - Position).X;
        float y = (endpoint - Position).Y;
        for (float i = 0; i < 1; i += 1 / (float)count)
        {
            Vector2 pos = GetEasedPos(tween.Percent + i);
            if (!CullHelper.IsRectangleVisible(pos.X, pos.Y, sprite.Width, sprite.Height)) continue;
            sprite.DrawCentered(pos, color * getAlphaEase(tween.Percent + i) * activationAlpha * alphaMult, 1, orientSprite ? (float)Math.Atan2(y, x) : 0);
        }
    }

    public float getAlphaEase(float t)
    {
        t = t % 1;
        if(t < alphaInPercent)
        {
            return alphaEaserIn(t * (1 / Math.Max(alphaInPercent, float.Epsilon)));
        }
        else if (t > alphaInPercent && t < 1 - alphaOutPercent)
        {
            return 1;
        }
        else
        {
            return alphaEaserOut((1 - t) * (1 / Math.Max(alphaOutPercent, float.Epsilon)));
        }
    }

    public Vector2 GetEasedPos(float t)
    {
        return Vector2.Lerp(Position, endpoint, positionEaser(t%1));
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

