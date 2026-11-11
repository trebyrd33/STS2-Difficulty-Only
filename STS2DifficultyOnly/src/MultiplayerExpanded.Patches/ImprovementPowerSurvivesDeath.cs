using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch(typeof(PowerModel), "ShouldPowerBeRemovedAfterOwnerDeath")]
internal static class ImprovementPowerSurvivesDeath
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPostfix]
	private static void Postfix(PowerModel __instance, ref bool __result)
	{
		if (MultiplayerExpandedConfig.MultiplayerEvents && __instance is ImprovementPower)
		{
			__result = false;
		}
	}
}
