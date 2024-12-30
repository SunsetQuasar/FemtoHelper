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
    public readonly MTexture Texture = GFX.Game["objects/FemtoHelper/rotateRefillCCW/indicator_a"];

    public override void Update()
    {
        base.Update();
    }

    public override void Render()
    {
        if (FemtoModule.Session.HasRotateDash && (Entity as Player).Speed != Vector2.Zero)
        {
            float newAngle = CodecumberPortStuff.VectorToAngle((Entity as Player).Speed) - FemtoModule.Session.RotateDashAngle;
            Texture.DrawOutlineCentered(Entity.Center, Color.Black, 1, newAngle);
            Texture.DrawOutlineCentered(Entity.Center + Vector2.UnitY, Color.Black, 1, newAngle);
            Texture.DrawCentered(Entity.Center + Vector2.UnitY, FemtoModule.Session.RotateDashColors[2], 1, newAngle);
            Texture.DrawCentered(Entity.Center, FemtoModule.Session.RotateDashColors[1], 1, newAngle);
        }
        base.Render();
    }
}