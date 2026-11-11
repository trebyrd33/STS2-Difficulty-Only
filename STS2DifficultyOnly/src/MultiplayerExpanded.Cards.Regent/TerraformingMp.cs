using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Regent;

public sealed class TerraformingMp : CardRework
{
	public override Type BaseCard => typeof(Terraforming);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)5;

	public override IEnumerable<DynamicVar> Vars => new DynamicVar[1] { (DynamicVar)new PowerVar<VigorPower>(Beta.Pick(6m, 7m)) };

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[1] { HoverTipFactory.FromPower<VigorPower>((int?)null) };

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await PowerCmd.Apply<VigorPower>(choiceContext, cardPlay.Target, card.DynamicVars["VigorPower"].BaseValue, card.Owner.Creature, card, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		card.DynamicVars["VigorPower"].UpgradeValueBy(Beta.Pick(2m, 3m));
	}
}
