namespace MultiplayerExpanded.Features.Settings;
internal static class DeathModeRules {
 internal static bool IsActive(MultiplayerDeathMode selected, MultiplayerDeathMode requested, int players, bool multiplayer) =>
  requested != MultiplayerDeathMode.Vanilla && selected == requested && players > 1 && multiplayer;
}
