using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class HoardingPower : MultiplayerExpandedPower
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
		CardPile pile = card.Pile;
		if (pile != null && (int)pile.Type == 2)
		{
			return card.Pile.Cards.Count > 2;
		}
		return true;
	}
}
