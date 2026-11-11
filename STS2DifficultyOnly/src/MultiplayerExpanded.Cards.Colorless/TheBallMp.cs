using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Powers.Colorless;

namespace MultiplayerExpanded.Cards.Colorless;

[BetaOnly]
public sealed class TheBallMp : CardRework
{
	private static FieldInfo? _extraDamageFromPlays;

	public override Type BaseCard => typeof(TheBall);

	public override int Cost => 1;

	public override CardType Type => (CardType)1;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)2;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[2]
			{
				(DynamicVar)new DamageVar(10m, (ValueProp)8),
				new DynamicVar("Increase", 10m)
			};
		}
	}

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)2 };

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[1] { HoverTipFactory.FromPower<WasDunkedOnPower>((int?)null) };

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await DamageCmd.Attack(((DynamicVar)card.DynamicVars.Damage).BaseValue).FromCardCompat(card, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash", (string)null, (string)null)
			.Execute(choiceContext);
		decimal baseValue = card.DynamicVars["Increase"].BaseValue;
		DamageVar damage = card.DynamicVars.Damage;
		((DynamicVar)damage).BaseValue = ((DynamicVar)damage).BaseValue + baseValue;
		FieldInfo fieldInfo = _extraDamageFromPlays ?? (_extraDamageFromPlays = AccessTools.DeclaredField(typeof(TheBall), "_extraDamageFromPlays"));
		if (fieldInfo != null)
		{
			fieldInfo.SetValue(card, (decimal)fieldInfo.GetValue(card) + baseValue);
		}
		else
		{
			MainFile.Logger.Error("[Reworks] TheBall._extraDamageFromPlays not found; a downgrade will lose The Ball's increases.", 1);
		}
		if (!card.IsDupe && cardPlay.IsLastInSeries)
		{
			Creature val;
			if (!cardPlay.Target.IsAlive)
			{
				Rng combatTargets = card.Owner.RunState.Rng.CombatTargets;
				ICombatState combatState = card.CombatState;
				val = combatTargets.NextItem<Creature>((IEnumerable<Creature>)(((combatState != null) ? combatState.HittableEnemies : null) ?? Array.Empty<Creature>()));
			}
			else
			{
				val = cardPlay.Target;
			}
			Creature val2 = val;
			if (val2 != null)
			{
				await WasDunkedOnPower.Dunk(choiceContext, card, val2);
			}
		}
	}

	public override void OnUpgrade(CardModel card)
	{
		card.DynamicVars["Increase"].UpgradeValueBy(5m);
	}
}
