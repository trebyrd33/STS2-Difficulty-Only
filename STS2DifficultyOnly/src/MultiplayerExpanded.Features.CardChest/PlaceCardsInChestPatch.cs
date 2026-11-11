using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace MultiplayerExpanded.Features.CardChest;

[HarmonyPatch(typeof(NTreasureRoomRelicCollection), "InitializeRelics")]
internal static class PlaceCardsInChestPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool PlaceCards()
	{
		CardChestDisplay cardChestDisplay = CardChestRoom.Active?.Display;
		if (cardChestDisplay == null)
		{
			return true;
		}
		cardChestDisplay.PlaceCards();
		return false;
	}
}
