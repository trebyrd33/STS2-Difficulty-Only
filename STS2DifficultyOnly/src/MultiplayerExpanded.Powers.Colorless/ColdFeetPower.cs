using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class ColdFeetPower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)2;

	public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
	{
		if ((int)autoPlayType != 0)
		{
			return true;
		}
		Player owner = card.Owner;
		if (((owner != null) ? owner.Creature : null) != this.Owner)
		{
			return true;
		}
		if (Enumerable.Count(this.CombatState.Players, (Player p) => p.Creature.IsAlive) < 2)
		{
			return true;
		}
		List<CardPlayStartedEntry> list = CombatManager.Instance.History.CardPlaysStarted.ToList();
		int num = list.FindLastIndex((CardPlayStartedEntry e) => e.CardPlay.Card.Owner != card.Owner);
		if (list.FindLastIndex((CardPlayStartedEntry e) => e.CardPlay.Card.Owner == card.Owner) > num)
		{
			return false;
		}
		if (num >= 0)
		{
			return list[num].CardPlay.Card.Type == card.Type;
		}
		return true;
	}
}
