using IL.MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper;
public class Utils
{
    public static int EvaluateExpressionAsInt(string exp, Session session)
    {
        if (FemtoModule.FrostHelperSupport.TryCreateSessionExpression.Invoke(exp, out object obj))
        {
            return FemtoModule.FrostHelperSupport.GetIntSessionExpressionValue.Invoke(obj, session);
        }
        else return 0;
    }
    public static bool EvaluateExpressionAsBool(string exp, Session session)
    {
        if (FemtoModule.FrostHelperSupport.TryCreateSessionExpression.Invoke(exp, out object obj))
        {
            return FemtoModule.FrostHelperSupport.GetIntSessionExpressionValue.Invoke(obj, session) != 0;
        }
        else return false;
    }
}
public static class LevelExtensions
{
    public static bool FancyCheckFlag(this Level level, string flag)
    {
        if (flag.StartsWith('!'))
        {
            return !level.Session.GetFlag(flag.Substring(1));
        }
        else
        {
            return string.IsNullOrEmpty(flag) || level.Session.GetFlag(flag);
        }
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
