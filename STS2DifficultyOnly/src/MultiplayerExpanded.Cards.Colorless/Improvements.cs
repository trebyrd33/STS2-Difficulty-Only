using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MultiplayerExpanded.Powers.Colorless;

namespace MultiplayerExpanded.Cards.Colorless;

[Pool(typeof(EventCardPool))]
public sealed class Improvements : ColorlessCard
{
	public override CardPoolModel Pool => (CardPoolModel)(object)ModelDb.CardPool<EventCardPool>();

	public override CardPoolModel VisualCardPool => (CardPoolModel)(object)ModelDb.CardPool<ColorlessCardPool>();

	public override bool CanBeGeneratedInCombat => false;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[1] { (CardKeyword)1 };

	public Improvements()
		: base(3, (CardType)2, (CardRarity)6, (TargetType)6)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await CreatureCmd.TriggerAnim(((CardModel)this).Owner.Creature, "PowerUp", ((CardModel)this).Owner.Character.PowerUpAnimDelay);
		await PowerCmd.Apply<ImprovementsPower>(choiceContext, cardPlay.Target, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
