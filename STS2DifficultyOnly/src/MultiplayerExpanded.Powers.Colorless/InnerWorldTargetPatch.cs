using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Powers.Colorless;

[HarmonyPatch(typeof(CardModel), "IsValidTarget")]
internal class InnerWorldTargetPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(CardModel __instance, Creature? target, ref bool __result)
	{
		if (!__result || !((AbstractModel)__instance).IsMutable)
		{
			return;
		}
		Player val = ((target != null) ? target.Player : null);
		if (val != null && val != __instance.Owner)
		{
			Player owner = __instance.Owner;
			if (owner != null && owner.Creature.HasPower<InnerWorldPower>())
			{
				__result = false;
			}
		}
	}
}
