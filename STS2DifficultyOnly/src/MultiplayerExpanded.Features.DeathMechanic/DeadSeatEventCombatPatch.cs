using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MultiplayerExpanded.Features.Settings;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Features.DeathMechanic;

[StableOnly]
[HarmonyPatch(typeof(EventModel), "EnterCombatWithoutExitingEvent", new Type[]
{
	typeof(EncounterModel),
	typeof(IReadOnlyList<Reward>),
	typeof(bool)
})]
internal static class DeadSeatEventCombatPatch
{
	private static readonly MethodInfo? EnterMethod = AccessTools.Method(typeof(EventModel), "EnterCombatWithoutExitingEvent", new Type[3]
	{
		typeof(EncounterModel),
		typeof(IReadOnlyList<Reward>),
		typeof(bool)
	}, (Type[])null);

	private static readonly ConditionalWeakTable<EventModel, StrongBox<uint>> EnteredOnPage = new ConditionalWeakTable<EventModel, StrongBox<uint>>();

	private static bool Prepare()
	{
		if (DeadSeatVotes.Arm("DeadSeatEventCombatPatch"))
		{
			return EnterMethod != null;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(EventModel __instance, EncounterModel mutableEncounter, IReadOnlyList<Reward> extraRewards, bool shouldResumeAfterCombat)
	{
		try
		{
            if (!DeathMechanicGate.IsActive(MultiplayerDeathMode.StayDeadUntilMended,__instance.Owner?.RunState ?? RunManager.Instance.DebugOnlyGetState())) return;
            if (__instance.Owner == null || LocalContext.IsMe(__instance.Owner))
			{
				return;
			}
			EventSynchronizer eventSynchronizer = RunManager.Instance.EventSynchronizer;
			EventModel localEvent = eventSynchronizer.GetLocalEvent();
			if (localEvent == __instance || !localEvent.IsFinished)
			{
				return;
			}
			Player owner = localEvent.Owner;
			Creature val = ((owner != null) ? owner.Creature : null);
			if (val != null && val.IsDead)
			{
				uint num = DeadSeatVotes.PageIndex(eventSynchronizer);
				if (!EnteredOnPage.TryGetValue(localEvent, out StrongBox<uint> value) || value.Value != num)
				{
					EnteredOnPage.AddOrUpdate(localEvent, new StrongBox<uint>(num));
					MainFile.Logger.Info($"[DeathMechanic] Local player is dead; entering {((AbstractModel)__instance).Id}'s combat with their allies.", 1);
					EnterMethod.Invoke(localEvent, new object[3] { mutableEncounter, extraRewards, shouldResumeAfterCombat });
				}
			}
		}
		catch (Exception value2)
		{
			MainFile.Logger.Warn($"[DeathMechanic] Dead player could not follow the event into combat: {value2}", 1);
		}
	}
}
