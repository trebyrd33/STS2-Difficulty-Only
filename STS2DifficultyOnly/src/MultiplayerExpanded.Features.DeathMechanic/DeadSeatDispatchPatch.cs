using System.Runtime.CompilerServices;
using HarmonyLib;
using MultiplayerExpanded.Features.Settings;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace MultiplayerExpanded.Features.DeathMechanic;

[HarmonyPatch(typeof(EventSynchronizer), "ChooseOptionForEvent")]
internal static class DeadSeatDispatchPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(EventSynchronizer __instance, Player player)
	{
        if (!DeathMechanicGate.IsActive(MultiplayerDeathMode.StayDeadUntilMended,player.RunState)) return true;
        Creature creature = player.Creature;
		if (creature == null || !creature.IsDead)
		{
			return true;
		}
		if (!__instance.GetEventForPlayer(player).IsFinished)
		{
			return true;
		}
		MainFile.Logger.Info($"[DeathMechanic] Player {player.NetId} is dead; their event takes no option.", 1);
		return false;
	}
}
