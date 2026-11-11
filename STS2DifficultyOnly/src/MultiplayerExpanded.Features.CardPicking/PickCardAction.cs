using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;

namespace MultiplayerExpanded.Features.CardPicking;

public sealed class PickCardAction : GameAction
{
	private static readonly Logger _logger = new Logger("PickCardAction", (LogType)3);

	private readonly Player _player;

	private readonly int? _cardIndex;

	private readonly int _round;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => (GameActionType)4;

	public PickCardAction(Player player, int? cardIndex, int round)
	{
		_player = player;
		_cardIndex = cardIndex;
		_round = round;
	}

	protected override Task ExecuteAction()
	{
		CardPickingSession active = CardPickingSession.Active;
		if (active == null)
		{
			_logger.Warn($"{this} arrived with no card picking session open; the vote is dropped.", 1);
			return Task.CompletedTask;
		}
		active.OnPicked(_player, _cardIndex, _round);
		return Task.CompletedTask;
	}

	public override INetAction ToNetAction()
	{
		return (INetAction)(object)new NetPickCardAction
		{
			cardIndex = _cardIndex,
			round = _round
		};
	}

	public override string ToString()
	{
		return $"{"PickCardAction"} for player {_player.NetId} index {_cardIndex} round {_round}";
	}

	static PickCardAction()
	{
	}
}
