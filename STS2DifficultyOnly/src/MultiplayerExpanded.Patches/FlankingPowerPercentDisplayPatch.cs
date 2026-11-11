using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch(typeof(NPower), "RefreshAmount")]
internal static class FlankingPowerPercentDisplayPatch
{
	private static readonly AccessTools.FieldRef<NPower, PowerModel?>? ModelField = Ref<PowerModel>("_model");

	private static readonly AccessTools.FieldRef<NPower, MegaLabel>? AmountLabel = Ref<MegaLabel>("_amountLabel");

	private static AccessTools.FieldRef<NPower, T>? Ref<T>(string name)
	{
		if (!(AccessTools.DeclaredField(typeof(NPower), name) != null))
		{
			return null;
		}
		return AccessTools.FieldRefAccess<NPower, T>(name);
	}

	private static bool Prepare()
	{
		if (AccessTools.DeclaredMethod(typeof(NPower), "RefreshAmount", (Type[])null, (Type[])null) != null && ModelField != null && AmountLabel != null)
		{
			return true;
		}
		MainFile.Logger.Error("[FlankingPowerPercentDisplay] NPower.RefreshAmount, _model or _amountLabel not resolved; the Flanking icon will show the raw multiplier.", 1);
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPostfix]
	private static void Postfix(NPower __instance)
	{
		if (!MultiplayerExpanded.Features.Reworks.ReworkPatches.IsMp()) return;
		try
		{
			PowerModel obj = ModelField.Invoke(__instance);
			FlankingPower val = (FlankingPower)(object)((obj is FlankingPower) ? obj : null);
			if (val != null && (int)((PowerModel)val).StackType == 1)
			{
				int num = (((PowerModel)val).DisplayAmount - 1) * 100;
				AmountLabel.Invoke(__instance).SetTextAutoSize(num.ToString(CultureInfo.InvariantCulture));
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[FlankingPowerPercentDisplay] {value}", 1);
		}
	}
}
