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
        else
        {
            return string.IsNullOrEmpty(flag) || level.Session.GetFlag(flag);
        }
    }
}
