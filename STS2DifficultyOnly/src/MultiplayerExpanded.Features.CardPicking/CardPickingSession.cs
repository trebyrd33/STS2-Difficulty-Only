using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerExpanded.Features.CardPicking;

public sealed class CardPickingSession
{
	public sealed class PlayerVote
	{
		public int? Index;

		public bool VoteReceived;
	}

	private static readonly Logger _logger = new Logger("CardPickingSession", (LogType)3);

	private readonly IPlayerCollection _players;

	private readonly ulong _localPlayerId;

	private readonly ICardPickTransport _transport;

	private readonly Rng _rng;

	private readonly List<PlayerVote> _votes = new List<PlayerVote>();

	private readonly List<CardPickingResult> _results = new List<CardPickingResult>();

	private readonly List<(IReadOnlyList<CardPickingResult> Results, bool Final)> _resolvedRounds = new List<(IReadOnlyList<CardPickingResult>, bool)>();

	private readonly TaskCompletionSource<IReadOnlyList<CardPickingResult>> _completion = new TaskCompletionSource<IReadOnlyList<CardPickingResult>>();

	private bool[] _settled = Array.Empty<bool>();

	private bool[] _voting = Array.Empty<bool>();

	private bool[] _taken = Array.Empty<bool>();

	private PlayerVote? _predictedVote;

	public static CardPickingSession? Active { get; private set; }

	public IReadOnlyList<CardModel> Cards { get; }

	public IReadOnlyList<Player> Participants { get; }

	public bool AllowSkip { get; }

	public bool RepickLosers { get; }

	public int Round { get; private set; }

	public IReadOnlyList<(IReadOnlyList<CardPickingResult> Results, bool Final)> ResolvedRounds => _resolvedRounds;

	private Player LocalPlayer => _players.GetPlayer(_localPlayerId);

	public bool LocalPlayerParticipates => Participants.Contains(LocalPlayer);

	public Task<IReadOnlyList<CardPickingResult>> Completion => _completion.Task;

	public event Action<IReadOnlyList<CardPickingResult>, bool>? RoundResolved;

	public event Action? VotesChanged;

	public CardPickingSession(IReadOnlyList<CardModel> cards, IPlayerCollection players, IReadOnlyList<Player>? participants, ulong localPlayerId, ICardPickTransport transport, Rng rng, bool allowSkip = true, bool repickLosers = false)
	{
		Cards = cards;
		_players = players;
		Participants = (participants ?? players.Players).Where((Player p) =>
		{
			Creature creature = p.Creature;
			return creature == null || !creature.IsDead;
		}).OrderBy((Func<Player, int>)players.GetPlayerSlotIndex).ToList();
		_localPlayerId = localPlayerId;
		_transport = transport;
		_rng = rng;
		AllowSkip = allowSkip;
		RepickLosers = repickLosers;
	}

	public void Begin()
	{
		if (Active != null)
		{
			_logger.Error($"Starting a card picking session while one with {Active.Cards.Count} card(s) was still open. " + "The previous pick was abandoned without every vote landing; discarding it.", 1);
			Active.Abort();
		}
		Active = this;
		foreach (Player participant in Participants)
		{
			_ = participant;
			_votes.Add(new PlayerVote
			{
				VoteReceived = false
			});
		}
		_settled = new bool[Participants.Count];
		_voting = Enumerable.Repeat(element: true, Participants.Count).ToArray();
		_taken = new bool[Cards.Count];
		if (Cards.Count == 0 || Participants.Count == 0)
		{
			Finish(new List<CardPickingResult>());
			return;
		}
		if (!AllowSkip && Cards.Count < Participants.Count)
		{
			_logger.Warn($"Only {Cards.Count} card(s) for {Participants.Count} participant(s); some come away empty.", 1);
		}
		_transport.Open(this);
		RollBotVotes();
		VotesChanged?.Invoke();
	}

	private void RollBotVotes()
	{
		if (!RunManager.Instance.IsSingleplayerOrFakeMultiplayer || Participants.Count <= 1)
		{
			return;
		}
		List<int> list = (from i in Enumerable.Range(0, Cards.Count)
			where !_taken[i]
			select i).ToList();
		for (int num = 0; num < Participants.Count; num++)
		{
			if (Participants[num] != LocalPlayer && _voting[num])
			{
				PlayerVote playerVote = _votes[num];
				playerVote.Index = list[_rng.NextInt(list.Count)];
				playerVote.VoteReceived = true;
			}
		}
	}

	public bool IsVoting(Player player)
	{
		int num = ParticipantIndex(player);
		if (num >= 0 && num < _voting.Length)
		{
			return _voting[num];
		}
		return false;
	}

	public bool IsAvailable(int index)
	{
		if (index >= 0 && index < _taken.Length)
		{
			return !_taken[index];
		}
		return false;
	}

	public void PickLocally(int? index)
	{
		if (!IsVoting(LocalPlayer) || _completion.Task.IsCompleted)
		{
			return;
		}
		if (index.HasValue)
		{
			int valueOrDefault = index.GetValueOrDefault();
			if (!IsAvailable(valueOrDefault))
			{
				return;
			}
		}
		if (_transport.CanRevote || !GetVote(LocalPlayer).VoteReceived)
		{
			_predictedVote = new PlayerVote
			{
				Index = index,
				VoteReceived = true
			};
			if (!_transport.SendLocal(LocalPlayer, index, Round))
			{
				_predictedVote = null;
			}
			VotesChanged?.Invoke();
		}
	}

	public void SkipLocally()
	{
		PickLocally(null);
	}

	public void OnPicked(Player player, int? index, int round)
	{
		if (_completion.Task.IsCompleted || round != Round)
		{
			return;
		}
		int num = ParticipantIndex(player);
		if (num < 0 || !_voting[num])
		{
			_logger.Warn($"Ignoring a vote from player {player.NetId}, who does not vote in round {Round}.", 1);
			return;
		}
		if (index >= Cards.Count)
		{
			throw new IndexOutOfRangeException($"Attempted to pick card at index {index}, but only {Cards.Count} were offered!");
		}
		if (index.HasValue)
		{
			int valueOrDefault = index.GetValueOrDefault();
			if (_taken[valueOrDefault])
			{
				_logger.Warn($"Ignoring player {player.NetId}'s vote for card {valueOrDefault}, which is already taken.", 1);
				return;
			}
		}
		PlayerVote playerVote = _votes[num];
		playerVote.Index = index;
		playerVote.VoteReceived = true;
		if (player == LocalPlayer && _predictedVote != null)
		{
			bool flag = _predictedVote.Index != index;
			_predictedVote = null;
			if (flag)
			{
				VotesChanged?.Invoke();
			}
		}
		VotesChanged?.Invoke();
		if (_votes.Where((PlayerVote _, int s) => _voting[s]).All((PlayerVote v) => v.VoteReceived))
		{
			ResolveRound();
		}
	}

	public PlayerVote GetVote(Player player)
	{
		if (player == LocalPlayer && _predictedVote != null)
		{
			return _predictedVote;
		}
		int num = ParticipantIndex(player);
		if (num < 0 || num >= _votes.Count)
		{
			return new PlayerVote();
		}
		return _votes[num];
	}

	private int ParticipantIndex(Player player)
	{
		for (int i = 0; i < Participants.Count; i++)
		{
			if (Participants[i] == player)
			{
				return i;
			}
		}
		return -1;
	}

	private void ResolveRound()
	{
		Dictionary<int, List<Player>> dictionary = new Dictionary<int, List<Player>>();
		for (int i = 0; i < Cards.Count; i++)
		{
			dictionary[i] = new List<Player>();
		}
		for (int j = 0; j < _votes.Count; j++)
		{
			PlayerVote playerVote = _votes[j];
			if (_voting[j] && playerVote.Index.HasValue)
			{
				dictionary[playerVote.Index.Value].Add(Participants[j]);
			}
		}
		List<CardPickingResult> list = new List<CardPickingResult>();
		for (int k = 0; k < Cards.Count; k++)
		{
			List<Player> list2 = dictionary[k];
			if (_taken[k] || list2.Count == 0)
			{
				continue;
			}
			CardPickingResult cardPickingResult;
			if (list2.Count == 1)
			{
				cardPickingResult = new CardPickingResult
				{
					Type = CardPickingResultType.OnlyOnePlayerVoted,
					Card = Cards[k],
					Player = list2[0]
				};
			}
			else
			{
				RelicPickingFightMove[] possibleMoves = Enum.GetValues<RelicPickingFightMove>();
				(Player Winner, RelicPickingFight Fight) tuple = PickingFight.Generate(list2, () =>
				{
					return _rng.NextItem<RelicPickingFightMove>((IEnumerable<RelicPickingFightMove>)possibleMoves);
				});
				Player item = tuple.Winner;
				RelicPickingFight item2 = tuple.Fight;
				cardPickingResult = new CardPickingResult
				{
					Type = CardPickingResultType.FoughtOver,
					Card = Cards[k],
					Player = item,
					Fight = item2
				};
			}
			list.Add(cardPickingResult);
			_taken[k] = true;
			_settled[ParticipantIndex(cardPickingResult.Player)] = true;
		}
		List<int> list3 = (from s in Enumerable.Range(0, Participants.Count)
			where _voting[s] && !_settled[s] && _votes[s].Index.HasValue
			select s).ToList();
		if (RepickLosers && list3.Count > 0 && Enumerable.Contains(_taken, value: false))
		{
			StartNextRound(list3, list);
			return;
		}
		List<Player> list4 = Participants.Where((Player p, int slot) =>
		{
			PlayerVote playerVote2 = _votes[slot];
			bool flag = AllowSkip && playerVote2.VoteReceived && !playerVote2.Index.HasValue;
			return !_settled[slot] && !flag;
		}).ToList();
		List<int> list5 = (from num2 in Enumerable.Range(0, Cards.Count)
			where !_taken[num2]
			select num2).ToList();
		ListExtensions.StableShuffle<int>(list5, _rng);
		for (int num = 0; num < list5.Count; num++)
		{
			list.Add(new CardPickingResult
			{
				Type = ((num < list4.Count) ? CardPickingResultType.ConsolationPrize : CardPickingResultType.Skipped),
				Player = ((num < list4.Count) ? list4[num] : null),
				Card = Cards[list5[num]]
			});
		}
		_results.AddRange(list);
		_resolvedRounds.Add((list, true));
		RoundResolved?.Invoke(list, arg2: true);
		Finish(_results);
	}

	private void StartNextRound(List<int> losers, List<CardPickingResult> results)
	{
		_results.AddRange(results);
		Round++;
		for (int i = 0; i < Participants.Count; i++)
		{
			_voting[i] = losers.Contains(i);
			if (_voting[i])
			{
				_votes[i] = new PlayerVote();
			}
		}
		_predictedVote = null;
		_transport.OpenRound(this);
		RollBotVotes();
		_resolvedRounds.Add((results, false));
		RoundResolved?.Invoke(results, arg2: false);
		VotesChanged?.Invoke();
	}

	private void Finish(List<CardPickingResult> results)
	{
		Active = null;
		_completion.TrySetResult(results);
	}

	public void Abort()
	{
		if (Active == this)
		{
			Active = null;
		}
		_completion.TrySetResult(Array.Empty<CardPickingResult>());
	}

	public static void Reset()
	{
		if (Active != null)
		{
			_logger.Debug("Run torn down with a card pick still open; discarding it.", 1);
			Active.Abort();
			Active = null;
		}
	}

	static CardPickingSession()
	{
	}
}
