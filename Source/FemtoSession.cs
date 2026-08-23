using System.Collections.Generic;

namespace Celeste.Mod.FemtoHelper;

public class FemtoHelperSession : EverestModuleSession
{
    /*
    public Dictionary<string, object> CustomWipeData;

    public T GetWipeData<T>(string key, T defaultValue)
    {
        if (CustomWipeData.TryGetValue(key, out object value) && value is T)
        {
            return (T)value;
        }
        return defaultValue;
    }
    */

    public Dictionary<string, List<string>> SessionHearts { get; set; } = [];
    public int SessionHeartCount(string key)
    {
        if (SessionHearts.TryGetValue(key, out var list) && list is not null)
        {
            return list.Count;
        }
        else return 0;
    }

    public int SessionHeartCount()
    {
        int result = 0;
        foreach(var kvp in SessionHearts)
        {
            result += kvp.Value.Count;
        }
        return result;
    }
}