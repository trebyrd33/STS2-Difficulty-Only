using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(PowerCmd), "Apply", new Type[]
{
	typeof(PlayerChoiceContext),
	typeof(PowerModel),
	typeof(Creature),
	typeof(decimal),
	typeof(Creature),
	typeof(CardModel),
	typeof(bool)
})]
internal static class InfestedPrismVitalSparkPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool Prefix(PlayerChoiceContext choiceContext, PowerModel power, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent, ref Task __result)
	{
		if (!(power is VitalSparkPower) || !(((applier != null) ? applier.Monster : null) is InfestedPrism) || !MonsterRework.IsMultiplayer(applier.CombatState))
		{
			return true;
		}
		__result = PowerCmd.Apply<VitalSparkMpPower>(choiceContext, target, amount, applier, cardSource, silent);
		return false;
	}
}
