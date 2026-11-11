using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
using MultiplayerExpanded.Features.Reworks;

namespace MultiplayerExpanded.Cards.Colorless;

[HarmonyPatch(typeof(Hook), "AfterCardChangedPiles")]
internal static class BurnMpPileChangePatch
{
	private static void Postfix(ref Task __result, ICombatState? combatState)
	{
		if (combatState != null && ReworkPatches.IsMp())
		{
			__result = BurnMp.SyncAfter(__result, combatState);
		}
	}
}
