using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MultiplayerExpanded.Cards.Ironclad;

public sealed class InfernalBladeMp : CardRework
{
	public override Type BaseCard => typeof(InfernalBlade);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)5;

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)1 };

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Player player = cardPlay.Target.Player;
		if (player != null)
		{
			Player owner = card.Owner;
			CardModel val = CardFactory.GetDistinctForCombat(player, owner.Character.CardPool.GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint).Where((CardModel c) =>
			{
				return (int)c.Type == 1;
			}), 1, owner.RunState.Rng.CombatCardGeneration).FirstOrDefault();
			if (val != null)
			{
				val.SetToFreeThisTurn();
				await CardPileCmd.AddGeneratedCardToCombat(val, (PileType)2, player, (CardPilePosition)1);
			}
		}
	}

	public override void OnUpgrade(CardModel card)
	{
		card.EnergyCost.UpgradeBy(-1);
	}
}
