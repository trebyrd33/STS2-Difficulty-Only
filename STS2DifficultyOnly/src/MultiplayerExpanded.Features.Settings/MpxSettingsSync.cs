using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using BaseLib.Config;
namespace MultiplayerExpanded.Features.Settings;
internal static class MpxSettingsSync {
 private static readonly PropertyInfo[] Properties = typeof(MultiplayerExpandedConfig)
  .GetProperties(BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly)
  .Where(p => (p.PropertyType == typeof(bool) || p.PropertyType == typeof(MultiplayerDeathMode)) && p.CanRead && p.CanWrite).OrderBy(p=>p.Name).ToArray();
 private static string? _originalJson;
 private static string? _hostJson;
 private static string? _runJson;
 private static bool _applying;
 public static bool OverrideActive { get; private set; }
 public static string Snapshot() => JsonSerializer.Serialize(Properties.ToDictionary(p=>p.Name,p=>p.GetValue(null)!));
 private static void ApplyJson(string json) {
  var values = SettingsPayload.Parse(json);
  foreach(var p in Properties) p.SetValue(null, values[p.Name]);
 }
 public static void ApplyOverride(string? json) {
  if (json == null) return;
  // Validate every field, including DeathMode, before changing any setting.
  SettingsPayload.Parse(json);
  if (_runJson != null && json != _hostJson) throw new InvalidOperationException("Difficulty settings are locked until the run ends.");
  if (!OverrideActive) { _originalJson = Snapshot(); OverrideActive = true; }
  _hostJson = json;
  ApplyJson(json);
  ModConfigRegistry.Get(MainFile.ModId)?.ConfigReloaded();
 }
 public static void Restore() {
  _runJson = null;
  if (!OverrideActive) return;
  if (_originalJson != null) ApplyJson(_originalJson);
  OverrideActive = false; _originalJson = null; _hostJson = null;
  ModConfigRegistry.Get(MainFile.ModId)?.ConfigReloaded();
 }
 internal static void LockRun() { _runJson = Snapshot(); }
 internal static void UnlockRun() { _runJson = null; }
 internal static void EnforceCurrentSettings() {
  if (_applying) return;
  var locked = _runJson ?? (OverrideActive ? _hostJson : null);
  if (locked == null) return;
  _applying = true;
  try { ApplyJson(locked); ModConfigRegistry.Get(MainFile.ModId)?.ConfigReloaded(); }
  finally { _applying = false; }
 }
 internal static bool CanSave => !OverrideActive;
}
