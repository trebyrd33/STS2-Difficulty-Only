namespace MultiplayerExpanded.Features.AllSelect;

internal interface IAllSelectEvent
{
	int MaxRounds { get; }

	AllSelectContention ContentionFor(int optionIndex);

	void ApplyRound(AllSelectRound round);
}
