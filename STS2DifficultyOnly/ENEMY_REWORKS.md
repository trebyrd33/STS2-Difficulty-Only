# Enemy Reworks — complete retained change list

This describes the MPX v0.3.16 behavior retained in STS2 Difficulty Only, compared with the supplied vanilla game DLL. It is a source audit, not an in-game verification or a claim about an unprovided stable build. The option activates in parties with more than one player; solo follows MPX's original solo behavior. P means the number of players. Numerical attack amounts below are before normal modifiers and the independently toggled Enemy Damage Compensation.

## Shared intent targeting

- **Challenger:** chooses the living player with the most Block.
- **Vengeful:** chooses the living player with the most recorded damage dealt to enemies during the current player turn. Damage by pets is credited to their owner.
- **Patronizing:** affects everyone except that highest-damage player. Soul Nexus uses this for its attack while applying debuffs to the excluded player.
- **Chosen/assigned:** the monster selects a specific player; Bowlbug Rock uses a seeded round-specific random selection, while Gas Bomb uses its assigned player.
- Ties use seeded, encounter/round/monster-specific selection so every peer can derive the same result. Invalid/dead selected targets fall back to a living player. Actual network agreement still requires a game test.
- Targeted attacks, debuffs and status intents receive matching hover text/target previews and target badges. The standalone test pack supplies replacement support visuals.
- Dismissive and related targeting helpers remain in supporting code, but no current retained monster move directly creates a Dismissive intent; it is not an additional enemy change.

## All 21 registered enemy reworks

| Enemy | Retained changes |
|---|---|
| **Bowlbug Rock** | Headbutt focuses one seeded random living player for the round instead of attacking the party. Base damage is 12×P, or 13×P at A9+, instead of the supplied game's 15/16 per player. Off Balance → Dizzy/stun behavior and move ordering are retained. |
| **Ceremonial Beast** | Stomp and Crush additionally apply 1 Ringing to the highest-damage player. Their original attacks remain; the targeted Ringing is added afterward. Existing Stamp/Plow/stun/Beast Cry sequence remains. |
| **Eye With Teeth** | Distract puts 3×P Dazed into the highest-damage player's discard pile, concentrating the party's status burden onto that player. Repeats Distract as before. |
| **Gas Bomb** | Explosion uses a targeted Death Blow intent and attacks its assigned player, then the bomb dies. Its marker identifies the intended player. |
| **Globe Head** | Playing a Galvanized card gives the enemy Setup Strike equal to its Galvanic amount, replacing the supplied game's immediate damage to the card's player. Reworked card text and power/affliction hover support accompany it. |
| **Infested Prism** | Vital Spark becomes a party-aware power. Skills receive Tainted; playing an affected skill gives its owner Tainted. At the end of the player side, living players tied for the highest positive Tainted amount gain 1 Energy next turn. New skills and changed power amounts are handled; removing the power cleans up Tainted afflictions. |
| **Kin Priest** | After Orb of Frailty's normal attack/Frail effect, affected living players gain Taking Turns. An affected player cannot make consecutive manual card plays; automatically played cards do not count for this alternation rule. There is no fixed required player order. |
| **Knowledge Demon** | Each of the three curse-choice rounds offers the two original choices plus two approved MPX choices. Players draft through the original shared choice/Rock-Paper-Scissors contention system; losers can repick remaining choices. Disintegration damage is moved from the ordinary turn-end handler to the start of the enemy side. Slap/Knowledge Overwhelming/Ponder cycle and three choice rounds remain. Full choice/effect list follows below. |
| **Living Fog** | Bloat tries to summon one Gas Bomb per living player, stopping when no encounter slot is available. Each bomb is assigned to that player and receives fixed 10 HP, or 12 at A8+. Bloat's attack and the Advanced Gas → Bloat → Super Gas Blast cycle remain. |
| **Parafright** | On entering the room, max/current HP are set to ceil(75% of the HP it otherwise has). This happens under Enemy Reworks independently of Increased Enemy Scaling. |
| **Punch Construct** | Gains Counterpunch. When its Block completely stops an attack without breaking, the attacking player (or the attacking pet's owner) receives Countered. That player takes 50% more powered attack damage from enemies with Counterpunch through the enemy side; Countered is removed afterward. |
| **Soul Nexus** | Soul Burn targets the highest-Block player for 18×P base damage, or 24×P at A9+, instead of the supplied game's 29/31 party-wide attack. Drain Life attacks everyone except the highest-damage player for 25 base damage, or 27 at A9+. That excluded player receives Weak and Vulnerable: 2 stacks at 2P, 3 at 3P+. At A9+, also receives Debilitate: 1 at 2P, 2 at 3P+. Maelstrom and random non-repeating move selection remain. Player Debilitate ticks down at the end of the enemy side under the rework hook. |
| **The Forgotten** | Miasma applies -P Dexterity to the highest-Block player and grants +P Dexterity to the enemy, instead of the supplied game's -2 to its ordinary targets/+2 to itself. The existing 8-base-Block gain and Miasma/Dread alternation remain. |
| **The Lost** | Debilitating Smog applies -P Strength to the highest-damage player and grants +P Strength to the enemy, replacing the supplied game's ordinary-target -2/+2 behavior. Eye Lasers and its alternating move pattern remain. |
| **The Obscura** | Gains Paranoia. A player who damages Obscura during the player turn takes 50% more powered attack damage from Parafright. This is an interaction with Obscura's summoned Parafright, not a bonus to Obscura's own attacks. |
| **The Insatiable** | Liquify applies the MPX Sandpit countdown of 4. Each affected player's draw pile and discard pile each receive 2 Frantic Escape + 1 Frantic Pull (6 cards total). Sandpit decreases at enemy-side start; reaching zero triggers the original MPX kill sequence. Artifact does not block this Sandpit. Frantic Escape increases your countdown by 1; Frantic Pull increases a chosen player's countdown by 1. Each gains +1 Energy cost for the combat after being played. Player/pet positioning follows the countdown and updates as pets enter/revive. Existing Thrash/Bite/Salivate sequence remains. |
| **Vantom** | After Prepare, restores 2 Slippery if it has none; otherwise adds P Slippery. The normal Prepare move executes first. |
| **Waterfall Giant** | Steam Eruption/Explode can survive a lethal hit against player(s): if surviving players remain and total overkill against killed players is positive, the next explosion's damage becomes that overkill total. Otherwise the Giant dies normally after exploding. |
| **Aeonglass** | Replaces individual Withering Presence applications with one party-wide counter. Every sixth player card across the team gives a Wither to the player who played that sixth card, then resets to 6. The normal individual counter is suppressed. The boss's other moves and existing Wither upgrades remain. |
| **Test Subject** | Keeps normal Enrage Strength gain but records which player contributed it by playing a Skill. Contributor-specific tracker powers/icons expose that attribution. This does not introduce a new attack or replace the base Enrage amount. |
| **Queen** | Extra moves apply only in a column encounter, such as simultaneous double bosses. Puppet Strings and You're Mine affect the living party rather than one column. The first support opportunity, then each third support opportunity, uses Frozen in Place in place of Burn Bright. After the amalgam dies, Enrage also freezes the party. Frozen players cannot rotate columns or attack enemies in the other column, and other players cannot swap a frozen occupant out. If the amalgam dies while Frozen in Place is queued, Queen changes immediately to Enrage. Existing attack phase is retained. |

## Knowledge Demon's approved choices

The six MPX additions are choice cards that apply powers when selected; they are not ordinary player reward cards.

| Choice round | Existing game choices | Added MPX choices |
|---|---|---|
| First | Disintegration, Mind Rot | Haze, Hoarding |
| Second | Disintegration, Sloth (the game's debuff choice) | Frustration, Inner World |
| Third | Disintegration, Waste Away | Cold Feet, Martyrdom |

| MPX choice | Effect retained from source |
|---|---|
| **Haze** | At your turn end, adds 1 Dazed to your draw pile. |
| **Hoarding** | Prevents manually playing cards from hand when you have two or fewer cards left. |
| **Frustration** | At your turn end, every living player takes 2 damage for each card left in your hand. |
| **Inner World** | Restricts player-targeting cards and prevents targeting another player. |
| **Cold Feet** | Prevents consecutive manual plays by you; after another player's play, your next manual card must match its type. Automatic plays bypass the permission check. |
| **Martyrdom** | Your own cards cannot grant Block to you. Block from other sources/players is not suppressed by that check. |

## Character rotation / facing

Two separate mechanics are present:

1. **Taking Turns / Cold Feet:** restrictions on which player may play a card next; they are not a mandatory 1→2→3→4 order.
2. **Column facing in double-boss encounters:** targeting an enemy with a card/potion changes your column. Each side has ceil(P/2) capacity; entering a full side displaces its longest-standing eligible occupant. Enemy attacks normally hit that enemy's column and fall back to surviving players when its own column is empty. Queen's Frozen in Place restricts these swaps and cross-column Attack targets. Pet/Osty visuals follow their owners.

Basic column/facing support is part of the independent double-boss system and still works with Enemy Reworks off. Queen's extra freeze behavior requires Enemy Reworks.

## Additional restored multiplayer curses and death modes

The enemy catalogue above is complete for the 21 registered reworks. Separately, the user approved all original multiplayer curses except Loneliness: Sloth, Envy, Gluttony, Vogelfrei and Neow's Clutch. The original Decay rework is active in co-op. The four original death choices are restored, with Vanilla as default. Full curse/death effects and activation rules are listed in CURSES_AND_DEATH.md and ALL_FEATURES.md. The original game Sloth debuff choice remains distinct from the playable MPX Sloth curse.

## Separate settings and broad hooks

The optional +10%/+25% Enemy Damage Compensation and Increased Enemy Scaling formulas remain separate from Enemy Reworks. The P multipliers/rework-specific HP changes listed above belong to Enemy Reworks itself and do not require those options. A10 simultaneous boss replacement is another independent option.

Some original hooks operate on shared powers rather than checking a specific monster's type: Enrage contribution tracking, player Debilitate timing, Disintegration timing and Galvanic card handling can affect other holders of those powers in a multiplayer combat. The registered monster list above is therefore the concrete rework registry, not a promise that those power hooks can never affect another enemy/mod.

All of this remains subject to actual STS2 testing, including stable/beta differences and multiplayer synchronization.
