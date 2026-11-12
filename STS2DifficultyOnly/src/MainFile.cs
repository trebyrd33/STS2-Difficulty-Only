using System;
using System.Linq;
using BaseLib.Config;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Features.Settings;
namespace MultiplayerExpanded;
[ModInitializer(nameof(Initialize))]
public static class MainFile {
 public const string ModId = "STS2DifficultyOnly";
 public const string ResPath = "res://STS2DifficultyOnly";
 public static Logger Logger { get; } = new Logger(ModId, (LogType)0);
 public static void Initialize() {
  if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "MultiplayerExpanded"))
   throw new InvalidOperationException("Disable Multiplayer Expanded before enabling Difficulty Only.");
  foreach (var resource in new[] {
   "res://STS2DifficultyOnly/scenes/encounters/double_boss.tscn",
   "res://STS2DifficultyOnly/scenes/encounters/double_boss_queen.tscn",
   "res://STS2DifficultyOnly/localization/eng/powers.json",
   "res://STS2DifficultyOnly/localization/eng/intents.json",
   "res://STS2DifficultyOnly/images/powers/power.tres"
  }) {
   if (!Godot.FileAccess.FileExists(resource))
    throw new System.IO.FileNotFoundException("Install STS2DifficultyOnly.pck beside the DLL; missing resource: " + resource);
  }
  var config = new MultiplayerExpandedConfig();
  ModConfigRegistry.Register(ModId, config);
  config.ConfigChanged += (_, _) => { MpxSettingsSync.EnforceCurrentSettings(); MultiplayerExpanded.Features.Reworks.ReworkPatches.RefreshSettings(); };
  Logger.Info($"Difficulty Only 0.5.0-test2; branch: {(Beta.IsOn ? "beta" : "stable")}; game: {BetaMainCompatibility.Version}",1);
  GameApi.Init();
  var harmony = new Harmony(ModId);
  try {
   BranchPatching.PatchAll(harmony);
   if (BranchPatching.Failed.Count != 0 || StableResolve.Unresolved.Count != 0)
    throw new InvalidOperationException("Difficulty patches incomplete: " + string.Join("; ", BranchPatching.Failed.Concat(StableResolve.Unresolved)));
   MonsterReworkPatches.Install(harmony);
            MultiplayerExpanded.Features.Reworks.ReworkPatches.Install(harmony);
   MultiplayerExpanded.Patches.CardPotionOnUsePatch.Apply(harmony);
  } catch { harmony.UnpatchAll(ModId); throw; }
 }
}
