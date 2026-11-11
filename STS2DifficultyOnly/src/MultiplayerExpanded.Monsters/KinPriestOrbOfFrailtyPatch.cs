using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch]
internal static class KinPriestOrbOfFrailtyPatch
{
	private static MethodBase? _target;

	private static bool Prepare()
	{
		if ((object)_target == null)
		{
			_target = AccessTools.Method(typeof(KinPriest), "OrbOfFrailtyMove", new Type[1] { typeof(IReadOnlyList<Creature>) }, (Type[])null);
		}
		if (_target == null)
		{
			MainFile.Logger.Error("[KinPriestRework] KinPriest.OrbOfFrailtyMove not found; Taking Turns is not applied.", 1);
		}
		return _target != null;
	}

	private static MethodBase TargetMethod()
	{
		return _target;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(KinPriest __instance, IReadOnlyList<Creature> targets, ref Task __result)
	{
		if (MonsterRework.IsMultiplayer((MonsterModel)(object)__instance))
		{
			__result = ApplyAfter(__result, __instance, targets);
		}
	}

	private static async Task ApplyAfter(Task vanilla, KinPriest priest, IReadOnlyList<Creature> targets)
	{
		await vanilla;
		List<Creature> list = targets.Where((Creature t) => t.IsAlive && !t.HasPower<TakingTurnsPower>()).ToList();
		if (list.Count == 0)
		{
			return;
		}
		try
		{
			await PowerCmd.Apply<TakingTurnsPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)list, 1m, ((MonsterModel)priest).Creature, (CardModel)null, false);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[KinPriestRework] applying Taking Turns failed: " + ex.Message, 1);
		}
	}
}
