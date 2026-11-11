using System;

namespace MultiplayerExpanded.Features.Intents;

public sealed class VengefulAttackIntent : TargetedAttackIntent
{
	public override string OverlayStem => "vengeful_intent";

	public VengefulAttackIntent(int damage, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly), () => damage)
	{
	}

	public VengefulAttackIntent(Func<decimal> damageCalc, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly), damageCalc)
	{
	}

	public VengefulAttackIntent(int damage, int repeat, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly), () => damage, () => repeat)
	{
	}

	public VengefulAttackIntent(int damage, Func<int> repeatCalc, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly), () => damage, repeatCalc)
	{
	}
}
