using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch]
internal static class GlobeHeadGalvanizedCardTextPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.GetDeclaredMethods(typeof(CardModel)).First((MethodInfo m) => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		return GlobeHeadGalvanizedText.ReplaceCall(instructions, AccessTools.PropertyGetter(typeof(AfflictionModel), "DynamicExtraCardText"), AccessTools.Method(typeof(GlobeHeadGalvanizedText), "ExtraCardText", (Type[])null, (Type[])null), "CardModel.GetDescriptionForPile");
	}
}
