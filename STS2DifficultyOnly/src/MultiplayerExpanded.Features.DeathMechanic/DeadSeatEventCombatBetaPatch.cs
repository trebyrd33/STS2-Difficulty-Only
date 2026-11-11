using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MultiplayerExpanded.Features.Settings;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Features.DeathMechanic;

[BetaOnly]
[HarmonyPatch(typeof(EventCombatSynchronizer), "ReadyToEnterCombat")]
internal static class DeadSeatEventCombatBetaPatch
{
	private static readonly FieldInfo? RunStateField = AccessTools.Field(typeof(EventCombatSynchronizer), "_runState");

	private static readonly FieldInfo? StatesField = AccessTools.Field(typeof(EventCombatSynchronizer), "_states");

	[ThreadStatic]
	private static bool _registering;

	private static bool Prepare()
	{
		if (RunStateField != null && StatesField != null)
		{
			return true;
		}
		MainFile.Logger.Error("[DeathMechanic] EventCombatSynchronizer internals not found; event combats will wait for dead players.", 1);
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Prefix(EventCombatSynchronizer __instance, EncounterModel canonicalEncounter, bool shouldResumeAfterCombat)
	{
		if (_registering)
		{
			return;
		}
		try
		{
            IRunState val = (IRunState)RunStateField.GetValue(__instance);
            if (!DeathMechanicGate.IsActive(MultiplayerDeathMode.StayDeadUntilMended,val)) return;
			IList list = (IList)StatesField.GetValue(__instance);
			_registering = true;
			for (int i = 0; i < ((IPlayerCollection)val).Players.Count && i < list.Count; i++)
			{
				Player val2 = ((IPlayerCollection)val).Players[i];
				if (list[i] == null && val2.Creature.IsDead)
				{
					MainFile.Logger.Info($"[DeathMechanic] Registering dead player {val2.NetId} for {((AbstractModel)canonicalEncounter).Id}.", 1);
					__instance.ReadyToEnterCombat(canonicalEncounter, val2, (IReadOnlyList<Reward>)Array.Empty<Reward>(), shouldResumeAfterCombat);
				}
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[DeathMechanic] Dead players could not be registered for the event combat: {value}", 1);
		}
		finally
		{
			_registering = false;
		}
	}
}
