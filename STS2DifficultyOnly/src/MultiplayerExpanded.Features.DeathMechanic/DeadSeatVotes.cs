using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.Settings;

namespace MultiplayerExpanded.Features.DeathMechanic;

internal static class DeadSeatVotes
{
	internal const uint Placeholder = uint.MaxValue;

	private static readonly FieldInfo? CanonicalEventField = AccessTools.Field(typeof(EventSynchronizer), "_canonicalEvent");

	private static readonly FieldInfo? PlayerVotesField = AccessTools.Field(typeof(EventSynchronizer), "_playerVotes");

	private static readonly FieldInfo? PlayerCollectionField = AccessTools.Field(typeof(EventSynchronizer), "_playerCollection");

	private static readonly FieldInfo? PageIndexField = AccessTools.Field(typeof(EventSynchronizer), "_pageIndex");

	private static readonly FieldInfo? NetServiceField = AccessTools.Field(typeof(EventSynchronizer), "_netService");

	private static readonly FieldInfo? MessageBufferField = AccessTools.Field(typeof(EventSynchronizer), "_messageBuffer");

	private static readonly FieldInfo? OptionSelectionRngField = AccessTools.Field(typeof(EventSynchronizer), "_multiplayerOptionSelectionRng");

	private static readonly MethodInfo? ChooseOptionForSharedEventMethod = AccessTools.Method(typeof(EventSynchronizer), "ChooseOptionForSharedEvent", (Type[])null, (Type[])null);

	private static bool? _armed;

	internal static bool Arm(string patchName)
	{
		bool valueOrDefault = _armed == true;
		if (!_armed.HasValue)
		{
			valueOrDefault = CanonicalEventField != null && PlayerVotesField != null && PlayerCollectionField != null && PageIndexField != null && NetServiceField != null && MessageBufferField != null && OptionSelectionRngField != null && ChooseOptionForSharedEventMethod != null;
			_armed = valueOrDefault;
		}
		if (_armed.Value)
		{
			return true;
		}
		MainFile.Logger.Error($"[DeathMechanic] {patchName} not armed: EventSynchronizer's internals did not resolve (canonicalEvent={CanonicalEventField != null}, playerVotes={PlayerVotesField != null}, playerCollection={PlayerCollectionField != null}, pageIndex={PageIndexField != null}, netService={NetServiceField != null}, messageBuffer={MessageBufferField != null}, multiplayerOptionSelectionRng={OptionSelectionRngField != null}, chooseOptionForSharedEvent={ChooseOptionForSharedEventMethod != null}). " + "Shared events will wait for dead players' votes.", 1);
		return false;
	}

	internal static bool Applies(EventSynchronizer sync)
	{
        var players = (PlayerCollectionField.GetValue(sync) as IPlayerCollection)?.Players;
        if (players == null || players.Count == 0 || !DeathMechanicGate.IsActive(MultiplayerDeathMode.StayDeadUntilMended,players[0].RunState)) return false;
        object? value = CanonicalEventField.GetValue(sync);
		EventModel val = (EventModel)((value is EventModel) ? value : null);
		if (val != null && val.IsShared)
		{
			return true;
		}
		return false;
	}

	internal static List<uint?> Votes(EventSynchronizer sync)
	{
		return (List<uint?>)PlayerVotesField.GetValue(sync);
	}

	internal static void SeatDead(EventSynchronizer sync)
	{
		try
		{
			if (!Applies(sync))
			{
				return;
			}
			List<uint?> list = Votes(sync);
			IReadOnlyList<Player> players = ((IPlayerCollection)PlayerCollectionField.GetValue(sync)).Players;
			for (int i = 0; i < players.Count && i < list.Count; i++)
			{
				Creature creature = players[i].Creature;
				if (creature != null && creature.IsDead && !list[i].HasValue)
				{
					list[i] = uint.MaxValue;
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[DeathMechanic] Could not seat dead players in a shared event vote: " + ex.Message, 1);
		}
	}

	internal static uint PageIndex(EventSynchronizer sync)
	{
		return (uint)PageIndexField.GetValue(sync);
	}

	internal static uint PickAmong(EventSynchronizer sync, List<uint> cast)
	{
		return ((Rng)OptionSelectionRngField.GetValue(sync)).NextItem<uint>((IEnumerable<uint>)cast);
	}

	internal static void Announce(EventSynchronizer sync, uint optionIndex)
	{
		INetGameService val = (INetGameService)NetServiceField.GetValue(sync);
		RunLocationTargetedMessageBuffer val2 = (RunLocationTargetedMessageBuffer)MessageBufferField.GetValue(sync);
		val.SendMessage<SharedEventOptionChosenMessage>(new SharedEventOptionChosenMessage
		{
			optionIndex = optionIndex,
			pageIndex = (uint)PageIndexField.GetValue(sync),
			location = val2.CurrentLocation
		});
		ChooseOptionForSharedEventMethod.Invoke(sync, new object[1] { optionIndex });
	}
}
