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
internal static class AeonglassWitheringPresencePatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool Prefix(PowerModel power, Creature? applier, ref Task __result)
	{
		if (!(power is WitheringPresencePower) || !(((applier != null) ? applier.Monster : null) is Aeonglass) || !MonsterRework.IsMultiplayer(applier.CombatState))
		{
			return true;
		}
		__result = Task.CompletedTask;
		return false;
	}
}
