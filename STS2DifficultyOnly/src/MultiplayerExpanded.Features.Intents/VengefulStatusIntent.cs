namespace MultiplayerExpanded.Features.Intents;

public sealed class VengefulStatusIntent : TargetedStatusIntent
{
	public override string OverlayStem => "vengeful_intent";

	public VengefulStatusIntent(int count, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly), count)
	{
	}
}
