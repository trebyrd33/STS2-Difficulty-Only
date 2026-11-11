using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Features.Reworks;

[HarmonyPatch(typeof(CardModel), "AfterCreated")]
internal static class ReworkAfterCreatedPatch
{
	private static readonly FieldInfo DynamicVarsField = AccessTools.Field(typeof(CardModel), "_dynamicVars");

	private static readonly FieldInfo KeywordsField = AccessTools.Field(typeof(CardModel), "_keywords");

	private static readonly FieldInfo EnergyCostField = AccessTools.Field(typeof(CardModel), "_energyCost");

	internal static void Rebuild(CardModel card)
	{
		DynamicVarsField.SetValue(card, null);
		KeywordsField.SetValue(card, null);
		EnergyCostField.SetValue(card, null);
		_ = card.DynamicVars;
		_ = card.Keywords;
		_ = card.EnergyCost;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(CardModel __instance)
	{
		if (ReworkPatches.ShowReworked() && ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo _))
		{
			Rebuild(__instance);
		}
	}
}
