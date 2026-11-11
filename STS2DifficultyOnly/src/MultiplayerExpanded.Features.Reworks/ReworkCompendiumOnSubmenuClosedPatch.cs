using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace MultiplayerExpanded.Features.Reworks;

[HarmonyPatch(typeof(NCardLibrary), "OnSubmenuClosed")]
internal static class ReworkCompendiumOnSubmenuClosedPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix()
	{
		ReworkCompendium.ApplyPreview(on: false);
	}
}
