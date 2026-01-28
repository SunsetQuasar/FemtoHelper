namespace Celeste.Mod.FemtoHelper.Triggers;

[Tracked]
[CustomEntity("FemtoHelper/VitalSafetyTrigger")]

public class VitalSafetyTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    public readonly float OverrideValue = data.Float("overrideValue", 0f);
    public readonly bool Override = data.Bool("override", false);
    public readonly string FlagToggle = data.String("flagToggle", ""); //supports session expressions
}