using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Cards;
using MultiplayerExpanded.Features.Reworks;

namespace MultiplayerExpanded.Features.PartyScaling;

internal static class PartyScaledVarsPatch
{
	[HarmonyPatch(typeof(DynamicVarSet), "InitializeWithOwner")]
	private static class InstallComputedVars
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		[HarmonyPostfix]
		private static void Postfix(DynamicVarSet __instance, AbstractModel model)
		{
			CardModel val = (CardModel)(object)((model is CardModel) ? model : null);
			if (val == null)
			{
				return;
			}
			IEnumerable<MultiplayerExpandedCard.PartyScaledVar> enumerable = RulesFor(val);
			if (enumerable == null)
			{
				return;
			}
			Dictionary<string, DynamicVar> dictionary = (Dictionary<string, DynamicVar>)VarsField.GetValue(__instance);
			foreach (MultiplayerExpandedCard.PartyScaledVar item in enumerable)
			{
				MultiplayerExpandedCard.PartyScaledVar rule = item;
				if (!dictionary.TryGetValue(rule.Name, out var value))
				{
					MainFile.Logger.Error($"[PartyScaled] {((AbstractModel)val).Id.Entry} scales '{rule.Name}', but no var by that name is " + "declared in its vars. Check the name against CanonicalVars / CardRework.Vars.", 1);
				}
				else
				{
					DynamicVar val2 = PartyScaledDynamicVar.Wrap(value, in rule);
					if (val2 != null)
					{
						val2.SetOwner(model);
						dictionary[rule.Name] = val2;
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(DynamicVarSet), "ClearPreview")]
	private static class KeepComputedPreview
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		[HarmonyPostfix]
		private static void Postfix(DynamicVarSet __instance)
		{
			foreach (DynamicVar value in __instance.Values)
			{
				if (value is IPartyScaledDynamicVar partyScaledDynamicVar)
				{
					partyScaledDynamicVar.RefreshPreview();
				}
			}
		}
	}

	private static readonly FieldInfo VarsField = AccessTools.Field(typeof(DynamicVarSet), "_vars");

	private static IEnumerable<MultiplayerExpandedCard.PartyScaledVar>? RulesFor(CardModel card)
	{
		if (!(card is MultiplayerExpandedCard multiplayerExpandedCard))
		{
			return ReworkPatches.PartyScaledRulesFor(card);
		}
		return multiplayerExpandedCard.PartyScaledVarRules;
	}
}
