using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Extensions;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class TagTeamMpPower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)2;

	public override PowerInstanceType InstanceType => (PowerInstanceType)1;

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new StringVar("Teammate", "the caster") };
		}
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (applier != null)
		{
			SetTeammate(applier);
		}
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		CardModel card = cardPlay.Card;
		if ((int)card.Type != 1 || card.Owner.Creature == ((PowerModel)this).Applier || cardPlay.IsAutoPlay || !cardPlay.IsLastInSeries || (((int)card.TargetType == 2) ? (cardPlay.Target != ((PowerModel)this).Owner) : ((int)card.TargetType != 3)))
		{
			return;
		}
		Creature applier = ((PowerModel)this).Applier;
		Player copier = ((applier != null) ? applier.Player : null);
		if (copier != null)
		{
			Creature creature = copier.Creature;
			if (creature != null && creature.IsAlive)
			{
				this.Flash();
				await PowerCmd.Remove((PowerModel)(object)this);
				CardModel clone = card.CloneForPlayer(copier);
				await CardPileCmd.AddGeneratedCardToCombat(clone, (PileType)5, copier, (CardPilePosition)1);
				Creature val = (((int)card.TargetType == 2 && ((PowerModel)this).Owner.IsAlive) ? ((PowerModel)this).Owner : null);
				await CardCmd.AutoPlay(context, clone, val, (AutoPlayType)1, false, false);
				CardPile pile = clone.Pile;
				if (pile != null && pile.IsCombatPile)
				{
					await CardPileCmd.RemoveFromCombat(clone, false);
				}
				return;
			}
		}
		await PowerCmd.Remove((PowerModel)(object)this);
	}
}
