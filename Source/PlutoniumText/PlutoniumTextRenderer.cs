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

    private static VirtualRenderTarget belowBG, aboveBG, belowFG, aboveFG;

    public VirtualRenderTarget Buffer => Layer switch
    {
        TextLayer.BelowBG => belowBG,
        TextLayer.AboveBG => aboveBG,
        TextLayer.BelowFG => belowFG,
        TextLayer.AboveFG => aboveFG,
        _ => null
    };

    private static readonly Comparison<PlutoniumTextComponent> compareDepth = (a, b) => (int)(b.Entity.actualDepth - a.Entity.actualDepth);
    public void SortEntities()
    {
        texts.Sort(compareDepth);
    }

    public static void LoadContent()
    {
        CreateBuffers();
    }

    public static void Load()
    {
        On.Celeste.Level.Begin += Level_Begin;
        On.Celeste.Level.End += Level_End;
        IL.Celeste.Level.Render += Level_Render;
        Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
    }

    public static void Unload()
    {
        On.Celeste.Level.Begin -= Level_Begin;
        On.Celeste.Level.End -= Level_End;
        IL.Celeste.Level.Render -= Level_Render;
        Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
    }

    private static void CreateBuffers()
    {
        belowBG = VirtualContent.CreateRenderTarget("plutonium_text_buffer_belowBG", 320, 180);
        aboveBG = VirtualContent.CreateRenderTarget("plutonium_text_buffer_aboveBG", 320, 180);
        belowFG = VirtualContent.CreateRenderTarget("plutonium_text_buffer_belowFG", 320, 180);
        aboveFG = VirtualContent.CreateRenderTarget("plutonium_text_buffer_aboveFG", 320, 180);
    }

    private static void Level_Begin(On.Celeste.Level.orig_Begin orig, Level self)
    {
        orig(self);
        CreateBuffers();
    }

    private static void Level_End(On.Celeste.Level.orig_End orig, Level self)
    {
        orig(self);
        belowBG?.Dispose();
        aboveBG?.Dispose();
        belowFG?.Dispose();
        aboveFG?.Dispose();

        belowBG = aboveBG = belowFG = aboveFG = null;
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
    }
    private static void LevelLoader_OnLoadingThread(Level level)
    {
        level.Add(new PlutoniumTextRenderer(TextLayer.BelowBG));
        level.Add(new PlutoniumTextRenderer(TextLayer.AboveBG));
        level.Add(new PlutoniumTextRenderer(TextLayer.BelowFG));
        level.Add(new PlutoniumTextRenderer(TextLayer.AboveFG));
        level.Add(new PlutoniumTextRenderer(TextLayer.HUD));
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
        if (layer == TextLayer.HUD) AddTag(TagsExt.SubHUD);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
    }

    private static VirtualRenderTarget EnsureValidBuffer(VirtualRenderTarget buffer)
    {
        var gpBuffer = GameplayBuffers.Gameplay;

        string name = buffer.Name;

        int targetWidth = gpBuffer?.Width ?? 320;
        int targetHeight = gpBuffer?.Height ?? 180;

        if (gpBuffer is null || gpBuffer.Width == 320 || gpBuffer.Width == 321)
        {
            buffer ??= VirtualContent.CreateRenderTarget(name, targetWidth, targetHeight);
            return buffer;
        }

        // We need a bigger buffer due to zoomout.
        // We'll keep the 320x180 buffer around, in case some other cloudscape wants to render with ZoomBehavior=StaySame
        if (buffer is null || buffer.IsDisposed || buffer.Width != gpBuffer.Width)
        {
            buffer?.Dispose();
            buffer ??= VirtualContent.CreateRenderTarget(name, targetWidth, targetHeight);
        }

        return buffer;
    }

    public override void Render()
    {
        base.Render();

        //bullshit
        if (Layer == TextLayer.HUD)
        {
            Level level = Scene as Level;

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

            //am i stupid? works for now

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGradeNoPremultiply.Effect, Matrix.CreateTranslation(SceneAs<Level>().Camera.Matrix.Translation * 6f) * Matrix.CreateScale(level.Zoom) * (SubHudRenderer.DrawToBuffer ? Matrix.Identity : Engine.ScreenMatrix));

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
