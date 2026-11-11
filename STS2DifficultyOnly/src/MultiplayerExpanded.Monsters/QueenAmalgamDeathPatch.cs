using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(Queen), "AfterDeath")]
internal static class QueenAmalgamDeathPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(Queen __instance, Creature creature)
	{
		if (creature.Monster is TorchHeadAmalgam && MonsterRework.IsMultiplayer((MonsterModel)(object)__instance) && ((MonsterModel)__instance).Creature.IsAlive && !(((MonsterState)((MonsterModel)__instance).NextMove).Id != "FROZEN_IN_PLACE_MOVE"))
		{
			MonsterMoveStateMachine moveStateMachine = ((MonsterModel)__instance).MoveStateMachine;
			MonsterState? obj = ((moveStateMachine != null) ? moveStateMachine.States.GetValueOrDefault("ENRAGE_MOVE") : null);
			MoveState val = (MoveState)(object)((obj is MoveState) ? obj : null);
			if (val != null)
			{
				((MonsterModel)__instance).SetMoveImmediate(val, false);
			}
		}
	}
}
