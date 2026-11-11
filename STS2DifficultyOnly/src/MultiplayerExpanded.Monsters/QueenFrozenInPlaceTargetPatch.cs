using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(CardModel), "IsValidTarget")]
internal static class QueenFrozenInPlaceTargetPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(CardModel __instance, Creature? target, ref bool __result)
	{
		if (__result && target != null && ((AbstractModel)__instance).IsMutable && (int)__instance.Type == 1)
		{
			Player owner = __instance.Owner;
			Creature val = ((owner != null) ? owner.Creature : null);
			if (val != null && val.HasPower<FrozenInPlacePower>() && FrozenInPlacePower.InOtherColumn(val, target))
			{
				__result = false;
			}
		}
	}
}
