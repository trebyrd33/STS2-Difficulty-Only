using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerExpanded.Features.Intents;

[HarmonyPatch(typeof(RunManager), "CleanUp")]
internal static class AttackOriginalTargetsCleanUpPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPostfix]
	private static void Postfix()
	{
		AttackOriginalTargets.Reset();
	}
}
