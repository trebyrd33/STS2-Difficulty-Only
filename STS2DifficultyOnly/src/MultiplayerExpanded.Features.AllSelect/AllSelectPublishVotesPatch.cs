using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BaseLib.Abstracts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace MultiplayerExpanded.Features.AllSelect;

[HarmonyPatch(typeof(EventSynchronizer), "ChooseSharedEventOption")]
internal static class AllSelectPublishVotesPatch
{
	private static bool Prepare()
	{
		return AllSelectAccess.Arm("AllSelectPublishVotesPatch");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Prefix(EventSynchronizer __instance)
	{
		try
		{
			AllSelectSession active = AllSelectSession.Active;
			if (active == null || AllSelectAccess.Rules(__instance) == null)
			{
				return;
			}
			List<uint?> list = AllSelectAccess.Votes(__instance);
			if (list != null)
			{
				AllSelectVotesMessage allSelectVotesMessage = active.PublishVotes(list, AllSelectAccess.Page(__instance), AllSelectAccess.Location(__instance));
				INetGameService val = AllSelectAccess.NetService(__instance);
				if (val != null && (int)val.Type == 2)
				{
					CustomTargetedMessageWrapper.Send((ICustomTargetedMessage)(object)allSelectVotesMessage, val);
				}
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Error($"[AllSelect] Failed to publish the vote vector: {value}", 1);
		}
	}
}
