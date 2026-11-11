using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace MultiplayerExpanded.Features.Intents;

public abstract class TargetedDebuffIntent : DebuffIntent, ITargetedIntent
{
	private string? _prefix;

	public IntentTarget Target { get; }

	public abstract string OverlayStem { get; }

	protected sealed override string IntentPrefix => _prefix ?? (_prefix = TargetedIntentKeys.Prefix(this.GetType()));

	protected TargetedDebuffIntent(IntentTarget target, bool strong)
		: base(strong)
	{
		Target = target;
		this._cachedAnimationName = "debuff";
	}

	protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
	{
		LocString intentDescription = base.GetIntentDescription(targets, owner);
		Target.AddDescriptionVars(intentDescription, owner);
		return intentDescription;
	}
}
