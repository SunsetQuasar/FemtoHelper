using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;

public class RotateDashIndicator() : Component(true, true)
{
    public bool Anim = false;
    public float ArrowAngle = 0;
    public readonly MTexture[] Textures = [GFX.Game["objects/FemtoHelper/rotateRefillCCW/indicator_a"], GFX.Game["objects/FemtoHelper/rotateRefillCCW/indicator_b"]];
    public float Timer;

    public override void Update()
    {
        base.Update();
        if (Timer > 0 && !Anim) Timer = Math.Max(Timer - Engine.DeltaTime * 4, 0);
    }

    public override void Render()
    {
        if (FemtoModule.Session.HasRotateDash && (Entity as Player).Speed != Vector2.Zero)
        {
            float newAngle = CodecumberPortStuff.VectorToAngle((Entity as Player).Speed) - FemtoModule.Session.RotateDashAngle;
            Textures[0].DrawOutlineCentered(Entity.Center, Color.Black, 1, newAngle);
            Textures[0].DrawOutlineCentered(Entity.Center + Vector2.UnitY, Color.Black, 1, newAngle);
            Textures[0].DrawCentered(Entity.Center + Vector2.UnitY, FemtoModule.Session.RotateDashColors[2], 1, newAngle);
            Textures[0].DrawCentered(Entity.Center, FemtoModule.Session.RotateDashColors[1], 1, newAngle);

        }
        if (Anim || Timer > 0)
        {
            Textures[1].DrawCentered(Entity.Center, FemtoModule.Session.RotateDashColors[1] * Timer * 0.4f, 1, ArrowAngle);
        }
        base.Render();
    }
}