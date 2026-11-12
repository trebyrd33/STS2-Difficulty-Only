# Events and optional content — 0.5.0-test2

Three independent switches are added alongside the original four difficulty settings and default-Vanilla death-mode selector. All three default to On, synchronize from the host with the same validated snapshot, and lock when a run launches. Change them before a new run. Every peer must use this build; earlier settings payloads are rejected rather than partially applied.

| Switch | On | Off |
|---|---|---|
| MPX Vanilla Card Reworks | All branch-eligible original definitions (43 stable / 51 beta), including Decay/statuses, All for One pool restriction and supporting patches | Vanilla card behavior and text; no MPX card-library preview. Event reward cards remain independent. |
| MPX Potion Changes | Ally targeting/recipient choices for Attack, Skill, Power and Colorless Potions; potion-bar reconciliation | Vanilla use/targeting; MPX potion-bar patch inactive. |
| MPX Events | Five original events may be selected under their original eligibility rules | No new MPX event selection; no removal of rewards already in saves. |

Card/potion effects remain co-op-only. Events retain their original requirement of more than one player. These switches do not depend on Enemy Reworks or any other difficulty option.

## Restored events

| Event | Original behavior retained |
|---|---|
| An Empty Chest | Each living player offers one unenchanted Common/Uncommon/Rare card; shared chest selection distributes offered cards. Requires at least two living players with two eligible cards each, a prior other event, and no visited Colorful Philosophers. Act-specific replacement portraits. |
| Fading Memories | Shared choice of one player's randomly offered removable non-Basic Attack/Skill. That player loses the offered card; other players lose a removable Basic Strike/Defend matching the type. Original party/dead-seat step support retained. |
| Guardian Angel | Hive/Glory. Players choose independently: Pray competes for Immortal; Loot offers a three-card Attack reward. Original sole-winner contention and reprompts retained. Excludes runs that visited Hammering Lesson. |
| Hammering Lesson | Hive/Glory. Players choose independently: Learn competes for Improvements; Search offers a three-card Skill reward. Up to five contention rounds, with original resolution. Excludes runs that visited Guardian Angel. |
| Labyrinth of Mirrors | Glory. Smash Through loses 15% maximum HP and grants 100–130 Gold. Hold Hands heals 5% maximum HP, awards A Lingering Something, and redeals living players' removable cards using event RNG while preserving each player's movable-card count and keeping nonremovable cards in place. |

## Necessary event rewards

- **Immortal:** event-only Skill, cost 1, targets another ally. Protects from the next death by healing 50% maximum HP (100% upgraded), with a combined protection cap of 100%. Removes its deck copy when played; Exhaust. Original Ethereal keyword is removed by its upgrade.
- **Improvements:** event-only Skill, cost 3 (2 upgraded), targets another ally; Exhaust. Gives one Improvements stack. After combat, each stack upgrades a random upgradable deck card without choosing the same card twice in that resolution. Power persists through death.
- **A Lingering Something:** event relic; +1 maximum Energy and +1 card draw for its owner.

The two cards cannot be generated in combat and use EventCardPool. The relic uses EventRelicPool. They and the event classes remain registered for serialization even with events disabled; turning the switch off prevents event eligibility rather than removing save model definitions. No other new player cards, relics, characters or potion models are added. Loneliness remains excluded.

## Assets and verification

The original MPX PCK is not required. Event portraits and reward icons are replacement gradient textures; event narrative/option text is newly authored from the retained effects. Standard event layouts are used; original artwork and prose are not reproduced. The independent-choice synchronizer and card-selection/chest support are retained from source.

Compilation, pure settings tests, static coverage and native Godot resource checks passed. No actual STS2 event, combat, co-op, save/load or stable/beta test has run here. See TEST_RESULTS.md for the runtime blocker and test matrix.
