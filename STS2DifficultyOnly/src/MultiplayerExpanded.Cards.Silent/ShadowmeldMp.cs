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

namespace MultiplayerExpanded.Cards.Silent;

public sealed class ShadowmeldMp : CardRework
{
	public override Type BaseCard => typeof(Shadowmeld);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)5;

	public override IEnumerable<IHoverTip> HoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static((StaticHoverTip)5, Array.Empty<DynamicVar>()));

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await PowerCmd.Apply<ShadowmeldPower>(choiceContext, cardPlay.Target, 1m, card.Owner.Creature, card, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		card.EnergyCost.UpgradeBy(-1);
	}
}
