namespace MultiplayerExpanded.Features.Intents;

public sealed class DismissiveDebuffIntent : TargetedDebuffIntent
{
	public override string OverlayStem => "dismissive_intent";

	public DismissiveDebuffIntent(bool strong = false)
		: base(IntentTarget.MostBlock().EveryoneElse(), strong)
	{
	}
}
