using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Features.Intents;

[HarmonyPatch(typeof(Hook), "ModifyDamage")]
internal static class RedirectOnlyHitPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPostfix]
	[HarmonyPriority(800)]
	private static void Postfix(Creature? target, Creature? dealer, ValueProp props, ModifyDamageHookType modifyDamageHookType, ref decimal __result)
	{
		if (target != null && dealer != null && ((int)modifyDamageHookType & 0xE) == 14 && ValuePropExtensions.IsPoweredAttack(props) && dealer.Side != target.Side)
		{
			IReadOnlyCollection<Creature> readOnlyCollection = AttackOriginalTargets.Of(dealer);
			if (readOnlyCollection != null && !readOnlyCollection.Contains(target))
			{
				__result = 0m;
			}
		}
	}
}
