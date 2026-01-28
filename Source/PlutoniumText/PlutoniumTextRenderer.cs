using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.UI;

namespace Celeste.Mod.FemtoHelper.PlutoniumText;

[Tracked]
public class PlutoniumTextRenderer : Entity
{
    public TextLayer Layer;

    private List<PlutoniumTextComponent> texts = [];
    public PlutoniumTextRenderer(TextLayer layer) : base()
    {
        this.Layer = layer;
        AddTag(Tags.Global);
        if (layer == TextLayer.HUD) AddTag(TagsExt.SubHUD);
        Add(new BeforeRenderHook(BeforeRender));
    }

    public override void Render()
    {
        Level level = Scene as Level;
        base.Render();
        if(Layer == TextLayer.HUD)
        {
            MTexture orDefault = GFX.ColorGrades.GetOrDefault(level.lastColorGrade, GFX.ColorGrades["none"]);
            MTexture orDefault2 = GFX.ColorGrades.GetOrDefault(level.Session.ColorGrade, GFX.ColorGrades["none"]);

            if (level.colorGradeEase > 0f && orDefault != orDefault2)
            {
                ColorGradeNoPremultiply.Set(orDefault, orDefault2, level.colorGradeEase);
            }
            else
            {
                ColorGradeNoPremultiply.Set(orDefault2);
            }

            SubHudRenderer.EndRender();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGradeNoPremultiply.Effect, Matrix.CreateTranslation(SceneAs<Level>().Camera.Matrix.Translation * 6f) * (SubHudRenderer.DrawToBuffer ? Matrix.Identity : Engine.ScreenMatrix));

            foreach (PlutoniumTextComponent ptc in texts)
            {
                ptc.RenderCallback?.Invoke(level);
            }

            SubHudRenderer.EndRender();
            SubHudRenderer.BeginRender();
        }
    }

    public void BeforeRender()
    {
        foreach (PlutoniumTextComponent ptc in texts)
        {
            if (ptc.Scene is Level l) ptc.BeforeRenderCallback?.Invoke(l);
        }
    }

    public static void Track(PlutoniumTextComponent comp)
    {
        if (comp.Layer != TextLayer.Gameplay) GetRenderer(comp.Scene, comp.Layer).texts.Add(comp);
    }

    public static void Untrack(PlutoniumTextComponent comp)
    {
        if (comp.Layer != TextLayer.Gameplay) GetRenderer(comp.Scene, comp.Layer).texts.Remove(comp);
    }

    public static PlutoniumTextRenderer GetRenderer(Scene scene, TextLayer layer)
    {
        List<Entity> renderers = scene.Tracker.GetEntities<PlutoniumTextRenderer>();
        if (renderers.Any(e => e is PlutoniumTextRenderer r && r.Layer == layer))
        {
            return renderers[0] as PlutoniumTextRenderer;
        }
        else
        {
            PlutoniumTextRenderer ret = new(layer);
            scene.Add(ret);
            return ret;
        }
    }
}
