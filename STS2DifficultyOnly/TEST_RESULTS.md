# Verification — 0.5.0-test2

The user reported that test1 loads and displays the settings UI, but hosting fails with Steam `k_EResultNoConnection`. Test2 has not been tested in STS2. See LOBBY_INVESTIGATION.md for the evidence and limits of the targeted lifecycle fix.

## Executed checks

- Release DLL compiled with .NET SDK 9.0.321 against the existing STS2/GodotSharp/Harmony/BaseLib references. No errors; existing framework/nullable warnings remain in build.log.
- 42 settings-sync lifecycle assertions passed against production sync classes using fake external APIs; this exercises binding, sends, snapshots, run locks and cleanup, not Steam or Harmony.
- 26,784 pure rule/schema assertions passed: all 128 settings masks, 1–4 players, four death modes, payload validation and original damage/scaling behavior.
- 373 static network-boundary checks, 351 rework checks, 85 optional-content checks and 910 source/resource coverage checks passed.
- Unchanged PCK mounted in native Godot 4.5.1: 250 checks, zero failures.
- Compiled metadata audit: 95 direct targets found, zero missing, 10 dynamic target classes not executed.
- All gameplay implementation files outside settings sync and version labeling match test1. Four difficulty toggles, three optional switches, four death modes, 52 card reworks, four potion changes, five events, 14 custom cards and one event relic remain.

## Not verified here

The full game/Steam runtime is absent. Earlier managed runtime testing was blocked by the missing Sentry.Godot dependency at the sts2 module initializer; native Godot resource tests do not run the game. Actual Harmony installation, Steam lobby creation, multiplayer packet timing, settings reception in a live lobby, gameplay, save/resume, stable and beta compatibility remain untested. Root cause of the native Steam error is not established.

Use the hosting/settings checklist in LOBBY_INVESTIGATION.md, then the existing gameplay inventories/checklists in ALL_FEATURES.md, ENEMY_REWORKS.md, CURSES_AND_DEATH.md, VANILLA_CARD_REWORKS.md and EVENTS_AND_TOGGLES.md. Public Workshop release remains pending real-game verification.
