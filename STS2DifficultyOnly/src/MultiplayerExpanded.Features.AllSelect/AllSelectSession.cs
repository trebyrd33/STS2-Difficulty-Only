using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Features.AllSelect;

internal sealed class AllSelectSession
{
	internal const int MaxPages = 14;

	internal const uint FinishedSentinel = uint.MaxValue;

	private static readonly Logger Logger = new Logger("AllSelect", (LogType)3);

	private readonly IPlayerCollection _players;

	private readonly IAllSelectEvent _rules;

	private readonly string _eventIdEntry;

	private readonly ulong _runSeed;

	private bool[] _stillPlaying;

	private AllSelectVotesMessage? _pending;

	public static AllSelectSession? Active { get; private set; }

	public IReadOnlyList<bool> StillPlaying => _stillPlaying;

	private AllSelectSession(IPlayerCollection players, IAllSelectEvent rules, string eventIdEntry, ulong runSeed, bool[] stillPlaying)
	{
		_players = players;
		_rules = rules;
		_eventIdEntry = eventIdEntry;
		_runSeed = runSeed;
		_stillPlaying = stillPlaying;
	}

	public static void Reset()
	{
		Active = null;
	}

	public static AllSelectSession Begin(IPlayerCollection players, IAllSelectEvent rules, string eventIdEntry, ulong runSeed, bool[] stillPlaying)
	{
		if (Active != null)
		{
			Logger.Error("Beginning an all-select event while a previous session was still open; discarding it.", 1);
		}
		Active = new AllSelectSession(players, rules, eventIdEntry, runSeed, stillPlaying);
		return Active;
	}

	public AllSelectVotesMessage PublishVotes(IReadOnlyList<uint?> votes, uint pageIndex, RunLocation location)
	{
		return _pending = AllSelectVotesMessage.From(votes, _stillPlaying, pageIndex, location);
	}

	public void ReceiveVotes(AllSelectVotesMessage message)
	{
		_pending = message;
	}

	public AllSelectVotesMessage? TakePending(uint pageIndex)
	{
		AllSelectVotesMessage pending = _pending;
		_pending = null;
		if (pending == null)
		{
			Logger.Error($"No all-select vote vector for page {pageIndex}; falling back to a shared vote.", 1);
			return null;
		}
		if (pending.PageIndex != pageIndex)
		{
			Logger.Error($"All-select vote vector is for page {pending.PageIndex} but we are resolving page {pageIndex}; " + "falling back to a shared vote.", 1);
			return null;
		}
		return pending;
	}

	public void ReconcileMask(IReadOnlyList<bool> hostMask)
	{
		if (hostMask.Count != _stillPlaying.Length || hostMask.Where((bool t, int i) => t != _stillPlaying[i]).Any())
		{
			Logger.Error($"All-select participation mask diverged from the host: ours [{string.Join(",", _stillPlaying)}], host [{string.Join(",", hostMask)}]. Adopting the host's.", 1);
			_stillPlaying = hostMask.ToArray();
		}
	}

	public void AdvanceTo(AllSelectPlan plan)
	{
		_stillPlaying = plan.StillPlayingAfter;
	}

	public AllSelectPlan Resolve(IReadOnlyList<uint?> votes, uint pageIndex, bool forceEnd)
	{
		return Resolve(_players.Players, votes, _stillPlaying, (int)pageIndex, _rules, pageIndex, _runSeed, _eventIdEntry, forceEnd);
	}

	internal static AllSelectPlan Resolve(IReadOnlyList<Player> players, IReadOnlyList<uint?> votes, IReadOnlyList<bool> stillPlaying, int roundNumber, IAllSelectEvent rules, uint pageIndex, ulong runSeed, string eventIdEntry, bool forceEnd)
	{
		int num = Math.Clamp(rules.MaxRounds, 1, 14);
		bool flag = forceEnd || roundNumber + 1 >= num;
		List<AllSelectPick> list = new List<AllSelectPick>();
		for (int i = 0; i < players.Count; i++)
		{
			if (i >= stillPlaying.Count || !stillPlaying[i] || i >= votes.Count)
			{
				continue;
			}
			uint? num2 = votes[i];
			if (num2.HasValue)
			{
				uint valueOrDefault = num2.GetValueOrDefault();
				if (valueOrDefault != uint.MaxValue)
				{
					list.Add(new AllSelectPick(players[i], i, (int)valueOrDefault));
				}
			}
		}
		bool[] array = new bool[players.Count];
		bool[] array2 = new bool[players.Count];
		Dictionary<int, Player> dictionary = new Dictionary<int, Player>();
		Rng val = GameApi.NewRng(runSeed, $"mpx_all_select_{eventIdEntry}_{pageIndex}");
		foreach (int optionIndex in from result in list.Select((AllSelectPick p) => p.OptionIndex).Distinct()
			orderby result
			select result)
		{
			List<AllSelectPick> list2 = list.Where((AllSelectPick p) => p.OptionIndex == optionIndex).ToList();
			AllSelectContention allSelectContention = rules.ContentionFor(optionIndex);
			if (allSelectContention == AllSelectContention.Shared)
			{
				dictionary[optionIndex] = null;
				foreach (AllSelectPick item in list2)
				{
					array[item.SlotIndex] = true;
				}
				continue;
			}
			if (list2.Count == 1)
			{
				dictionary[optionIndex] = list2[0].Player;
				array[list2[0].SlotIndex] = true;
				continue;
			}
			if (allSelectContention == AllSelectContention.SoleOnly && !flag)
			{
				dictionary[optionIndex] = null;
				foreach (AllSelectPick item2 in list2)
				{
					array2[item2.SlotIndex] = true;
				}
				continue;
			}
			AllSelectPick allSelectPick = list2[val.NextInt(list2.Count)];
			dictionary[optionIndex] = allSelectPick.Player;
			array[allSelectPick.SlotIndex] = true;
			foreach (AllSelectPick item3 in list2)
			{
				if (item3.SlotIndex != allSelectPick.SlotIndex && !flag)
				{
					array2[item3.SlotIndex] = true;
				}
			}
		}
		bool[] stillPlayingAfter = new bool[players.Count];
		for (int num3 = 0; num3 < players.Count; num3++)
		{
			stillPlayingAfter[num3] = array2[num3];
		}
		List<Player> stillPlaying2 = players.Where((Player _, int slot) => stillPlayingAfter[slot]).ToList();
		AllSelectRound[] array3 = new AllSelectRound[players.Count];
		foreach (AllSelectPick item4 in list)
		{
			array3[item4.SlotIndex] = new AllSelectRound(roundNumber, list, dictionary, stillPlaying2, item4.Player, item4.OptionIndex, array[item4.SlotIndex], array2[item4.SlotIndex], flag);
		}
		return new AllSelectPlan
		{
			PerSlot = array3,
			StillPlayingAfter = stillPlayingAfter
		};
	}

	static AllSelectSession()
	{
	}
}
