using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MultiplayerExpanded.Features.CardPicking;

public sealed class ActionQueuePickTransport(ActionQueueSynchronizer actionQueue) : ICardPickTransport
{
	public bool CanRevote => true;

	public void Open(CardPickingSession session)
	{
	}

	public void OpenRound(CardPickingSession session)
	{
	}

	public bool SendLocal(Player localPlayer, int? index, int round)
	{
		actionQueue.RequestEnqueue((GameAction)(object)new PickCardAction(localPlayer, index, round));
		return true;
	}
}
