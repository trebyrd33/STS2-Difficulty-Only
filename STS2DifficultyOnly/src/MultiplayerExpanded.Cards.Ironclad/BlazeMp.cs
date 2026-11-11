using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Ironclad;

[BetaOnly]
public sealed class BlazeMp : CardRework
{
	public override Type BaseCard => typeof(Blaze);

	public override int Cost => 2;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)6;

	public override IEnumerable<DynamicVar> Vars => new DynamicVar[1] { (DynamicVar)new PowerVar<StrengthPower>(3m) };

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)1 };

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[1] { HoverTipFactory.FromPower<StrengthPower>((int?)null) };

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, ((DynamicVar)card.DynamicVars.Strength).BaseValue, card.Owner.Creature, card, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Strength).UpgradeValueBy(2m);
	}
}
