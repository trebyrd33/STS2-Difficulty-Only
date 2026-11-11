namespace MultiplayerExpanded.Features.Intents;

public sealed class VengefulDebuffIntent : TargetedDebuffIntent
{
	public override string OverlayStem => "vengeful_intent";

	public VengefulDebuffIntent(bool strong = false, bool toOwnerOnly = false)
		: base(IntentTarget.MostDamageThisTurn(toOwnerOnly), strong)
	{
	}
}
