using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;

namespace MultiplayerExpanded.Features.Columns;

[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class ColumnAttackAimPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(AttackCommand __instance)
	{
		ColumnAttacks.Aim(__instance);
	}
}
