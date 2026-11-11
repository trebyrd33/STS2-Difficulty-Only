using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Patches;

internal static class RedirectShare
{
	[ThreadStatic]
	private static Creature? _measuring;

	internal static bool AnyMeasuring => _measuring != null;

	internal static bool IsMeasuring(Creature creature)
	{
		return _measuring == creature;
	}

	internal static Creature? BeginMeasuring(Creature from)
	{
		Creature? measuring = _measuring;
		_measuring = from;
		return measuring;
	}

	internal static void EndMeasuring(Creature? previous)
	{
		_measuring = previous;
	}

	internal static decimal AddShares(IRunState runState, ICombatState combatState, Creature target, Creature dealer, decimal damage, decimal shares, ValueProp props, CardModel? cardSource, CardPreviewMode previewMode)
	{
		if (shares == 0m)
		{
			return damage;
		}
		IEnumerable<AbstractModel> modifiers;
		return GameApi.ModifyDamage(runState, combatState, target, dealer, damage + shares, props, cardSource, null, (ModifyDamageHookType)8, previewMode, out modifiers);
	}

	internal static decimal Measure(IRunState runState, ICombatState combatState, Creature from, Creature dealer, decimal damage, ValueProp props, CardModel? cardSource, ModifyDamageHookType modifyDamageHookType, CardPreviewMode previewMode)
	{
		Creature previous = BeginMeasuring(from);
		try
		{
			IEnumerable<AbstractModel> modifiers;
			return decimal.Truncate(GameApi.ModifyDamage(runState, combatState, from, dealer, damage, props, cardSource, null, (ModifyDamageHookType)((int)modifyDamageHookType & -9), previewMode, out modifiers));
		}
		finally
		{
			EndMeasuring(previous);
		}
	}
}
