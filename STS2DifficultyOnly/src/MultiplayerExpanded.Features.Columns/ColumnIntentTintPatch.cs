using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.TestSupport;

namespace MultiplayerExpanded.Features.Columns;

[HarmonyPatch(typeof(NIntent), "UpdateVisuals")]
internal static class ColumnIntentTintPatch
{
	private static readonly AccessTools.FieldRef<NIntent, Creature?>? OwnerRef = ((AccessTools.Field(typeof(NIntent), "_owner") != null) ? AccessTools.FieldRefAccess<NIntent, Creature>("_owner") : null);

	private static bool Prepare()
	{
		if (ColumnView.IntentRef == null)
		{
			MainFile.Logger.Error("[Columns] NIntent._intent not found; a protector's attack intents from the other column stay greyed out.", 1);
		}
		if (OwnerRef != null)
		{
			return true;
		}
		MainFile.Logger.Error("[Columns] NIntent._owner not found; intents of the other column aren't greyed out.", 1);
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPostfix]
	private static void Postfix(NIntent __instance)
	{
		if (TestMode.IsOn)
		{
			return;
		}
		Creature val = OwnerRef.Invoke(__instance);
		if (val == null)
		{
			return;
		}
		try
		{
			ColumnView.TintIntent(__instance, val);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[Columns] tinting an intent failed: " + ex.Message, 1);
		}
	}
}
