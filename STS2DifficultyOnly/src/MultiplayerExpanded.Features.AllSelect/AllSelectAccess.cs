using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerExpanded.Features.AllSelect;

internal static class AllSelectAccess
{
	internal static readonly FieldInfo? CanonicalEventField = AccessTools.Field(typeof(EventSynchronizer), "_canonicalEvent");

	internal static readonly FieldInfo? EventsField = AccessTools.Field(typeof(EventSynchronizer), "_events");

	internal static readonly FieldInfo? PlayerVotesField = AccessTools.Field(typeof(EventSynchronizer), "_playerVotes");

	internal static readonly FieldInfo? PageIndexField = AccessTools.Field(typeof(EventSynchronizer), "_pageIndex");

	internal static readonly FieldInfo? PlayerCollectionField = AccessTools.Field(typeof(EventSynchronizer), "_playerCollection");

	internal static readonly FieldInfo? NetServiceField = AccessTools.Field(typeof(EventSynchronizer), "_netService");

	internal static readonly FieldInfo? MessageBufferField = AccessTools.Field(typeof(EventSynchronizer), "_messageBuffer");

	internal static readonly MethodInfo? ChooseOptionForEventMethod = AccessTools.Method(typeof(EventSynchronizer), "ChooseOptionForEvent", (Type[])null, (Type[])null);

	internal static bool Resolved
	{
		get
		{
			if (CanonicalEventField != null && EventsField != null && PlayerVotesField != null && PageIndexField != null && PlayerCollectionField != null && NetServiceField != null && MessageBufferField != null)
			{
				return ChooseOptionForEventMethod != null;
			}
			return false;
		}
	}

	internal static bool Arm(string patchName)
	{
		if (Resolved)
		{
			return true;
		}
		MainFile.Logger.Error($"[AllSelect] {patchName} not armed: EventSynchronizer's internals did not resolve (canonicalEvent={CanonicalEventField != null}, events={EventsField != null}, playerVotes={PlayerVotesField != null}, pageIndex={PageIndexField != null}, playerCollection={PlayerCollectionField != null}, netService={NetServiceField != null}, messageBuffer={MessageBufferField != null}, chooseOptionForEvent={ChooseOptionForEventMethod != null}). " + "All-select events will fall back to a plain shared vote.", 1);
		return false;
	}

	internal static IAllSelectEvent? Rules(EventSynchronizer sync)
	{
		RunManager instance = RunManager.Instance;
		if (instance == null || !instance.IsInProgress)
		{
			return null;
		}
		return CanonicalEventField?.GetValue(sync) as IAllSelectEvent;
	}

	internal static List<EventModel>? Events(EventSynchronizer sync)
	{
		return EventsField?.GetValue(sync) as List<EventModel>;
	}

	internal static List<uint?>? Votes(EventSynchronizer sync)
	{
		return PlayerVotesField?.GetValue(sync) as List<uint?>;
	}

	internal static uint Page(EventSynchronizer sync)
	{
		object obj = PageIndexField?.GetValue(sync);
		if (obj is uint)
		{
			return (uint)obj;
		}
		return 0u;
	}

	internal static void SetPage(EventSynchronizer sync, uint page)
	{
		PageIndexField?.SetValue(sync, page);
	}

	internal static IPlayerCollection? Players(EventSynchronizer sync)
	{
		object? obj = PlayerCollectionField?.GetValue(sync);
		return (IPlayerCollection?)((obj is IPlayerCollection) ? obj : null);
	}

	internal static INetGameService? NetService(EventSynchronizer sync)
	{
		object? obj = NetServiceField?.GetValue(sync);
		return (INetGameService?)((obj is INetGameService) ? obj : null);
	}

	internal static RunLocation Location(EventSynchronizer sync)
	{
		object? obj = MessageBufferField?.GetValue(sync);
		RunLocationTargetedMessageBuffer val = (RunLocationTargetedMessageBuffer)((obj is RunLocationTargetedMessageBuffer) ? obj : null);
		if (val == null)
		{
			return default;
		}
		return val.CurrentLocation;
	}

	internal static void Dispatch(EventSynchronizer sync, object player, int optionIndex)
	{
		ChooseOptionForEventMethod?.Invoke(sync, new object[2] { player, optionIndex });
	}
}
