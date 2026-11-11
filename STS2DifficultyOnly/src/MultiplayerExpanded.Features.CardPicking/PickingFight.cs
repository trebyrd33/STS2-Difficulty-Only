using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;

namespace MultiplayerExpanded.Features.CardPicking;

public static class PickingFight
{
	public static (Player Winner, RelicPickingFight Fight) Generate(List<Player> players, Func<RelicPickingFightMove> generateMove)
	{
		RelicPickingFight val = new RelicPickingFight();
		val.playersInvolved.AddRange(players);
		HashSet<Player> hashSet = new HashSet<Player>(players);
		while (hashSet.Count > 1)
		{
			RelicPickingFightRound val2 = new RelicPickingFightRound();
			foreach (Player player in players)
			{
				val2.moves.Add(hashSet.Contains(player) ? (RelicPickingFightMove?)generateMove() : ((RelicPickingFightMove?)null));
			}
			val.rounds.Add(val2);
			List<RelicPickingFightMove> list = val2.moves.OfType<RelicPickingFightMove>().Distinct().ToList();
			if (list.Count != 2)
			{
				continue;
			}
			RelicPickingFightMove losingMove = GetLosingMove(list[0], list[1]);
			for (int i = 0; i < players.Count; i++)
			{
				if (val2.moves[i] == (RelicPickingFightMove?)losingMove)
				{
					hashSet.Remove(players[i]);
				}
			}
		}
		return (Winner: hashSet.First(), Fight: val);
	}

	private static RelicPickingFightMove GetLosingMove(RelicPickingFightMove move1, RelicPickingFightMove move2)
	{
		if ((RelicPickingFightMove)(((int)move1 + 1) % 3) != move2)
		{
			return move2;
		}
		return move1;
	}
}
