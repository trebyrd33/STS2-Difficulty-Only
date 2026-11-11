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
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Cards.Tokens;

public sealed class FranticPull : TokenCard
{
	public override int MaxUpgradeLevel => 0;

	public override bool CanBeGeneratedInCombat => false;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<SandpitMpPower>((int?)null));

	public FranticPull()
		: base(1, (CardType)4, (CardRarity)8, (TargetType)6)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Player val = cardPlay.Target.Player ?? cardPlay.Target.PetOwner;
		Creature obj = ((val != null) ? val.Creature : null);
		SandpitMpPower sandpitMpPower = ((obj != null) ? obj.GetPower<SandpitMpPower>() : null);
		if (sandpitMpPower != null)
		{
			await PowerCmd.ModifyAmount(choiceContext, (PowerModel)(object)sandpitMpPower, 1m, ((PowerModel)sandpitMpPower).Applier, (CardModel)(object)this, false);
		}
		this.EnergyCost.AddThisCombat(1, false);
	}
}
