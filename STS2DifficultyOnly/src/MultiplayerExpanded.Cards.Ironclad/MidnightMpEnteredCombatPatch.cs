using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Ironclad;

[BetaOnly]
[HarmonyPatch(typeof(Midnight), "AfterCardEnteredCombat")]
internal static class MidnightMpEnteredCombatPatch
{
	private static bool Prefix(CardModel __instance, CardModel card, ref Task __result)
	{
		if (!MidnightMp.Active())
		{
			return true;
		}
		__result = Task.CompletedTask;
		if (card != __instance || __instance.IsClone)
		{
			return false;
		}
		int num = CombatManager.Instance.History.Entries.OfType<CardExhaustedEntry>().Count();
		__instance.EnergyCost.AddUntilPlayed(-num, false);
		return false;
	}
}
