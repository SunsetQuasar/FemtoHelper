using System;
using Celeste;
using Celeste.Mod.FemtoHelper.Entities;
using FMOD.Studio;

[TrackedAs(typeof(Water))]
[CustomEntity("FemtoHelper/WaterSwapBlock")]
public class WaterSwapBlock : GenericWaterBlock
{
    private class PathRenderer : Entity
    {
        private WaterSwapBlock block;

        private float timer;

        
        public PathRenderer(WaterSwapBlock block)
            : base(block.Position)
        {
            this.block = block;
            Depth = 8999;
            timer = Calc.Random.NextFloat();
        }

        
        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime * 4f;
        }

        
        public override void Render()
        {
            float num = 0.5f * (0.5f + ((float)Math.Sin(timer) + 1f) * 0.25f);
            block.DrawBlockStyle(new Vector2(block.moveRect.X, block.moveRect.Y), block.moveRect.Width, block.moveRect.Height, block.nineSliceTarget, null, Color.White * num);
        }
    }

    private WaterSprite waterSprite;

    private Vector2 start;

    private Vector2 end;

    private Rectangle moveRect;

    private DisplacementRenderer.Burst burst;

    public bool Swapping;

    private float lerp;

    private int target;

    private float returnTimer;

    private float redAlpha = 1f;

    private float speed;

    private float maxForwardSpeed;

    private float maxBackwardSpeed;

    private EventInstance moveSfx;

    private EventInstance returnSfx;

    private MTexture[,] nineSliceTarget;

    private PathRenderer path;

    private Sprite middleGreen;

    private Sprite middleRed;

    private float particlesRemainder;

    private readonly bool toggle;
    public WaterSwapBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, data.Bool("canCarry", true))
    {
        toggle = data.Bool("toggle", false);
        Depth = -9999;
        string prefix = data.Attr("spritePath", "objects/FemtoHelper/waterSwapBlock/");
        Vector2 node = data.Nodes[0] + offset;
        start = Position;
        end = node;
        Add(waterSprite = new WaterSprite(prefix + "nineSlice"));
        Add(new DashListener
        {
            OnDash = OnDash
        });
        int num = (int)MathHelper.Min(X, node.X);
        int num2 = (int)MathHelper.Min(Y, node.Y);
        int num3 = (int)MathHelper.Max(X + Width, node.X + Width);
        int num4 = (int)MathHelper.Max(Y + Height, node.Y + Height);
        moveRect = new Rectangle(num, num2, num3 - num, num4 - num2);
        maxForwardSpeed = 360f / Vector2.Distance(start, end);
        maxBackwardSpeed = maxForwardSpeed * 0.4f;
        MTexture mTexture3 = GFX.Game[prefix+"target"+(toggle?"Toggle":"")];
        nineSliceTarget = new MTexture[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                nineSliceTarget[i, j] = mTexture3.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }
        Add(middleGreen = new Sprite(GFX.Game, prefix + "midBlock")
        {
            Position = new Vector2(Width, Height) / 2
        });
        middleGreen.AddLoop("idle", "", 0.08f);
        middleGreen.Play("idle");
        middleGreen.CenterOrigin();
        Add(middleRed = new Sprite(GFX.Game, prefix + "midBlockRed")
        {
            Position = new Vector2(Width, Height) / 2
        });
        middleRed.AddLoop("idle", "", 0.08f);
        middleRed.Play("idle");
        middleRed.CenterOrigin();

    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        scene.Add(path = new PathRenderer(this));
    }

    public override void Update()
    {
        base.Update();
        if (returnTimer > 0f)
        {
            returnTimer -= Engine.DeltaTime;
            if (returnTimer <= 0f)
            {
                speed = 0f;
                if (!toggle)
                {
                    target = 0;
                    returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", Center);
                }
            }
        }
        if (burst != null)
        {
            burst.Position = Center;
        }
        redAlpha = Calc.Approach(redAlpha, (target != 1) ? 1 : 0, Engine.DeltaTime * 32f);
        if (target == 0 && lerp == 0f)
        {
            middleRed.SetAnimationFrame(0);
            middleGreen.SetAnimationFrame(0);
        }
        if (target == 1)
        {
            speed = Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);
        }
        else
        {
            speed = Calc.Approach(speed, maxBackwardSpeed, maxBackwardSpeed / 1.5f * Engine.DeltaTime);
        }
        float num = lerp;
        lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
        if (start == end)
        {
            lerp = target;
        }
        if (lerp != num)
        {
            Vector2 liftSpeed = (end - start) * speed;
            Vector2 position = Position;
            if (target == 1)
            {
                liftSpeed = (end - start) * maxForwardSpeed;
            }
            if (lerp < num)
            {
                liftSpeed *= -1f;
            }
            if ((target == 1 || toggle) && Scene.OnInterval(0.02f))
            {
                Vector2 normal = ((target != 1) ? (start - end) : (end - start));
                MoveParticles(normal);
            }
            if (start != end)
            {
                MoveTo(Vector2.Lerp(start, end, lerp), liftSpeed);
            }
            //MoveTo(Vector2.Lerp(start, end, lerp), liftSpeed);
            if (position != Position)
            {
                Audio.Position(moveSfx, Center);
                Audio.Position(returnSfx, Center);
                if (Position == start && target == 0)
                {
                    Audio.SetParameter(returnSfx, "end", 1f);
                    Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", Center);
                    waterSprite.Wiggle.Start();
                }
                else if (Position == end && target == 1)
                {
                    Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
                    waterSprite.Wiggle.Start();
                }
            }
        }
        if (Swapping && lerp >= 1f)
        {
            Swapping = false;
        }
    }

    private void OnDash(Vector2 direction)
    {
        Swapping = lerp < 1f;
        target = toggle ? ((target != 1) ? 1 : 0) : 1;
        returnTimer = 0.8f;
        burst = (Scene as Level).Displacement.AddBurst(Center, 0.2f, 0f, 16f);
        waterSprite.Wiggle.Start();
        if (lerp >= 0.2f)
        {
            speed = maxForwardSpeed;
        }
        else
        {
            speed = MathHelper.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, lerp / 0.2f);
        }
        Audio.Stop(returnSfx);
        Audio.Stop(moveSfx);
        if (!Swapping)
        {
            Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
        }
        else
        {
            moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", Center);
        }
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        Audio.Stop(moveSfx);
        Audio.Stop(returnSfx);
    }
    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        Audio.Stop(moveSfx);
        Audio.Stop(returnSfx);
    }

    private void MoveParticles(Vector2 normal)
    {
        Vector2 position;
        Vector2 positionRange;
        float direction;
        float num;
        if (normal.X > 0f)
        {
            position = CenterLeft;
            positionRange = Vector2.UnitY * (Height - 6f);
            direction = MathF.PI;
            num = Math.Max(2f, Height / 14f);
        }
        else if (normal.X < 0f)
        {
            position = CenterRight;
            positionRange = Vector2.UnitY * (Height - 6f);
            direction = 0f;
            num = Math.Max(2f, Height / 14f);
        }
        else if (normal.Y > 0f)
        {
            position = TopCenter;
            positionRange = Vector2.UnitX * (Width - 6f);
            direction = -MathF.PI / 2f;
            num = Math.Max(2f, Width / 14f);
        }
        else
        {
            position = BottomCenter;
            positionRange = Vector2.UnitX * (Width - 6f);
            direction = MathF.PI / 2f;
            num = Math.Max(2f, Width / 14f);
        }
        particlesRemainder += num;
        int num2 = (int)particlesRemainder;
        particlesRemainder -= num2;
        positionRange *= 0.5f;
        SceneAs<Level>().Particles.Emit(Tinydrops, num2, position, positionRange, direction);
    }

    public override void Render()
    {
        if (redAlpha < 1f)
        {
            middleGreen.Visible = true;
            middleRed.Visible = false;
        }
        if (redAlpha > 0f)
        {
            middleGreen.Visible = false;
            middleRed.Visible = true;
        }
        if (toggle)
        {
            middleRed.Visible = !middleRed.Visible;
            middleGreen.Visible = !middleGreen.Visible;
        }
        base.Render();
    }

    private void DrawBlockStyle(Vector2 pos, float width, float height, MTexture[,] ninSlice, Sprite middle, Color color)
    {
        int num = (int)(width / 8f);
        int num2 = (int)(height / 8f);
        ninSlice[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
        ninSlice[2, 0].Draw(pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
        ninSlice[0, 2].Draw(pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
        ninSlice[2, 2].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);
        for (int i = 1; i < num - 1; i++)
        {
            ninSlice[1, 0].Draw(pos + new Vector2(i * 8, 0f), Vector2.Zero, color);
            ninSlice[1, 2].Draw(pos + new Vector2(i * 8, height - 8f), Vector2.Zero, color);
        }
        for (int j = 1; j < num2 - 1; j++)
        {
            ninSlice[0, 1].Draw(pos + new Vector2(0f, j * 8), Vector2.Zero, color);
            ninSlice[2, 1].Draw(pos + new Vector2(width - 8f, j * 8), Vector2.Zero, color);
        }
        for (int k = 1; k < num - 1; k++)
        {
            for (int l = 1; l < num2 - 1; l++)
            {
                ninSlice[1, 1].Draw(pos + new Vector2(k, l) * 8f, Vector2.Zero, color);
            }
        }
    }
}
