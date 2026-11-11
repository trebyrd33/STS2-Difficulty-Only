using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Cards.Regent;

public sealed class ParticleWallMp : CardRework
{
	public override Type BaseCard => typeof(ParticleWall);

	public override int Cost => 0;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)5;

	public override int? StarCost => 2;

	public override bool GainsBlock => true;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new BlockVar(9m, (ValueProp)8) };
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await CreatureCmd.GainBlock(cardPlay.Target, card.DynamicVars.Block, cardPlay, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Block).UpgradeValueBy(3m);
	}
}
