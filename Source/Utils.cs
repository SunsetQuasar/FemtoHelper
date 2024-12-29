using IL.MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.TrackSpinner;

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
            return !level.Session.GetFlag(flag[1..]);
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

    public static Vector2 ExplodeLaunch(this Holdable hold, Vector2 from, bool snapUp = true, bool sidesOnly = false)
    {
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        Celeste.Freeze(0.1f);
        Vector2 vector = (hold.Entity.Center - from).SafeNormalize(-Vector2.UnitY);
        float num = Vector2.Dot(vector, Vector2.UnitY);
        if (snapUp && num <= -0.7f)
        {
            vector.X = 0f;
            vector.Y = -1f;
        }
        else if (num <= 0.65f && num >= -0.55f)
        {
            vector.Y = 0f;
            vector.X = Math.Sign(vector.X);
        }
        if (sidesOnly && vector.X != 0f)
        {
            vector.Y = 0f;
            vector.X = Math.Sign(vector.X);
        }
        hold.SetSpeed(280f * vector);
        if (hold.GetSpeed().Y <= 50f)
        {
            hold.SetSpeed(new Vector2(hold.GetSpeed().X, Math.Min(-150f, hold.GetSpeed().Y)));
        }
        SlashFx.Burst(hold.Entity.Center, hold.GetSpeed().Angle());
        return vector;
    }

    public static bool TrySquishWiggleNoPusher(this Actor actor, int wiggleX = 3, int wiggleY = 3)
    {
        for (int i = 0; i <= wiggleX; i++)
        {
            for (int j = 0; j <= wiggleY; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                for (int num = 1; num >= -1; num -= 2)
                {
                    for (int num2 = 1; num2 >= -1; num2 -= 2)
                    {
                        Vector2 vector = new Vector2(i * num, j * num2);
                        if (actor.CollideCheck<Solid>(actor.Position + vector)) continue;
                        actor.Position += vector;
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
