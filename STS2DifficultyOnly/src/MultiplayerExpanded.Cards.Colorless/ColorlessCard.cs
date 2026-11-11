using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace MultiplayerExpanded.Cards.Colorless;

[Pool(typeof(ColorlessCardPool))]
public abstract class ColorlessCard : MultiplayerExpandedCard
{
	public override CardPoolModel Pool => (CardPoolModel)(object)ModelDb.CardPool<ColorlessCardPool>();

	protected ColorlessCard(int cost, CardType type, CardRarity rarity, TargetType target)
		: base(cost, type, rarity, target)
	{
	}
}
