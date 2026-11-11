using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace MultiplayerExpanded.Features.Intents;

public abstract class TargetedAttackIntent : AttackIntent, ITargetedIntent
{
	private readonly Func<int>? _repeatCalc;

	private string? _prefix;

	public IntentTarget Target { get; }

	public abstract string OverlayStem { get; }

	protected sealed override string IntentPrefix => _prefix ?? (_prefix = TargetedIntentKeys.Prefix(this.GetType()));

	private bool IsMulti => _repeatCalc != null;

	public override int Repeats => _repeatCalc?.Invoke() ?? 1;

	protected override LocString IntentLabelFormat
	{
		get
		{
			return new LocString("intents", IsMulti ? "FORMAT_DAMAGE_MULTI" : "FORMAT_DAMAGE_SINGLE");
		}
	}

	protected TargetedAttackIntent(IntentTarget target, Func<decimal> damageCalc, Func<int>? repeatCalc = null)
	{
		Target = target.Attacking();
		this.DamageCalc = damageCalc;
		_repeatCalc = repeatCalc;
		this._cachedAnimationName = "attack";
	}

	private int PreviewDamage(Creature owner)
	{
		return Target.PreviewDamage(owner, this.DamageCalc());
	}

	public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner)
	{
		return PreviewDamage(owner) * this.Repeats;
	}

	public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
	{
		LocString intentLabelFormat = this.IntentLabelFormat;
		intentLabelFormat.Add("Damage", (decimal)PreviewDamage(owner));
		if (IsMulti)
		{
			intentLabelFormat.Add("Repeat", (decimal)this.Repeats);
		}
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
