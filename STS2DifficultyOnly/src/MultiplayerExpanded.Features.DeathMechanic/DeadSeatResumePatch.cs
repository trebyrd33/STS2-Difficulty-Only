using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace MultiplayerExpanded.Features.DeathMechanic;

[HarmonyPatch(typeof(EventSynchronizer), "ResumeEvents")]
internal static class DeadSeatResumePatch
{
	private static bool Prepare()
	{
		return DeadSeatVotes.Arm("DeadSeatResumePatch");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(EventSynchronizer __instance)
	{
		DeadSeatVotes.SeatDead(__instance);
	}
}
