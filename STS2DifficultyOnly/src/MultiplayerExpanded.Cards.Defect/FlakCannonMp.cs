using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace MultiplayerExpanded.Cards.Defect;

public sealed class FlakCannonMp : CardRework
{
	public override Type BaseCard => typeof(FlakCannon);

	public override int Cost => 2;

	public override CardType Type => (CardType)1;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)4;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[4]
			{
				(DynamicVar)new DamageVar(7m, (ValueProp)8),
				(DynamicVar)new CalculationBaseVar(0m),
				(DynamicVar)new CalculationExtraVar(1m),
				(DynamicVar)new CalculatedVar("CalculatedHits").WithMultiplier((Func<CardModel, Creature, decimal>)((CardModel card, Creature? _) => ExhaustableStatuses(card).Count()))
			};
		}
	}

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[1] { HoverTipFactory.FromKeyword((CardKeyword)1) };

	private static IEnumerable<CardModel> ExhaustableStatuses(CardModel card)
	{
		return card.CombatState.Players.Where((Player p) => card.IsUpgraded || p == card.Owner).SelectMany((Player p) =>
		{
			PlayerCombatState playerCombatState = p.PlayerCombatState;
			return (((playerCombatState != null) ? playerCombatState.AllCards : null) ?? Enumerable.Empty<CardModel>()).Where((CardModel c) =>
			{
				if ((int)c.Type == 4)
				{
					CardPile pile = c.Pile;
					if (pile == null)
					{
						return true;
					}
					return (int)pile.Type != 4;
				}
				return false;
			});
		});
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		List<CardModel> list = ExhaustableStatuses(card).ToList();
		int count = list.Count;
		foreach (CardModel item in list)
		{
			await GameApi.Exhaust(choiceContext, item);
		}
		if (count > 0)
		{
			await DamageCmd.Attack(((DynamicVar)card.DynamicVars.Damage).BaseValue).WithHitCount(count).FromCardCompat(card, cardPlay)
				.TargetingRandomOpponents(card.CombatState, true)
				.WithHitFx("vfx/vfx_attack_blunt", (string)null, (string)null)
				.Execute(choiceContext);
		}
	}
}
