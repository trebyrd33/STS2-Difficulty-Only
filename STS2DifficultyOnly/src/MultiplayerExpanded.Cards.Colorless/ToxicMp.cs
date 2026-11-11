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

namespace MultiplayerExpanded.Cards.Colorless;

public sealed class ToxicMp : CardRework
{
	public override Type BaseCard => typeof(Toxic);

	public override int Cost => 1;

	public override CardType Type => (CardType)4;

	public override CardRarity Rarity => (CardRarity)8;

	public override TargetType Target => (TargetType)0;

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)1 };

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new DamageVar(5m, (ValueProp)12) };
		}
	}

	public override async Task OnTurnEndInHand(CardModel card, PlayerChoiceContext choiceContext)
	{
		if (card.Owner.Creature.IsAlive)
		{
			await PowerCmd.Apply<ToxifiedPower>(choiceContext, card.Owner.Creature, ((DynamicVar)card.DynamicVars.Damage).BaseValue, (Creature)null, card, false);
		}
	}
}
