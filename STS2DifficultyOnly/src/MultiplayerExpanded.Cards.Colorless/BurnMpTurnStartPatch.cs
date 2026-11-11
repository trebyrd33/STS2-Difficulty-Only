using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
using MultiplayerExpanded.Features.Reworks;

namespace MultiplayerExpanded.Cards.Colorless;

[HarmonyPatch(typeof(Hook), "AfterSideTurnStart")]
internal static class BurnMpTurnStartPatch
{
	private static void Postfix(ref Task __result, ICombatState combatState, CombatSide side)
	{
		if ((int)side == 1 && ReworkPatches.IsMp())
		{
			__result = BurnMp.SyncAfter(__result, combatState);
		}
	}
}
