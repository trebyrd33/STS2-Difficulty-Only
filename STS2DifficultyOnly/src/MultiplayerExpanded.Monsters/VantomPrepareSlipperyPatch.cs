using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch]
internal static class VantomPrepareSlipperyPatch
{
	private static MethodBase? _target;

	private static bool Prepare()
	{
		if ((object)_target == null)
		{
			_target = AccessTools.Method(typeof(Vantom), "PrepareMove", new Type[1] { typeof(IReadOnlyList<Creature>) }, (Type[])null);
		}
		if (_target == null)
		{
			MainFile.Logger.Error("[VantomRework] Vantom.PrepareMove not found; Slippery is not recovered.", 1);
		}
		return _target != null;
	}

	private static MethodBase TargetMethod()
	{
		return _target;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(Vantom __instance, ref Task __result)
	{
		if (MonsterRework.IsMultiplayer((MonsterModel)(object)__instance))
		{
			__result = RecoverAfter(__result, __instance);
		}
	}

	private static async Task RecoverAfter(Task vanilla, Vantom vantom)
	{
		await vanilla;
		try
		{
			Creature creature = ((MonsterModel)vantom).Creature;
			if (creature.IsAlive)
			{
				ICombatState combatState = creature.CombatState;
				if (combatState != null)
				{
					int count = combatState.Players.Count;
					decimal num = ((creature.GetPowerAmount<SlipperyPower>() > 0) ? ((decimal)count) : 2m);
					await PowerCmd.Apply<SlipperyPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), creature, num, creature, (CardModel)null, false);
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[VantomRework] recovering Slippery failed: " + ex.Message, 1);
		}
	}
}
