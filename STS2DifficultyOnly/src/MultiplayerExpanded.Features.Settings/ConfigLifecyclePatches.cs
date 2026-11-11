using BaseLib.Config;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
namespace MultiplayerExpanded.Features.Settings;
// This BaseLib version has no _savingDisabled field. Prevent host overrides
// from reaching the client's config file using the actual public Save method.
[HarmonyPatch(typeof(ModConfig), nameof(ModConfig.Save))]
internal static class ClientConfigSavePatch {
 private static bool Prefix(ModConfig __instance) => !(__instance is MultiplayerExpandedConfig) || MpxSettingsSync.CanSave;
}
[HarmonyPatch(typeof(RunManager), nameof(RunManager.Launch))]
internal static class FreezeDifficultyAtLaunchPatch {
 private static void Prefix() => MpxSettingsSync.LockRun();
}
[HarmonyPatch(typeof(RunManager), nameof(RunManager.CleanUp))]
internal static class UnlockDifficultyAtCleanupPatch {
 private static void Postfix() => MpxSettingsSync.UnlockRun();
}
