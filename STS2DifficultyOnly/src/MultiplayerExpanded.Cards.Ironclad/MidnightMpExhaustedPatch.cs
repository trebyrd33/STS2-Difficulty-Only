using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Ironclad;

[BetaOnly]
[HarmonyPatch(typeof(Midnight), "AfterCardExhausted")]
internal static class MidnightMpExhaustedPatch
{
	private static bool Prefix(CardModel __instance, ref Task __result)
	{
		if (!MidnightMp.Active())
		{
			return true;
		}
		__instance.EnergyCost.AddUntilPlayed(-1, false);
		__result = Task.CompletedTask;
		return false;
	}
}
