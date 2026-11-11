using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Cards.Curses;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch(typeof(CardModel), "CanPlayTargeting")]
internal class VogelfreiUntargetablePatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(CardModel __instance, Creature? target, ref bool __result)
	{
		if (__result && HoldsVogelfreiAgainst(__instance, target))
		{
			__result = false;
		}
	}

	internal static bool HoldsVogelfreiAgainst(CardModel card, Creature? target)
	{
		Player val = ((target != null) ? target.Player : null);
		if (val == null || val == card.Owner)
		{
			return false;
		}
		return PileTypeExtensions.GetPile((PileType)2, val).Cards.Any((CardModel c) => c is Vogelfrei);
	}
}
