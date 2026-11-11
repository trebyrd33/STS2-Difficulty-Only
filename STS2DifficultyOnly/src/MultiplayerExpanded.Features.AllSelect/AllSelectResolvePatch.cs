using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerExpanded.Features.AllSelect;

[HarmonyPatch(typeof(EventSynchronizer), "ChooseOptionForSharedEvent")]
internal static class AllSelectResolvePatch
{
	private static bool Prepare()
	{
		return AllSelectAccess.Arm("AllSelectResolvePatch");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(EventSynchronizer __instance)
	{
		try
		{
			AllSelectSession active = AllSelectSession.Active;
			if (active == null || AllSelectAccess.Rules(__instance) == null)
			{
				return true;
			}
			List<EventModel> list = AllSelectAccess.Events(__instance);
			List<uint?> list2 = AllSelectAccess.Votes(__instance);
			IPlayerCollection val = AllSelectAccess.Players(__instance);
			if (list == null || list2 == null || val == null)
			{
				return true;
			}
			uint num = AllSelectAccess.Page(__instance);
			AllSelectVotesMessage allSelectVotesMessage = active.TakePending(num);
			if (allSelectVotesMessage == null)
			{
				return true;
			}
			active.ReconcileMask(allSelectVotesMessage.StillPlaying);
			AllSelectPlan allSelectPlan = active.Resolve(allSelectVotesMessage.Votes, num, AllSelectBots.ShouldForceEnd(active, val));
			if (!IsDispatchable(allSelectPlan, list, val.Players.Count))
			{
				return true;
			}
			for (int i = 0; i < allSelectPlan.PerSlot.Length; i++)
			{
				AllSelectRound allSelectRound = allSelectPlan.PerSlot[i];
				if (allSelectRound != null && list[i] is IAllSelectEvent allSelectEvent)
				{
					allSelectEvent.ApplyRound(allSelectRound);
				}
			}
			for (int j = 0; j < list2.Count && j < allSelectPlan.StillPlayingAfter.Length; j++)
			{
				if (allSelectPlan.StillPlayingAfter[j])
				{
					list2[j] = null;
				}
			}
			AllSelectAccess.SetPage(__instance, num + 1);
			active.AdvanceTo(allSelectPlan);
			for (int k = 0; k < allSelectPlan.PerSlot.Length; k++)
			{
				AllSelectRound allSelectRound2 = allSelectPlan.PerSlot[k];
				if (allSelectRound2 != null)
				{
					AllSelectAccess.Dispatch(__instance, val.Players[k], allSelectRound2.MyOption);
				}
			}
			AllSelectBots.FillVotes(__instance, active, list, list2, val, num + 1);
			return false;
		}
		catch (Exception value)
		{
			MainFile.Logger.Error($"[AllSelect] Resolution failed, falling back to a shared vote: {value}", 1);
			return true;
		}
	}

	private static bool IsDispatchable(AllSelectPlan plan, List<EventModel> events, int playerCount)
	{
		for (int i = 0; i < plan.PerSlot.Length && i < playerCount; i++)
		{
			AllSelectRound allSelectRound = plan.PerSlot[i];
			if (allSelectRound != null)
			{
				if (i >= events.Count)
				{
					return Fail($"slot {i} has no event instance");
				}
				EventModel val = events[i];
				if (val.IsFinished)
				{
					return Fail($"slot {i}'s event is already finished");
				}
				if (allSelectRound.MyOption < 0 || allSelectRound.MyOption >= val.CurrentOptions.Count)
				{
					return Fail($"slot {i} picked option {allSelectRound.MyOption} but only {val.CurrentOptions.Count} are available");
				}
			}
		}
		return true;
		static bool Fail(string why)
		{
			MainFile.Logger.Error("[AllSelect] Refusing to dispatch the round: " + why + ".", 1);
			return false;
		}
	}
}
