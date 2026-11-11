using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Powers.Colorless;

public abstract class DelayedSelfDamagePower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	protected virtual ValueProp DamageProps => (ValueProp)12;

	protected virtual void PlayTriggerVfx()
	{
	}

	protected virtual Task AfterSelfDamage(ICombatState combatState, int amount, IEnumerable<DamageResult> results)
	{
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if ((int)side == 2 && this.Owner.IsAlive)
		{
			this.Flash();
			PlayTriggerVfx();
			int amount = this.Amount;
			await AfterSelfDamage(combatState, amount, await GameApi.Damage((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), this.Owner, amount, DamageProps, null, null, null));
			if (this.Owner.IsAlive)
			{
				await PowerCmd.Remove((PowerModel)(object)this);
			}
		}
	}
}
