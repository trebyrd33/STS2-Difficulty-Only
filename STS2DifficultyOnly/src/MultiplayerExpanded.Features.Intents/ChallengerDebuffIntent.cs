namespace MultiplayerExpanded.Features.Intents;

public sealed class ChallengerDebuffIntent : TargetedDebuffIntent
{
	public override string OverlayStem => "challenger_intent";

	public ChallengerDebuffIntent(bool strong = false)
		: base(IntentTarget.MostBlock(), strong)
	{
	}
}
