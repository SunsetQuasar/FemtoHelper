using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.FemtoHelper.Entities.Sokoban;

public struct SokobanObjectType
{
    public string SourcePath;
    public Rectangle SourceRect;
    public bool IsSolid;
    public bool IsPushable;
    public bool IsPullable;
    public int Depth;
    public Func<SokobanObject, Vector2, bool, Vector2> OnInput;
    public Func<SokobanObject, Vector2, Vector2> TryMove;
    public Func<SokobanObject, Vector2, bool> TryPush;
    public Func<SokobanObject, Vector2, bool> TryPull;
}

public enum Direction
{
    Right,
    Down,
    Left, 
    Up,
}

public class SokobanObject
{
    static SokobanObject()
    {
        types.Add("Player", new SokobanObjectType
        {
            SourcePath = "objects/FemtoHelper/sokoban/atlas",
            SourceRect = new Rectangle(0, 0, 8, 8),
            IsSolid = false,
            IsPushable = false,
            IsPullable = false,
            Depth = 0,
            OnInput = (obj, dir, space) =>
            {
                return dir * 32;
            },
            TryMove = (obj, move) =>
            {
                if (move == Vector2.Zero) return move;
                Vector2 castMove = Vector2.Zero;
                Vector2 step = move.Sign() * 8;
                while(!obj.CollideAnyCond(castMove + step, mustBeSolid: true) && castMove != move)
                {
                    castMove += step;
                }
                return castMove;
            }
        });
        types.Add("Rock", new SokobanObjectType
        {
            SourcePath = "objects/FemtoHelper/sokoban/atlas",
            SourceRect = new Rectangle(8, 0, 8, 8),
            IsSolid = true,
            IsPushable = true,
            IsPullable = false,
            Depth = 100,
        });
        types.Add("Gem", new SokobanObjectType
        {
            SourcePath = "objects/FemtoHelper/sokoban/atlas",
            SourceRect = new Rectangle(16, 0, 8, 8),
            IsSolid = true,
            IsPushable = false,
            IsPullable = true,
            Depth = 150,
        });
        types.Add("Slot", new SokobanObjectType
        {
            SourcePath = "objects/FemtoHelper/sokoban/atlas",
            SourceRect = new Rectangle(24, 0, 8, 8),
            IsSolid = false,
            IsPushable = false,
            IsPullable = false,
            Depth = 600,
        });
        types.Add("Swap", new SokobanObjectType
        {
            SourcePath = "objects/FemtoHelper/sokoban/atlas",
            SourceRect = new Rectangle(32, 0, 8, 8),
            IsSolid = false,
            IsPushable = false,
            IsPullable = false,
            Depth = 400,
        });
        types.Add("Wall", new SokobanObjectType
        {
            SourcePath = "objects/FemtoHelper/sokoban/atlas",
            SourceRect = new Rectangle(48, 0, 8, 8),
            IsSolid = true,
            IsPushable = false,
            IsPullable = false,
            Depth = 200,
        });
        types.Add("Tile", new SokobanObjectType
        {
            SourcePath = "objects/FemtoHelper/sokoban/atlas",
            SourceRect = new Rectangle(40, 0, 8, 8),
            IsSolid = false,
            IsPushable = false,
            IsPullable = false,
            Depth = 300,
        });
        types.Add("Bomb", new SokobanObjectType
        {
            SourcePath = "objects/FemtoHelper/sokoban/atlas",
            SourceRect = new Rectangle(0, 8, 8, 14),
            IsSolid = true,
            IsPushable = true,
            IsPullable = false,
            Depth = 50,
        });
    }
    private static List<string> registeredTypes =
    [
        "Player",
        "Rock",
        "Gem",
        "Slot",
        "Swap",
        "Wall",
        "Tile",
        "Bomb",
    ];
    private static Dictionary<string, SokobanObjectType> types = [];
    public SokobanBoard parent;
    public Vector2 Pos;
    public Vector2 PrevPos;
    public string Type;
    public string PrevType;
    public Direction Dir;
    public Rectangle Collider => new((int)Pos.X, (int)Pos.Y, 8, 8);
    public Rectangle ColliderAt(Vector2 at) => new((int)(Pos.X + at.X), (int)(Pos.Y + at.Y), 8, 8);

    public SokobanObject()
    {

    }

    public SokobanObject(Vector2 pos, string type) : this()
    {
        Pos = PrevPos = pos;
        Type = PrevType = type;
    }

    public SokobanObjectType GetSokobanType()
    {
        return types[Type];
    }

    public void Render()
    {
        SokobanObjectType type = GetSokobanType();
        Vector2 pos = Vector2.Lerp(PrevPos, Pos, parent.stepPercent);
        GFX.Game[type.SourcePath].GetSubtexture(type.SourceRect).DrawCentered(pos);
    }

    public List<T> CollideAll<T>(Vector2 at) where T : SokobanObject
    {
        //god bless/damn linq
        return [.. parent.Objects.Select(o => o as T).Where(t => t is not null && ColliderAt(at).Intersects(t.Collider))];
        /*
        List<T> result = [];

        foreach (SokobanObject obj in parent.Objects)
        {
            if (obj is not T Tobj) continue;
            if (Collider.Intersects(Tobj.Collider))
            {
                result.Add(Tobj);
            }
        }

        return result;
        */
    }

    public bool CollideAny<T>(Vector2 at) where T : SokobanObject
    {
        return parent.Objects.Select(o => o as T).Any(t => t is not null && ColliderAt(at).Intersects(t.Collider));
    }
    public List<SokobanObject> CollideAllCond(Vector2 at, bool mustBeSolid = false, bool mustBePushable = false, bool mustBePullable = false)
    {
        return [.. parent.Objects.Where(o => {
            SokobanObjectType type = o.GetSokobanType();
            return ColliderAt(at).Intersects(o.Collider) && (!mustBeSolid || type.IsSolid) && (!mustBePushable || type.IsPushable) && (!mustBePullable || type.IsPullable);
            })];
    }

    public bool CollideAnyCond(Vector2 at, bool mustBeSolid = false, bool mustBePushable = false, bool mustBePullable = false)
    {
        return parent.Objects.Any(o =>
        {
            SokobanObjectType type = o.GetSokobanType();
            return ColliderAt(at).Intersects(o.Collider) && (!mustBeSolid || type.IsSolid) && (!mustBePushable || type.IsPushable) && (!mustBePullable || type.IsPullable);
        });
    }
}
