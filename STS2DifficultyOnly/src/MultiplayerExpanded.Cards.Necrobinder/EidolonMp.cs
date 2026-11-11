using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Necrobinder;

[StableOnly]
public sealed class EidolonMp : CardRework
{
	public override Type BaseCard => typeof(Eidolon);

	public override int Cost => 2;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)1;

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)1 };

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[2]
	{
		HoverTipFactory.FromKeyword((CardKeyword)1),
		HoverTipFactory.FromPower<IntangiblePower>((int?)null)
	};

	public override void OnUpgrade(CardModel card)
	{
		card.EnergyCost.UpgradeBy(-1);
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		List<CardModel> list = PileTypeExtensions.GetPile((PileType)2, card.Owner).Cards.ToList();
		int exhausted = 0;
		foreach (CardModel item in list)
		{
			await GameApi.Exhaust(choiceContext, item);
			exhausted++;
		}
		if (exhausted < 9)
		{
			return;
		}
		foreach (Player item2 in card.CombatState.Players.Where((Player p) => p.Creature.IsAlive))
		{
			await PowerCmd.Apply<IntangiblePower>(choiceContext, item2.Creature, 1m, card.Owner.Creature, card, false);
		}
	}
}
