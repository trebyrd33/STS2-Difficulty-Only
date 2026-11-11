using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.Settings;
namespace MultiplayerExpanded.Features.Difficulty;
internal static class ChallengeOptions {
 public static decimal IncreasedScaling(decimal vanilla) => DifficultyRules.Scale(vanilla,
  RunManager.Instance.DebugOnlyGetState()?.Players.Count ?? 1, MultiplayerExpandedConfig.IncreasedEnemyScaling);
 public static decimal CompensatedBaseDamage(decimal damage, ICombatState? combatState) =>
  DifficultyRules.BaseDamage(damage, combatState?.Players.Count ?? 1, MultiplayerExpandedConfig.EnemyDamageCompensation);
}
