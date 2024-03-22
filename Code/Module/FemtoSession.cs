using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.FemtoHelper;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.FemtoHelper
{
	public class FemtoHelperSession : EverestModuleSession
	{
        public bool HasRotateDash { get; set; } = false;
        public float RotateDashAngle { get; set; } = 0;
        public float RotateDashScalar { get; set; } = 1;
        public bool HasStartedRotateDashing { get; set; } = false;
        public Color[] RotateDashColors { get; set; } = [Calc.HexToColor("7958ad"), Calc.HexToColor("cbace6"), Calc.HexToColor("634691")];

        public Entities.ExtraTrailManager TrailManager { get; set; } = new Entities.ExtraTrailManager();
    }
}