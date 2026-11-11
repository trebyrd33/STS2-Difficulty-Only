using System;

namespace MultiplayerExpanded.Features.Intents;

public sealed class ChallengerAttackIntent : TargetedAttackIntent
{
	public override string OverlayStem => "challenger_intent";

	public ChallengerAttackIntent(int damage)
		: base(IntentTarget.MostBlock(), () => damage)
	{
	}

	public ChallengerAttackIntent(Func<decimal> damageCalc)
		: base(IntentTarget.MostBlock(), damageCalc)
	{
	}

	public ChallengerAttackIntent(int damage, int repeat)
		: base(IntentTarget.MostBlock(), () => damage, () => repeat)
	{
	}

	public ChallengerAttackIntent(int damage, Func<int> repeatCalc)
		: base(IntentTarget.MostBlock(), () => damage, repeatCalc)
	{
	}
}
