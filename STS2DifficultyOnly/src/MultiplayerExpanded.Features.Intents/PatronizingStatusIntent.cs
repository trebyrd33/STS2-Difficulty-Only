namespace MultiplayerExpanded.Features.Intents;

public sealed class PatronizingStatusIntent : TargetedStatusIntent
{
	public override string OverlayStem => "patronizing_intent";

	public PatronizingStatusIntent(int count, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly).EveryoneElse(), count)
	{
	}
}
