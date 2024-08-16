using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Triggers
{
    [Tracked]
    [CustomEntity("FemtoHelper/VitalSafetyTrigger")]
    public class VitalSafetyTrigger : Trigger
    {
        public VitalSafetyTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
