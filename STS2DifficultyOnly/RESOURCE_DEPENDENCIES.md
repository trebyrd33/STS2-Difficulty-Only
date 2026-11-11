# Resource dependency audit

## Findings

The original PCK is unnecessary for the numerical systems. Both Enemy Damage Compensation and Increased Enemy Scaling are C# patches using vanilla game APIs and models. They require no MPX-owned scenes, images or sounds.

Double Boss and Enemy Reworks reference MPX-owned presentation resources. Missing scene data disables the original column behavior through HasScene and causes row-layout fallback. Therefore a missing original PCK cannot be treated as equivalent to the authored encounter. This build supplies replacement resources under its own loader ID and removes that silent row fallback.

## Boss scenes — exact original paths

| Original missing path | Purpose | Replacement |
|---|---|---|
| res://MultiplayerExpanded/scenes/encounters/double_boss.tscn | Control root with direct Marker2D children left/right; Aeonglass + Test Subject placement | res://STS2DifficultyOnly/scenes/encounters/double_boss.tscn |
| res://MultiplayerExpanded/scenes/encounters/double_boss_queen.tscn | Control root with direct Marker2D children left/amalgam/queen; both Queen combinations | res://STS2DifficultyOnly/scenes/encounters/double_boss_queen.tscn |

BaseLib.CustomEncounterModel.Slots reads direct Marker2D child names. NCombatRoom.CreateEnemyNodes loads the scene as Control and positions creatures at their named markers. The replacement scenes satisfy that exact contract. Coordinates are new: left=(400,740), right=(1520,740); Queen layout left=(400,740), amalgam=(1270,780), queen=(1580,660). The original C# maps monsters to those slots and preserves the side/targeting rules. Original coordinates were not recoverable from the C# and have not been claimed.

## Enemy rework UI — exact source-derived path families

- res://MultiplayerExpanded/images/intents/challenger_intent.png
- res://MultiplayerExpanded/images/intents/vengeful_intent.png
- res://MultiplayerExpanded/images/intents/dismissive_intent.png
- res://MultiplayerExpanded/images/intents/patronizing_intent.png
- res://MultiplayerExpanded/images/intents/targeted_intent.png

These were targeting overlays; they do not determine the selected player or attack damage. Replaced by self-contained GradientTexture2D resources at res://STS2DifficultyOnly/images/intents/<same-stem>.tres. Target-player hover text and class-color badges remain available.

Power UI requested res://MultiplayerExpanded/images/powers/<entry>.png and images/powers/big/<entry>.png, with power.png as fallback. Character-target badges requested ironclad.png, silent.png, defect.png, necrobinder.png and regent.png. The complete source-derived entry list is in original_resource_paths.json. Replacements use res://STS2DifficultyOnly/images/powers/<entry>.tres; packed and big icons share a resource. Powers which already used vanilla icons still use those vanilla game icons.

The seven approved encounter cards requested res://MultiplayerExpanded/images/card_portraits/<entry>.png and card_portraits/big/<entry>.png, plus optional card_portraits/beta/<entry>.png and card.png fallback. These were UI portraits, not implementations of the cards. All three portrait properties now map to their own generated card_portraits/<entry>.tres resources. Card effects remain in the original C#.

## Text

Original model IDs use the MULTIPLAYEREXPANDED- prefix. Original mod localization would normally be loaded from res://MultiplayerExpanded/localization/<language>/<table>.json, and the old beta overlay code referenced res://MultiplayerExpanded/localization_beta/<language>/<table>.json. The game asks for tables including powers.json, intents.json, encounters.json, cards.json, afflictions.json and settings_ui.json.

This build supplies newly written English tables for those six files under res://STS2DifficultyOnly/localization/eng/. Model IDs remain unchanged; config labels use STS2DIFFICULTYONLY-. A loader patch supplies these English tables as fallback for other languages. No old localization path or beta overlay is needed.

## Loader

The supplied ModManager.LoadMod implementation independently honors hasDll and hasPck; DLL-only mods are supported when has_pck is false. However this mod now uses two encounter scenes and support UI files, so its manifest correctly declares has_pck=true. Its own PCK is included and mounts successfully in Godot. A dummy or empty PCK would not satisfy the scene/UI dependencies.

No original-PCK resource path remains in the compiled source. Vanilla STS2 resource paths are provided by the installed game. See tests/results/ for native pack-load and coverage results.

## Additional approved curse support in 0.2.1-test1

MPX Sloth uses res://STS2DifficultyOnly/images/card_portraits/sloth.tres and its own MULTIPLAYEREXPANDED-SLOTH card strings. The existing Decay card uses an override description, with res://STS2DifficultyOnly/images/powers/decayed_power.tres and Decayed power strings. These are supplied by the replacement PCK and introduce no original-PCK dependency.

## 0.3.0 death-mode and remaining curse support

Gluttony, Envy, Neow's Clutch and Vogelfrei have replacement portraits under res://STS2DifficultyOnly/images/card_portraits/. Gluttony Hex uses the game's vanilla Hex icon plus local text. Revive All replaces the source-referenced res://MultiplayerExpanded/images/ui/rest_site/option_revive_all.png with res://STS2DifficultyOnly/images/ui/rest_site/option_revive_all.tres. Its OPTION_REVIVE_ALL labels are supplied in localization/eng/rest_site_ui.json. DeathMode labels are in settings_ui.json. No original MPX PCK is needed. Original Thigh Pillow relic integration and AllSelect custom-event integration are not included because those content models are excluded.

## 0.5.0 optional events

Replacement event portraits are in res://STS2DifficultyOnly/images/events/ (five base portraits and three act-specific chest portraits). Event reward card portraits, Immortal/Improvements power icons and A Lingering Something normal/outline relic icons are included under the matching images subfolders. New events.json/relics.json and extended cards.json/powers.json/settings_ui.json supply the text. All are declared in asset_manifest.json. No original PCK resource is required by restored events; standard layout and animations use base-game resources.
