using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Silent;

[BetaOnly]
public sealed class FadeMp : CardRework
{
	public override Type BaseCard => typeof(Fade);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)6;

	public override IEnumerable<CardKeyword> Keywords => new _003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		(CardKeyword)5,
		(CardKeyword)1
	});

	public override IEnumerable<IHoverTip> HoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static((StaticHoverTip)5, Array.Empty<DynamicVar>()));

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Player player = cardPlay.Target.Player;
		if (player == null)
		{
			return;
		}
		Player owner = card.Owner;
		CardModel val = CardFactory.GetDistinctForCombat(player, owner.Character.CardPool.GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint).Where((CardModel c) =>
		{
			return (int)c.Type == 2 && c.GainsBlock;
		}), 1, owner.RunState.Rng.CombatCardGeneration).FirstOrDefault();
		if (val != null)
		{
			if (card.IsUpgraded)
			{
				CardCmd.Upgrade(val, (CardPreviewStyle)1);
			}
			val.SetToFreeThisTurn();
			await CardPileCmd.AddGeneratedCardToCombat(val, (PileType)2, player, (CardPilePosition)1);
		}
	}
}
