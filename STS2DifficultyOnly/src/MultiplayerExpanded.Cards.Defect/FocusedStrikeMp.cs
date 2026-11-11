using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Defect;

public sealed class FocusedStrikeMp : CardRework
{
	public override Type BaseCard => typeof(FocusedStrike);

	public override int Cost => 1;

	public override CardType Type => (CardType)1;

	public override CardRarity Rarity => (CardRarity)2;

	public override TargetType Target => (TargetType)2;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[2]
			{
				(DynamicVar)new DamageVar(9m, (ValueProp)8),
				(DynamicVar)new PowerVar<HotfixPower>(1m)
			};
		}
	}

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[1] { HoverTipFactory.FromPower<FocusPower>((int?)null) };

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await DamageCmd.Attack(((DynamicVar)card.DynamicVars.Damage).BaseValue).FromCardCompat(card, cardPlay).Targeting(cardPlay.Target)
			.Execute(choiceContext);
		foreach (Player player in card.CombatState.Players)
		{
			if (player.Creature.IsAlive)
			{
				await PowerCmd.Apply<HotfixPower>(choiceContext, player.Creature, DynamicVarSetExtensions.Power<HotfixPower>(card.DynamicVars).BaseValue, card.Owner.Creature, card, false);
			}
		}
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Damage).UpgradeValueBy(2m);
		DynamicVarSetExtensions.Power<HotfixPower>(card.DynamicVars).UpgradeValueBy(1m);
	}
}
