using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerExpanded.Cards.Silent;

public sealed class FlankMp : CardRework
{
	public override Type BaseCard => typeof(Flanking);

	public override int Cost => 2;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)2;

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)1 };

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new IntVar("DamageBonus", 100m) };
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		decimal num = card.DynamicVars["DamageBonus"].BaseValue / 100m;
		FlankingPower val = cardPlay.Target.Powers.OfType<FlankingPower>().FirstOrDefault((FlankingPower p) => ((PowerModel)p).Applier == card.Owner.Creature);
		if (val != null)
		{
			await PowerCmd.ModifyAmount(choiceContext, (PowerModel)(object)val, num, card.Owner.Creature, card, false);
		}
		else
		{
			await PowerCmd.Apply<FlankingPower>(choiceContext, cardPlay.Target, num + 1m, card.Owner.Creature, card, false);
		}
	}

	public override void OnUpgrade(CardModel card)
	{
		card.EnergyCost.UpgradeBy(-1);
	}
}
