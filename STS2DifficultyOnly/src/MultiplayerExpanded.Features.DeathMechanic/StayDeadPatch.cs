using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.Settings;

namespace MultiplayerExpanded.Features.DeathMechanic;

[HarmonyPatch(typeof(Player), "ReviveBeforeCombatEnd")]
internal static class StayDeadPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(Player __instance, ref Task __result)
	{
		if (!DeathMechanicGate.IsActive(MultiplayerDeathMode.StayDeadUntilMended, __instance.RunState))
		{
			return true;
		}
        // Original MPX exception: boss-room completion can revive players.
        if (__instance.RunState?.CurrentRoom?.RoomType == RoomType.Boss) return true;
		__result = Task.CompletedTask;
		return false;
	}
}
