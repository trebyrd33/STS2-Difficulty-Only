using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Silent;

public sealed class MementoMoriMp : CardRework
{
	public override Type BaseCard => typeof(MementoMori);

	public override int Cost => 1;

	public override CardType Type => (CardType)1;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)2;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[3]
			{
				(DynamicVar)new CalculationBaseVar(9m),
				(DynamicVar)new ExtraDamageVar(4m),
				(DynamicVar)((CalculatedVar)new CalculatedDamageVar((ValueProp)8)).WithMultiplier((Func<CardModel, Creature, decimal>)((CardModel card, Creature? _) => CombatManager.Instance.History.Entries.OfType<CardDiscardedEntry>().Count((CardDiscardedEntry e) => ((CombatHistoryEntry)e).HappenedThisTurn(card.CombatState))))
			};
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await DamageCmd.Attack(card.DynamicVars.CalculatedDamage).FromCardCompat(card, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash", (string)null, (string)null)
			.Execute(choiceContext);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.CalculationBase).UpgradeValueBy(2m);
		((DynamicVar)card.DynamicVars.ExtraDamage).UpgradeValueBy(1m);
	}
}
