using System;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace MultiplayerExpanded.Features.AllSelect;

internal static class AllSelectUi
{
	internal static readonly AccessTools.FieldRef<NEventLayout, EventModel>? EventRef = Ref<EventModel>("_event");

	internal static readonly AccessTools.FieldRef<NEventLayout, MegaLabel?>? LabelRef = Ref<MegaLabel>("_sharedEventLabel");

	internal static bool Armed
	{
		get
		{
			if (EventRef != null)
			{
				return LabelRef != null;
			}
			return false;
		}
	}

	private static AccessTools.FieldRef<NEventLayout, T>? Ref<T>(string field)
	{
		try
		{
			return AccessTools.FieldRefAccess<NEventLayout, T>(field);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Error("[AllSelect] Could not reach NEventLayout." + field + ": " + ex.Message, 1);
			return null;
		}
	}

	internal static EventModel? EventOf(NEventLayout layout)
	{
		AccessTools.FieldRef<NEventLayout, EventModel>? eventRef = EventRef;
		if (eventRef == null)
		{
			return null;
		}
		return eventRef.Invoke(layout);
	}
}
