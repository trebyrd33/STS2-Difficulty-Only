using System;
using BaseLib.Config;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace MultiplayerExpanded.Features.Settings;

// Bind only after vanilla establishes the session. Never initialize Steam or create a lobby here.
internal static class MpxSettingsSyncWiring
{
 private static INetGameService? _activeNet;
 private static INetHostGameService? _hostNet;
 private static INetClientGameService? _clientNet;
 private static Action<NetErrorInfo>? _disconnected;
 private static ModConfig? _config;
 private static bool _handlingHostChange;

 public static void AttachClient(INetGameService net)
 {
  if (!net.IsConnected || net is not INetClientGameService client || client.NetClient == null) return;
  if (ReferenceEquals(_activeNet, net)) return;
  Detach();
  _activeNet = net;
  _clientNet = client;
  net.RegisterMessageHandler<MpxConfigSyncMessage>(OnConfigMessage);
  Subscribe(net);
  MainFile.Logger.Info("[SettingsSync] Client attached after successful vanilla handshake.", 1);
 }

 private static void AttachHost(INetHostGameService net)
 {
  if (!net.IsConnected || ReferenceEquals(_activeNet, net)) return;
  Detach();
  _activeNet = net;
  _hostNet = net;
  Subscribe(net);
  if (_config != null) _config.ConfigChanged += OnHostConfigChanged;
  MainFile.Logger.Info("[SettingsSync] Host attached after vanilla peer readiness.", 1);
 }

 private static void Subscribe(INetGameService net)
 {
  // Capture the service: a delayed disconnect from an old session cannot reset a newer one.
  _disconnected = _ => { if (ReferenceEquals(_activeNet, net)) Detach(); };
  net.Disconnected += _disconnected;
  _config = ModConfigRegistry.Get(MainFile.ModId);
  if (_config != null) _config.ConfigChanged += OnAnyConfigChanged;
 }

 public static void SendSnapshotTo(INetHostGameService net, ulong peerId)
 {
  // The only caller is the postfix on SetPeerReadyForBroadcasting, after verifying its ready peer.
  if (!net.IsConnected || !net.GetVersionInfoForPeer(peerId).HasValue) return;
  AttachHost(net);
  net.SendMessage(new MpxConfigSyncMessage { json = MpxSettingsSync.Snapshot() }, peerId);
  MainFile.Logger.Info($"[SettingsSync] Sent settings to ready peer {peerId}.", 1);
 }

 private static void OnAnyConfigChanged(object? sender, EventArgs e) => MpxSettingsSync.EnforceCurrentSettings();

 private static void OnConfigMessage(MpxConfigSyncMessage message, ulong senderId)
 {
  var client = _clientNet;
  if (client == null || !client.IsConnected || client.NetClient == null || senderId != client.NetClient.HostNetId) return;
  try { MpxSettingsSync.ApplyOverride(message.json); }
  catch (Exception ex) { MainFile.Logger.Error("[SettingsSync] Rejected host payload: " + ex.Message, 1); throw; }
 }

 private static void OnHostConfigChanged(object? sender, EventArgs e)
 {
  if (_handlingHostChange || _hostNet == null || !_hostNet.IsConnected) return;
  _handlingHostChange = true;
  try {
   MpxSettingsSync.EnforceCurrentSettings();
   // Vanilla broadcasts only to peers that completed readiness; keep its transport untouched.
   _hostNet.SendMessage(new MpxConfigSyncMessage { json = MpxSettingsSync.Snapshot() });
   _config?.SaveDebounced(1000);
   _config?.ConfigReloaded();
  } catch (Exception ex) { MainFile.Logger.Warn("[SettingsSync] Host config re-broadcast failed: " + ex.Message, 1); }
  finally { _handlingHostChange = false; }
 }

 private static void Detach()
 {
  var previous = _activeNet;
  var wasClient = _clientNet != null;
  _activeNet = null;
  if (_config != null) {
   _config.ConfigChanged -= OnHostConfigChanged;
   _config.ConfigChanged -= OnAnyConfigChanged;
  }
  if (previous != null) {
   if (_disconnected != null) previous.Disconnected -= _disconnected;
   if (wasClient) previous.UnregisterMessageHandler<MpxConfigSyncMessage>(OnConfigMessage);
  }
  _hostNet = null;
  _clientNet = null;
  _disconnected = null;
  _config = null;
  if (previous != null) MpxSettingsSync.Restore();
 }
}
