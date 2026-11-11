using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Features.Reworks;

[HarmonyPatch(typeof(CardModel), "Description", MethodType.Getter)]
internal static class ReworkDescriptionPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(CardModel __instance, ref LocString __result)
	{
		__result = ReworkDescriptionCallSitePatch.Redirect(__result, __instance);
	}
}
