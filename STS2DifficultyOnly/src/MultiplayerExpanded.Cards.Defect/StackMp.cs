using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Cards.Defect;

public sealed class StackMp : CardRework
{
	public override Type BaseCard => typeof(Stack);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)6;

	public override TargetType Target => (TargetType)5;

	public override bool GainsBlock => true;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[3]
			{
				(DynamicVar)new CalculationBaseVar(0m),
				(DynamicVar)new CalculationExtraVar(1m),
				(DynamicVar)((CalculatedVar)new CalculatedBlockVar((ValueProp)8)).WithMultiplier((Func<CardModel, Creature, decimal>)((CardModel card, Creature? _) => Enumerable.Count(PileTypeExtensions.GetPile((PileType)3, card.Owner).Cards)))
			};
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		CalculatedBlockVar calculatedBlock = card.DynamicVars.CalculatedBlock;
		await CreatureCmd.GainBlock(cardPlay.Target, ((CalculatedVar)calculatedBlock).Calculate(cardPlay.Target), calculatedBlock.Props, cardPlay, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.CalculationBase).UpgradeValueBy(3m);
	}
}
