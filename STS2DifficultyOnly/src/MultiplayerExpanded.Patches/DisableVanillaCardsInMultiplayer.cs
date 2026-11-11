using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch(typeof(CardPoolModel), "GetUnlockedCards")]
internal class DisableVanillaCardsInMultiplayer
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(ref IEnumerable<CardModel> __result, CardMultiplayerConstraint multiplayerConstraint)
	{
		if (MultiplayerExpandedConfig.VanillaCardReworks && (int)multiplayerConstraint == 1)
		{
			__result = __result.Where((CardModel c) => !(c is AllForOne));
		}
	}
}
