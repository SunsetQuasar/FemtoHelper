using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.PlutoniumText;

public static class ColorGradeNoPremultiply
{
    public static bool Enabled = true;

    private static MTexture from;

    private static MTexture to;

    private static float percent = 0f;

    private static Effect effect;
    public static Effect Effect {
        get
        {
            return effect ?? ColorGrade.Effect;
        }
        internal set
        {
            effect = value;
        }
    }

    public static float Percent
    {
        get
        {
            return percent;
        }
        set
        {
            Set(from, to, value);
        }
    }

    public static void Set(MTexture grade)
    {
        Set(grade, grade, 0f);
    }

    public static void LoadContent()
    {
        if (Everest.Content.TryGet("Effects/FemtoHelper/ColorGradeNoPremultiply.cso", out var modAsset))
        {
            Effect = new(Engine.Graphics.GraphicsDevice, modAsset.Data);
        } 
        else
        {
            Log("Failed to get Effect 'Effects/FemtoHelper/ColorGradeNoPremultiply.cso', falling back to ColorGrade.Effect. Some Plutonium Text will look incorrect!", LogLevel.Error);
        }
    }

    public static void Set(MTexture fromTex, MTexture toTex, float p)
    {
        if (!Enabled || fromTex == null || toTex == null)
        {
            from = GFX.ColorGrades["none"];
            to = GFX.ColorGrades["none"];
        }
        else
        {
            from = fromTex;
            to = toTex;
        }
        percent = Calc.Clamp(p, 0f, 1f);
        if (from == to || percent <= 0f)
        {
            Effect.CurrentTechnique = Effect.Techniques["ColorGradeSingle"];
            Engine.Graphics.GraphicsDevice.Textures[1] = from.Texture.Texture_Safe;
            return;
        }
        if (percent >= 1f)
        {
            Effect.CurrentTechnique = Effect.Techniques["ColorGradeSingle"];
            Engine.Graphics.GraphicsDevice.Textures[1] = to.Texture.Texture_Safe;
            return;
        }
        Effect.CurrentTechnique = Effect.Techniques["ColorGrade"];
        Effect.Parameters["percent"].SetValue(percent);
        Engine.Graphics.GraphicsDevice.Textures[1] = from.Texture.Texture_Safe;
        Engine.Graphics.GraphicsDevice.Textures[2] = to.Texture.Texture_Safe;
    }
}
