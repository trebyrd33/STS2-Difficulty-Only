# Verification — 0.5.0-test1

## Executed

- Real .NET 9 DLL compiled against supplied STS2, GodotSharp, Harmony and BaseLib references using SDK 9.0.321. Assembly version 0.5.0.0. No compile errors; the log retains framework-unification and nullable warnings.
- 26,784 pure C# assertions passed: all 128 difficulty/content masks at 1–4 players across all four death modes; A9/A10/A11; expected scaling, base-damage rounding/order contract; exact eight-field schema with enum validation; duplicate/unknown/missing/wrong-typed fields; Vanilla/solo death-mode isolation. These do not simulate actual combat/network transport.
- Native Godot 4.5.1: replacement PCK built and mounted from an empty project; 250 resource/scene/JSON/texture checks passed, 0 failed. Correct Control/Marker2D scene structure/coordinates and all packed resources load.
- 912 source/resource coverage checks passed, including all retained card/power/intent resources, no old PCK paths, no Loneliness model/asset/text, default Vanilla and mode-scoped dead-seat support.
- Compiled metadata: 94 directly declared beta patch targets found, 0 missing; 12 dynamic target classes remain unexecuted. Target existence does not prove Harmony installation or transpiler correctness.
- Compiled model inventory: 14 custom cards (7 encounter cards + 5 multiplayer curses + 2 event rewards), 5 original events, 1 event relic and no added characters or potion models. Existing Decay is patched without a new card model. One custom rest-site action is included for the requested Stay Dead mode. Twenty-one original enemy rework classes remain.
- Original Gluttony IL was inspected to repair the decompiler's enum range comparison: phases 4/5 use an unsigned range test, preserved in the corrected C#.

- 351 additional static rework checks passed: exact original 52-class inventory, branch guards (42 shared / 9 beta-only / 1 stable-only), all replacement descriptions, getter patch targets, Burn/Midnight lifecycle hooks, no duplicate Decay patches, and potion recipient/gating wiring. These checks do not execute card/potion effects.
- All 52 card rework classes and the four potion OnUse prefixes compile. Dynamic card/potion Harmony installation remains unexecuted.

## Runtime blocker / no game compatibility claim

The full STS2 game is not installed here. An earlier real Harmony application attempt against the provided sts2.dll failed at module initialization with `System.TypeInitializationException: The type initializer for '<Module>' threw an exception.` Inner cause: missing `Sentry.Godot, Version=1.0.0.0` at SentryAutoInit.Init(). Other runtime assemblies, executable and base-game assets are absent. The standard Godot resource tests do not load the STS2 managed game or exercise its gameplay hooks.

The original MPX PCK is no longer required: mod-owned scenes/icons/text are included in the replacement PCK. Only one STS2 reference assembly was supplied; it matches the original beta heuristic. No stable or beta running-game compatibility has been verified.

## Local test checklist — all NOT RUN

Record game/branch version, platform, BaseLib version, mod checksums and host/client logs.

| Test | Required result | Status |
|---|---|---|
| Loader and settings UI | DLL/PCK load; seven toggles + death dropdown; no failed patches/resources | NOT RUN |
| All difficulty options off | Vanilla encounters/damage/scaling at 1/2/3/4P | NOT RUN |
| Boss only, A9 vs A10+, solo/2/3/4P | Normal A9; all 3 pairs simultaneous at A10+, correct targets, swaps, rewards and victory | NOT RUN |
| Enemy reworks only | All 21 source-listed changes, targeting, previews, rotation and powers | NOT RUN |
| Enemy encounter cards | All 7 approved models and vanilla status interactions | NOT RUN |
| Damage only | +0/+0/+10/+25% base, midpoint-away rounding before normal modifiers, no double application | NOT RUN |
| Scaling only | HP/Block/buffs, original spawn/hatchling/Test Subject/Flutter exceptions | NOT RUN |
| All remaining difficulty/content masks | No dependency between the four toggles | NOT RUN |
| Sloth and Envy | Correct targets/penalties; declining saved gains; Ethereal/Exhaust; save/load | NOT RUN |
| Gluttony | Hand/pile/turn reconciliation; Hexed/Ethereal; no conflict with vanilla Hex/other afflictions | NOT RUN |
| Vogelfrei | Other-player card targeting blocked only while held; Retain/Exhaust | NOT RUN |
| Neow's Clutch | Special pool exclusion, Eternal/Bing Bong exemption, Vanilla+ award | NOT RUN |
| Decay | Original delayed damage/stacking/timing in co-op; normal solo behavior | NOT RUN |
| Vanilla death | Normal revival; no custom punishment or dead-seat/rest-site changes | NOT RUN |
| Vanilla+ death | Normal revival then Neow's Clutch, once per applicable revival | NOT RUN |
| Everyone Dies | Actual player death wipes survivors; no recursive wipe or abandoned-run issue | NOT RUN |
| Stay Dead | Dead players stay down outside original boss-room exception; rewards/events/event combats/rest-site don't block | NOT RUN |
| Revive All | One living player's rest action revives all dead peers with normal Mend amounts; no removed-relic bonus | NOT RUN |
| Solo death | No custom mode affects normal solo death | NOT RUN |
| Co-op configuration | Host snapshot before gameplay; client edits/saves protected; join/rejoin/disconnect and all mode values | NOT RUN |
| Save/load/resume | Same modes/flags/models/dead state on all peers; no desync or stuck events | NOT RUN |
| Presentation | Rebuilt boss placement at 4P; readable icons, portraits, UI and death option text | NOT RUN |
| Stable and beta | Repeat relevant matrix independently on each branch | NOT RUN |

This is an asset-complete, locally installable test candidate. It is not a verified playable/public release.

## Added card/potion gameplay checks — NOT RUN

See VANILLA_CARD_REWORKS.md. Verify all branch-eligible base/upgraded reworks; Infernal Blade owner pool vs recipient; Delay 11/13 Block and 1/2 next-turn Energy; all four potion choices and recipient ownership; solo vanilla behavior; Burn party totals/timing; Midnight all-player exhaustion; Intercept direct/excluded/column targeting and previews; Royalties whole-number shares; compendium preview; save/resume and host/client determinism.

## New optional-content verification

- 26,784 pure assertions passed after recompiling the test executable: all 128 seven-boolean masks at 1–4 players and all 4 death modes; eight-field schema roundtrips and missing/wrong-typed/duplicate optional fields. These prove numerical/schema behavior, not game/network hook execution.
- 85 additional static checks passed for independent card/potion gates, preview and All for One handling, event eligibility gating, exact five-event inventory, event-only card pools, one event relic, and required localization/portraits.
- 351 rework checks and 912 asset/source checks passed. Native Godot 4.5.1 mounted the PCK from an empty project: 250 checks, zero failures.
- Compiled metadata audit: 94 declared beta targets found, zero missing, 12 dynamic target classes unexecuted.
- Running-game tests remain NOT RUN: card-off/potion-on and card-on/potion-off; all three switches off; Decay/status behavior; previews after toggling; host/client setting lock/save/restore; all five events and choices at 2/3/4 players; collision/reprompt/disconnect/dead-seat behavior; shared chest ownership; mirror redistribution preserving upgrades/enchantments/eternal cards; event reward cards/relics; save/load and both branches.
