using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class InnerWorldPower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)2;

	public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
	{
		if ((int)autoPlayType == 0 && card.Owner.Creature == this.Owner)
		{
			return (int)card.TargetType != 6;
		}
		return true;
	}
}
