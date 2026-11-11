using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class ImmortalPower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool ShouldDie(Creature creature)
	{
		return creature != ((PowerModel)this).Owner;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		if (creature == ((PowerModel)this).Owner)
		{
			this.Flash();
			decimal num = Math.Max(1m, (decimal)((PowerModel)this).Owner.MaxHp * (decimal)((PowerModel)this).Amount / 100m);
			await CreatureCmd.Heal(((PowerModel)this).Owner, num, true);
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
