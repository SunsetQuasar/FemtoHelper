using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities
{
    public class RotateDashIndicator : Component
    {
        public bool Anim;
        public float ArrowAngle;
        public MTexture[] textures;
        public float timer;

        public RotateDashIndicator() : base(true, true)
        {
            Anim = false;
            ArrowAngle = 0;
            textures = [GFX.Game["objects/FemtoHelper/rotateRefillCCW/indicator_a"], GFX.Game["objects/FemtoHelper/rotateRefillCCW/indicator_b"]];
        }

        public override void Update()
        {
            base.Update();
            if (timer > 0 && !Anim) timer = Math.Max(timer - (Engine.DeltaTime * 4), 0);
        }

        public override void Render()
        {
            if (FemtoModule.Session.HasRotateDash && (Entity as Player).Speed != Vector2.Zero)
            {
                float newangle = CodecumberPortStuff.VectorToAngle((Entity as Player).Speed) - FemtoModule.Session.RotateDashAngle;
                textures[0].DrawOutlineCentered(Entity.Center, Color.Black, 1, newangle);
                textures[0].DrawOutlineCentered(Entity.Center + Vector2.UnitY, Color.Black, 1, newangle);
                textures[0].DrawCentered(Entity.Center + Vector2.UnitY, FemtoModule.Session.RotateDashColors[2], 1, newangle);
                textures[0].DrawCentered(Entity.Center, FemtoModule.Session.RotateDashColors[1], 1, newangle);

            }
            if (Anim || timer > 0)
            {
                textures[1].DrawCentered(Entity.Center, FemtoModule.Session.RotateDashColors[1] * timer * 0.4f, 1, ArrowAngle);
            }
            base.Render();
        }
    }
}
