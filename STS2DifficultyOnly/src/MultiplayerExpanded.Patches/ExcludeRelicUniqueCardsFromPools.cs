using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Cards;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch(typeof(CardPoolModel), "GetUnlockedCards")]
internal class ExcludeRelicUniqueCardsFromPools
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(ref IEnumerable<CardModel> __result)
	{
		__result = __result.Where((CardModel c) => !(c is MultiplayerExpandedCard multiplayerExpandedCard) || !multiplayerExpandedCard.IsRelicUnique);
	}
}
