using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.Settings;

namespace MultiplayerExpanded.Features.DeathMechanic;

[HarmonyPatch(typeof(RestSiteOption), "Generate")]
internal static class ReviveAllRestSiteOptionInject
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(Player player, List<RestSiteOption> __result)
	{
		try
		{
			if (DeathMechanicGate.IsActive(MultiplayerDeathMode.StayDeadUntilMended, player.RunState))
			{
				Creature creature = player.Creature;
				if (creature != null && creature.IsAlive && ((IPlayerCollection)player.RunState).Players.Any((Player p) =>
				{
					Creature creature2 = p.Creature;
					return creature2 != null && creature2.IsDead;
				}))
				{
					__result.Add((RestSiteOption)(object)new ReviveAllRestSiteOption(player));
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[DeathMechanic] Revive All injection failed: " + ex.Message, 1);
		}
	}
}
