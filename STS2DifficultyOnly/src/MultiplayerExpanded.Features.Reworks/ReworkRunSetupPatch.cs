using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerExpanded.Features.Reworks;

[HarmonyPatch(typeof(RunManager), "InitializeShared")]
internal static class ReworkRunSetupPatch
{
	private static readonly PropertyInfo? StateProperty = AccessTools.Property(typeof(RunManager), "State");

	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPriority(0)]
	private static void Postfix(RunManager __instance)
	{
		if (ReworkRegistry.Built)
		{
			object? obj = StateProperty?.GetValue(__instance);
			object? state = ((obj is RunState) ? obj : null);
			if (StateProperty == null)
			{
				MainFile.Logger.Error("[Reworks] RunManager.State not found — run deck cards will keep the vars they were cloned with, which desyncs reworked cards in a loaded multiplayer run.", 1);
			}
			int value = ReworkStateSync.RebakeTemplates();
			int value2 = ReworkStateSync.RebakeRunCards((RunState?)state);
			MainFile.Logger.Info($"[Reworks] run setup (mp={ReworkPatches.IsMp()}): re-baked {value} template(s) and {value2} deck card(s).", 1);
		}
	}
}
