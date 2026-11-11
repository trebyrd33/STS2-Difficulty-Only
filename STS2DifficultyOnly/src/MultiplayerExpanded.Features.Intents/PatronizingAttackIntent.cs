using System;

namespace MultiplayerExpanded.Features.Intents;

public sealed class PatronizingAttackIntent : TargetedAttackIntent
{
	public override string OverlayStem => "patronizing_intent";

	public PatronizingAttackIntent(int damage, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly).EveryoneElse(), () => damage)
	{
	}

	public PatronizingAttackIntent(Func<decimal> damageCalc, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly).EveryoneElse(), damageCalc)
	{
	}

	public PatronizingAttackIntent(int damage, int repeat, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly).EveryoneElse(), () => damage, () => repeat)
	{
	}

	public PatronizingAttackIntent(int damage, Func<int> repeatCalc, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly).EveryoneElse(), () => damage, repeatCalc)
	{
	}
}
