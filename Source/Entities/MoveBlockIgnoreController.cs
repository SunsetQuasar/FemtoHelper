using System.Collections.Generic;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/MoveBlockIgnoreController")]
[Tracked]
internal class MoveBlockIgnoreController(EntityData data, Vector2 offset) : Entity()
{
    public string[] ignores = data.String("ignore", "XaphanHelper/TimedDashSwitch").Split(",");

    public static void Load()
    {
        On.Celeste.MoveBlock.MoveCheck += MoveBlock_MoveCheck;
    }

    private static bool MoveBlock_MoveCheck(On.Celeste.MoveBlock.orig_MoveCheck orig, MoveBlock self, Vector2 speed)
    {
        MoveBlockIgnoreController c = self.Scene.Tracker.GetEntity<MoveBlockIgnoreController>();

        if (c is not null)
        {
            Vector2 nextPos = self.Position + speed;

            bool itsokay = true;

            List<Entity> all = self.CollideAll<Solid>(nextPos);

            foreach (Entity e in all)
            {
                if (e is Solid s) //unnecessary check?
                {
                    bool islist = false;
                    for (var i = 0; i < c.ignores.Length; i++)
                    {
                        if ((s.SourceData?.Name ?? "") == c.ignores[i])
                        {
                            islist = true;
                            break;
                        }
                    }

                    Log($"{e.SourceData?.Name ?? " - "} - {islist}");

                    if (!islist)
                    {
                        itsokay = false;
                        break;
                    }
                }
            }

            if (itsokay)
            {
                self.MoveV(speed.Y);
                self.MoveH(speed.X);
                return false;
            }
        }

        bool ret = orig(self, speed);

        return ret;
    }

    public static void Unload()
    {
        On.Celeste.MoveBlock.MoveCheck -= MoveBlock_MoveCheck;
    }
}
