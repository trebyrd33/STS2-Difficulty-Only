using System;
using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Cards;

namespace MultiplayerExpanded.Features.PartyScaling;

internal static class PartyScaledDynamicVar
{
	internal static decimal Amount(AbstractModel? owner, in MultiplayerExpandedCard.PartyScaledVar rule, decimal printed)
	{
		CardModel val = (CardModel)(object)((owner is CardModel) ? owner : null);
		if (val == null)
		{
			return printed;
		}
		ICombatState combatState = val.CombatState;
		if (combatState != null)
		{
			return rule.For(val, combatState.Players.Count);
		}
		return printed;
	}

	internal static DynamicVar? Wrap(DynamicVar declared, in MultiplayerExpandedCard.PartyScaledVar rule)
	{
		if (declared is IPartyScaledDynamicVar)
		{
			return null;
		}
		decimal baseValue = declared.BaseValue;
		if (declared is IntVar)
		{
			return (DynamicVar?)(object)new PartyScaledIntVar(declared.Name, baseValue, rule);
		}
		Type type = ((object)declared).GetType();
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PowerVar<>))
		{
			return (DynamicVar)Activator.CreateInstance(typeof(PartyScaledPowerVar<>).MakeGenericType(type.GetGenericArguments()), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[3] { declared.Name, baseValue, rule }, null);
		}
		MainFile.Logger.Error($"[PartyScaled] No computed var for flavour {type.Name} ('{declared.Name}'); it will keep its " + "printed amount. Add a wrapper for it in PartyScaledDynamicVar.cs.", 1);
		return null;
	}
}
