using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MultiplayerExpanded.Features.CardPicking;

namespace MultiplayerExpanded.Features.CardChest;

public sealed class CardChestRoom : TreasureRoom
{
	public static CardChestRoom? Active { get; set; }

	public required IReadOnlyList<CardModel> Cards { get; init; }

	public required CardPickingSession Session { get; init; }

	public CardPickLayout Layout { get; init; }

	public required bool PaysNormalRewards { get; init; }

	public CardChestDisplay? Display { get; set; }

	public TaskCompletionSource PickingBegan { get; } = new TaskCompletionSource();

	public TaskCompletionSource PickingFinished { get; } = new TaskCompletionSource();

	public CardChestRoom(int actIndex)
		: base(actIndex)
	{
	}
}
