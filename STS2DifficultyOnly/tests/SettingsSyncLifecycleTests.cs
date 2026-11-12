global using MultiplayerExpandedConfig = STS2DifficultyOnly.MultiplayerExpandedConfig;
using System;
using System.Collections.Generic;
using MultiplayerExpanded.Features.Settings;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;

// Minimal substitutes for external game/BaseLib APIs. The tested sync, payload and config
// classes are compiled directly from production source, without loading the STS2 module.
namespace BaseLib.Config {
 [AttributeUsage(AttributeTargets.Class)] public sealed class ConfigHoverTipsByDefaultAttribute : Attribute { }
 public class ModConfig {
  public event EventHandler? ConfigChanged;
  public int Subscribers => ConfigChanged?.GetInvocationList().Length ?? 0;
  public void Change() => ConfigChanged?.Invoke(this,EventArgs.Empty);
  public void ConfigReloaded() { }
  public void SaveDebounced(int milliseconds) { }
 }
 public class SimpleModConfig : ModConfig { }
 public static class ModConfigRegistry {
  public static readonly ModConfig Config = new ModConfig();
  public static ModConfig? Get(string id) => Config;
 }
}
namespace MegaCrit.Sts2.Core.Entities.Multiplayer { public readonly struct NetErrorInfo { } }
namespace MegaCrit.Sts2.Core.Multiplayer.Game {
 public delegate void MessageHandlerDelegate<T>(T message, ulong senderId);
 public interface INetGameService {
  bool IsConnected { get; }
  event Action<NetErrorInfo> Disconnected;
  void RegisterMessageHandler<T>(MessageHandlerDelegate<T> handler);
  void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> handler);
  void SendMessage<T>(T message);
  void SendMessage<T>(T message,ulong peer);
 }
 public sealed class FakeTransport { public ulong HostNetId = 7; }
 public interface INetClientGameService : INetGameService { FakeTransport? NetClient { get; } }
 public interface INetHostGameService : INetGameService { int? GetVersionInfoForPeer(ulong peer); }
}
namespace MultiplayerExpanded {
 public static class MainFile {
  public const string ModId="STS2DifficultyOnly";
  public static readonly FakeLogger Logger=new FakeLogger();
 }
 public sealed class FakeLogger {
  public void Info(string message,int level) { }
  public void Warn(string message,int level) { }
  public void Error(string message,int level) { }
 }
}
namespace MultiplayerExpanded.Features.Settings { public struct MpxConfigSyncMessage { public string? json; } }
sealed class FakeNet : INetHostGameService, INetClientGameService {
 public bool IsConnected { get; set; }
 public FakeTransport? NetClient { get; set; } = new FakeTransport();
 public event Action<NetErrorInfo>? Disconnected;
 public int Subscribers => Disconnected?.GetInvocationList().Length ?? 0;
 public readonly HashSet<ulong> Peers = new HashSet<ulong>();
 public int Registrations,Unregistrations,Sends;
 public MpxConfigSyncMessage Last;
 public MessageHandlerDelegate<MpxConfigSyncMessage>? Handler;
 public int? GetVersionInfoForPeer(ulong peer) => Peers.Contains(peer) ? 1 : null;
 public void RegisterMessageHandler<T>(MessageHandlerDelegate<T> handler) { Registrations++;Handler=(MessageHandlerDelegate<MpxConfigSyncMessage>)(object)handler; }
 public void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> handler) { Unregistrations++;Handler=null; }
 public void SendMessage<T>(T message) { if(!IsConnected)throw new Exception("Early send");Sends++;Last=(MpxConfigSyncMessage)(object)message!; }
 public void SendMessage<T>(T message,ulong peer) { if(!Peers.Contains(peer))throw new Exception("Unestablished peer");SendMessage(message); }
 public Action<NetErrorInfo>? CaptureDisconnect() => Disconnected;
 public void Disconnect() { IsConnected=false;Disconnected?.Invoke(new NetErrorInfo()); }
 public void Receive(string json,ulong sender) => Handler?.Invoke(new MpxConfigSyncMessage {json=json},sender);
}
class SettingsSyncLifecycleTests {
 static int checks;
 static void Eq<T>(T actual,T expected,string label) {checks++;if(!EqualityComparer<T>.Default.Equals(actual,expected))throw new Exception($"{label}: {actual} != {expected}");}
 static void Main() {
  var config=BaseLib.Config.ModConfigRegistry.Config;
  var host=new FakeNet();host.Peers.Add(8);
  MpxSettingsSyncWiring.SendSnapshotTo(host,8);
  Eq(host.Sends,0,"Disconnected host sends nothing");Eq(host.Subscribers,0,"Disconnected host not subscribed");Eq(config.Subscribers,0,"No pre-lobby config subscription");
  var client=new FakeNet();MpxSettingsSyncWiring.AttachClient(client);
  Eq(client.Registrations,0,"No pre-handshake registration");Eq(client.Subscribers,0,"No pre-handshake subscription");
  config.Change();Eq(host.Sends,0,"Config edit during failed lobby does not send");
  host.IsConnected=true;MpxSettingsSyncWiring.SendSnapshotTo(host,99);
  Eq(host.Sends,0,"Unknown peer does not initialize host");Eq(host.Subscribers,0,"Unknown peer not subscribed");
  MpxSettingsSyncWiring.SendSnapshotTo(host,8);Eq(host.Sends,1,"First established peer receives snapshot");Eq(host.Subscribers,1,"Host attached once");Eq(config.Subscribers,2,"Host listeners attached");
  var decoded=SettingsPayload.Parse(host.Last.json!);Eq(decoded.Count,8,"All eight settings sent");
  MpxSettingsSyncWiring.SendSnapshotTo(host,8);Eq(host.Subscribers,1,"Repeat readiness idempotent");Eq(config.Subscribers,2,"No duplicate config listeners");
  config.Change();Eq(host.Sends,3,"Host edit still broadcasts");
  var staleHostDisconnect=host.CaptureDisconnect();host.Disconnect();Eq(config.Subscribers,0,"Host disconnect detaches config");Eq(host.Subscribers,0,"Host disconnect detaches net");
  config.Change();Eq(host.Sends,3,"Disconnected host cannot broadcast");
  client.IsConnected=true;client.NetClient=null;MpxSettingsSyncWiring.AttachClient(client);Eq(client.Registrations,0,"Connected flag without transport rejected");
  client.NetClient=new FakeTransport();MpxSettingsSyncWiring.AttachClient(client);MpxSettingsSyncWiring.AttachClient(client);
  Eq(client.Registrations,1,"Post-handshake handler registered once");Eq(client.Subscribers,1,"Client disconnect subscribed once");Eq(config.Subscribers,1,"Client config listener once");
  MultiplayerExpandedConfig.EnemyReworks=true;string original=MpxSettingsSync.Snapshot();MultiplayerExpandedConfig.EnemyReworks=false;string payload=MpxSettingsSync.Snapshot();MultiplayerExpandedConfig.EnemyReworks=true;
  client.Receive(payload,99);Eq(MultiplayerExpandedConfig.EnemyReworks,true,"Non-host payload ignored");Eq(MpxSettingsSync.OverrideActive,false,"Non-host cannot create override");
  client.Receive(payload,7);Eq(MultiplayerExpandedConfig.EnemyReworks,false,"Host payload applied");Eq(MpxSettingsSync.OverrideActive,true,"Override active");Eq(MpxSettingsSync.CanSave,false,"Client preferences protected");
  MultiplayerExpandedConfig.EnemyReworks=true;config.Change();Eq(MultiplayerExpandedConfig.EnemyReworks,false,"Client edits reset to host settings");
  staleHostDisconnect?.Invoke(new NetErrorInfo());Eq(client.Subscribers,1,"Old disconnect leaves new session attached");Eq(MpxSettingsSync.OverrideActive,true,"Old disconnect cannot clear new override");
  MpxSettingsSync.LockRun();MultiplayerExpandedConfig.VanillaCardReworks=false;config.Change();Eq(MultiplayerExpandedConfig.VanillaCardReworks,true,"Run settings stay locked");
  bool rejected=false;try{client.Receive("{}",7);}catch(ArgumentException){rejected=true;}Eq(rejected,true,"Invalid host schema still rejected");Eq(MultiplayerExpandedConfig.EnemyReworks,false,"Invalid message cannot partially change settings");
  var staleClientDisconnect=client.CaptureDisconnect();client.Disconnect();Eq(client.Unregistrations,1,"Handler removed on disconnect");Eq(client.Subscribers,0,"Client event removed");Eq(config.Subscribers,0,"Client config removed");Eq(MpxSettingsSync.OverrideActive,false,"Disconnect clears override");Eq(MpxSettingsSync.Snapshot(),original,"Original local preferences restored");Eq(MpxSettingsSync.CanSave,true,"Save protection released");
  var replacement=new FakeNet {IsConnected=true};MpxSettingsSyncWiring.AttachClient(replacement);staleClientDisconnect?.Invoke(new NetErrorInfo());Eq(replacement.Subscribers,1,"Delayed old-client event ignored");
  MpxSettingsSyncWiring.AttachClient(client);Eq(replacement.Subscribers,1,"Failed retry leaves valid session alone");
  replacement.Disconnect();Eq(config.Subscribers,0,"Replacement cleanup");
  Console.WriteLine($"PASS: {checks} settings-sync lifecycle assertions; production sync code with fake external APIs, no game or Steam runtime.");
 }
}
