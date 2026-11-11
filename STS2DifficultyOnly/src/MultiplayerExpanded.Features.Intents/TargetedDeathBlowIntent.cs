using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace MultiplayerExpanded.Features.Intents;

public sealed class TargetedDeathBlowIntent : DeathBlowIntent, ITargetedIntent
{
	private string? _prefix;

	public IntentTarget Target { get; }

	public string OverlayStem => "targeted_intent";

	protected override string IntentPrefix => _prefix ?? (_prefix = TargetedIntentKeys.Prefix(this.GetType()));

	public TargetedDeathBlowIntent(int damage, Func<Creature, Player?> target)
		: this(() => damage, target)
	{
	}

	public TargetedDeathBlowIntent(Func<decimal> damageCalc, Func<Creature, Player?> target)
		: base(damageCalc)
	{
		Target = IntentTarget.Chosen(target).Attacking();
		this._cachedAnimationName = "death_blow";
	}

	private int PreviewDamage(Creature owner)
	{
		return Target.PreviewDamage(owner, this.DamageCalc());
	}

	public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner)
	{
		return PreviewDamage(owner);
	}

	public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
	{
		LocString intentLabelFormat = this.IntentLabelFormat;
		intentLabelFormat.Add("Damage", (decimal)PreviewDamage(owner));
		return intentLabelFormat;
	}

	protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
	{
		LocString intentDescription = base.GetIntentDescription(targets, owner);
		intentDescription.Add("Damage", (decimal)PreviewDamage(owner));
		Target.AddDescriptionVars(intentDescription, owner);
		return intentDescription;
	}
}
