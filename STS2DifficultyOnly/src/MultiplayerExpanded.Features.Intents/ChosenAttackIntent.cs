using System;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MultiplayerExpanded.Features.Intents;

public sealed class ChosenAttackIntent : TargetedAttackIntent
{
	public override string OverlayStem => "targeted_intent";

	public ChosenAttackIntent(int damage, Func<Creature, Player?> target)
		: base(IntentTarget.Chosen(target), () => damage)
	{
	}

	public ChosenAttackIntent(Func<decimal> damageCalc, Func<Creature, Player?> target)
		: base(IntentTarget.Chosen(target), damageCalc)
	{
	}

	public ChosenAttackIntent(int damage, int repeat, Func<Creature, Player?> target)
		: base(IntentTarget.Chosen(target), () => damage, () => repeat)
	{
	}

	public ChosenAttackIntent(int damage, Func<int> repeatCalc, Func<Creature, Player?> target)
		: base(IntentTarget.Chosen(target), () => damage, repeatCalc)
	{
	}
}
