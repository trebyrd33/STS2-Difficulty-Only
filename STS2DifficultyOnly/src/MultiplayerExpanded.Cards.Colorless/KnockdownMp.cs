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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Colorless;

public sealed class KnockdownMp : CardRework
{
	public override Type BaseCard => typeof(Knockdown);

	public override int Cost => 3;

	public override CardType Type => (CardType)1;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)2;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[2]
			{
				(DynamicVar)new DamageVar(10m, (ValueProp)8),
				(DynamicVar)new IntVar("DamageBonus", 100m)
			};
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await DamageCmd.Attack(((DynamicVar)card.DynamicVars.Damage).BaseValue).FromCardCompat(card, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash", (string)null, (string)null)
			.Execute(choiceContext);
		Creature applier = card.Owner.Creature;
		decimal baseValue = card.DynamicVars["DamageBonus"].BaseValue;
		KnockdownPower val = cardPlay.Target.GetPowerInstances<KnockdownPower>().FirstOrDefault((KnockdownPower p) => ((PowerModel)p).Applier == applier);
		if (val != null)
		{
			await PowerCmd.ModifyAmount(choiceContext, (PowerModel)(object)val, baseValue, applier, card, false);
		}
		else
		{
			await PowerCmd.Apply<KnockdownPower>(choiceContext, cardPlay.Target, baseValue, applier, card, false);
		}
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Damage).UpgradeValueBy(4m);
		card.DynamicVars["DamageBonus"].UpgradeValueBy(100m);
	}
}
