using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace MultiplayerExpanded.Features.DeathMechanic;

[HarmonyPatch(typeof(EventSynchronizer), "ChooseSharedEventOption")]
internal static class DeadSeatPickPatch
{
	private static bool Prepare()
	{
		return DeadSeatVotes.Arm("DeadSeatPickPatch");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(EventSynchronizer __instance)
	{
		uint optionIndex;
		try
		{
			if (!DeadSeatVotes.Applies(__instance))
			{
				return true;
			}
			List<uint?> list = DeadSeatVotes.Votes(__instance);
			if (!list.Contains(uint.MaxValue))
			{
				return true;
			}
			List<uint> list2 = (from v in list
				where v.HasValue && v.Value != uint.MaxValue
				select v.Value).ToList();
			if (list2.Count == 0)
			{
				return true;
			}
			optionIndex = DeadSeatVotes.PickAmong(__instance, list2);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[DeathMechanic] Dead-seat vote pick failed, using vanilla's: " + ex.Message, 1);
			return true;
		}
		DeadSeatVotes.Announce(__instance, optionIndex);
		return false;
	}
}
