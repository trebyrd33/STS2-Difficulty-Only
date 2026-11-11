using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Singleton;

namespace MultiplayerExpanded.Features.Difficulty;

[HarmonyPatch(typeof(MultiplayerScalingModel), "GetMultiplayerScaling")]
internal static class IncreasedEnemyScalingPatch
{
	[ThreadStatic]
	private static int _vanillaDepth;

	internal static int Enter()
	{
		return _vanillaDepth++;
	}

	internal static void Exit(int previous)
	{
		_vanillaDepth = previous;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(ref decimal __result)
	{
		if (_vanillaDepth == 0)
		{
			__result = ChallengeOptions.IncreasedScaling(__result);
		}
	}
}
