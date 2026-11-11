using MegaCrit.Sts2.Core.Entities.Players;

namespace MultiplayerExpanded.Features.CardPicking;

public interface ICardPickTransport
{
	bool CanRevote { get; }

	void Open(CardPickingSession session);

	void OpenRound(CardPickingSession session);

	bool SendLocal(Player localPlayer, int? index, int round);
}
