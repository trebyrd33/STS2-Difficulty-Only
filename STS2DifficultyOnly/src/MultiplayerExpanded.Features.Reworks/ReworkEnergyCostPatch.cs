using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Features.Reworks;

[HarmonyPatch(typeof(CardModel), "CanonicalEnergyCost", MethodType.Getter)]
internal static class ReworkEnergyCostPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(CardModel __instance, ref int __result)
	{
		if (ReworkPatches.ShowReworked() && ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo info))
		{
			__result = info.Rework.Cost;
		}
	}
}
