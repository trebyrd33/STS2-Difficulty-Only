using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Powers.Silent;

public sealed class TrackingMpPower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	

	

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (!ValuePropExtensions.IsPoweredAttack(props))
		{
			return 1m;
		}
		if (dealer != ((PowerModel)this).Owner && !((PowerModel)this).Owner.Pets.Contains(dealer))
		{
			return 1m;
		}
		if (target == null || !target.IsMonster || !target.HasPower<WeakPower>())
		{
			return 1m;
		}
		return 1m + (decimal)((PowerModel)this).Amount / 100m;
	}
}
