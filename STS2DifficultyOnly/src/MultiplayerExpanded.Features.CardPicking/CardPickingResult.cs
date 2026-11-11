using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Features.CardPicking;

public sealed class CardPickingResult
{
	public required CardPickingResultType Type { get; init; }

	public required CardModel Card { get; init; }

	public required Player? Player { get; init; }

	public RelicPickingFight? Fight { get; init; }
}
