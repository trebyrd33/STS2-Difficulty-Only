using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace MultiplayerExpanded.Features.Columns;

[HarmonyPatch(typeof(AttackCommand), "GetPossibleTargets")]
internal static class ColumnAttackTargetsPatch
{
	private static bool Prepare()
	{
		if (AccessTools.Method(typeof(AttackCommand), "GetPossibleTargets", (Type[])null, (Type[])null) != null)
		{
			return true;
		}
		MainFile.Logger.Error("[Columns] AttackCommand.GetPossibleTargets not found; column enemies attack every player.", 1);
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
	{
		if (ColumnAttacks.TryGetHittable(__instance, out Creature[] hittable))
		{
			__result = __result.Where(((IEnumerable<Creature>)hittable).Contains).ToList();
		}
	}
}
