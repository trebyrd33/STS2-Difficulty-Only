# Multiplayer curses and death modes — 0.5.0-test2

Restored at the user's explicit request. The original four difficulty toggles remain independent; death behavior is one additional selector. Default: Vanilla. No Loneliness card or supporting relic has been restored.

## All included multiplayer curses

| Curse | Original behavior retained |
|---|---|
| Sloth | Costs 1; Ethereal and Exhaust. Gives the selected player -3 Dexterity and its owner initially +5 Dexterity. The owner's gain permanently decreases by 1 after each play, down to zero, including its saved deck version. If left in hand at turn end, every living player loses 1 Dexterity. |
| Envy | Costs 1; Ethereal and Exhaust. Gives the selected player -3 Strength and its owner initially +5 Strength. The owner's gain permanently decreases by 1 after each play, down to zero, including its saved deck version. If left in hand at turn end, every living player loses 1 Strength. |
| Gluttony | Unplayable. While a player holds Gluttony, its Gluttony Hex affects the party. Unafflicted combat cards receive Hexed; affected Hexed cards gain Ethereal. Reconciles on pile/turn changes and removes its hex when no Gluttony remains held. Preserves interaction with an existing vanilla Hex power and already-afflicted cards. |
| Vogelfrei | Costs 2; Retain and Exhaust. While held, other players cannot target its holder with cards. The card can be played/exhausted to remove that restriction. |
| Neow's Clutch | Unplayable and Eternal; no upgrade. Exempt from Bing Bong's card-change effect. Preserves its original special-acquisition restriction, so it is excluded from normal random curse pools. Vanilla+ death mode grants it after a dead player completes normal revival. |

Sloth, Envy, Gluttony and Vogelfrei use the original multiplayer Curse pool registration/constraint. Neow's Clutch is registered for explicit granting but remains special-acquisition-only. The original game Sloth status choice used by Knowledge Demon is a different card; it is not an extra newly registered curse or a duplicate of the MPX playable curse.

**Excluded:** Loneliness. BFFs Bracelet, which originally granted it, remains excluded along with other MPX relics.

## Decay rework

Decay remains the existing STS2 card, not another custom card model. In co-op, ending the turn with Decay in hand applies Decayed for its damage amount (normally 2). At enemy-side start, Decayed deals the accumulated damage to its owner and removes itself. Its original cost, Unplayable keyword, rarity and damage variable already match the supplied vanilla card. The change therefore restores the original MPX turn-end effect and delayed-damage power without enabling unrelated card reworks. Solo retains normal Decay behavior.

## Death selector — all four modes

| Mode | Behavior |
|---|---|
| Vanilla (default) | Normal STS2 death and revival behavior. Custom punishment, stay-dead event handling and rest-site revival logic are inactive. |
| Vanilla+ | Normal revival, followed by a Neow's Clutch curse for the player who was dead. The original hook awards it after ReviveBeforeCombatEnd completes. |
| Everyone Dies | After an actual player death, kills remaining living players. Preserves the original re-entry guard and abandoned-run handling. |
| Stay Dead Until Mended | Suppresses normal between-combat revival outside the original boss-room exception. A living player can spend their rest-site action on Revive All, healing dead teammates by their normal Mend amounts. Dead players cannot take rest-site actions and are handled by the original reward/event synchronization support. |

All custom death modes require a multiplayer run with more than one player. Selecting a non-Vanilla mode has no death-rule effect in solo. The original Stay Dead boss-room exception is retained: normal boss-room completion can revive players. This is not a newly invented permadeath mode.

## Supporting behavior

- The host synchronizes four difficulty booleans and the selected death enum (0–3). Invalid enum values, duplicate fields, missing fields and unknown fields are rejected before any override is applied.
- Clients accept configuration only from the host; their local preferences are restored on disconnect and host overrides are prevented from saving into their config file.
- Settings lock when a new run launches. The death selector is not coupled to any difficulty toggle.
- Stay Dead's supporting patches handle dead seats in shared-event voting, event combats, reward auto-proceed and rest-site completion so a dead player need not block living teammates. They are scoped to Stay Dead Until Mended, leaving Vanilla/Vanilla+/Everyone Dies event behavior untouched.
- Revive All has its own replacement icon and English text in the supplied PCK. It uses normal Mend healing; the optional original Thigh Pillow relic bonus is absent because that relic is not part of this mod.
- Neow's Clutch keeps its original random-pool exclusion and Bing Bong exemption. No new Bing Bong relic or event is added.

## Verification limits

These effects are implemented and compile, but have not been tested in a running STS2 installation. Revival timing, curse interactions, save/load, event voting, rest-site synchronization and stable/beta compatibility require actual local/co-op tests. The replacement PCK is self-contained; the original MultiplayerExpanded.pck is not required.
