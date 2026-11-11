using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Models.PowerModel), "HoverTips", MethodType.Getter)]
internal static class GlobeHeadGalvanicHoverTipsPatch
{
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		MethodInfo extraTips = AccessTools.PropertyGetter(typeof(PowerModel), "ExtraHoverTips");
		MethodInfo redirect = AccessTools.Method(typeof(GlobeHeadGalvanizedText), "PowerExtraHoverTips", (Type[])null, (Type[])null);
		int injected = 0;
		foreach (CodeInstruction ins in instructions)
		{
			yield return ins;
			if (CodeInstructionExtensions.Calls(ins, extraTips))
			{
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Call, (object)redirect);
				injected++;
			}
		}
		if (injected == 0)
		{
			MainFile.Logger.Error("[GlobeHead] PowerModel.HoverTips transpiler found no get_ExtraHoverTips call — Galvanic shows vanilla Galvanized text in multiplayer. The method's IL likely changed in a game update.", 1);
		}
	}
}
