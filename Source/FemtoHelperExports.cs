
using MonoMod.ModInterop;
using Celeste.Mod.FemtoHelper.Entities;

namespace Celeste.Mod.FemtoHelper;

public static class FemtoHelperExports
{
	[ModExportName("FemtoHelper.SMWBlockInfo")]
	public static class SMWBlockInfo
	{
		public static void GetHitMethod(Entity block, Player player, int dir) => (block as Generic_SMWBlock)?.Hit(player, dir);

		public static bool IsActive(Entity block)
		{
                return (block as Generic_SMWBlock)?.active ?? false;
		}

		public static bool CanHitTop(Entity block)
		{
			return (block as Generic_SMWBlock)?.canHitTop ?? false;
		}
		public static bool CanHitBottom(Entity block)
		{
			return (block as Generic_SMWBlock)?.canHitBottom ?? false;
		}
		public static bool CanHitLeft(Entity block)
		{
			return (block as Generic_SMWBlock)?.canHitLeft ?? false;
		}
		public static bool CanHitRight(Entity block)
		{
			return (block as Generic_SMWBlock)?.canHitRight ?? false;
		}
	}

	internal static void Initialize()
	{
		typeof(SMWBlockInfo).ModInterop();
	}
}


