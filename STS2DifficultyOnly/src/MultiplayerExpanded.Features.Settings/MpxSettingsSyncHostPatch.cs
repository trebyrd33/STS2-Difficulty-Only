using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Features.Settings;

[HarmonyPatch]
internal static class MpxSettingsSyncHostPatch
{
	private static MethodBase TargetMethod()
	{
		return StableResolve.SoleConstructor(typeof(NetHostGameService));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(NetHostGameService __instance)
	{
		try
		{
			MpxSettingsSyncWiring.AttachHost((INetGameService)(object)__instance);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[SettingsSync] Host hook failed: " + ex.Message, 1);
		}
	}
}
