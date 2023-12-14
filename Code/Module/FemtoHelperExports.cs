using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using MonoMod.ModInterop;
using Celeste.Mod.FemtoHelper.Entities;

namespace Celeste.Mod.FemtoHelper
{
	public static class FemtoHelperExports
	{
		[ModExportName("FemtoHelper.SMWBlockInfo")]
		public static class SMWBlockInfo
		{

			public static void GetHitMethod(Entity block, Player player, int dir) => (block as Generic_SMWBlock).Hit(player, dir);

			public static bool IsActive(Entity block)
			{
                return (block as Generic_SMWBlock).active;
			}

			public static bool CanHitTop(Entity block)
			{
				return (block as Generic_SMWBlock).canHitTop;
			}
			public static bool CanHitBottom(Entity block)
			{
				return (block as Generic_SMWBlock).canHitBottom;
			}
			public static bool CanHitLeft(Entity block)
			{
				return (block as Generic_SMWBlock).canHitLeft;
			}
			public static bool CanHitRight(Entity block)
			{
				return (block as Generic_SMWBlock).canHitRight;
			}
		}

		internal static void Initialize()
		{
			typeof(SMWBlockInfo).ModInterop();
		}
	}

}


