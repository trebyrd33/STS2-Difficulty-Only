using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MultiplayerExpanded.Cards.Defect;

public sealed class ChaosMp : CardRework
{
	public override Type BaseCard => typeof(Chaos);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)5;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new IntVar("Orbs", 1m) };
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Player targetPlayer = cardPlay.Target.Player;
		int count = (int)card.DynamicVars["Orbs"].BaseValue;
		for (int i = 0; i < count; i++)
		{
			OrbModel val = OrbModel.GetRandomOrb(targetPlayer.RunState.Rng.CombatOrbGeneration).ToMutable(0);
			await OrbCmd.Channel(choiceContext, val, targetPlayer);
		}
	}

	public override void OnUpgrade(CardModel card)
	{
		card.DynamicVars["Orbs"].UpgradeValueBy(1m);
	}
}
