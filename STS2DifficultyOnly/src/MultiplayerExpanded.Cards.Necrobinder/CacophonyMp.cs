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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Necrobinder;

[BetaOnly]
public sealed class CacophonyMp : CardRework
{
	public override Type BaseCard => typeof(Cacophony);

	public override int Cost => 2;

	public override CardType Type => (CardType)3;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)1;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[2]
			{
				(DynamicVar)new CardsVar(33),
				(DynamicVar)new DamageVar(66m, (ValueProp)4)
			};
		}
	}

	public override Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return ApplyCacophony(choiceContext, card.Owner.Creature, card);
	}

	private static Task ApplyCacophony(PlayerChoiceContext choiceContext, Creature owner, CardModel card)
	{
		return PowerCmd.Apply<CacophonyPower>(choiceContext, owner, (decimal)((DynamicVar)card.DynamicVars.Damage).IntValue, owner, card, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Damage).UpgradeValueBy(33m);
	}
}
