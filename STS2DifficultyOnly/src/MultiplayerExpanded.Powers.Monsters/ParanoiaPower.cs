using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Powers.Monsters;

public sealed class ParanoiaPower : MultiplayerExpandedPower
{
	private const string DamageIncreaseKey = "DamageIncrease";

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new DynamicVar[1]
			{
				new DynamicVar("DamageIncrease", 50m)
			};
		}
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (!(((dealer != null) ? dealer.Monster : null) is Parafright) || !ValuePropExtensions.IsPoweredAttack(props))
		{
			return 1m;
		}
		Player val = MonsterTargeting.PlayerOf(target);
		if (val == null || !HitOwnerThisTurn(val))
		{
			return 1m;
		}
		return 1m + this.DynamicVars["DamageIncrease"].BaseValue / 100m;
	}

	public override Task AfterModifyingDamageAmount(CardModel? cardSource)
	{
		this.Flash();
		return Task.CompletedTask;
	}

	private bool HitOwnerThisTurn(Player player)
	{
		ICombatState combatState = this.Owner.CombatState;
		if (combatState != null)
		{
			return MonsterTargeting.LatestPlayerTurnDamage(combatState).Any((DamageReceivedEntry e) =>
			{
				return e.Receiver == this.Owner && ValuePropExtensions.IsPoweredAttack(e.Result.Props) && e.Result.TotalDamage > 0 && MonsterTargeting.PlayerOf(e.Dealer) == player;
			});
		}
		return false;
	}
}
