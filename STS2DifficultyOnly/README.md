# STS2 Difficulty Only — 0.5.0-test1

Compiled local test build derived from Multiplayer Expanded v0.3.16 by Elec. Includes four independent difficulty toggles, the requested multiplayer curses except Loneliness, the existing Decay rework, and four selectable death modes. Its own small PCK is included; MultiplayerExpanded.pck is not required.

Actual STS2 loading/playability, co-op synchronization and stable/beta compatibility remain unverified. Use a fresh test run. The complete content inventory is ALL_FEATURES.md; detailed enemy changes are ENEMY_REWORKS.md; curse/death details are CURSES_AND_DEATH.md.

## Installation

1. Disable Multiplayer Expanded. The two DLLs must not be active together; initialization rejects that combination. Do not delete saves.
2. Install BaseLib >=3.4.7. The package includes the unchanged user-supplied BaseLib archive under dependencies/; its three files can be extracted into the game's mods/BaseLib/ folder if needed.
3. Copy the package's mods/STS2DifficultyOnly/ directory into the game's mods/ directory. STS2DifficultyOnly.dll, STS2DifficultyOnly.json and STS2DifficultyOnly.pck must be together.
4. Start the game, enable the mod in its loader and configure it through BaseLib before starting a fresh test run.
5. All co-op players need the same game branch, mod/dependency versions and DLL/PCK checksums. The host's settings govern the party.

Optional PowerShell installer: `./Install.ps1 -GameDirectory "C:\path\to\Slay the Spire 2"`. Add `-InstallBundledBaseLib` to install the local bundled dependency when absent. Existing mod files are backed up. The installer does not alter saves or enable mods automatically. A Workshop-installed dependency can instead be used with the manual install steps.

Do not copy the source project's sts2.dll, GodotSharp.dll or Harmony build references into the installed mod directory.

## Controls

| Control | Default |
|---|---|
| Act 3 Double Boss (A10+) / SurroundedByBosses | On |
| Enemy Reworks / EnemyReworks | On |
| Enemy Damage Compensation / EnemyDamageCompensation | Off |
| Increased Enemy Scaling / IncreasedEnemyScaling | Off |
| MPX Vanilla Card Reworks / VanillaCardReworks | On |
| MPX Potion Changes / MultiplayerPotionChanges | On |
| MPX Events / MultiplayerEvents | On |
| Multiplayer Death Mode / DeathMode | Vanilla |

The four difficulty toggles stay independent. DeathMode is an additional selector requested by the user, with Vanilla, VanillaPlus, EveryoneDies and StayDeadUntilMended. Custom death handling is co-op-only. The source preserves MPX's multiplayer activation for enemy reworks and its multiplayer curses; solo keeps normal enemy-rework/curse/death eligibility while still supporting A10+ simultaneous boss eligibility.

BaseLib writes STS2DifficultyOnly.cfg under the game user-data mod_configs directory, separate from MultiplayerExpanded.cfg. All seven booleans and the death enum synchronize from the host, validate atomically, lock at new-run launch and restore client preferences on disconnect. Host overrides are prevented from saving into clients' local config files. Saved-run/rejoin ordering still requires real game testing; match the intended settings on all peers before a resume test.

## Build

Use .NET SDK 9.0.300 or a newer 9.0 SDK: `dotnet build DifficultyOnly.csproj -c Release`. The four user-supplied references are in references/. No NuGet package dependencies. `python build.py` is the offline compiler fallback; set DOTNET_EXE if dotnet is not on PATH. Output: bin/Release/net9.0/STS2DifficultyOnly.dll.

Rebuild the PCK with Godot 4.5.1: `godot --headless --path assets --script /absolute/path/to/tools/pack_assets.gd -- /absolute/path/to/STS2DifficultyOnly.pck`. No editor binary is distributed. The packer includes only assets/STS2DifficultyOnly/ and no assemblies/scripts/editor project configuration.

Tests: `dotnet run --project tests/RulesTests.csproj`; `python tests/check_asset_coverage.py`. Run tests/verify_assets.gd from an empty Godot project with the built PCK's absolute path after `--` to test packed resources without loose source assets masking a missing file.

## Presentation and compatibility limits

The replacement PCK uses Godot pack format 3, built/tested with 4.5.1 matching the supplied GodotSharp version. Boss scenes, coordinates, icons, portraits and text are replacements; original slot names and C# encounter mechanics are retained. English fallback is supplied for other game languages. Older engines and both game branches have not been tested. The manifest minimum game version is inherited metadata, not a verified compatibility floor.

The complete test status and remaining real-game matrix are in TEST_RESULTS.md. Workshop materials remain a draft; nothing was uploaded.

## Vanilla card and potion multiplayer changes

This candidate includes the original 52 vanilla-card rework definitions and four card-generating potion changes. Their separate switches default to on. When enabled, they activate in co-op independently of the difficulty toggles; solo retains vanilla cards/potions. All peers must use this same version. See VANILLA_CARD_REWORKS.md for the complete inventory and pending gameplay checks.

## Events and switching content

The MPX Events option restores the five original events and their two event-only cards plus one event relic. See EVENTS_AND_TOGGLES.md. Configure all switches before starting a fresh run; they lock during the run. Off restores vanilla card/potion behavior or excludes MPX event appearances, respectively. Card reworks includes Decay and status reworks. The switches are independent, synchronized with the host, and all peers must update to 0.5.0-test1 because the network settings schema has changed. Already-earned event cards/relics remain functional when events are disabled for a subsequent run/resume.
