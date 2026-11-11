using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Features.Reworks;

[HarmonyPatch(typeof(CardModel), "DowngradeInternal")]
internal static class ReworkDowngradePatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Prefix(CardModel __instance)
	{
		if (ReworkPatches.ShowReworked() && ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo _))
		{
			ReworkAfterCreatedPatch.Rebuild(__instance);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(CardModel __instance)
	{
		if (ReworkPatches.ShowReworked() && ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo _))
		{
			ReworkAfterCreatedPatch.Rebuild(__instance);
			EnchantmentModel enchantment = __instance.Enchantment;
			if (enchantment != null)
			{
				enchantment.ModifyCard();
			}
			AfflictionModel affliction = __instance.Affliction;
			if (affliction != null)
			{
				affliction.AfterApplied();
			}
		}
	}
}
