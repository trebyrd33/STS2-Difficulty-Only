using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Necrobinder;

[BetaOnly]
public sealed class UnderworldMp : CardRework
{
	public override Type BaseCard => typeof(Underworld);

	public override int Cost => 3;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)1;

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)1 };

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[1] { HoverTipFactory.FromPower<DoomPower>((int?)null) };

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Player owner = card.Owner;
		await CreatureCmd.TriggerAnim(owner.Creature, "PowerUp", owner.Character.PowerUpAnimDelay);
		await ApplyUnderworld(choiceContext, owner.Creature, card);
	}

	private static Task ApplyUnderworld(PlayerChoiceContext choiceContext, Creature owner, CardModel card)
	{
		return PowerCmd.Apply<UnderworldPower>(choiceContext, owner, 1m, owner, card, false);
	}

	public override void OnUpgrade(CardModel card)
	{
		card.EnergyCost.UpgradeBy(-1);
	}
}
