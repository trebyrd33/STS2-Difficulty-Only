# Hosting investigation — v0.5.0-test2

## Finding and limits

The native Steam failure's root cause is **not yet established**. The user's isolation test is evidence that enabling this mod is associated with the failure, but source inspection cannot establish which runtime condition makes Steam return `k_EResultNoConnection`. No running Steam/ST​​S2 lobby test is available in this environment. This package is a targeted settings-sync lifecycle correction for retesting, not a confirmed Steam hosting fix.

In the supplied game assembly, `SteamHost.StartHost(int)` calls `SteamMatchmaking.CreateLobby`, awaits its `LobbyCreated_t` callback, and returns `new NetErrorInfo(result.m_eResult)` when the result is not success. `NetHostGameService.StartSteamHost(int)` creates the vanilla SteamHost and returns that task. This failure path precedes peer readiness and settings exchange. No retained mod patch targets SteamHost, SteamClient, SteamInitializer, SteamMatchmaking, SteamNetworkingSockets or CreateLobby. The patch does not replace, retry, suppress, or fabricate Steam results.

## Confirmed lifecycle defect in test1

`MpxSettingsSyncHostPatch` and `MpxSettingsSyncClientPatch` patched the respective service constructors. Their postfixes attached config/disconnect listeners, and the client registered its custom settings handler, before a transport/session existed. Thus a failed or abandoned service could retain mod listeners. The shared static disconnect callback did not identify its originating service; a delayed old-service disconnect could clear references and host overrides belonging to a newer session.

The old host send path already checked `IsConnected`, so there is **no code evidence that test1 sent settings packets before Steam lobby creation**. Constructor attachment alone must not be reported as the proven cause of Steam's native error.

## Targeted changes

- Delete the host-constructor patch entirely. No host synchronization attaches during lobby creation.
- Move the client hook to a postfix on vanilla `NetClientGameService.HandshakeSucceeded`. Vanilla sets `IsConnected` before this method completes; attachment also requires a real client transport.
- Keep the host's vanilla `SetPeerReadyForBroadcasting` postfix, but require an actually connected peer marked ready by vanilla. SendSnapshotTo also requires successful peer version/handshake information. Attach host sync at this point, and send the current full snapshot.
- Make attachment idempotent. Remove old client handlers and config/disconnect subscriptions when detaching. Capture the owning service in the disconnect callback, so stale callbacks cannot detach a newer session.
- Keep host-only validation, all eight settings fields, client save protection, run setting locks, live host config broadcasts through vanilla ready-peer transport, disconnect preference restoration, and multiplayer support.
- Add lifecycle diagnostics under `[SettingsSync]` after client handshake, host readiness and per-peer snapshot delivery. These are mod lifecycle logs, not proof that Steam hosting succeeds.

All four difficulty toggles, three optional-content toggles and death-mode selector are preserved. No card, potion, event, enemy, scaling or death behavior was changed. The replacement PCK is byte-for-byte identical to test1.

## Automated verification

- .NET 9 release DLL compiled against the same supplied references, no compile errors.
- 42 lifecycle assertions execute the production settings wiring/snapshot/parser/config with fake external game/BaseLib APIs: no early attach/send, unknown-peer rejection, connected attachment, duplicate readiness, continued broadcast, host-only overrides, eight-field snapshot, invalid schema rejection, run locks/save protection, detach/retry and stale-disconnect handling.
- 26,784 pure rules/schema assertions pass across all 128 seven-boolean combinations, 1–4 players and four death modes.
- 373 static network-boundary checks pass: no constructor or native Steam transport patches; handshake/readiness guards present.
- 351 card-rework, 85 optional-content and 910 asset/source coverage checks pass.
- Native Godot 4.5.1 mounts the unchanged PCK: 250 checks, zero failures.
- Compiled metadata: 95 directly declared targets found, zero missing; 10 dynamic target classes remain unexecuted. Existence checks do not prove Harmony application in the live game.

## Required in-game retest

1. Update every peer to test2, retain the same BaseLib/game versions, and record game branch/version and BaseLib version.
2. Restart STS2 through Steam while Steam is online. Enable BaseLib and Difficulty Only; attempt to host before changing any settings. Record the complete game log, including SteamHost errors and SettingsSync lines.
3. Repeat host/cancel/retry and failed-host/retry. A host SettingsSync attachment should appear only after a peer has completed handshake/readiness, not during CreateLobby.
4. Join a second client; compare all seven toggles and the death selector. Change host settings before launch and verify clients receive them; client-only edits must not override host settings or persist host values locally.
5. Launch/resume, verify locked settings and existing gameplay; leave/disconnect/rejoin and check restored local preferences. Repeat on the stable and beta branches separately.
6. If the same native error persists, provide the failure log and a matching successful disabled-mod log. Do not treat this package as resolving `k_EResultNoConnection` until that comparison is tested.
