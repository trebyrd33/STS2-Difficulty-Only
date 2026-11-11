using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch(typeof(InterceptPower), "ModifyDamageMultiplicative")]
internal static class InterceptPowerNoMultiplierPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool Prefix(ref decimal __result)
	{
		if (!MultiplayerExpanded.Features.Reworks.ReworkPatches.IsMp()) return true;
		__result = 1m;
		return false;
	}
}
