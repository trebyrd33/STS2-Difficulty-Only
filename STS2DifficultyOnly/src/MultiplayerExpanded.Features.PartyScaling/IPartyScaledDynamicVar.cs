namespace MultiplayerExpanded.Features.PartyScaling;

internal interface IPartyScaledDynamicVar
{
	decimal Current { get; }

	void RefreshPreview();
}
