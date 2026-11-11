using BaseLib.Config;
using MultiplayerExpanded.Features.Settings;
namespace STS2DifficultyOnly;
[ConfigHoverTipsByDefault]
internal class MultiplayerExpandedConfig : SimpleModConfig {
 public static bool VanillaCardReworks { get; set; } = true;
 public static bool MultiplayerPotionChanges { get; set; } = true;
 public static bool MultiplayerEvents { get; set; } = true;
 public static MultiplayerDeathMode DeathMode { get; set; } = MultiplayerDeathMode.Vanilla;
 public static bool SurroundedByBosses { get; set; } = true;
 public static bool EnemyReworks { get; set; } = true;
 public static bool EnemyDamageCompensation { get; set; } = false;
 public static bool IncreasedEnemyScaling { get; set; } = false;
}
