using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities;
[TrackedAs(typeof(Water))]
[CustomEntity("FemtoHelper/WaterfallBlocker")]
public class WaterfallBlocker : Water
{
    public WaterfallBlocker(EntityData data, Vector2 offset) : base(data.Position + offset, false, false, data.Width, data.Height)
    {
        Visible = false;
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Components.RemoveAll<DisplacementRenderHook>();
        scene.OnEndOfFrame += RemoveSelf;
    }
    public override void Update()
    {
        RemoveSelf();
    }

    public override void Render()
    {
    }
}
