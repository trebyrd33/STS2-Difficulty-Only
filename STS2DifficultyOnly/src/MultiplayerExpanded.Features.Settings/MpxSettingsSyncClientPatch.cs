using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Features.Settings;

[HarmonyPatch]
internal static class MpxSettingsSyncClientPatch
{
	private static MethodBase TargetMethod()
	{
		return StableResolve.SoleConstructor(typeof(NetClientGameService));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(NetClientGameService __instance)
	{
		try
		{
			MpxSettingsSyncWiring.AttachClient((INetGameService)(object)__instance);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[SettingsSync] Client hook failed: " + ex.Message, 1);
		}
	}
}
