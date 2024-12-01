using IL.MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Utils;
public class Utils
{

}
public static class LevelExtensions
{
    public static bool FancyCheckFlag(this Level level, string flag)
    {
        if (flag.StartsWith('!'))
        {
            return !level.Session.GetFlag(flag.Substring(1));
        }
        return string.IsNullOrEmpty(flag) || level.Session.GetFlag(flag);
    }
}

public static class EntityExtensions
{
    public static T CollideFirstIgnoreCollidable<T>(this Entity entity, Vector2 at) where T : Entity
    {
        foreach(Entity t in entity.Scene.Tracker.Entities[typeof(T)])
        {
            bool collidable = t.Collidable;
            t.Collidable = true;
            if(entity.CollideCheck(t, at))
            {
                t.Collidable = collidable;
                return t as T;
            }
            t.Collidable = collidable;
        }
        return null;
    }
}
