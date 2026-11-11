using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Features.Reworks;

namespace MultiplayerExpanded.Patches;

[BetaOnly]
[HarmonyPatch(typeof(TheBall), "GetResultLocationForCardPlay")]
internal static class TheBallResultPilePatch
{
	private static void Postfix(CardModel __instance, ref CardLocation __result)
	{
		if (ReworkPatches.IsMp() && ReworkRegistry.TryGet(typeof(TheBall), out ReworkInfo _))
		{
			__result = new CardLocation(__instance.Owner, (PileType)0, (CardPilePosition)1);
		}
	}
}
