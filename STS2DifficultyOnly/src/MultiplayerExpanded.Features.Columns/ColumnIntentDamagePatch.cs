using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MultiplayerExpanded.Features.Intents;
using MultiplayerExpanded.Patches;

namespace MultiplayerExpanded.Features.Columns;

[HarmonyPatch(typeof(AttackIntent), "GetSingleDamage")]
internal static class ColumnIntentDamagePatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static void Prefix(AttackIntent __instance, Creature owner, out (AttackOriginalTargets.PreviewScope Preview, bool Measuring, Creature? PreviousMeasured)? __state)
	{
		__state = null;
		if (__instance is ITargetedIntent)
		{
			return;
		}
		IReadOnlyList<Creature> readOnlyList = ColumnCombat.TargetsOf(owner);
		if (readOnlyList == null)
		{
			return;
		}
		Player me = LocalContext.GetMe(owner.CombatState);
		Creature val = ((me != null) ? me.Creature : null);
		if (val != null)
		{
			if (ColumnAttacks.Reaches(readOnlyList, val))
			{
				__state = (AttackOriginalTargets.BeginPreview(owner, readOnlyList), false, null);
				return;
			}
			AttackOriginalTargets.PreviewScope item = AttackOriginalTargets.BeginPreview(owner, new Creature[1] { val });
			__state = (item, true, RedirectShare.BeginMeasuring(val));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, (AttackOriginalTargets.PreviewScope Preview, bool Measuring, Creature? PreviousMeasured)? __state)
	{
		if (__state.HasValue)
		{
			(AttackOriginalTargets.PreviewScope, bool, Creature) valueOrDefault = __state.GetValueOrDefault();
			if (valueOrDefault.Item2)
			{
				RedirectShare.EndMeasuring(valueOrDefault.Item3);
			}
			AttackOriginalTargets.EndPreview(valueOrDefault.Item1);
		}
		return __exception;
	}
}
