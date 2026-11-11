using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Regent;

[BetaOnly]
public sealed class TutorMp : CardRework
{
	public override Type BaseCard => typeof(Tutor);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)6;

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)1 };

	public override void OnUpgrade(CardModel card)
	{
		card.EnergyCost.UpgradeBy(-1);
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Player player = cardPlay.Target.Player;
		if (player != null)
		{
			LocString val = new LocString("cards", LocKey + ".selectionScreenPrompt");
			CardModel val2 = (await CardSelectCmd.FromCombatPile(choiceContext, PileTypeExtensions.GetPile((PileType)1, player), player, new CardSelectorPrefs(val, 1))).FirstOrDefault();
			if (val2 != null)
			{
				await CardPileCmd.Add(val2, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
			}
		}
	}
}
