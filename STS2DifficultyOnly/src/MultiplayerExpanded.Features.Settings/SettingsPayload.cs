using System;
using System.Collections.Generic;
using System.Text.Json;
namespace MultiplayerExpanded.Features.Settings;
internal static class SettingsPayload {
 internal static readonly string[] DifficultyNames = { "SurroundedByBosses", "EnemyReworks", "EnemyDamageCompensation", "IncreasedEnemyScaling" };
 internal static readonly string[] BooleanNames = { "SurroundedByBosses", "EnemyReworks", "EnemyDamageCompensation", "IncreasedEnemyScaling", "VanillaCardReworks", "MultiplayerPotionChanges", "MultiplayerEvents" };
 internal static readonly string[] Names = { "SurroundedByBosses", "EnemyReworks", "EnemyDamageCompensation", "IncreasedEnemyScaling", "VanillaCardReworks", "MultiplayerPotionChanges", "MultiplayerEvents", "DeathMode" };
 internal static Dictionary<string,object> Parse(string json) {
  if (json.Length > 1024) throw new ArgumentException("Settings payload exceeds 1024 characters.");
  using var document = JsonDocument.Parse(json);
  if (document.RootElement.ValueKind != JsonValueKind.Object) throw new ArgumentException("Settings must be an object.");
  var result = new Dictionary<string,object>(StringComparer.Ordinal);
  foreach(var property in document.RootElement.EnumerateObject()) {
   if (result.ContainsKey(property.Name)) throw new ArgumentException("Duplicate setting: " + property.Name);
   if (property.Name == "DeathMode") {
    if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetInt32(out var mode) || !Enum.IsDefined(typeof(MultiplayerDeathMode),mode))
     throw new ArgumentException("Invalid death mode.");
    result.Add(property.Name,(MultiplayerDeathMode)mode);
   } else if (Array.IndexOf(BooleanNames,property.Name) >= 0 && property.Value.ValueKind is JsonValueKind.True or JsonValueKind.False) {
    result.Add(property.Name,property.Value.GetBoolean());
   } else throw new ArgumentException("Unknown or non-boolean difficulty setting: " + property.Name);
  }
  if (result.Count != Names.Length) throw new ArgumentException("Expected seven boolean settings and DeathMode.");
  return result;
 }
}
