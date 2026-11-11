using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace MultiplayerExpanded.Features.Settings;

[HarmonyPatch(typeof(NetHostGameService), "SetPeerReadyForBroadcasting")]
internal static class MpxSettingsSyncPeerReadyPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(NetHostGameService __instance, ulong peerId)
	{
		try
		{
			MpxSettingsSyncWiring.SendSnapshotTo((INetHostGameService)(object)__instance, peerId);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn($"[SettingsSync] Failed to sync config to joiner {peerId}: {ex.Message}", 1);
		}
	}
}
