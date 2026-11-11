using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Features.Settings;

namespace MultiplayerExpanded.Features.DeathMechanic;

[HarmonyPatch(typeof(RestSiteSynchronizer), "BeginRestSite")]
internal static class DeadPlayerRestSiteLockoutPatch
{
	private static readonly Type PlayerRestSiteType = typeof(RestSiteSynchronizer).GetNestedType("PlayerRestSite", BindingFlags.NonPublic);

	private static readonly FieldInfo RestSitesField = AccessTools.Field(typeof(RestSiteSynchronizer), "_restSites");

	private static readonly FieldInfo PlayerCollectionField = AccessTools.Field(typeof(RestSiteSynchronizer), "_playerCollection");

	private static readonly FieldInfo OptionsField = AccessTools.Field(PlayerRestSiteType, "options");

	private static readonly FieldInfo? CompletionField = (Beta.IsOn ? AccessTools.Field(PlayerRestSiteType, "completionTaskSource") : null);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(RestSiteSynchronizer __instance)
	{
		try
		{
			object? value = PlayerCollectionField.GetValue(__instance);
			IPlayerCollection val = (IPlayerCollection)((value is IPlayerCollection) ? value : null);
			if (val == null || !(RestSitesField.GetValue(__instance) is IList list))
			{
				return;
			}
			IRunState runState = ((val.Players.Count > 0) ? val.Players[0].RunState : null);
			if (!DeathMechanicGate.IsActive(MultiplayerDeathMode.StayDeadUntilMended, runState))
			{
				return;
			}
			foreach (Player player in val.Players)
			{
				if (player.Creature == null || player.Creature.IsAlive)
				{
					continue;
				}
				int playerSlotIndex = val.GetPlayerSlotIndex(player);
				if (playerSlotIndex >= 0 && playerSlotIndex < list.Count)
				{
					if (OptionsField.GetValue(list[playerSlotIndex]) is IList list2)
					{
						list2.Clear();
					}
					(CompletionField?.GetValue(list[playerSlotIndex]) as TaskCompletionSource)?.TrySetResult();
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[DeathMechanic] Rest-site lockout failed: " + ex.Message, 1);
		}
	}
}
