# Everything included in STS2 Difficulty Only — 0.5.0-test1

This is the complete content/feature inventory for the compiled local test package. It includes the scope expansions explicitly requested by the user: multiplayer curses except Loneliness, the Decay rework, and the four death modes. Implementation is source-audited; actual STS2 playability and branch/co-op compatibility remain unverified.

## 1. Four independent difficulty toggles

| Setting | Default | Included behavior |
|---|---|---|
| Act 3 Double Boss (A10+) | On | At A10+, combines the two final-act bosses into one simultaneous encounter. Preserves the Queen+Test Subject, Queen+Aeonglass and Test Subject+Aeonglass pairings, original C# monster lineups/attack mechanics, two-sided column logic, camera settings and victory handling. Supports solo eligibility as well as multiplayer. Uses reconstructed slot scenes. A9 or disabled uses normal boss progression. |
| Enemy Reworks | On | Original MPX targeting, moves, powers, party interactions, enemy-generated card choices and rotation restrictions. The original multiplayer activation is retained: reworks apply at more than one player. All 21 affected enemies and exact source-derived effects are detailed in ENEMY_REWORKS.md. |
| Enemy Damage Compensation | Off | 1–2 players: no bonus. 3 players: +10% base enemy attack damage. 4 players: +25%. Midpoints round away from zero before Strength and other normal modifiers. Command tracking prevents repeated compensation of the same attack. Generic and targeted intent previews use the corresponding base calculation. |
| Increased Enemy Scaling | Off | Three players: 1+2*(ActScaling-1). Four players: 1+3*(ActScaling-1). Original HP/Block/buff behavior and hatchling, in-progress spawn, Test Subject and Flutter exceptions are retained. |

No difficulty toggle requires another. Queen's special freeze behavior needs an actual column encounter; the basic double-boss column system still works with Enemy Reworks off.

## 2. All 21 registered enemy reworks

1. Bowlbug Rock — concentrated, seeded random-player Headbutt with party-count-scaled damage.
2. Ceremonial Beast — Stomp/Crush add Ringing to the highest-damage player.
3. Eye With Teeth — party-count-scaled Dazed focused on the highest-damage player.
4. Gas Bomb — assigned-player targeted Death Blow, then self-destruction.
5. Globe Head — Galvanized card plays charge the enemy with Setup Strike instead of immediately damaging the player.
6. Infested Prism — party-aware Vital Spark/Tainted skills and Energy reward for highest-Tainted player(s).
7. Kin Priest — Taking Turns added after Orb of Frailty.
8. Knowledge Demon — shared curse-choice drafting, six extra approved choices, delayed Disintegration timing.
9. Living Fog — one player-assigned bomb per living player, subject to available slots.
10. Parafright — starts with 75% of its otherwise-applicable HP, rounded up.
11. Punch Construct — Counterpunch and Countered on fully blocked attacks.
12. Soul Nexus — highest-Block Soul Burn; split Drain Life damage/debuff targeting; revised Debilitate timing.
13. The Forgotten — focuses party-count-scaled Dexterity theft on the highest-Block player.
14. The Lost — focuses party-count-scaled Strength theft on the highest-damage player.
15. The Obscura — Paranoia lets its Parafright punish players who damage Obscura.
16. The Insatiable — four-count Sandpit, Frantic Escape/Frantic Pull distribution, player/pet movement and escalating escape costs.
17. Vantom — Slippery restoration after Prepare.
18. Waterfall Giant — explosion overkill can carry into another explosion against surviving players.
19. Aeonglass — one shared six-card Withering Presence counter across the party.
20. Test Subject — tracks player contributions to Enrage while keeping its normal Strength gain.
21. Queen — party-wide effects and Frozen in Place within column encounters, plus amalgam-death transition handling.

## 3. Targeting, rotation and encounter support

- Challenger targets the highest-Block living player; Vengeful targets the highest-damage living player from the current player turn; Patronizing affects everyone except that player. Chosen targets use a monster's assigned/seeded choice.
- Seeded tie-breaking and target badges/hover text are included; pet damage is attributed to its owner.
- Taking Turns prevents consecutive manual plays by an affected player; it does not require a fixed full-party order and ignores automatic plays for its alternation test.
- Cold Feet additionally requires a matching card type after another player's play.
- Double-boss column facing changes with enemy-targeting cards/potions. Each column has ceil(P/2) capacity; the original eligible-occupant swap rule is retained. Enemy targeting falls back to surviving players if its own column is empty.
- Queen's Frozen in Place restricts column changes and cross-column Attack targets. Pet/Osty visuals follow owners.
- Original Knowledge Demon shared drafting, contention resolution and loser repicks are included as supporting UI; no new event model is registered.
- Boss victory music/Queen-amalgam cleanup support prevents premature encounter completion behavior. Only existing game boss music is used; no menu music replacement is present.

## 4. Seven approved enemy encounter cards

| Card | Used by / effect |
|---|---|
| Haze | Knowledge Demon choice; adds Dazed to the owner's draw pile at turn end. |
| Hoarding | Knowledge Demon choice; prevents manual plays with two or fewer cards remaining in hand. |
| Frustration | Knowledge Demon choice; the party takes damage based on the holder's remaining hand. |
| Inner World | Knowledge Demon choice; restricts player-targeting cards/teammate targeting. |
| Cold Feet | Knowledge Demon choice; alternation and card-type-matching restriction. |
| Martyrdom | Knowledge Demon choice; the owner's cards cannot grant Block to that owner. |
| Frantic Pull | Insatiable-generated card; increases a chosen player's Sandpit countdown; cost increases after play. |

The six Knowledge Demon additions are displayed choices that apply powers. Existing game cards/statuses such as Disintegration, Mind Rot, Sloth, Waste Away, Dazed, Wither, Burn and Frantic Escape are still used where the retained enemies use them. Their vanilla models are not re-added as custom cards.

## 5. Five restored multiplayer curses, excluding Loneliness

- **Sloth:** playable Dexterity trade-off, declining permanent gain, party Dexterity penalty if held; Ethereal/Exhaust.
- **Envy:** Strength counterpart to Sloth, including declining permanent gain and the held-card party penalty; Ethereal/Exhaust.
- **Gluttony:** held-card party Hex effect, with Hexed/Ethereal reconciliation and existing-Hex compatibility.
- **Vogelfrei:** Retain/Exhaust; protects its holder from being targeted by other players' cards while held.
- **Neow's Clutch:** Unplayable/Eternal, Bing Bong-exempt, special-acquisition curse granted by Vanilla+ death mode; excluded from ordinary random pools.

These retain original multiplayer card eligibility. Sloth, Envy, Gluttony and Vogelfrei use the multiplayer Curse pool; Neow's Clutch keeps its special source. **Loneliness is absent.** There are 12 custom card models total: these five curses plus the seven encounter cards.

## 6. Existing Decay rework

In co-op, Decay applies Decayed when left in hand at turn end. Damage occurs at enemy-side start and the power removes itself. The normal damage is 2; multiple amounts stack through the original delayed-damage power. No duplicate Decay card model is added. Decay uses the restored multiplayer rework registry. Solo keeps normal Decay.

## 7. Four multiplayer death choices

| Choice | Included behavior |
|---|---|
| Vanilla (default) | Normal STS2 death/revival; no custom death punishment. |
| Vanilla+ | Normal revival, followed by Neow's Clutch for the player who was dead. |
| Everyone Dies | One actual player death causes the remaining living party to die. |
| Stay Dead Until Mended | Dead players stay down outside the original boss-room exception. Living players can spend a rest-site action on Revive All using normal Mend healing amounts. |

The custom modes are co-op-only and mutually exclusive. Selecting one does not affect solo death handling. Their supporting patches handle dead-player event votes, event-combat entry, rewards and rest-site completion in Stay Dead mode; Vanilla remains isolated from that support logic. The optional Thigh Pillow relic integration is removed because no such relic is included.

## 8. Configuration and synchronization

- Four boolean difficulty controls, three optional-content switches and one death-mode dropdown in BaseLib; no unique-character-selection, music or desync-reporting option.
- Host-authoritative reliable buffered synchronization of all eight values, with exact schema and enum validation before applying any override.
- Non-host config messages ignored; client personal preferences protected from host-override saves and restored on disconnect.
- Settings lock at new-run launch and unlock at cleanup. Actual saved-run resume/rejoin behavior remains in the required local test matrix.
- Separate STS2DifficultyOnly.cfg under the game's user-data mod_configs directory.

## 9. Standalone replacement assets and dependencies

- Included DLL, loader JSON manifest and minimal replacement PCK; no original MultiplayerExpanded.pck dependency.
- Reconstructed Control/Marker2D boss scenes with original slot names; original C# camera scale/offset and mechanics retained. Exact original artwork/coordinates are not claimed.
- Procedural placeholder textures for icons, intent overlays, portraits and Revive All; no imported-image cache dependency.
- Newly written English tables for settings, powers, intents, cards, encounters, afflictions and rest-site UI, with English fallback for other languages.
- BaseLib >=3.4.7 required. The unchanged user-supplied BaseLib archive is available for local installation; game/Godot/Harmony reference DLLs are development inputs only.
- Editable C# project, assets, native pack builder, pure rule/schema tests, resource tests, coverage checks, install instructions and Workshop preparation draft.
- Attribution to Elec / Multiplayer Expanded v0.3.16 and the user-reported permission to modify/redistribute retained.

## 10. Explicitly excluded

Loneliness; BFFs Bracelet and all other added relics except A Lingering Something; added characters; unrelated events; unrelated new player cards; unique-class selection; custom menu music; desync-report uploading; Shirk (a new player card) and its redirection integration. Intercept is restored as a vanilla-card rework. No Workshop upload has been performed.

## Verification status

The DLL compiles; 26,784 pure numerical/schema/death-mode activation assertions passed; 250 native Godot resource checks passed; 912 source/resource coverage checks passed. Metadata found 94 directly declared patch targets in the supplied beta-shaped game assembly; 12 dynamic target classes remain runtime-unverified. Compiled inventory: 14 custom cards, 5 events, 1 event relic and no added characters or potion models.

STs2 loading, combat, deaths/revival, network timing, save/load and both stable/beta branches have NOT been tested in a running game here. This is a locally installable test candidate, not a verified playable/public release. See TEST_RESULTS.md for the exact missing-runtime blocker and required test checklist.

## Restored vanilla card and potion multiplayer changes

All 52 original vanilla-card rework definitions are included (42 shared, 9 beta-only, 1 stable-only). When enabled, they activate in co-op independently of the four difficulty switches; solo uses vanilla behavior. The multiplayer card-library checkbox previews the changes. The MPX Vanilla Card Reworks switch controls all of these, including Decay and status cards. It defaults to on. See VANILLA_CARD_REWORKS.md for every card, branch, effect and supporting patch.

Attack, Skill, Power and Colorless Potions can target an ally, who chooses among three cards generated from the appropriate unlocked pool; the chosen card is free this turn and added to that ally’s hand. The potion-bar shrink/reconciliation fix is also included. No new potion models or potion-slot relics are registered.

## Optional multiplayer events

The MPX Events switch defaults to on and controls eligibility of An Empty Chest, Fading Memories, Guardian Angel, Hammering Lesson and Labyrinth of Mirrors. It works independently of card reworks, potions and difficulty. The original event selection restrictions and independent-choice contention/reprompt support are retained.

Event-only rewards are included: Immortal (one-use deck-removing ally death protection), Improvements (permanent random deck upgrades after combat), and A Lingering Something (+1 maximum Energy and +1 card draw). These are the only added player reward cards/relic in this expansion. They remain registered for save compatibility when the event switch is off; the switch excludes new event appearances, and does not remove already-owned rewards. See EVENTS_AND_TOGGLES.md for the complete event list and pending checks.

The MPX Potion Changes switch defaults to on. Turning it off restores vanilla card-potion use/targeting and disables MPX potion-bar reconciliation. It does not depend on the card switch. All seven booleans plus DeathMode synchronize and lock for a run.
