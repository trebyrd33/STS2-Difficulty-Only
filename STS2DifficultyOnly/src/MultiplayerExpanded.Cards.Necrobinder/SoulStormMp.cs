using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Necrobinder;

public sealed class SoulStormMp : CardRework
{
	public override Type BaseCard => typeof(SoulStorm);

	public override int Cost => 1;

	public override CardType Type => (CardType)1;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)2;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[3]
			{
				(DynamicVar)new CalculationBaseVar(9m),
				(DynamicVar)new ExtraDamageVar(Beta.Pick(2m, 4m)),
				(DynamicVar)((CalculatedVar)new CalculatedDamageVar((ValueProp)8)).WithMultiplier((Func<CardModel, Creature, decimal>)((CardModel card, Creature? _) =>
				{
					ICombatState combatState = card.CombatState;
					return (combatState != null) ? combatState.Players.Sum((Player p) =>
					{
						PlayerCombatState playerCombatState = p.PlayerCombatState;
						return (playerCombatState != null) ? Enumerable.Count(playerCombatState.ExhaustPile.Cards, (CardModel c) => c is Soul) : 0;
					}) : 0;
				}))
			};
		}
	}

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[1] { HoverTipFactory.FromCard<Soul>(false) };

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await DamageCmd.Attack(card.DynamicVars.CalculatedDamage).FromCardCompat(card, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash", (string)null, (string)null)
			.Execute(choiceContext);
	}

	public override void OnUpgrade(CardModel card)
	{
		card.DynamicVars["ExtraDamage"].UpgradeValueBy(Beta.Pick(1m, 2m));
	}
}
