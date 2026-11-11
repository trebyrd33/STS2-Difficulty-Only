using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace MultiplayerExpanded.Features.CardChest;

[HarmonyPatch(typeof(NTreasureRoomRelicCollection), "RelicPickingFinished")]
internal static class ChestPickingFinishedPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool UseCardPickingTask(ref Task __result)
	{
		CardChestRoom active = CardChestRoom.Active;
		if (active == null)
		{
			return true;
		}
		__result = active.PickingFinished.Task;
		return false;
	}
}
