using Celeste.Mod.Entities;
using Celeste.Mod.FemtoHelper.Entities;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Code.Entities;
[CustomEntity("FemtoHelper/EHIController")]
public class ExtraHoldableInteractionsController : Entity
{
    public ExtraHoldableInteractionsController() : base()
    {

    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (ExtraHoldableInteractionsController controller in scene.Tracker.GetEntities<ExtraHoldableInteractionsController>())
        {
            if (controller != this)
            {
                controller.RemoveSelf();
            }
        }
    }
}
