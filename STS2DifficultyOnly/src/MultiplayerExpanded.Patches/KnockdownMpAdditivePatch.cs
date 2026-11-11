using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch(typeof(KnockdownPower), "ModifyDamageMultiplicative")]
internal static class KnockdownMpAdditivePatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool Prefix(KnockdownPower __instance, Creature? target, ValueProp props, Creature? dealer, ref decimal __result)
	{
		if (!MultiplayerExpanded.Features.Reworks.ReworkPatches.IsMp()) return true;
		__result = 1m;
		if (target == null || target != ((PowerModel)__instance).Owner || !ValuePropExtensions.IsPoweredAttack(props))
		{
			return false;
		}
		KnockdownPower val = null;
		decimal num = 1m;
		foreach (PowerModel power in target.Powers)
		{
			KnockdownPower val2 = (KnockdownPower)(object)((power is KnockdownPower) ? power : null);
			if (val2 != null && ((PowerModel)val2).Applier != dealer)
			{
				if (val == null)
				{
					val = val2;
				}
				num += (decimal)((PowerModel)val2).Amount / 100m;
			}
		}
		if (val == __instance)
		{
			__result = num;
		}
		return false;
	}
}
