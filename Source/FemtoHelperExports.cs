
using MonoMod.ModInterop;
using Celeste.Mod.FemtoHelper.Entities;

namespace Celeste.Mod.FemtoHelper;

public static class FemtoHelperExports
{
	[ModExportName("FemtoHelper.SMWBlockInfo")]
	public static class SmwBlockInfo
	{
		public static void GetHitMethod(Entity block, Player player, int dir) => (block as GenericSmwBlock)?.Hit(player, dir);

		public static bool IsActive(Entity block)
		{
                return (block as GenericSmwBlock)?.Active ?? false;
		}

		public static bool CanHitTop(Entity block)
		{
			return (block as GenericSmwBlock)?.CanHitTop ?? false;
		}
		public static bool CanHitBottom(Entity block)
		{
			return (block as GenericSmwBlock)?.CanHitBottom ?? false;
		}
		public static bool CanHitLeft(Entity block)
		{
			return (block as GenericSmwBlock)?.CanHitLeft ?? false;
		}
		public static bool CanHitRight(Entity block)
		{
			return (block as GenericSmwBlock)?.CanHitRight ?? false;
		}
	}

	internal static void Initialize()
	{
		typeof(SmwBlockInfo).ModInterop();
	}
}


