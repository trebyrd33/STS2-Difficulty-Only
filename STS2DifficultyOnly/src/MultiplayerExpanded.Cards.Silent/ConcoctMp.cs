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
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Silent;

[BetaOnly]
public sealed class ConcoctMp : CardRework
{
	public override Type BaseCard => typeof(Concoct);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)6;

	public override IEnumerable<DynamicVar> Vars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)(object)new PowerVar<ConcoctPower>(3m));

	public override IEnumerable<IHoverTip> HoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<PoisonPower>((int?)null));

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await CreatureCmd.TriggerAnim(card.Owner.Creature, "PowerUp", card.Owner.Character.PowerUpAnimDelay);
		await ApplyConcoct(choiceContext, cardPlay.Target, card);
	}

	private static Task ApplyConcoct(PlayerChoiceContext choiceContext, Creature target, CardModel card)
	{
		return PowerCmd.Apply<ConcoctPower>(choiceContext, target, card.DynamicVars["ConcoctPower"].BaseValue, card.Owner.Creature, card, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		card.DynamicVars["ConcoctPower"].UpgradeValueBy(1m);
	}
}
