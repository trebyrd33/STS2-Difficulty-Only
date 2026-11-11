namespace MultiplayerExpanded.Features.AllSelect;

internal sealed class AllSelectPlan
{
	public required AllSelectRound?[] PerSlot;

	public required bool[] StillPlayingAfter;
}
