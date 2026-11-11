using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(DisintegrationPower), "AfterSideTurnEndLate")]
internal static class KnowledgeDemonDisintegrationTurnEndPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(DisintegrationPower __instance, ref Task __result)
	{
		if (!MonsterRework.IsMultiplayer(((PowerModel)__instance).Owner.CombatState))
		{
			return true;
		}
		__result = Task.CompletedTask;
		return false;
	}
}
