using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/HelixHazard")]
public class HelixHazard : Entity
{
    public float aWidth;
    public float aHeight;

    public float phase;
    public HelixHazard(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {
        aWidth = data.Width;
        aHeight = data.Height;

        Collider[] cols = new Collider[(int)(aWidth / 4)];

        for(int i = 0; i < cols.Length; i++)
        {
            cols[i] = new Hitbox(4, 4, i * 4, 0);
        }

        base.Collider = new ColliderList(cols);

        Add(new PlayerCollider(onPlayer, Collider));

        phase = Calc.Random.Range(0, (float)Math.PI * 2);
    }
    public void onPlayer(Player player)
    {
        player.Die(Vector2.Normalize(player.Position - Position));
    }

    public override void Update()
    {
        base.Update();
        for(int i = 0; i < (int)(aWidth / 4); i++)
        {
            (Collider as ColliderList).colliders[i].Position.Y = -2 + ((float)Math.Sin(phase + ((float)i)) * (aHeight / 2)) + (aHeight / 2);
        }
    }
    public override void Render()
    {
        phase += Engine.DeltaTime;

        base.Render();
        for (int i = 0; i < (int)(aWidth / 4); i++)
        {
            Draw.Rect(Position + (Collider as ColliderList).colliders[i].Position, 4, 4, Color.White);
        }
    }
}
