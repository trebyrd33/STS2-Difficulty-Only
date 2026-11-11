namespace MultiplayerExpanded.Features.Intents;

public interface ITargetedIntent
{
	IntentTarget Target { get; }

	string OverlayStem { get; }
}
