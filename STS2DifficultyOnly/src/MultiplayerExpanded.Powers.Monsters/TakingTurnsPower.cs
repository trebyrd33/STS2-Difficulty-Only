using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Powers.Monsters;

public sealed class TakingTurnsPower : MultiplayerExpandedPower
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
		CardPlayStartedEntry? val = CombatManager.Instance.History.CardPlaysStarted.LastOrDefault((CardPlayStartedEntry e) => !e.CardPlay.IsAutoPlay && ((CombatHistoryEntry)e).HappenedThisTurn(this.CombatState));
		return ((val != null) ? val.CardPlay.Card.Owner : null) != card.Owner;
	}
}
