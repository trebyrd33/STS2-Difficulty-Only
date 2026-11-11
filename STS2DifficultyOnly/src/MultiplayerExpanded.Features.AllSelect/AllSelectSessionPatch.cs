using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Features.AllSelect;

[HarmonyPatch(typeof(EventSynchronizer), "BeginEvent")]
internal static class AllSelectSessionPatch
{
	private static bool Prepare()
	{
		return AllSelectAccess.Arm("AllSelectSessionPatch");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(EventSynchronizer __instance, EventModel canonicalEvent)
	{
		try
		{
			AllSelectSession.Reset();
			if (!(canonicalEvent is IAllSelectEvent rules))
			{
				return;
			}
			List<EventModel> list = AllSelectAccess.Events(__instance);
			List<uint?> list2 = AllSelectAccess.Votes(__instance);
			IPlayerCollection val = AllSelectAccess.Players(__instance);
			if (list == null || list2 == null || val == null || val.Players.Count == 0)
			{
				return;
			}
			bool[] array = AllSelectBots.Participants(list, val);
			AllSelectSession.Begin(val, rules, ((AbstractModel)canonicalEvent).Id.Entry, GameApi.Seed(val.Players[0].RunState.Rng), array);
			for (int i = 0; i < list2.Count && i < array.Length; i++)
			{
				if (!array[i])
				{
					list2[i] = uint.MaxValue;
				}
			}
			if (!AllSelectBots.AnyParticipants(array))
			{
				MainFile.Logger.Warn("[AllSelect] " + ((AbstractModel)canonicalEvent).Id.Entry + " began with nobody able to choose; leaving it alone.", 1);
			}
			else
			{
				AllSelectBots.FillOpeningVotes(__instance, list, list2, val);
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Error($"[AllSelect] Could not open a session: {value}", 1);
			AllSelectSession.Reset();
		}
	}
}
