using System;
namespace MultiplayerExpanded.Features.Difficulty;
// Pure rules shared by the game patches and executable verification harness.
internal static class DifficultyRules {
 internal static decimal Scale(decimal vanilla, int players, bool enabled) =>
  enabled && players >= 3 ? 1m + (players - 1) * (vanilla - 1m) : vanilla;
 internal static decimal BaseDamage(decimal damage, int players, bool enabled) {
  if (!enabled || damage <= 0m || players < 3) return damage;
  return Math.Round(damage * (players >= 4 ? 1.25m : 1.1m), MidpointRounding.AwayFromZero);
 }
 internal static bool CombineBosses(bool enabled, int ascension) => enabled && ascension >= 10;
}
