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
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerExpanded.Cards.Defect;

public sealed class LoopMp : CardRework
{
	public override Type BaseCard => typeof(Loop);

	public override int Cost => 1;

	public override CardType Type => (CardType)3;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)1;

	public override IEnumerable<IHoverTip> HoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static((StaticHoverTip)2, Array.Empty<DynamicVar>()));

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<Creature> enumerable2;
		if (!card.IsUpgraded)
		{
			IEnumerable<Creature> enumerable = new Creature[1] { card.Owner.Creature };
			enumerable2 = enumerable;
		}
		else
		{
			enumerable2 = card.CombatState.Players.Select((Player p) => p.Creature);
		}
		IEnumerable<Creature> enumerable3 = enumerable2;
		await PowerCmd.Apply<LoopPower>(choiceContext, enumerable3, 1m, card.Owner.Creature, card, false);
	}
}
