using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MultiplayerExpanded.Features.AllSelect;

public sealed class AllSelectRound
{
	private readonly IReadOnlyDictionary<int, Player?> _winners;

	public int RoundNumber { get; }

	public IReadOnlyList<AllSelectPick> Picks { get; }

	public Player Owner { get; }

	public int MyOption { get; }

	public bool TakesIt { get; }

	public bool IsReprompt { get; }

	public bool WasForcedByCap { get; }

	public IReadOnlyList<Player> StillPlaying { get; }

	public bool IsContested => CountFor(MyOption) > 1;

	internal AllSelectRound(int roundNumber, IReadOnlyList<AllSelectPick> picks, IReadOnlyDictionary<int, Player?> winners, IReadOnlyList<Player> stillPlaying, Player owner, int myOption, bool takesIt, bool isReprompt, bool wasForcedByCap)
	{
		RoundNumber = roundNumber;
		Picks = picks;
		_winners = winners;
		StillPlaying = stillPlaying;
		Owner = owner;
		MyOption = myOption;
		TakesIt = takesIt;
		IsReprompt = isReprompt;
		WasForcedByCap = wasForcedByCap;
	}

	public IReadOnlyList<Player> WhoPicked(int optionIndex)
	{
		return (from p in Picks
			where p.OptionIndex == optionIndex
			select p.Player).ToList();
	}

	public int CountFor(int optionIndex)
	{
		return Enumerable.Count(Picks, (AllSelectPick p) => p.OptionIndex == optionIndex);
	}

	public Player? WinnerFor(int optionIndex)
	{
		if (!_winners.TryGetValue(optionIndex, out Player value))
		{
			return null;
		}
		return value;
	}
}
