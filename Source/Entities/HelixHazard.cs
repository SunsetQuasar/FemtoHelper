using System;

namespace Celeste.Mod.FemtoHelper.Entities;

[Tracked]
[CustomEntity("FemtoHelper/HelixHazard")]
public class HelixHazard : Entity
{
    public readonly float AWidth;
    public readonly float AHeight;

    public float Phase;
    public HelixHazard(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {
        AWidth = data.Width;
        AHeight = data.Height;

        Collider[] cols = new Collider[(int)(AWidth / 4)];

        for(int i = 0; i < cols.Length; i++)
        {
            cols[i] = new Hitbox(4, 4, i * 4, 0);
        }

        Collider = new ColliderList(cols);

        Add(new PlayerCollider(OnPlayer, Collider));

        Phase = Calc.Random.Range(0, (float)Math.PI * 2);
    }
    public void OnPlayer(Player player)
    {
        player.Die(Vector2.Normalize(player.Position - Position));
    }

    public override void Update()
    {
        base.Update();
        for(int i = 0; i < (int)(AWidth / 4); i++)
        {
            (Collider as ColliderList).colliders[i].Position.Y = -2 + (float)Math.Sin(Phase + i) * (AHeight / 2) + AHeight / 2;
        }
    }
    public override void Render()
    {
        Phase += Engine.DeltaTime;

        base.Render();
        for (int i = 0; i < (int)(AWidth / 4); i++)
        {
            Draw.Rect(Position + (Collider as ColliderList).colliders[i].Position, 4, 4, Color.White);
        }
    }
}
