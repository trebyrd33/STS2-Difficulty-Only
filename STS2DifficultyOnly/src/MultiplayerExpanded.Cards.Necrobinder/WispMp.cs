using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MultiplayerExpanded.Cards.Necrobinder;

public sealed class WispMp : CardRework
{
	public override Type BaseCard => typeof(Wisp);

	public override int Cost => 0;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)2;

	public override TargetType Target => (TargetType)5;

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)1 };

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return (IEnumerable<DynamicVar>)(object)new EnergyVar[1]
			{
				new EnergyVar(1)
			};
		}
	}

	public override void OnUpgrade(CardModel card)
	{
		card.AddKeyword((CardKeyword)5);
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await PlayerCmd.GainEnergy(((DynamicVar)card.DynamicVars.Energy).BaseValue, cardPlay.Target.Player);
	}
}
