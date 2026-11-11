using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.Settings;
namespace MultiplayerExpanded.Features.DeathMechanic;
internal static class DeathMechanicGate {
 public static bool IsActive(MultiplayerDeathMode mode, IRunState? runState) {
  var net = RunManager.Instance?.NetService;
  return DeathModeRules.IsActive(MultiplayerExpandedConfig.DeathMode,mode,runState?.Players.Count ?? 0,
   net != null && NetGameTypeExtensions.IsMultiplayer(net.Type));
 }
}
