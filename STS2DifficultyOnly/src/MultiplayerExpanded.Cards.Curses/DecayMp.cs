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
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Powers.Colorless;

namespace MultiplayerExpanded.Cards.Curses;

public sealed class DecayMp : CardRework
{
	public override Type BaseCard => typeof(Decay);

	public override int Cost => -1;

	public override CardType Type => (CardType)5;

	public override CardRarity Rarity => (CardRarity)9;

	public override TargetType Target => (TargetType)0;

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)4 };

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new DamageVar(2m, (ValueProp)12) };
		}
	}

	public override async Task OnTurnEndInHand(CardModel card, PlayerChoiceContext choiceContext)
	{
		if (card.Owner.Creature.IsAlive)
		{
			await PowerCmd.Apply<DecayedPower>(choiceContext, card.Owner.Creature, ((DynamicVar)card.DynamicVars.Damage).BaseValue, (Creature)null, card, false);
		}
	}
}
