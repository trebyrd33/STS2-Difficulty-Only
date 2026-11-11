using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(ArtifactPower), "TryModifyPowerAmountReceived")]
internal static class TheInsatiableSandpitArtifactPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(PowerModel canonicalPower, decimal amount, ref decimal modifiedAmount, ref bool __result)
	{
		if (!(canonicalPower is SandpitMpPower))
		{
			return true;
		}
		modifiedAmount = amount;
		__result = false;
		return false;
	}
}
