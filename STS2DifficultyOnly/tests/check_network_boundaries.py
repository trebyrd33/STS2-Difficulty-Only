from pathlib import Path
import re
r=Path(__file__).resolve().parents[1];src=r/'src';checks=0
def require(x,m):
 global checks
 checks+=1
 if not x:raise AssertionError(m)
s=src/'MultiplayerExpanded.Features.Settings'
require(not (s/'MpxSettingsSyncHostPatch.cs').exists(),'Host constructor hook restored')
client=(s/'MpxSettingsSyncClientPatch.cs').read_text();peer=(s/'MpxSettingsSyncPeerReadyPatch.cs').read_text();w=(s/'MpxSettingsSyncWiring.cs').read_text()
require('"HandshakeSucceeded"' in client,'Client does not attach after handshake')
require('"SetPeerReadyForBroadcasting"' in peer,'Host does not use vanilla readiness')
require('p.peerId == peerId && p.readyForBroadcasting' in peer,'Missing ready-peer check')
require('!net.GetVersionInfoForPeer(peerId).HasValue' in w,'Unknown peer accepted')
require('ReferenceEquals(_activeNet, net)' in w,'Missing session provenance guard')
for p in s.glob('*.cs'):
 text=p.read_text()
 require('SoleConstructor' not in text and 'MethodType.Constructor' not in text,'Settings patches constructor '+p.name)
 require(not re.search(r'\b(?:SteamMatchmaking|SteamNetworkingSockets|SteamAPI|CreateLobby|StartSteamHost)\b',text),'Settings layer touches Steam transport '+p.name)
for p in src.rglob('*.cs'):
 text=p.read_text()
 require(not re.search(r'\[HarmonyPatch\(typeof\((?:SteamHost|SteamClient|SteamInitializer|SteamMatchmaking|SteamNetworkingSockets)\)',text),'Native Steam hook '+str(p))
print(f'NETWORK BOUNDARIES: {checks} static checks passed; no settings constructor hooks or Steam transport patches; client handshake and host peer-ready guards present. Does not execute Steam.')
