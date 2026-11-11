using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MultiplayerExpanded.Features.Columns;

[HarmonyPatch(typeof(MoveState), "PerformMove")]
internal static class ColumnMoveTargetsPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static void Prefix(MoveState __instance, ref IEnumerable<Creature> targets)
	{
		Creature? val = targets.FirstOrDefault();
		ICombatState val2 = ((val != null) ? val.CombatState : null);
		if (ColumnCombat.Of(val2) == null)
		{
			return;
		}
		Creature val3 = val2.Enemies.FirstOrDefault((Creature e) =>
		{
			MonsterModel monster = e.Monster;
			return monster != null && monster.IsPerformingMove && monster.NextMove == __instance;
		});
		if (val3 != null)
		{
			IReadOnlyList<Creature> readOnlyList = ColumnCombat.TargetsOf(val3);
			if (readOnlyList != null)
			{
				targets = readOnlyList;
			}
		}
	}
}
