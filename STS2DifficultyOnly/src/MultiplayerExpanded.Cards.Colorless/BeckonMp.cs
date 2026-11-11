using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MultiplayerExpanded.Powers.Colorless;

namespace MultiplayerExpanded.Cards.Colorless;

public sealed class BeckonMp : CardRework
{
	public override Type BaseCard => typeof(Beckon);

	public override int Cost => 1;

	public override CardType Type => (CardType)4;

	public override CardRarity Rarity => (CardRarity)8;

	public override TargetType Target => (TargetType)0;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new HpLossVar(6m) };
		}
	}

	public override async Task OnTurnEndInHand(CardModel card, PlayerChoiceContext choiceContext)
	{
		if (card.Owner.Creature.IsAlive)
		{
			await PowerCmd.Apply<BeckonedPower>(choiceContext, card.Owner.Creature, ((DynamicVar)card.DynamicVars.HpLoss).BaseValue, (Creature)null, card, false);
		}
	}
}
