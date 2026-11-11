using System;

namespace MultiplayerExpanded.Features.Intents;

public sealed class DismissiveAttackIntent : TargetedAttackIntent
{
	public override string OverlayStem => "dismissive_intent";

	public DismissiveAttackIntent(int damage)
		: base(IntentTarget.MostBlock().EveryoneElse(), () => damage)
	{
	}

	public DismissiveAttackIntent(Func<decimal> damageCalc)
		: base(IntentTarget.MostBlock().EveryoneElse(), damageCalc)
	{
	}

	public DismissiveAttackIntent(int damage, int repeat)
		: base(IntentTarget.MostBlock().EveryoneElse(), () => damage, () => repeat)
	{
	}

	public DismissiveAttackIntent(int damage, Func<int> repeatCalc)
		: base(IntentTarget.MostBlock().EveryoneElse(), () => damage, repeatCalc)
	{
	}
}
