using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.FemtoHelper.Entities.Sokoban;

[CustomEntity("FemtoHelper/SokobanBoard")]
public class SokobanBoard : Entity
{
    internal static VirtualRenderTarget buffer;
    public List<SokobanObject> Objects = [];
    private Coroutine sequence;
    private Tween stepTween;
    public readonly string Room;
    public int boardWidth, boardHeight;
    public float stepPercent;
    public static bool LeftPressed => Input.Aim.Left.Pressed(Input.Gamepad, 0.25f);
    public static bool RightPressed => Input.Aim.Right.Pressed(Input.Gamepad, 0.25f);
    public static bool UpPressed => Input.Aim.Up.Pressed(Input.Gamepad, 0.25f);
    public static bool DownPressed => Input.Aim.Down.Pressed(Input.Gamepad, 0.25f);

    public Vector2 AimPress()
    {
        return 
            RightPressed ? new Vector2(1, 0) :
            DownPressed ? new Vector2(0, 1) :
            LeftPressed ? new Vector2(-1, 0) :
            UpPressed ? new Vector2(0, -1) : 
            Vector2.Zero;
    }

    public Stack<Stack<IHistoryAction>> History = new();

    public SokobanBoard(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Add(sequence = new(Sequence()));
        Add(new BeforeRenderHook(BeforeRender));
        Add(stepTween = new());
        Room = data.String("room");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Init(Room);
    }

    public void Init(string room)
    {
        LevelData data = (Scene as Level).Session.MapData.Get(room);
        if (data == null)
        {
            Warn($"SokobanBoard: Could not find room '{Room}'!");
            Add(new Text(Draw.DefaultFont, $"invalid room '{Room}'", Vector2.Zero, Color.Aquamarine));
            return;
        }

        boardWidth = data.TileBounds.Width;
        boardHeight = data.TileBounds.Height;

        foreach (EntityData d in data.Entities.Where(d => d.Name == "FemtoHelper/SokobanObject"))
        {
            Objects.Add(new SokobanObject()
            {
                parent = this,
                Pos = d.Position + Vector2.One * 4,
                PrevPos = d.Position + Vector2.One * 4,
                Type = d.Attr("type"),
                PrevType = d.Attr("type"),
            });
        }
    }

    public override void Update()
    {
        base.Update();
    }

    public void BeforeRender()
    {
        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.CreateTranslation(new Vector3(Position, 0)));
        foreach (SokobanObject obj in Objects)
        {
            obj.Render();
        }
        Draw.SpriteBatch.End();
    }

    public override void Render()
    {
        base.Render();
        Draw.SpriteBatch.Draw(buffer, Vector2.Zero, Color.White);
    }

    public void StepTween()
    {
        stepPercent = 0;

        stepTween.Init(Tween.TweenMode.Persist, Ease.CubeOut, 0.1f, true);
        stepTween.OnUpdate = (t) =>
        {
            stepPercent = t.Eased;
        };
        Add(stepTween);
    }

    public IEnumerator Sequence()
    {
        while (true)
        {
            while (true)
            {
                if (AimPress() != Vector2.Zero)
                {
                    Stack<IHistoryAction> current = new();
                    foreach (SokobanObject obj in Objects)
                    {
                        obj.PrevPos = obj.Pos;
                        SokobanObjectType type = obj.GetSokobanType();
                        Vector2 move = type.OnInput?.Invoke(obj, AimPress(), Input.MenuConfirm) ?? Vector2.Zero;
                        move = type.TryMove?.Invoke(obj, move) ?? Vector2.Zero;
                        if (move != Vector2.Zero)
                        {
                            SokobanMove a = new(obj, obj.Pos, obj.Pos + move);
                            current.Push(a);
                            a.Do();
                        } 
                    }
                    if (current.Count > 0) //if any actions were added
                    {
                        History.Push(current);
                    }
                    break;
                }
                else if (Input.MenuCancel.Pressed && History.Count > 0)
                {
                    Input.MenuCancel.ConsumeBuffer();
                    foreach (SokobanObject obj in Objects)
                    {
                        obj.PrevPos = obj.Pos;
                    }
                    if (History.TryPop(out var s))
                    {
                        while (s.TryPop(out var h))
                        {
                            h.Undo();
                        }
                    }
                    break;
                }
                yield return null;
            }
            StepTween();
            yield return null;
        }
    }

    [OnLoadContent]
    public static void OnLoadContent(bool firstLoad)
    {
        buffer ??= VirtualContent.CreateRenderTarget("femtohelper-sokoban", 320, 180);
    }

    [OnUnload]
    public static void Unload()
    {
        buffer.Dispose();
    }
}
