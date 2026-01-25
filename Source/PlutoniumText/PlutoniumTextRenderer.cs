using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        base.Render();
        foreach (PlutoniumTextComponent ptc in texts)
        {
            if (ptc.Scene is Level l) ptc.RenderCallback?.Invoke(l);
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
