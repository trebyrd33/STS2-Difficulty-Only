using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch(typeof(CoveredPower), "ModifyDamageMultiplicative")]
internal static class CoveredPowerMeasurePatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool Prefix(CoveredPower __instance, ref decimal __result)
	{
		if (!RedirectShare.IsMeasuring(((PowerModel)__instance).Owner))
		{
			return true;
		}
		__result = 1m;
		return false;
	}
}
