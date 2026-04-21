using System.Collections.Generic;

namespace Celeste.Mod.FemtoHelper.Metadata;

public class SessionHeartMetadata
{
    public List<SessionHeartDefinition> SessionHeartGroups { get; set; } = [];
}

public class SessionHeartDefinition
{
    public string Name { get; set; }
    public string UITexture { get; set; } = "FemtoHelper/SessionHearts/defaultIcon";
    public string Color { get; set; } = "FFFFFF";
}
