using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace MultiplayerExpanded.Cards.Curses;

[Pool(typeof(CurseCardPool))]
public abstract class CurseCard : MultiplayerExpandedCard
{
	public override CardPoolModel Pool => (CardPoolModel)(object)ModelDb.CardPool<CurseCardPool>();

	protected CurseCard(int cost, CardType type, CardRarity rarity, TargetType target)
		: base(cost, type, rarity, target)
	{
	}
}
