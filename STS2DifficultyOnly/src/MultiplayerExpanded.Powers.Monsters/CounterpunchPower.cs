using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Powers.Monsters;

public sealed class CounterpunchPower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[2]
	{
		HoverTipFactory.Static((StaticHoverTip)5, Array.Empty<DynamicVar>()),
		HoverTipFactory.FromPower<CounteredPower>((int?)null)
	};

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (target == this.Owner && ValuePropExtensions.IsPoweredAttack(props) && result.WasFullyBlocked && !result.WasBlockBroken && result.BlockedDamage > 0)
		{
			Player? val = MonsterTargeting.PlayerOf(dealer);
			Creature val2 = ((val != null) ? val.Creature : null);
			if (val2 != null && val2.IsAlive && !val2.HasPower<CounteredPower>())
			{
				this.Flash();
				await PowerCmd.Apply<CounteredPower>(choiceContext, val2, 1m, this.Owner, (CardModel)null, false);
			}
		}
	}
}
