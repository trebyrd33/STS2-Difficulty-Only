using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace MultiplayerExpanded.Features.Reworks;

internal static class ReworkCompendium
{
	private static readonly FieldInfo MultiplayerTickboxField = AccessTools.Field(typeof(NCardLibrary), "_viewMultiplayerCards");

	internal static bool ReadTickbox(NCardLibrary library)
	{
		object? obj = MultiplayerTickboxField?.GetValue(library);
		object? obj2 = ((obj is NTickbox) ? obj : null);
		if (obj2 == null)
		{
			return false;
		}
		return ((NTickbox)obj2).IsTicked;
	}

	internal static void ApplyPreview(bool on)
	{
		if (ReworkPatches.CompendiumPreview != on)
		{
			ReworkPatches.CompendiumPreview = on;
			int value = ReworkStateSync.RebakeTemplates();
			MainFile.Logger.Info($"[ReworkCompendium] preview={on}; re-baked {value} reworked template(s).", 1);
		}
	}
}
