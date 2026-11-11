using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
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

namespace MultiplayerExpanded.Cards.Necrobinder;

public sealed class PullFromBelowMp : CardRework
{
	private const string HitsKey = "CalculatedHits";

	public override Type BaseCard => typeof(PullFromBelow);

	public override int Cost => 1;

	public override CardType Type => (CardType)1;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)2;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[4]
			{
				(DynamicVar)new DamageVar(2m, (ValueProp)8),
				(DynamicVar)new CalculationBaseVar(0m),
				(DynamicVar)new CalculationExtraVar(1m),
				(DynamicVar)new CalculatedVar("CalculatedHits").WithMultiplier((Func<CardModel, Creature, decimal>)((CardModel _, Creature? __) => CombatManager.Instance.History.Entries.OfType<CardPlayFinishedEntry>().Count((CardPlayFinishedEntry e) => e.WasEthereal)))
			};
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await DamageCmd.Attack(((DynamicVar)card.DynamicVars.Damage).BaseValue).WithHitCount((int)((CalculatedVar)card.DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target)).FromCardCompat(card, cardPlay)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash", (string)null, (string)null)
			.Execute(choiceContext);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Damage).UpgradeValueBy(1m);
	}
}
