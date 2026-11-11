using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Features.AllSelect;

internal static class AllSelectBots
{
	private static bool IsActive(IPlayerCollection players)
	{
		RunManager instance = RunManager.Instance;
		if (instance != null && instance.IsSingleplayerOrFakeMultiplayer)
		{
			return players.Players.Count > 1;
		}
		return false;
	}

	internal static bool ShouldForceEnd(AllSelectSession session, IPlayerCollection players)
	{
		if (!IsActive(players))
		{
			return false;
		}
		for (int i = 0; i < players.Players.Count && i < session.StillPlaying.Count; i++)
		{
			if (session.StillPlaying[i] && LocalContext.IsMe(players.Players[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal static void FillVotes(EventSynchronizer sync, AllSelectSession session, List<EventModel> events, List<uint?> votes, IPlayerCollection players, uint pageIndex)
	{
		if (!IsActive(players))
		{
			return;
		}
		try
		{
			Rng val = GameApi.NewRng(GameApi.Seed(players.Players[0].RunState.Rng), $"mpx_all_select_bots_{pageIndex}");
			for (int i = 0; i < players.Players.Count; i++)
			{
				if (i < session.StillPlaying.Count && session.StillPlaying[i] && !LocalContext.IsMe(players.Players[i]) && i < events.Count && i < votes.Count)
				{
					int count = events[i].CurrentOptions.Count;
					if (count > 0)
					{
						votes[i] = (uint)val.NextInt(count);
					}
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[AllSelect] Could not fill fake-multiplayer votes: " + ex.Message, 1);
		}
	}

	internal static void FillOpeningVotes(EventSynchronizer sync, List<EventModel> events, List<uint?> votes, IPlayerCollection players)
	{
		FillVotes(sync, AllSelectSession.Active, events, votes, players, 0u);
	}

	internal static bool[] Participants(List<EventModel> events, IPlayerCollection players)
	{
		bool[] array = new bool[players.Players.Count];
		for (int i = 0; i < array.Length; i++)
		{
			Creature creature = players.Players[i].Creature;
			bool flag = creature != null && !creature.IsDead;
			bool flag2 = i >= events.Count || !events[i].IsFinished;
			array[i] = flag & flag2;
		}
		return array;
	}

	internal static bool AnyParticipants(IEnumerable<bool> mask)
	{
		return mask.Any((bool p) => p);
	}
}
