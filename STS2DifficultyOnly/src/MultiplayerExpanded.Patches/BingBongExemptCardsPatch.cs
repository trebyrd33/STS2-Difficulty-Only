using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MultiplayerExpanded.Cards;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch(typeof(BingBong), "AfterCardChangedPiles", new Type[]
{
	typeof(CardModel),
	typeof(PileType),
	typeof(AbstractModel)
})]
internal static class BingBongExemptCardsPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool Prefix(CardModel card, ref Task __result)
	{
		if (!(card is IBingBongExempt))
		{
			return true;
		}
		__result = Task.CompletedTask;
		return false;
	}
}
