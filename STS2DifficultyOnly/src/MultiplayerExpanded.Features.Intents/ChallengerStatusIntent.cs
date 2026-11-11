namespace MultiplayerExpanded.Features.Intents;

public sealed class ChallengerStatusIntent : TargetedStatusIntent
{
	public override string OverlayStem => "challenger_intent";

	public ChallengerStatusIntent(int count)
		: base(IntentTarget.MostBlock(), count)
	{
	}
}
