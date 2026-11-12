using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer;

namespace MultiplayerExpanded.Features.Settings;

// Vanilla has set IsConnected and completed the handshake before this postfix executes.
[HarmonyPatch(typeof(NetClientGameService), "HandshakeSucceeded")]
internal static class MpxSettingsSyncClientPatch
{
 private static void Postfix(NetClientGameService __instance)
 {
  try { MpxSettingsSyncWiring.AttachClient(__instance); }
  catch (Exception ex) { MainFile.Logger.Warn("[SettingsSync] Post-handshake client hook failed: " + ex, 1); }
 }
}
