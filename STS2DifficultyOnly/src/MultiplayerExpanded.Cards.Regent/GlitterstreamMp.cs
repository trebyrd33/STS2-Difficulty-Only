using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Cards.Regent;

public sealed class GlitterstreamMp : CardRework
{
	public override Type BaseCard => typeof(Glitterstream);

	public override int Cost => 2;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)2;

	public override TargetType Target => (TargetType)5;

	public override bool GainsBlock => true;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[2]
			{
				(DynamicVar)new BlockVar(11m, (ValueProp)8),
				(DynamicVar)new BlockVar("BlockNextTurn", 5m, (ValueProp)8)
			};
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await CreatureCmd.GainBlock(cardPlay.Target, card.DynamicVars.Block, cardPlay, false);
		BlockVar val = (BlockVar)card.DynamicVars["BlockNextTurn"];
		IEnumerable<AbstractModel> enumerable = default;
		decimal num = Hook.ModifyBlock(card.CombatState, cardPlay.Target, ((DynamicVar)val).BaseValue, val.Props, card, cardPlay, out enumerable);
		await PowerCmd.Apply<BlockNextTurnPower>(choiceContext, cardPlay.Target, num, card.Owner.Creature, card, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Block).UpgradeValueBy(2m);
		card.DynamicVars["BlockNextTurn"].UpgradeValueBy(2m);
	}
}
