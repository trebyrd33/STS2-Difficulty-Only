using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
namespace MultiplayerExpanded.Features.BetaCompat;
[HarmonyPatch(typeof(ModManager), "GetModdedLocTables")]
internal static class DifficultyLocalizationPatch {
 private static void Postfix(string language, string file, ref IEnumerable<string> __result) {
  var paths = __result.ToList();
  var english = $"res://STS2DifficultyOnly/localization/eng/{file}";
  // Keep localized entries last so they can override this English fallback.
  if (FileAccess.FileExists(english) && !paths.Contains(english)) paths.Insert(0,english);
  __result = paths;
 }
}
