using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Models.CardModel), "HoverTips", MethodType.Getter)]
internal static class GlobeHeadGalvanizedCardHoverTipsPatch
{
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		return GlobeHeadGalvanizedText.ReplaceCall(instructions, AccessTools.PropertyGetter(typeof(AfflictionModel), "HoverTips"), AccessTools.Method(typeof(GlobeHeadGalvanizedText), "AfflictionHoverTips", (Type[])null, (Type[])null), "CardModel.HoverTips");
	}
}
