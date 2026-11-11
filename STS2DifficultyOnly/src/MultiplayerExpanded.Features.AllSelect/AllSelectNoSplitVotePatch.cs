using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace MultiplayerExpanded.Features.AllSelect;

[HarmonyPatch(typeof(EventSplitVoteAnimation), "TryPlay")]
internal static class AllSelectNoSplitVotePatch
{
	private static readonly AccessTools.FieldRef<EventSplitVoteAnimation, NEventLayout>? LayoutRef = SafeFieldRef();

	private static AccessTools.FieldRef<EventSplitVoteAnimation, NEventLayout>? SafeFieldRef()
	{
		try
		{
			return AccessTools.FieldRefAccess<EventSplitVoteAnimation, NEventLayout>("_eventLayout");
		}
		catch (Exception ex)
		{
			MainFile.Logger.Error("[AllSelect] Could not reach EventSplitVoteAnimation._eventLayout: " + ex.Message, 1);
			return null;
		}
	}

	private static bool Prepare()
	{
		if (LayoutRef != null)
		{
			return true;
		}
		MainFile.Logger.Error("[AllSelect] Split-vote suppression not armed; all-select events will play the 'one option wins' animation, which misrepresents the result.", 1);
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(EventSplitVoteAnimation __instance, ref Task __result)
	{
		try
		{
			if (!(AllSelectUi.EventOf(LayoutRef.Invoke(__instance)) is IAllSelectEvent))
			{
				return true;
			}
			__result = Task.CompletedTask;
			return false;
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[AllSelect] Split-vote suppression failed: " + ex.Message, 1);
			return true;
		}
	}
}
