using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace MultiplayerExpanded.Features.Intents;

[HarmonyPatch(typeof(AttackCommand), "GetPossibleTargets")]
internal static class AttackExclusionTargetsPatch
{
	private static bool Prepare()
	{
		if (AccessTools.Method(typeof(AttackCommand), "GetPossibleTargets", (Type[])null, (Type[])null) != null)
		{
			return true;
		}
		MainFile.Logger.Error("[Intents] AttackCommand.GetPossibleTargets not found; everyone-except attacks will hit every player.", 1);
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
	{
		if (AttackExclusion.TryGetExcluded(__instance, out Creature excluded))
		{
			__result = __result.Where((Creature c) => c != excluded).ToList();
		}
	}
}
