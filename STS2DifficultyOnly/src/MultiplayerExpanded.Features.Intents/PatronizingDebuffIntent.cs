namespace MultiplayerExpanded.Features.Intents;

public sealed class PatronizingDebuffIntent : TargetedDebuffIntent
{
	public override string OverlayStem => "patronizing_intent";

	public PatronizingDebuffIntent(bool strong = false, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly).EveryoneElse(), strong)
	{
	}
}
