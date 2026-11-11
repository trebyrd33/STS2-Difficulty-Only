using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Rooms;

namespace MultiplayerExpanded.Features.CardChest;

[HarmonyPatch(typeof(TreasureRoom), "DoExtraRewardsIfNeeded")]
internal static class SuppressChestExtraRewardsPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool SkipExtraRewards(TreasureRoom __instance, ref Task __result)
	{
		if (!(__instance is CardChestRoom { PaysNormalRewards: false }))
		{
			return true;
		}
		__result = Task.CompletedTask;
		return false;
	}
}
