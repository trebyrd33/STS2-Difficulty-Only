using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Cards.Ironclad;

public sealed class FlameBarrierMp : CardRework
{
	public override Type BaseCard => typeof(FlameBarrier);

	public override int Cost => 2;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)5;

	public override bool GainsBlock => true;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[2]
			{
				(DynamicVar)new BlockVar(12m, (ValueProp)8),
				new DynamicVar("DamageBack", 4m)
			};
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		NFireBurningVfx val = NFireBurningVfx.Create(cardPlay.Target, 0.75f, false);
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance != null)
		{
			GodotTreeExtensions.AddChildSafely((Node)(object)instance.CombatVfxContainer, (Node)(object)val);
		}
		await CreatureCmd.GainBlock(cardPlay.Target, card.DynamicVars.Block, cardPlay, false);
		await PowerCmd.Apply<FlameBarrierPower>(choiceContext, cardPlay.Target, card.DynamicVars["DamageBack"].BaseValue, card.Owner.Creature, card, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Block).UpgradeValueBy(4m);
		card.DynamicVars["DamageBack"].UpgradeValueBy(2m);
	}
}
