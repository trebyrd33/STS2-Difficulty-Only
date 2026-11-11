using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Features.Reworks;

[HarmonyPatch(typeof(CardModel), "TitleLocString", MethodType.Getter)]
internal static class ReworkTitlePatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(CardModel __instance, ref LocString __result)
	{
		if (ReworkPatches.ShowReworked() && ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo info))
		{
			LocString val = new LocString("cards", info.LocKey + ".title");
			if (val.Exists())
			{
				__result = val;
			}
		}
	}
}
