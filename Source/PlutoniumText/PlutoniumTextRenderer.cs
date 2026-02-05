using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.UI;
using System;
using MonoMod.Cil;
using Celeste.Mod.Helpers;

namespace Celeste.Mod.FemtoHelper.PlutoniumText;

[Tracked]
public class PlutoniumTextRenderer : Entity
{
    public TextLayer Layer;

    private List<PlutoniumTextComponent> texts = [];

    private static readonly Comparison<PlutoniumTextComponent> compareDepth = (a, b) => (int)(b.Entity.actualDepth - a.Entity.actualDepth);
    public void SortEntities()
    {
        texts.Sort(compareDepth);
    }

    public static void LoadContent()
    {
        
    }

    public static void Load()
    {
        IL.Celeste.Level.Render += Level_Render;
        Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
    }

    public static void Unload()
    {
        IL.Celeste.Level.Render -= Level_Render;
        Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
    }

    private static void Level_Render(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNextBestFit(MoveType.Before, instr => instr.MatchLdarg0(), instr => instr.MatchLdfld<Level>("Background"), instr => instr.MatchLdarg0()))
        {
            cursor.EmitLdarg0();
            cursor.EmitLdcI4(0);
            cursor.EmitDelegate(RenderThis);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<Renderer>("Render")))
            {
                cursor.EmitLdarg0();
                cursor.EmitLdcI4(1);
                cursor.EmitDelegate(RenderThis);
            }
        }

        if (cursor.TryGotoNextBestFit(MoveType.Before, instr => instr.MatchLdarg0(), instr => instr.MatchLdfld<Level>("Foreground"), instr => instr.MatchLdarg0()))
        {
            cursor.EmitLdarg0();
            cursor.EmitLdcI4(2);
            cursor.EmitDelegate(RenderThis);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<Renderer>("Render")))
            {
                cursor.EmitLdarg0();
                cursor.EmitLdcI4(3);
                cursor.EmitDelegate(RenderThis);
            }
        }

        if (cursor.TryGotoNextBestFit(MoveType.Before, instr => instr.MatchLdarg0(), instr => instr.MatchLdfld<Level>("SubHudRenderer"), instr => instr.MatchLdarg0()))
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(RenderHUD);
        }
    }
    private static void LevelLoader_OnLoadingThread(Level level)
    {
        level.Add(new PlutoniumTextRenderer(TextLayer.BelowBG));
        level.Add(new PlutoniumTextRenderer(TextLayer.AboveBG));
        level.Add(new PlutoniumTextRenderer(TextLayer.BelowFG));
        level.Add(new PlutoniumTextRenderer(TextLayer.AboveFG));
        level.Add(new PlutoniumTextRenderer(TextLayer.HUD));
        level.Add(new PlutoniumTextRenderer(TextLayer.AdditiveHUD));
    }

    private static void RenderHUD(Level level)
    {
        PlutoniumTextRenderer renderer = GetRenderer(level, TextLayer.AdditiveHUD);

        if (renderer is null) return;

        renderer.DrawAdditiveHUD(level);
    }

    private static void RenderThis(Level level, int index)
    {
        TextLayer layer = index switch
        {
            0 => TextLayer.BelowBG,
            1 => TextLayer.AboveBG,
            2 => TextLayer.BelowFG,
            _ => TextLayer.AboveFG,
        };

        //Log($"index is {index}");

        PlutoniumTextRenderer renderer = GetRenderer(level, layer);

        if (renderer is null) return;

        renderer.DrawBuffer(level);
    }

    public void DrawAdditiveHUD(Level level)
    {
        foreach (PlutoniumTextComponent ptc in texts)
        {
            if (ptc.Scene is Level l) ptc.BeforeRenderCallback?.Invoke(l);
        }


        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, Matrix.CreateTranslation(SceneAs<Level>().Camera.Matrix.Translation * 6f) * Matrix.CreateScale(level.Zoom) * (Engine.ScreenMatrix));

        foreach (PlutoniumTextComponent ptc in texts)
        {
            if (ptc.Scene is Level l) ptc.RenderCallback?.Invoke(l);
        }

        Draw.SpriteBatch.End();

    }

    public void DrawBuffer(Level level)
    {
        foreach (PlutoniumTextComponent ptc in texts)
        {
            if (ptc.Scene is Level l) ptc.BeforeRenderCallback?.Invoke(l);
        }

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, SceneAs<Level>().Camera.Matrix);

        foreach (PlutoniumTextComponent ptc in texts)
        {
            if (ptc.Scene is Level l) ptc.RenderCallback?.Invoke(l);
        }

        Draw.SpriteBatch.End();
    }

    public PlutoniumTextRenderer(TextLayer layer) : base()
    {
        this.Layer = layer;
        AddTag(Tags.Global);
        if (layer == TextLayer.HUD || layer == TextLayer.AdditiveHUD) AddTag(TagsExt.SubHUD);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
    }

    public override void Render()
    {
        base.Render();

        Level level = Scene as Level;

        MTexture orDefault = GFX.ColorGrades.GetOrDefault(level.lastColorGrade, GFX.ColorGrades["none"]);
        MTexture orDefault2 = GFX.ColorGrades.GetOrDefault(level.Session.ColorGrade, GFX.ColorGrades["none"]);

        if (level.colorGradeEase > 0f && orDefault != orDefault2)
        {
            ColorGrade.Set(orDefault, orDefault2, level.colorGradeEase);
        }
        else
        {
            ColorGrade.Set(orDefault2);
        }

        //bullshit
        if (Layer == TextLayer.HUD)
        {
            SubHudRenderer.EndRender();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, Matrix.CreateTranslation(SceneAs<Level>().Camera.Matrix.Translation * 6f) * Matrix.CreateScale(level.Zoom) * (SubHudRenderer.DrawToBuffer ? Matrix.Identity : Engine.ScreenMatrix));

            foreach (PlutoniumTextComponent ptc in texts)
            {
                ptc.RenderCallback?.Invoke(level);
            }

            SubHudRenderer.EndRender();
            SubHudRenderer.BeginRender();
        }
    }

    public static void Track(PlutoniumTextComponent comp)
    {
        if (comp.Layer != TextLayer.Gameplay)
        {
            PlutoniumTextRenderer renderer = GetRenderer(comp.Scene, comp.Layer);
            if (renderer is null) return;
            renderer.texts.Add(comp);
            renderer.texts.Sort(compareDepth);
        }
    }

    public static void Untrack(PlutoniumTextComponent comp)
    {
        if (comp.Layer != TextLayer.Gameplay)
        {
            PlutoniumTextRenderer renderer = GetRenderer(comp.Scene, comp.Layer);
            if (renderer is null) return;
            renderer.texts.Remove(comp);
            renderer.texts.Sort(compareDepth);
        }
    }

    public static PlutoniumTextRenderer GetRenderer(Scene scene, TextLayer layer)
    {
        foreach (PlutoniumTextRenderer renderer in scene.Tracker.GetEntities<PlutoniumTextRenderer>().Where(e => e is PlutoniumTextRenderer r && r.Layer == layer))
        {
            return renderer;
        }

        return null;
    }
}
