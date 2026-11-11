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
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Ironclad;

public sealed class BloodWallMp : CardRework
{
	public override Type BaseCard => typeof(BloodWall);

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
				(DynamicVar)new HpLossVar(2m),
				(DynamicVar)new BlockVar(16m, (ValueProp)8)
			};
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		VfxCmd.PlayOnCreatureCenter(card.Owner.Creature, "vfx/vfx_bloody_impact");
		await GameApi.Damage(choiceContext, card.Owner.Creature, ((DynamicVar)card.DynamicVars.HpLoss).BaseValue, (ValueProp)14, card.Owner.Creature, card, cardPlay);
		await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
		SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_bloodwall", 1f);
		VfxCmd.PlayOnCreature(cardPlay.Target, "vfx/vfx_blood_wall");
		await CreatureCmd.GainBlock(cardPlay.Target, card.DynamicVars.Block, cardPlay, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Block).UpgradeValueBy(4m);
	}
}
