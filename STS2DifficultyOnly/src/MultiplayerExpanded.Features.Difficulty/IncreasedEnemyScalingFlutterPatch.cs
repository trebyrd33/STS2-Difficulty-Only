using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerExpanded.Features.Difficulty;

[HarmonyPatch(typeof(PowerModel), "GetScaledAmountForMultiplayer")]
internal static class IncreasedEnemyScalingFlutterPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Prefix(PowerModel __instance, out int? __state)
	{
		__state = ((__instance is FlutterPower) ? new int?(IncreasedEnemyScalingPatch.Enter()) : ((int?)null));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception? Finalizer(Exception? __exception, int? __state)
	{
		if (__state.HasValue)
		{
			int valueOrDefault = __state.GetValueOrDefault();
			IncreasedEnemyScalingPatch.Exit(valueOrDefault);
		}
		return __exception;
	}
}
