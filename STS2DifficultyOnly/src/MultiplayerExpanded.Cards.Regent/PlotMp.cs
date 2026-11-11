using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Powers.Regent;

namespace MultiplayerExpanded.Cards.Regent;

[BetaOnly]
public sealed class PlotMp : CardRework
{
	public override Type BaseCard => typeof(Plot);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)7;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new CardsVar(2) };
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<Creature> enumerable = from p in card.CombatState.Players
			select p.Creature into c
			where c.IsAlive
			select c;
		await PowerCmd.Apply<PlotPower>(choiceContext, enumerable, ((DynamicVar)card.DynamicVars.Cards).BaseValue, card.Owner.Creature, card, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		card.EnergyCost.UpgradeBy(-1);
	}
}
