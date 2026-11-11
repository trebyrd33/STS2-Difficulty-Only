using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerExpanded.Features.CardPicking;

public sealed class PlayerChoicePickTransport : ICardPickTransport
{
	private readonly Dictionary<Player, uint> _choiceIds = new Dictionary<Player, uint>();

	private CardPickingSession? _session;

	private bool _sent;

	public bool CanRevote => false;

	public void Open(CardPickingSession session)
	{
		_session = session;
		OpenRound(session);
	}

	public void OpenRound(CardPickingSession session)
	{
		_choiceIds.Clear();
		_sent = false;
		PlayerChoiceSynchronizer playerChoiceSynchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
		List<Player> list = session.Participants.Where(session.IsVoting).ToList();
		foreach (Player item in list)
		{
			_choiceIds[item] = playerChoiceSynchronizer.ReserveChoiceId(item);
		}
		if (RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
		{
			return;
		}
		foreach (Player item2 in list)
		{
			if (!ChoosesLocally(item2))
			{
				TaskHelper.RunSafely(DeliverRemote(session, item2, _choiceIds[item2], session.Round));
			}
		}
	}

	public bool SendLocal(Player localPlayer, int? index, int round)
	{
		if (_sent || !_choiceIds.TryGetValue(localPlayer, out var value))
		{
			return false;
		}
		_sent = true;
		RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(localPlayer, value, PlayerChoiceResult.FromIndex(index));
		_session?.OnPicked(localPlayer, index, round);
		return true;
	}

	private static bool ChoosesLocally(Player player)
	{
		if (LocalContext.IsMe(player))
		{
			return (int)RunManager.Instance.NetService.Type != 4;
		}
		return false;
	}

	private static async Task DeliverRemote(CardPickingSession session, Player player, uint choiceId, int round)
	{
		session.OnPicked(player, (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsIndexOrNull(), round);
	}
}
