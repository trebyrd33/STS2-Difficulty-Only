using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Rooms;

namespace MultiplayerExpanded.Features.CardChest;

[HarmonyPatch(typeof(TreasureRoom), "DoNormalRewards")]
internal static class SuppressChestGoldPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool SkipGold(TreasureRoom __instance, ref Task<int> __result)
	{
		if (!(__instance is CardChestRoom { PaysNormalRewards: false }))
		{
			return true;
		}
		__result = Task.FromResult(0);
		return false;
	}
}
