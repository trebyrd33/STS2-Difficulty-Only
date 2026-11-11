using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace MultiplayerExpanded.Cards.Tokens;

[Pool(typeof(TokenCardPool))]
public abstract class TokenCard : MultiplayerExpandedCard
{
	public override CardPoolModel Pool => (CardPoolModel)(object)ModelDb.CardPool<TokenCardPool>();

	protected TokenCard(int cost, CardType type, CardRarity rarity, TargetType target)
		: base(cost, type, rarity, target)
	{
	}
}
