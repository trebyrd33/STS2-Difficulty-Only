using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace MultiplayerExpanded.Features.DeathMechanic;

[HarmonyPatch(typeof(EventSynchronizer), "ChooseOptionForSharedEvent")]
internal static class DeadSeatNextPagePatch
{
	private static bool Prepare()
	{
		return DeadSeatVotes.Arm("DeadSeatNextPagePatch");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(EventSynchronizer __instance)
	{
		DeadSeatVotes.SeatDead(__instance);
	}
}
