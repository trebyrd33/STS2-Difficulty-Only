using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Features.Reworks;

namespace MultiplayerExpanded.Cards.Ironclad;

[BetaOnly]
public sealed class MidnightMp : CardRework
{
	private const int CostPerPlayer = 6;

	private const int CanonicalPlayerCount = 4;

	public override Type BaseCard => typeof(Midnight);

	public override int Cost => 24;

	public override CardType Type => (CardType)1;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)2;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new DamageVar(60m, (ValueProp)8) };
		}
	}

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[1] { HoverTipFactory.FromKeyword((CardKeyword)1) };

	public override int? EnergyCostForPlayerCount(CardModel card, int playerCount)
	{
		return 6 * playerCount;
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await DamageCmd.Attack(((DynamicVar)card.DynamicVars.Damage).BaseValue).FromCardCompat(card, cardPlay).Targeting(cardPlay.Target)
			.WithAttackerAnim(MegaCrit.Sts2.Core.Models.Characters.Ironclad.GetHeavyAnimIfApplicable(card.Owner.Character), MegaCrit.Sts2.Core.Models.Characters.Ironclad.GetHeavyAttackDelayIfApplicable(card.Owner.Character), (Creature)null)
			.WithHitFx("vfx/vfx_heavy_blunt", (string)null, "heavy_attack.mp3")
			.WithHitVfxSpawnedAtBase()
			.Execute(choiceContext);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Damage).UpgradeValueBy(12m);
	}

	internal static bool Active()
	{
		ReworkInfo info;
		if (ReworkPatches.IsMp())
		{
			return ReworkRegistry.TryGet(typeof(Midnight), out info);
		}
		return false;
	}
}
