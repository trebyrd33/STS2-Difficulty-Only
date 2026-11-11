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
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MultiplayerExpanded.Cards.Silent;

public sealed class StormOfSteelMp : CardRework
{
	public override Type BaseCard => typeof(StormOfSteel);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)5;

	public override IEnumerable<IHoverTip> HoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Shiv>(false));

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Player allyPlayer = cardPlay.Target.Player;
		List<CardModel> list = PileTypeExtensions.GetPile((PileType)2, allyPlayer).Cards.ToList();
		int discardCount = list.Count;
		foreach (CardModel item in list)
		{
			await CardCmd.Discard(choiceContext, item);
		}
		bool upgraded = card.IsUpgraded;
		await CardPileCmd.AddGeneratedCardsToCombat((IEnumerable<CardModel>)Enumerable.Range(0, discardCount).Select((int _) =>
		{
			Shiv val = card.CombatState.CreateCard<Shiv>(allyPlayer);
			if (upgraded)
			{
				CardCmd.Upgrade((CardModel)(object)val, (CardPreviewStyle)1);
			}
			return (CardModel)(object)val;
		}).ToList(), (PileType)2, allyPlayer, (CardPilePosition)1);
	}
}
