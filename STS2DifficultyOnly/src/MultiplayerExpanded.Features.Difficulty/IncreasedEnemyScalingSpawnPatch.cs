using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace MultiplayerExpanded.Features.Difficulty;

[HarmonyPatch(typeof(CombatState), "CreateCreature")]
internal static class IncreasedEnemyScalingSpawnPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Prefix(MonsterModel monster, out int? __state)
	{
		__state = ((CombatManager.Instance.IsInProgress || monster is TestSubject) ? new int?(IncreasedEnemyScalingPatch.Enter()) : ((int?)null));
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
