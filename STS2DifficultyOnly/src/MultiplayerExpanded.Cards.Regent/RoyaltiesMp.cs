using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.PartyScaling;

namespace MultiplayerExpanded.Cards.Regent;

public sealed class RoyaltiesMp : CardRework
{
	private const decimal SplitPoolAmount = 36m;

	public override Type BaseCard => typeof(Royalties);

	public override int Cost => 1;

	public override CardType Type => (CardType)3;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)1;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[4]
			{
				(DynamicVar)new GoldVar(30),
				(DynamicVar)new IntVar("SplitPool", 36m),
				(DynamicVar)new IntVar("Share", 0m),
				(DynamicVar)new IntVar("Payout", 0m)
			};
		}
	}

	public override IEnumerable<MultiplayerExpandedCard.PartyScaledVar> PartyScaledVars => new _003C_003Ez__ReadOnlyArray<MultiplayerExpandedCard.PartyScaledVar>(new MultiplayerExpandedCard.PartyScaledVar[2]
	{
		new MultiplayerExpandedCard.PartyScaledVar("Share", (CardModel card, int players) => ShareFor(players), 0m),
		new MultiplayerExpandedCard.PartyScaledVar("Payout", (CardModel card, int players) => ((DynamicVar)card.DynamicVars.Gold).BaseValue + ShareFor(players), 0m)
	});

	private static decimal ShareFor(int players)
	{
		return Math.Floor(36m / (decimal)Math.Max(1, players));
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(card.Owner.Creature, "PowerUp", card.Owner.Character.PowerUpAnimDelay);
		decimal baseValue = ((DynamicVar)card.DynamicVars.Gold).BaseValue;
		await PowerCmd.Apply<RoyaltiesPower>(choiceContext, card.Owner.Creature, baseValue, card.Owner.Creature, card, false);
		if (card.IsUpgraded)
		{
			List<Creature> list = (from p in card.CombatState.Players
				where p.Creature.IsAlive
				select p.Creature).ToList();
			await PowerCmd.Apply<RoyaltiesPower>(choiceContext, (IEnumerable<Creature>)list, card.PartyScaled("Share"), card.Owner.Creature, card, false);
		}
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Gold).UpgradeValueBy(-20m);
	}
}
