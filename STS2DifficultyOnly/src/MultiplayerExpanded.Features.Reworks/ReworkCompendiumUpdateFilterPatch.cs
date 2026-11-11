using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace MultiplayerExpanded.Features.Reworks;

[HarmonyPatch(typeof(NCardLibrary), "UpdateFilter")]
internal static class ReworkCompendiumUpdateFilterPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Prefix(NCardLibrary __instance)
	{
		ReworkCompendium.ApplyPreview(ReworkCompendium.ReadTickbox(__instance));
	}
}
