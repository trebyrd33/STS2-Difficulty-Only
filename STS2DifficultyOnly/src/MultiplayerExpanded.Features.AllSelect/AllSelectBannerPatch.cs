using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace MultiplayerExpanded.Features.AllSelect;

[HarmonyPatch(typeof(NEventLayout), "AddOptions")]
internal static class AllSelectBannerPatch
{
	private static readonly LocString Banner = new LocString("events", "MULTIPLAYEREXPANDED-ALL_SELECT.banner");

	private static bool Prepare()
	{
		return AllSelectUi.Armed;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(NEventLayout __instance)
	{
		try
		{
			if (AllSelectUi.EventOf(__instance) is IAllSelectEvent)
			{
				MegaLabel obj = AllSelectUi.LabelRef.Invoke(__instance);
				if (obj != null)
				{
					obj.SetTextAutoSize(Banner.GetFormattedText());
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[AllSelect] Could not relabel the shared-event banner: " + ex.Message, 1);
		}
	}

	static AllSelectBannerPatch()
	{
	}
}
