using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Powers.Silent;

namespace MultiplayerExpanded.Cards.Silent;

public sealed class TrackingMp : CardRework
{
	public override Type BaseCard => typeof(Tracking);

	public override int Cost => 2;

	public override CardType Type => (CardType)3;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)1;

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[1] { HoverTipFactory.FromPower<WeakPower>((int?)null) };

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (card.IsUpgraded)
		{
			foreach (Player player in card.CombatState.Players)
			{
				if (player.Creature.IsAlive)
				{
					await PowerCmd.Apply<TrackingMpPower>(choiceContext, player.Creature, 50m, card.Owner.Creature, card, false);
				}
			}
		}
		else
		{
			await PowerCmd.Apply<TrackingMpPower>(choiceContext, card.Owner.Creature, 50m, card.Owner.Creature, card, false);
		}
	}
}
