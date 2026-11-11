using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Features.Reworks;

[HarmonyPatch]
internal static class ReworkDescriptionCallSitePatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.GetDeclaredMethods(typeof(CardModel)).First((MethodInfo m) => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		MethodInfo getDesc = AccessTools.PropertyGetter(typeof(CardModel), "Description");
		MethodInfo redirect = AccessTools.Method(typeof(ReworkDescriptionCallSitePatch), "Redirect", (Type[])null, (Type[])null);
		int injected = 0;
		foreach (CodeInstruction ins in instructions)
		{
			yield return ins;
			if (CodeInstructionExtensions.Calls(ins, getDesc))
			{
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Call, (object)redirect);
				injected++;
			}
		}
		if (injected == 0)
		{
			MainFile.Logger.Error("[Reworks] GetDescriptionForPile transpiler found no get_Description call — reworked cards will render base loc text. The method's IL likely changed in a game update.", 1);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static LocString Redirect(LocString loc, CardModel card)
	{
		if (!ReworkPatches.ShowReworked() || !ReworkRegistry.TryGet(((object)card).GetType(), out ReworkInfo info))
		{
			return loc;
		}
		LocString val = new LocString("cards", info.LocKey + (card.IsUpgraded ? ".descriptionUpgraded" : ".description"));
		if (!val.Exists())
		{
			return loc;
		}
		return val;
	}
}
