using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

[TrackedAs(typeof(Water))]
[CustomEntity("FemtoHelper/WaterSwitchGate")]
public class WaterSwitchGate : GenericWaterBlock
{
    private Sprite icon;

    private Vector2 node;

    private SoundSource openSfx;

    private bool persistent;

    private Color inactiveColor = Calc.HexToColor("5fcde4");

    private Color activeColor = Color.White;

    private Color finishColor = Calc.HexToColor("f141df");

    private WaterSprite waterSprite;

    private Wiggler wiggler;

    private Vector2 iconOffset;
    public WaterSwitchGate(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, data.Bool("canCarry", true))
    {
        string path = data.Attr("spritePath", "objects/FemtoHelper/waterSwitchGate/");

        Add(waterSprite = new WaterSprite(path + "nineSlice"));
        node = data.Nodes[0] + offset;
        persistent = data.Bool("persistent");
        Add(icon = new Sprite(GFX.Game, path + "icon"));
        icon.Add("spin", "", 0.1f, "spin");
        icon.Play("spin");
        icon.Rate = 0f;
        icon.Color = inactiveColor;
        icon.Position = (iconOffset = new Vector2(data.Width / 2f, data.Height / 2f));
        icon.CenterOrigin();
        Add(wiggler = Wiggler.Create(0.5f, 4f, (float f) =>
        {
            icon.Scale = Vector2.One * (1f + f);
        }));
        Add(openSfx = new SoundSource());
    }

    public void WiggleEverything()
    {
        waterSprite.Wiggle.Start();
        wiggler.Start();
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (Switch.CheckLevelFlag(SceneAs<Level>()))
        {
            MoveTo(node);
            icon.Rate = 0f;
            icon.SetAnimationFrame(0);
            icon.Color = finishColor;
        }
        else
        {
            Add(new Coroutine(Sequence(node)));
        }
    }


    private IEnumerator Sequence(Vector2 node)
    {
        Vector2 start = Position;
        while (!Switch.Check(Scene))
        {
            yield return null;
        }
        if (persistent)
        {
            Switch.SetLevelFlag(SceneAs<Level>());
        }
        yield return 0.1f;
        waterSprite.Wiggle.Start();
        openSfx.Play("event:/game/general/touchswitch_gate_open");
        StartShaking(0.5f);
        while (icon.Rate < 1f)
        {
            icon.Color = Color.Lerp(inactiveColor, activeColor, icon.Rate);
            icon.Rate += Engine.DeltaTime * 2f;
            yield return null;
        }
        yield return 0.1f;
        int particleAt = 0;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 2f, start: true);
        tween.OnUpdate = (Tween t) =>
        {
            MoveTo(Vector2.Lerp(start, node, t.Eased));
            if (Scene.OnInterval(0.1f))
            {
                particleAt++;
                particleAt %= 2;
                for (int i = 0; (float)i < Width / 8f; i++)
                {
                    for (int j = 0; (float)j < Height / 8f; j++)
                    {
                        if ((i + j) % 2 == particleAt)
                        {
                            SceneAs<Level>().ParticlesBG.Emit(Tinydrops, Position + new Vector2(i * 8, j * 8) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
                        }
                    }
                }
            }
        };
        Add(tween);
        yield return 1.8f;
        bool collidable = Collidable;
        Collidable = false;
        if (node.X <= start.X)
        {
            Vector2 vector = new Vector2(0f, 2f);
            for (int k = 0; (float)k < Height / 8f; k++)
            {
                Vector2 vector2 = new Vector2(Left - 1f, Top + 4f + (float)(k * 8));
                Vector2 point = vector2 + Vector2.UnitX;
                if (Scene.CollideCheck<Solid>(vector2) && !Scene.CollideCheck<Solid>(point))
                {
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector2 + vector, MathF.PI);
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector2 - vector, MathF.PI);
                }
            }
        }
        if (node.X >= start.X)
        {
            Vector2 vector3 = new Vector2(0f, 2f);
            for (int l = 0; (float)l < Height / 8f; l++)
            {
                Vector2 vector4 = new Vector2(Right + 1f, Top + 4f + (float)(l * 8));
                Vector2 point2 = vector4 - Vector2.UnitX * 2f;
                if (Scene.CollideCheck<Solid>(vector4) && !Scene.CollideCheck<Solid>(point2))
                {
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector4 + vector3, 0f);
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector4 - vector3, 0f);
                }
            }
        }
        if (node.Y <= start.Y)
        {
            Vector2 vector5 = new Vector2(2f, 0f);
            for (int m = 0; (float)m < Width / 8f; m++)
            {
                Vector2 vector6 = new Vector2(Left + 4f + (float)(m * 8), Top - 1f);
                Vector2 point3 = vector6 + Vector2.UnitY;
                if (Scene.CollideCheck<Solid>(vector6) && !Scene.CollideCheck<Solid>(point3))
                {
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector6 + vector5, -MathF.PI / 2f);
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector6 - vector5, -MathF.PI / 2f);
                }
            }
        }
        if (node.Y >= start.Y)
        {
            Vector2 vector7 = new Vector2(2f, 0f);
            for (int n = 0; (float)n < Width / 8f; n++)
            {
                Vector2 vector8 = new Vector2(Left + 4f + (float)(n * 8), Bottom + 1f);
                Vector2 point4 = vector8 - Vector2.UnitY * 2f;
                if (Scene.CollideCheck<Solid>(vector8) && !Scene.CollideCheck<Solid>(point4))
                {
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector8 + vector7, MathF.PI / 2f);
                    SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, vector8 - vector7, MathF.PI / 2f);
                }
            }
        }
        Collidable = collidable;
        Audio.Play("event:/game/general/touchswitch_gate_finish", Position);
        StartShaking(0.2f);
        while (icon.Rate > 0f)
        {
            icon.Color = Color.Lerp(activeColor, finishColor, 1f - icon.Rate);
            icon.Rate -= Engine.DeltaTime * 4f;
            yield return null;
        }
        icon.Rate = 0f;
        icon.SetAnimationFrame(0);
        WiggleEverything();
        bool collidable2 = Collidable;
        Collidable = false;
        if (!Scene.CollideCheck<Solid>(Center))
        {
            for (int num = 0; num < 32; num++)
            {
                float num2 = Calc.Random.NextFloat(MathF.PI * 2f);
                SceneAs<Level>().ParticlesFG.Emit(TouchSwitch.P_Fire, Position + iconOffset + Calc.AngleToVector(num2, 4f), num2);
            }
        }
        Collidable = collidable2;
    }

    public override void Render()
    {
        Vector2 temp = Position;
        Position += Shake;
        icon.Position = iconOffset + Shake;
        icon.DrawOutline();
        base.Render();
        Position = temp;
    }
}
