# Vanilla card multiplayer reworks — 0.5.0-test1

All 52 original CardRework definitions are retained: 42 shared, 9 beta-only, 1 stable-only. This yields 43 eligible reworks on stable and 51 on beta. Branch selection follows the original source. These are changes to existing vanilla models; they do not add collectible card models. Effects activate in co-op while MPX Vanilla Card Reworks is enabled, independently of the four difficulty toggles. Solo keeps vanilla cards. The card library multiplayer checkbox previews reworks. Actual gameplay and branch compatibility remain unverified.

Original mechanics, costs, targets, variables, upgrades, save/load rebaking and party calculations are retained. English descriptions are replacement text because the original MPX PCK is unavailable. Braced names below refer to game-calculated variables; see editable source for exact base values and upgrade deltas.

| Group | Vanilla card | Branch | Base effect | Upgrade effect |
|---|---|---|---|---|
| Colorless | Beckon | Both branches | At the end of your turn, gain Beckoned for {HpLoss} HP loss at the start of the enemy turn. | Original numeric/cost/keyword upgrade retained; see source. |
| Colorless | Burn | Both branches | Unplayable. All living players take the combined Burn damage from all living players' hands at the start of the enemy turn. This Burn contributes {Damage}. | Original numeric/cost/keyword upgrade retained; see source. |
| Colorless | Infection | Both branches | Unplayable. At turn end, gain Infected for {Damage} damage at the start of the enemy turn. If unblocked, add Dazed to every living player's discard pile (1 per 3 Infected damage). | Original numeric/cost/keyword upgrade retained; see source. |
| Colorless | Intercept | Both branches | Gain {Block} Block. Cover another player, redirecting their incoming powered attacks to you this turn. | Original numeric/cost/keyword upgrade retained; see source. |
| Colorless | Knockdown | Both branches | Deal {Damage} damage. Add {DamageBonus}% Knockdown for your applied instance. | Original numeric/cost/keyword upgrade retained; see source. |
| Colorless | Tag Team | Both branches | Deal {Damage} damage. When another player manually plays an eligible Attack against this enemy, copy and automatically play that Attack as you, then remove the copy. | Original numeric/cost/keyword upgrade retained; see source. |
| Colorless | The Ball | Beta only | Deal {Damage} damage. Increase this card's damage by {Increase} this combat. Dunk on the target, then pass the card to another living player when returned. | Original numeric/cost/keyword upgrade retained; see source. |
| Colorless | Toxic | Both branches | Exhaust. At turn end, gain Toxified for {Damage} damage at the start of the enemy turn. | Original numeric/cost/keyword upgrade retained; see source. |
| Colorless | Wither | Both branches | Unplayable. At turn end, gain Withered for {Damage} damage at the start of the enemy turn. | Original numeric/cost/keyword upgrade retained; see source. |
| Curses | Decay | Both branches | Unplayable. At turn end, gain Decayed for {Damage} damage at the start of the enemy turn. | Original numeric/cost/keyword upgrade retained; see source. |
| Defect | Chaos | Both branches | Channel {Orbs} random Orb(s) for the targeted player. | Original numeric/cost/keyword upgrade retained; see source. |
| Defect | Compact | Both branches | Give the targeted player {Block} Block. Transform transformable Status cards in their hand into Fuel. | Give the targeted player {Block} Block. Transform transformable Status cards in their hand into upgraded Fuel. |
| Defect | Darkness | Both branches | Channel a Dark Orb for the targeted player. Trigger every living player's Dark Orb passives once. | Channel a Dark Orb for the targeted player. Trigger every living player's Dark Orb passives twice. |
| Defect | Flak Cannon | Both branches | Exhaust all your unexhausted Status cards. Deal {Damage} damage to a random enemy for each card exhausted. | Exhaust all players' unexhausted Status cards. Deal {Damage} damage to a random enemy for each card exhausted. |
| Defect | Focused Strike | Both branches | Deal {Damage} damage. Give each living player {HotfixPower} temporary Focus. | Original numeric/cost/keyword upgrade retained; see source. |
| Defect | Hotfix | Both branches | Give each living player {HotfixPower} temporary Focus. Exhaust. | Give each living player {HotfixPower} temporary Focus. |
| Defect | Loop | Both branches | Gain 1 Loop. | All players gain 1 Loop. |
| Defect | Shadow Shield | Both branches | Give the targeted player {Block} Block and channel a Dark Orb for them. | Original numeric/cost/keyword upgrade retained; see source. |
| Defect | Stack | Both branches | Give the targeted player {CalculatedBlock} Block, based on the number of cards in your discard pile. | Original numeric/cost/keyword upgrade retained; see source. |
| Defect | White Noise | Both branches | Add a random Power from your character's card pool to the targeted player's hand. It costs 0 this turn. Exhaust. | Original numeric/cost/keyword upgrade retained; see source. |
| Ironclad | Armaments | Both branches | Gain {Block} Block. Upgrade a card in the targeted player's hand for this combat. | Gain {Block} Block. Upgrade all cards in the targeted player's hand for this combat. |
| Ironclad | Blaze | Beta only | Give another player {StrengthPower} Strength. Exhaust. | Original numeric/cost/keyword upgrade retained; see source. |
| Ironclad | Blood Wall | Both branches | Lose {HpLoss} HP. Give the targeted player {Block} Block. | Original numeric/cost/keyword upgrade retained; see source. |
| Ironclad | Cruelty | Both branches | Gain Cruelty: increase the Vulnerable damage bonus by 25%. | All players gain Cruelty: increase the Vulnerable damage bonus by 25%. |
| Ironclad | Evil Eye | Both branches | Give the targeted player {Block} Block. Repeat if you exhausted a card this turn. | Original numeric/cost/keyword upgrade retained; see source. |
| Ironclad | Flame Barrier | Both branches | Give the targeted player {Block} Block and Flame Barrier, dealing {DamageBack} damage back when attacked this turn. | Original numeric/cost/keyword upgrade retained; see source. |
| Ironclad | Infernal Blade | Both branches | Add a random Attack from your character's card pool to the targeted player's hand. It costs 0 this turn. Exhaust. | Original numeric/cost/keyword upgrade retained; see source. |
| Ironclad | Midnight | Beta only | Deal {Damage} damage. Costs 6 Energy per player before reductions. Each card exhausted by any player reduces its cost by 1 until played. | Original numeric/cost/keyword upgrade retained; see source. |
| Necrobinder | Cacophony | Beta only | Gain Cacophony for {Damage} damage, with its 33-card trigger. | Original numeric/cost/keyword upgrade retained; see source. |
| Necrobinder | Delay | Both branches | Give the targeted player {Block} Block and {Energy} Energy next turn. | Original numeric/cost/keyword upgrade retained; see source. |
| Necrobinder | Eidolon | Stable only | Exhaust your hand. If at least 9 cards were exhausted, give each living player 1 Intangible. Exhaust. | Original numeric/cost/keyword upgrade retained; see source. |
| Necrobinder | Grave Warden | Both branches | Give the targeted player {Block} Block. Shuffle a Soul into your draw pile. | Original numeric/cost/keyword upgrade retained; see source. |
| Necrobinder | Pull From Below | Both branches | Deal {Damage} damage {CalculatedHits} time(s), once for each Ethereal card played by any player this combat. | Original numeric/cost/keyword upgrade retained; see source. |
| Necrobinder | Soul Storm | Both branches | Deal {CalculatedDamage} damage: {CalculationBase} plus {ExtraDamage} for each Soul in all players' exhaust piles. | Original numeric/cost/keyword upgrade retained; see source. |
| Necrobinder | Undeath | Both branches | Give the targeted player {Block} Block. Add a copy of this card to their discard pile. | Original numeric/cost/keyword upgrade retained; see source. |
| Necrobinder | Underworld | Beta only | Gain 1 Underworld. Exhaust. | Original numeric/cost/keyword upgrade retained; see source. |
| Necrobinder | Wisp | Both branches | Give the targeted player {Energy} Energy. Exhaust. | Retain. Give the targeted player {Energy} Energy. Exhaust. |
| Regent | Bulwark | Both branches | Give the targeted player {Block} Block. Forge {Forge} for yourself. | Original numeric/cost/keyword upgrade retained; see source. |
| Regent | Glitterstream | Both branches | Give the targeted player {Block} Block now and {BlockNextTurn} Block next turn. | Original numeric/cost/keyword upgrade retained; see source. |
| Regent | Particle Wall | Both branches | Give the targeted player {Block} Block. | Original numeric/cost/keyword upgrade retained; see source. |
| Regent | Plot | Beta only | Each living player gains Plot: before their next hand draw, draw up to {Cards} colorless cards from their draw pile in addition to their normal draw. | Original numeric/cost/keyword upgrade retained; see source. |
| Regent | Reflect | Both branches | Give the targeted player {Block} Block and 1 Reflect. | Original numeric/cost/keyword upgrade retained; see source. |
| Regent | Royalties | Both branches | Gain {Gold} Royalties. | Gain {Gold} Royalties. Each living player also gains {Share} Royalties, a whole-number share of a 36-point pool. |
| Regent | Terraforming | Both branches | Give the targeted player {VigorPower} Vigor. | Original numeric/cost/keyword upgrade retained; see source. |
| Regent | Tutor | Beta only | Choose a card from another player's draw pile and put it in their hand. Exhaust. | Original numeric/cost/keyword upgrade retained; see source. |
| Silent | Concoct | Beta only | Give another player {ConcoctPower} Concoct. | Original numeric/cost/keyword upgrade retained; see source. |
| Silent | Fade | Beta only | Retain. Add a random Block-gaining Skill from your character's pool to another player's hand. It costs 0 this turn. Exhaust. | Retain. Add a random upgraded Block-gaining Skill from your character's pool to another player's hand. It costs 0 this turn. Exhaust. |
| Silent | Flanking | Both branches | Apply Flanking: increase the damage bonus by {DamageBonus}%. Exhaust. | Original numeric/cost/keyword upgrade retained; see source. |
| Silent | Memento Mori | Both branches | Deal {CalculatedDamage} damage: {CalculationBase} plus {ExtraDamage} for each card discarded by any player this turn. | Original numeric/cost/keyword upgrade retained; see source. |
| Silent | Shadowmeld | Both branches | Give the targeted player 1 Shadowmeld. | Original numeric/cost/keyword upgrade retained; see source. |
| Silent | Storm Of Steel | Both branches | Discard the targeted player's hand. Add that many Shivs to their hand. | Discard the targeted player's hand. Add that many upgraded Shivs to their hand. |
| Silent | Tracking | Both branches | Your and your pets' powered attacks deal 50% more damage to Weak monsters. | Each living player and their pets deal 50% more powered attack damage to Weak monsters. |

## Additional original card support

- All for One is removed from multiplayer unlocked-card pools, matching MPX. This is a pool restriction, not a rework class.
- Burn synchronizes the combined party-hand total after pile changes and player-turn start, then locks it at turn end.
- Midnight counts every player's exhausted cards, including cards exhausted before it entered combat, and uses 6 Energy per player before reductions.
- Intercept includes original Covered/Intercept damage measurement and intent/column redirection; Shirk remains excluded because it is an unrelated new player card.
- Knockdown additive stacking, Flanking percentage display, The Ball result-location handling, party-dependent cost coloring and Royalties shares are retained.
- Existing Decay now uses the shared rework system; its former narrow patch was removed to prevent duplicate effects.

## Multiplayer potions

Attack Potion, Skill Potion, Power Potion and Colorless Potion can target any ally, including their owner. The targeted player chooses among three generated cards; the chosen card is free this turn and goes into that player's hand. Attack/Skill/Power choices use the potion owner's unlocked character pool; Colorless uses the unlocked colorless pool. Solo retains vanilla behavior. MPX Potion Changes controls these four potion adaptations independently of the card switch.

The original source created the card for the target but passed the potion owner to the final hand-add command. This candidate consistently passes the target to both, correcting that recipient mismatch; real co-op verification is pending. The original potion-bar shrinking/reconciliation patch is included. No new potion models, potion-slot-granting relics or unrelated potion effects are added.

## Required local checks (not run)

Test all branch-eligible cards unupgraded/upgraded, self/ally targeting, generated-card ownership, save/resume, multiplayer-library previews, Burn synchronization, Intercept redirects and damage previews, Midnight reductions, Royalties rounding at 2/3/4 players, and all four potion choices with host and client as owner/recipient. Repeat with every difficulty setting off and on.

## Numeric and upgrade reference from retained source

Values below are source declarations. `Beta.Pick(stable, beta)` retains branch-specific values; computed variables are described in the effect table above. All costs are Energy unless the StarCost entry says otherwise.

| Card | Cost / Stars | Declared variables | Upgrade operations |
|---|---|---|---|
| Beckon | 1 | `HpLossVar(6m)` | Conditional upgrade effect in table; otherwise none. |
| Burn | -1 | `DamageVar(2m, (ValueProp)`; `CalculationBaseVar(0m)`; `CalculationExtraVar(1m)` | Conditional upgrade effect in table; otherwise none. |
| Infection | -1 | `DamageVar(3m, (ValueProp)` | Conditional upgrade effect in table; otherwise none. |
| Intercept | 1 | `BlockVar(9m, (ValueProp)` | `(card.DynamicVars.Block).UpgradeValueBy(4m)` |
| Knockdown | 3 | `DamageVar(10m, (ValueProp)`; `IntVar("DamageBonus", 100m)` | `(card.DynamicVars.Damage).UpgradeValueBy(4m)`; `card.DynamicVars["DamageBonus"].UpgradeValueBy(100m)` |
| TagTeam | 2 | `DamageVar(11m, (ValueProp)` | `(card.DynamicVars.Damage).UpgradeValueBy(4m)` |
| TheBall | 1 | `DamageVar(10m, (ValueProp)`; `DynamicVar("Increase", 10m)` | `card.DynamicVars["Increase"].UpgradeValueBy(5m)` |
| Toxic | 1 | `DamageVar(5m, (ValueProp)` | Conditional upgrade effect in table; otherwise none. |
| Wither | -1 | `DamageVar(3m, (ValueProp)` | Conditional upgrade effect in table; otherwise none. |
| Decay | -1 | `DamageVar(2m, (ValueProp)` | Conditional upgrade effect in table; otherwise none. |
| Chaos | 1 | `IntVar("Orbs", 1m)` | `card.DynamicVars["Orbs"].UpgradeValueBy(1m)` |
| Compact | 1 | `BlockVar(6m, (ValueProp)` | `(card.DynamicVars.Block).UpgradeValueBy(1m)` |
| Darkness | 1 | None | Conditional upgrade effect in table; otherwise none. |
| FlakCannon | 2 | `DamageVar(7m, (ValueProp)`; `CalculationBaseVar(0m)`; `CalculationExtraVar(1m)` | Conditional upgrade effect in table; otherwise none. |
| FocusedStrike | 1 | `DamageVar(9m, (ValueProp)`; `PowerVar<HotfixPower>(1m)` | `(card.DynamicVars.Damage).UpgradeValueBy(2m)`; `DynamicVarSetExtensions.Power<HotfixPower>(card.DynamicVars).UpgradeValueBy(1m)` |
| Hotfix | 0 | `PowerVar<HotfixPower>(2m)` | `card.RemoveKeyword((CardKeyword)1)` |
| Loop | 1 | None | Conditional upgrade effect in table; otherwise none. |
| ShadowShield | 2 | `BlockVar(11m, (ValueProp)` | `(card.DynamicVars.Block).UpgradeValueBy(4m)` |
| Stack | 1 | `CalculationBaseVar(0m)`; `CalculationExtraVar(1m)` | `(card.DynamicVars.CalculationBase).UpgradeValueBy(3m)` |
| WhiteNoise | 1 | None | `card.EnergyCost.UpgradeBy(-1)` |
| Armaments | 1 | `BlockVar(5m, (ValueProp)` | Conditional upgrade effect in table; otherwise none. |
| Blaze | 2 | `PowerVar<StrengthPower>(3m)` | `(card.DynamicVars.Strength).UpgradeValueBy(2m)` |
| BloodWall | 2 | `HpLossVar(2m)`; `BlockVar(16m, (ValueProp)` | `(card.DynamicVars.Block).UpgradeValueBy(4m)` |
| Cruelty | 1 | None | Conditional upgrade effect in table; otherwise none. |
| EvilEye | 1 | `BlockVar(8m, (ValueProp)` | `(card.DynamicVars.Block).UpgradeValueBy(3m)` |
| FlameBarrier | 2 | `BlockVar(12m, (ValueProp)`; `DynamicVar("DamageBack", 4m)` | `(card.DynamicVars.Block).UpgradeValueBy(4m)`; `card.DynamicVars["DamageBack"].UpgradeValueBy(2m)` |
| InfernalBlade | 1 | None | `card.EnergyCost.UpgradeBy(-1)` |
| Midnight | 24 | `DamageVar(60m, (ValueProp)` | `(card.DynamicVars.Damage).UpgradeValueBy(12m)` |
| Cacophony | 2 | `CardsVar(33)`; `DamageVar(66m, (ValueProp)` | `(card.DynamicVars.Damage).UpgradeValueBy(33m)` |
| Delay | 2 | `BlockVar(11m, (ValueProp)`; `EnergyVar(1)` | `(card.DynamicVars.Block).UpgradeValueBy(2m)`; `(card.DynamicVars.Energy).UpgradeValueBy(1m)` |
| Eidolon | 2 | None | `card.EnergyCost.UpgradeBy(-1)` |
| GraveWarden | 1 | `BlockVar(8m, (ValueProp)` | `(card.DynamicVars.Block).UpgradeValueBy(3m)` |
| PullFromBelow | 1 | `DamageVar(2m, (ValueProp)`; `CalculationBaseVar(0m)`; `CalculationExtraVar(1m)` | `(card.DynamicVars.Damage).UpgradeValueBy(1m)` |
| SoulStorm | 1 | `CalculationBaseVar(9m)`; `ExtraDamageVar(Beta.Pick(2m, 4m)` | `card.DynamicVars["ExtraDamage"].UpgradeValueBy(Beta.Pick(1m, 2m))` |
| Undeath | 0 | `BlockVar(7m, (ValueProp)` | `(card.DynamicVars.Block).UpgradeValueBy(2m)` |
| Underworld | 3 | None | `card.EnergyCost.UpgradeBy(-1)` |
| Wisp | 0 | `EnergyVar(1)` | `card.AddKeyword((CardKeyword)5)` |
| Bulwark | 2 | `BlockVar(12m, (ValueProp)`; `ForgeVar(10)` | `(card.DynamicVars.Block).UpgradeValueBy(3m)`; `(card.DynamicVars.Forge).UpgradeValueBy(3m)` |
| Glitterstream | 2 | `BlockVar(11m, (ValueProp)`; `BlockVar("BlockNextTurn", 5m, (ValueProp)` | `(card.DynamicVars.Block).UpgradeValueBy(2m)`; `card.DynamicVars["BlockNextTurn"].UpgradeValueBy(2m)` |
| ParticleWall | 0 / 2 Stars | `BlockVar(9m, (ValueProp)` | `(card.DynamicVars.Block).UpgradeValueBy(3m)` |
| Plot | 1 | `CardsVar(2)` | `card.EnergyCost.UpgradeBy(-1)` |
| Reflect | 1 / 3 Stars | `BlockVar(16m, (ValueProp)` | `(card.DynamicVars.Block).UpgradeValueBy(4m)` |
| Royalties | 1 | `GoldVar(30)`; `IntVar("SplitPool", 36m)`; `IntVar("Share", 0m)`; `IntVar("Payout", 0m)` | `(card.DynamicVars.Gold).UpgradeValueBy(-20m)` |
| Terraforming | 1 | `PowerVar<VigorPower>(Beta.Pick(6m, 7m)` | `card.DynamicVars["VigorPower"].UpgradeValueBy(Beta.Pick(2m, 3m))` |
| Tutor | 1 | None | `card.EnergyCost.UpgradeBy(-1)` |
| Concoct | 1 | `PowerVar<ConcoctPower>(3m)` | `card.DynamicVars["ConcoctPower"].UpgradeValueBy(1m)` |
| Fade | 1 | None | Conditional upgrade effect in table; otherwise none. |
| Flanking | 2 | `IntVar("DamageBonus", 100m)` | `card.EnergyCost.UpgradeBy(-1)` |
| MementoMori | 1 | `CalculationBaseVar(9m)`; `ExtraDamageVar(4m)` | `(card.DynamicVars.CalculationBase).UpgradeValueBy(2m)`; `(card.DynamicVars.ExtraDamage).UpgradeValueBy(1m)` |
| Shadowmeld | 1 | None | `card.EnergyCost.UpgradeBy(-1)` |
| StormOfSteel | 1 | None | Conditional upgrade effect in table; otherwise none. |
| Tracking | 2 | None | Conditional upgrade effect in table; otherwise none. |
