using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FemtoHelper.Entities
{
    [Tracked]
    [CustomEntity("FemtoHelper/AssistHazardController")]
    public class AssistHazardController : Entity
    {
        public AssistHazardController() : base(Vector2.Zero)
        {
            Tag = Tags.Global | Tags.Persistent;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = SceneAs<Level>();
            foreach (AssistHazardController entity in level.Tracker.GetEntities<AssistHazardController>())
            {
                if (entity != this)
                {
                    entity.RemoveSelf();
                }
            }
        }
    }
}
