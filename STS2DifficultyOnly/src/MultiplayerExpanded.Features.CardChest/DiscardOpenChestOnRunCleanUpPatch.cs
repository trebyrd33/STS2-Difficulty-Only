using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

using MultiplayerExpanded.Features.CardPicking;

namespace MultiplayerExpanded.Features.CardChest;

[HarmonyPatch(typeof(RunManager), "CleanUp")]
internal static class DiscardOpenChestOnRunCleanUpPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPostfix]
	private static void DropChest()
	{
		CardPickingSession.Reset();
		CardChestRoom.Active = null;
		
	}
}
