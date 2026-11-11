using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.Settings;

namespace MultiplayerExpanded.Features.DeathMechanic;

[HarmonyPatch(typeof(RewardsSet), "Offer")]
internal static class DeadPlayerAutoProceedPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(RewardsSet __instance)
	{
		Player player = __instance.Player;
		if (((player != null) ? player.Creature : null) != null && !player.Creature.IsAlive && LocalContext.IsMe(player) && DeathMechanicGate.IsActive(MultiplayerDeathMode.StayDeadUntilMended, player.RunState))
		{
			Callable val = Callable.From<Task>((Func<Task>)(() => TaskHelper.RunSafely(RunManager.Instance.ProceedFromTerminalRewardsScreen())));
			val.CallDeferred(Array.Empty<Variant>());
		}
	}
}
