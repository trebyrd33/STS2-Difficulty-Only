namespace MultiplayerExpanded.Features.Intents;

public sealed class DismissiveStatusIntent : TargetedStatusIntent
{
	public override string OverlayStem => "dismissive_intent";

	public DismissiveStatusIntent(int count)
		: base(IntentTarget.MostBlock().EveryoneElse(), count)
	{
	}
}
